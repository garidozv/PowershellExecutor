using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// A service that provides a simple interface for working with PowerShell
/// </summary>
public class PowerShellService
{
    private readonly IPowerShell _powerShell;

    /// <summary>
    /// Creates an instance of <see cref="PowerShellService"/> and sets the
    /// working directory to the user's home directory 
    /// </summary>
    public PowerShellService(IPowerShell powerShell)
    {
        _powerShell = powerShell;
        SetToHomeDirectory();
    }

    /// <summary>
    /// Gets the path to current PowerShell working directory 
    /// </summary>
    public string WorkingDirectoryPath => _powerShell.WorkingDirectoryPath;

    /// <summary>
    /// Registers a custom cmdlet for use within the current PowerShell execution context.
    /// The cmdlet type is determined by the generic type parameter
    /// </summary>
    /// <typeparam name="T">
    /// The type of the custom cmdlet to register. It must be derived from <see cref="PSCmdlet"/>
    /// </typeparam>
    /// <remarks>
    /// Use this method only for cmdlets that conflict with existing functions of the same name.
    /// A common example is the 'Clear-Host' command, which is implemented as a function
    /// </remarks>
    public void RegisterCustomCmdlet<T>() where T : PSCmdlet
    {
        var cmdletAttribute = typeof(T).GetCustomAttribute<CmdletAttribute>();
        
        if (cmdletAttribute is null)
            throw new InvalidOperationException($"The type '{typeof(T).FullName}' does not have a {nameof(CmdletAttribute)}");
        
        var cmdletName = $"{cmdletAttribute.VerbName}-{cmdletAttribute.NounName}";
        
        // Try to remove any existing functions since they will override the cmdlet
        RemoveExistingFunction(cmdletName);
        ImportModule(typeof(T).Assembly);
    }

    /// <summary>
    /// Executes the given PowerShell script and returns the execution result
    /// </summary>
    /// <param name="script">The PowerShell script to be executed</param>
    /// <param name="outputAsString">
    /// If <c>true</c>, appends <c>| Out-String</c> to the script before execution to return a string representation of the output.
    /// If <c>false</c>, returns the raw result of the script.
    /// </param>
    /// <returns>The string representation of the script result, or <c>null</c> if there were no results</returns>
    public IEnumerable<PSObject>? ExecuteScript(string script, bool outputAsString = false)
    {
        ArgumentNullException.ThrowIfNull(script);
        
        try
        {
            _powerShell.Clear();
            _powerShell.AddScript(script);
            
            if (outputAsString)
                _powerShell.AddCommand("out-string");
            
            var invocationResult = _powerShell.Invoke();
            return invocationResult.Count > 0 ? invocationResult : null;
        }
        catch (ParseException e)
        {
            if (e.Errors is null)
            {
                _powerShell.ErrorStream.Add(e.ErrorRecord);
            }
            else
            {
                foreach (var error in e.Errors)
                {
                    _powerShell.ErrorStream.Add(new ErrorRecord(
                        new Exception(error.Message), "Parse error", ErrorCategory.ParserError, null));
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Sets a variable in the PowerShell runspace session state with the specified name and value
    /// </summary>
    /// <param name="name">The name of the variable to set in the session state</param>
    /// <param name="value">The value to assign to the variable</param>
    public void SetVariable(string name, object value) =>
        _powerShell.SetVariable(name, value);

    /// <summary>
    /// Subscribes to the PowerShell error stream
    /// </summary>
    public void SubscribeToErrorStream(Action<ErrorRecord> action) =>
        SubscribeToStream(action, _powerShell.ErrorStream);

    /// <summary>
    /// Subscribes to the PowerShell verbose stream
    /// </summary>
    public void SubscribeToVerboseStream(Action<VerboseRecord> action) =>
        SubscribeToStream(action, _powerShell.VerboseStream);

    /// <summary>
    /// Subscribes to the PowerShell warning stream
    /// </summary>
    public void SubscribeToWarningStream(Action<WarningRecord> action) =>
        SubscribeToStream(action, _powerShell.WarningStream);

    /// <summary>
    /// Subscribes to the PowerShell debug stream
    /// </summary>
    public void SubscribeToDebugStream(Action<DebugRecord> action) =>
        SubscribeToStream(action, _powerShell.DebugStream);

    /// <summary>
    /// Subscribes to the PowerShell information stream
    /// </summary>
    public void SubscribeToInformationStream(Action<InformationRecord> action) =>
        SubscribeToStream(action, _powerShell.InformationStream);
    
    /// <summary>
    /// Retrieves the list of command completions based on the given input and cursor position
    /// </summary>
    /// <param name="input">The input string for which completions are to be generated</param>
    /// <param name="cursorIndex">The index of the cursor position in the input string</param>
    /// <returns>A <see cref="CommandCompletion"/> object containing the available completions</returns>
    public CommandCompletion GetCommandCompletions(string input, int cursorIndex) =>
         _powerShell.GetCommandCompletion(input, cursorIndex);
    
    /// <summary>
    /// Determines whether the given completion corresponds to a directory path.
    /// If the provided completion is a relative path, it is combined with the
    /// current working directory to form the full path.
    /// </summary>
    /// <param name="completion">The completion string to check</param>
    /// <returns><c>true</c> if the completion corresponds to a directory; otherwise, <c>false</c>.</returns>
    public bool IsDirectoryCompletion(string completion)
    {
        ArgumentNullException.ThrowIfNull(completion);
        
        var fullPath = Path.IsPathRooted(completion) ? completion : Path.Combine(WorkingDirectoryPath, completion);
        return Directory.Exists(fullPath);
    }
    
    /// <summary>
    /// Removes an existing PowerShell function with the specified name
    /// </summary>
    /// <param name="cmdletName">The name of the function to be removed, including its namespace if necessary</param>
    private void RemoveExistingFunction(string cmdletName)
    {
        _powerShell.Clear();
        
        _powerShell.AddCommand("Remove-Item")
            .AddParameter("Path", $"function:{cmdletName}")
            .Invoke();
    }

    /// <summary>
    /// Imports a PowerShell module from the provided assembly
    /// </summary>
    /// <param name="assembly">The assembly containing the PowerShell module to be imported</param>
    private void ImportModule(Assembly assembly)
    {
        _powerShell.Clear();

        _powerShell.AddCommand("Import-Module")
            .AddParameter("Assembly", assembly)
            .Invoke();
    }

    /// <summary>
    /// Tries to set the current runspace location (working directory) to home directory
    /// </summary>
    private void SetToHomeDirectory()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _powerShell.WorkingDirectoryPath = homePath;
    }
    
    /// <summary>
    /// Subscribes to a specified PowerShell stream to handle its data items using the provided action
    /// </summary>
    /// <param name="action">The action to execute for each data item in the stream</param>
    /// <param name="stream">The PowerShell data stream to subscribe to</param>
    /// <typeparam name="T">The type of data items contained in the stream</typeparam>
    private static void SubscribeToStream<T>(Action<T> action, PSDataCollection<T> stream)
    {
        stream.DataAdded += (sender, args) =>
        {
            if (args.Index >= 0 && args.Index < stream.Count)
                action(stream[args.Index]);
        };
    }
}