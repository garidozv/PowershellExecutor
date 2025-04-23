using System.Management.Automation;

namespace PowerShellExecutor.Interfaces;

/// <summary>
/// Provides an interface for interacting with PowerShell runtime
/// </summary>
public interface IPowerShellHostService
{

    /// <summary>
    /// Gets the path to the current PowerShell working directory 
    /// </summary>
    public string WorkingDirectoryPath { get; }

    /// <summary>
    /// Stops the execution of the currently running PowerShell pipeline
    /// </summary>
    /// <remarks>
    /// Since <see cref="ExecuteScript"/> uses synchronous <c>Invoke</c> method,
    /// stopping the pipeline will return a partial result
    /// </remarks>
    public void StopExecution();

    /// <summary>
    /// Executes the given PowerShell script and returns the execution result
    /// </summary>
    /// <param name="script">The PowerShell script to be executed</param>
    /// <param name="outputAsString">
    /// If <c>true</c>, appends <c>| Out-String</c> to the script before execution to return a string representation of the output.
    /// If <c>false</c>, returns the raw result of the script.
    /// </param>
    /// <returns>The string representation of the script result, or <c>null</c> if execution was stopped</returns>
    public IEnumerable<PSObject>? ExecuteScript(string script, bool outputAsString = false);

    /// <summary>
    /// Sets a variable in the PowerShell runspace session state with the specified name and value
    /// </summary>
    /// <param name="name">The name of the variable to set in the session state</param>
    /// <param name="value">The value to assign to the variable</param>
    public void SetVariable(string name, object value);

    /// <summary>
    /// Subscribes to the PowerShell error stream
    /// </summary>
    public void SubscribeToErrorStream(Action<ErrorRecord> action);

    /// <summary>
    /// Subscribes to the PowerShell verbose stream
    /// </summary>
    public void SubscribeToVerboseStream(Action<VerboseRecord> action);

    /// <summary>
    /// Subscribes to the PowerShell warning stream
    /// </summary>
    public void SubscribeToWarningStream(Action<WarningRecord> action);
    /// <summary>
    /// Subscribes to the PowerShell debug stream
    /// </summary>
    public void SubscribeToDebugStream(Action<DebugRecord> action);

    /// <summary>
    /// Subscribes to the PowerShell information stream
    /// </summary>
    public void SubscribeToInformationStream(Action<InformationRecord> action);
}