namespace CommandDock.Domain.ValueObjects;

public enum OutputStream
{
    Stdout,
    Stderr,
}

public readonly record struct OutputLine(OutputStream Stream, string Text);
