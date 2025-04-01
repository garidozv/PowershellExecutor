namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// Represents the source of output returned after executing a PowerShell command
/// </summary>
public enum ResultOutputSource : int
{
    /// <summary>
    /// Indicates an output produced by successful command execution
    /// </summary>
    SuccessfulExecution,
    /// <summary>
    /// Indicates an output produced by a command execution error
    /// </summary>
    ExecutionError,
    /// <summary>
    /// Indicates an output produced by a command parsing error
    /// </summary>
    ParseError,
    /// <summary>
    /// Indicates an output produced by an exception or unexpected error that occurred during command execution
    /// </summary>
    Exception
}

/// <summary>
/// Represents the result of executing a PowerShell command
/// </summary>
public readonly struct PowerShellCommandResult(string output, ResultOutputSource outputSource)
{
    public string Output { get; init; } = output;
    public ResultOutputSource OutputSource { get; init; } = outputSource;
}