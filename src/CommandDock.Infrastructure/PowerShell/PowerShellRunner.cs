using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandDock.Application.Abstractions;
using CommandDock.Domain.ValueObjects;

namespace CommandDock.Infrastructure.PowerShell;

public sealed class PowerShellRunner : IRunner
{
    public async Task<ExecutionResult> ExecuteAsync(
        string commandText,
        Action<OutputLine> onOutput,
        CancellationToken cancellationToken)
    {
        var wrapped =
            "$OutputEncoding=[System.Text.UTF8Encoding]::new();" +
            "[Console]::OutputEncoding=[System.Text.UTF8Encoding]::new();" +
            commandText;
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(wrapped));

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        psi.ArgumentList.Add("-NoProfile");
        psi.ArgumentList.Add("-NonInteractive");
        psi.ArgumentList.Add("-EncodedCommand");
        psi.ArgumentList.Add(encoded);

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) onOutput(new OutputLine(OutputStream.Stdout, e.Data));
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) onOutput(new OutputLine(OutputStream.Stderr, e.Data));
        };

        var stopwatch = Stopwatch.StartNew();
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            stopwatch.Stop();
            return new ExecutionResult(process.ExitCode, stopwatch.Elapsed, Cancelled: false);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); }
            catch { /* already exited */ }
            stopwatch.Stop();
            return new ExecutionResult(ExitCode: -1, stopwatch.Elapsed, Cancelled: true);
        }
    }
}
