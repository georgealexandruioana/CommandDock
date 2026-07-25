using System;

namespace CommandDock.Domain.ValueObjects;

public readonly record struct ExecutionResult(int ExitCode, TimeSpan Duration, bool Cancelled);
