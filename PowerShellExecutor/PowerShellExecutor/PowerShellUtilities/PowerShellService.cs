using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// A service that provides a simple interface for working with PowerShell
/// </summary>
public class PowerShellService : IDisposable
{
    private bool _isDisposed;
    
    private readonly Runspace _runspace;
    private readonly PowerShell _powerShell;
    
    /// <summary>
    /// Creates and opens a dedicated <see cref="Runspace"/>.
    /// Uses the created <see cref="Runspace"/> to create a <see cref="PowerShell"/> instance
    /// used for executing PowerShell commands and sets its working directory to home directory
    /// </summary>
    public PowerShellService()
    {
        _runspace = RunspaceFactory.CreateRunspace();
        _runspace.Open();
        _powerShell = PowerShell.Create(_runspace);
        
        SetLocationToHomeDirectory();
    }
    
    /// <summary>
    /// Gets the path to current working directory of the PowerShell runspace
    /// </summary>
    public string WorkingDirectoryPath => _powerShell.Runspace.SessionStateProxy.Path.CurrentLocation.Path;

    /// <summary>
    /// Registers a custom cmdlet for use within the current PowerShell execution context.
    /// The cmdlet type is determined by the generic type parameter
    /// </summary>
    /// <typeparam name="T">
    /// The type of the custom cmdlet to register. It must be derived from <see cref="PSCmdlet"/>
    /// </typeparam>
    public void RegisterCustomCmdlet<T>() where T : PSCmdlet
    {
        var cmdletAttribute = typeof(T).GetCustomAttribute<CmdletAttribute>();
        if (cmdletAttribute is null)
            throw new InvalidOperationException($"The type '{typeof(T).FullName}' does not have a CmdletAttribute");
        var cmdletName = $"{cmdletAttribute.VerbName}-{cmdletAttribute.NounName}";

        /*
         * Try to remove any existing functions since they will override the cmdlet
         * For example Clear-Host is defined as function for some reason
         */
        RemoveExistingFunction(cmdletName);
        
        ImportModule(typeof(T).Assembly);
    }

    /// <summary>
    /// Executes the given PowerShell script and returns the execution result
    /// </summary>
    /// <param name="script">The PowerShell script to be executed</param>
    /// <returns>The string representation of the script result, or <c>null</c> if there were no results</returns>
    public PSObject? ExecuteScript(string script)
    {
        ArgumentNullException.ThrowIfNull(script);
        
        try
        {
            ClearCommandPipelineAndStreams();

            _powerShell.AddScript(script)
                .AddCommand("out-string");
            
            var invocationResult = _powerShell.Invoke();
            return invocationResult.Count > 0 ? invocationResult[0] : null;
        }
        catch (ParseException e)
        {
            if (e.Errors is null)
            {
                _powerShell.Streams.Error.Add(e.ErrorRecord);
            }
            else
            {
                foreach (var error in e.Errors)
                {
                    _powerShell.Streams.Error.Add(new ErrorRecord(
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
        _runspace.SessionStateProxy.SetVariable(name, value);

    /// <summary>
    /// Subscribes to the PowerShell error stream
    /// </summary>
    public void SubscribeToErrorStream(Action<ErrorRecord> action) =>
        SubscribeToStream(action, _powerShell.Streams.Error);

    /// <summary>
    /// Subscribes to the PowerShell verbose stream
    /// </summary>
    public void SubscribeToVerboseStream(Action<VerboseRecord> action) =>
        SubscribeToStream(action, _powerShell.Streams.Verbose);

    /// <summary>
    /// Subscribes to the PowerShell warning stream
    /// </summary>
    public void SubscribeToWarningStream(Action<WarningRecord> action) =>
        SubscribeToStream(action, _powerShell.Streams.Warning);

    /// <summary>
    /// Subscribes to the PowerShell debug stream
    /// </summary>
    public void SubscribeToDebugStream(Action<DebugRecord> action) =>
        SubscribeToStream(action, _powerShell.Streams.Debug);

    /// <summary>
    /// Subscribes to the PowerShell information stream
    /// </summary>
    public void SubscribeToInformationStream(Action<InformationRecord> action) =>
        SubscribeToStream(action, _powerShell.Streams.Information);
    
    /// <summary>
    /// Retrieves the list of command completions based on the given input and cursor position
    /// </summary>
    /// <param name="input">The input string for which completions are to be generated</param>
    /// <param name="cursorIndex">The index of the cursor position in the input string</param>
    /// <returns>A <see cref="CommandCompletion"/> object containing the available completions</returns>
    public CommandCompletion GetCommandCompletions(string input, int cursorIndex) =>
        CommandCompletion.CompleteInput(input, cursorIndex, null, _powerShell);
    
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
        ClearCommandPipelineAndStreams();
        
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
        ClearCommandPipelineAndStreams();

        _powerShell.AddCommand("Import-Module")
            .AddParameter("Assembly", assembly)
            .Invoke();
    }

    /// <summary>
    /// Tries to set the current runspace location (working directory) to home directory
    /// </summary>
    private void SetLocationToHomeDirectory()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _powerShell.Runspace.SessionStateProxy.Path.SetLocation(homePath);
    }

    /// <summary>
    /// Clears the PowerShell command pipeline and associated streams
    /// </summary>
    private void ClearCommandPipelineAndStreams()
    {
        _powerShell.Streams.ClearStreams();
        _powerShell.Commands.Clear();
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
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            if (disposing)
            {
                _powerShell.Dispose();
                _runspace.Close();
                _runspace.Dispose();
            }
        }
    }
}