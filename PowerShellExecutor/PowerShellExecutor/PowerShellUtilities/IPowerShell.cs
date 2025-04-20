using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// Represents an abstraction for executing PowerShell scripts and commands
/// </summary>
/// <remarks>
/// The main purpose of this type is to allow for easier testing of PowerShell dependant classes
/// </remarks>
public interface IPowerShell
{
    /// <summary>
    /// Gets the path to current working directory of the PowerShell runspace
    /// </summary>
    string WorkingDirectoryPath { get; set; }

    /// <summary>
    /// Gets the collection of error records generated during the execution of PowerShell commands
    /// </summary>
    PSDataCollection<ErrorRecord> ErrorStream { get; }

    /// <summary>
    /// Gets the collection of warning records generated during the execution of PowerShell commands
    /// </summary>
    PSDataCollection<WarningRecord> WarningStream { get; }

    /// <summary>
    /// Gets the collection of verbose records generated during the execution of PowerShell commands
    /// </summary>
    PSDataCollection<VerboseRecord> VerboseStream { get; }

    /// <summary>
    /// Gets the collection of debug records generated during the execution of PowerShell commands
    /// </summary>
    PSDataCollection<DebugRecord> DebugStream { get; }

    /// <summary>
    /// Gets the collection of information records generated during the execution of PowerShell commands
    /// </summary>
    PSDataCollection<InformationRecord> InformationStream { get; }

    /// <summary>
    /// Sets a variable in the current PowerShell session
    /// </summary>
    /// <param name="name">The name of the variable to set</param>
    /// <param name="value">The value to assign to the variable</param>
    void SetVariable(string name, object value);

    /// <summary>
    /// Retrieves the value of a variable from the current PowerShell session
    /// </summary>
    /// <param name="name">The name of the variable to retrieve</param>
    /// <returns>The value of the variable if it exists</returns>
    object GetVariable(string name);

    /// <summary>
    /// Adds a script to the current PowerShell pipeline
    /// </summary>
    /// <param name="script">The PowerShell script to add</param>
    /// <returns>An instance of <see cref="IPowerShell"/> with the added script</returns>
    IPowerShell AddScript(string script);

    /// <summary>
    /// Adds a cmdlet to the current PowerShell pipeline
    /// </summary>
    /// <param name="cmdlet">The name of the cmdlet to add to the PowerShell pipeline</param>
    /// <returns>An instance of <see cref="IPowerShell"/> with the added command</returns>
    IPowerShell AddCommand(string cmdlet);

    /// <summary>
    /// Adds a parameter to the current PowerShell command
    /// </summary>
    /// <param name="paramName">The name of the parameter to add</param>
    /// <param name="value">The value of the parameter to add</param>
    /// <returns>An instance of <see cref="IPowerShell"/> with the added parameter</returns>
    IPowerShell AddParameter(string paramName, object value);
    
    /// <summary>
    /// Adds a parameter to the current PowerShell command
    /// </summary>
    /// <param name="paramName">The name of the parameter to add</param>
    /// <returns>An instance of <see cref="IPowerShell"/> with the added parameter</returns>
    IPowerShell AddParameter(string paramName);

    /// <summary>
    /// Executes the current PowerShell pipeline and retrieves the results
    /// </summary>
    /// <returns>A collection of <see cref="PSObject"/> containing the results of the PowerShell execution</returns>
    Collection<PSObject> Invoke();

    /// <summary>
    /// Stops the execution of the current PowerShell pipeline
    /// </summary>
    void Stop();

    /// <summary>
    /// Retrieves command completion results for a specified command at a given position within the command string
    /// </summary>
    /// <param name="commandName">The command string for which completion results are to be generated</param>
    /// <param name="commandPosition">The position in the command string where completion should be evaluated</param>
    /// <returns>A <see cref="CommandCompletion"/> object containing possible completions for the specified command at the given position</returns>
    CommandCompletion GetCommandCompletion(string commandName, int commandPosition);

    /// <summary>
    /// Clears PowerShell pipeline and streams
    /// </summary>
    void Clear();
}