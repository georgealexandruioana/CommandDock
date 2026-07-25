using System;
using System.Threading;
using System.Threading.Tasks;
using CommandDock.Domain.ValueObjects;

namespace CommandDock.Application.Abstractions;

public interface IRunner
{
    Task<ExecutionResult> ExecuteAsync(
        string commandText,
        Action<OutputLine> onOutput,
        CancellationToken cancellationToken);
}
