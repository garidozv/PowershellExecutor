using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
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

    public Action? ExitCommandHandler { get; set; }
    
    /// <summary>
    /// Gets the path to current working directory of the PowerShell runspace
    /// </summary>
    public string WorkingDirectoryPath => _powerShell.Runspace.SessionStateProxy.Path.CurrentLocation.Path;

    /// <summary>
    /// Registers a custom cmdlet for use within the current PowerShell execution context.
    /// The cmdlet type is determined by the generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the custom cmdlet to register. It must be derived from <see cref="Cmdlet"/></typeparam>
    public void RegisterCustomCmdlet<T>() where T : Cmdlet
    {
        var cmdletAttribute = typeof(T).GetCustomAttribute<CmdletAttribute>();
        if (cmdletAttribute is null)
            throw new InvalidOperationException($"The type '{typeof(T).FullName}' does not have a CmdletAttribute");
        var cmdletName = $"{cmdletAttribute.VerbName}-{cmdletAttribute.NounName}";
        
        _powerShell.Commands.Clear();
        _powerShell.Streams.ClearStreams();

        /*
         * Try to remove any existing functions since they will override the cmdlet
         * For example Clear-Host is defined as function for some reason
         */
        _powerShell.AddCommand("Remove-Item")
            .AddParameter("Path", $"function:{cmdletName}")
            .Invoke();
        
        _powerShell.Commands.Clear();
        _powerShell.Streams.ClearStreams();
        
        _powerShell.AddCommand("Import-Module")
            .AddParameter("Assembly", typeof(T).Assembly)
            .Invoke();
    }

    /// <summary>
    /// Tries to set the current runspace location (working directory) to home directory
    /// </summary>
    private void SetLocationToHomeDirectory()
    {
        var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
        var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
    
        var homeDirectoryPath = homeDrive != null && homePath != null 
            ? $"{homeDrive}{homePath}" 
            : null;

        if (homeDirectoryPath is not null)
            _powerShell.Runspace.SessionStateProxy.Path.SetLocation(homeDirectoryPath);
    }

    /// <summary>
    /// Executes the given PowerShell script and returns the execution result
    /// </summary>
    /// <param name="script">The PowerShell script to be executed</param>
    /// <returns>The string representation of the script result</returns>
    public PSObject? ExecuteScript(string script)
    {
        ArgumentNullException.ThrowIfNull(script);
        
        try
        {
            _powerShell.Streams.ClearStreams();
            _powerShell.Commands.Clear();

            _powerShell.AddScript(script)
                .AddCommand("out-string");
            
            var invocationResult = _powerShell.Invoke();
            return invocationResult.Count > 0 ? invocationResult[0] : null;
        }
        catch (ParseException e)
        {
            foreach (var error in e.Errors)
            {
                _powerShell.Streams.Error.Add(new ErrorRecord(
                    new Exception(error.Message), "Parse error", ErrorCategory.ParserError, null));
            }
        }
        
        return null;
    }

    /// <summary>
    /// Sets a variable in the PowerShell runspace session state with the specified name and value
    /// </summary>
    /// <param name="name">The name of the variable to set in the session state</param>
    /// <param name="value">The value to assign to the variable</param>
    public void SetVariable(string name, object value)
    {
        _runspace.SessionStateProxy.SetVariable(name, value);
    }

    public void SubscribeToErrorStream(Action<ErrorRecord> action)
    {
        _powerShell.Streams.Error.DataAdded += (sender, args) => action(_powerShell.Streams.Error[args.Index]);
    }

    public void SubscribeToVerboseStream(Action<VerboseRecord> action)
    {
        _powerShell.Streams.Verbose.DataAdded += (sender, args) => action(_powerShell.Streams.Verbose[args.Index]);
    }

    public void SubscribeToWarningStream(Action<WarningRecord> action)
    {
        _powerShell.Streams.Warning.DataAdded += (sender, args) => action(_powerShell.Streams.Warning[args.Index]);
    }

    public void SubscribeToDebugStream(Action<DebugRecord> action)
    {
        _powerShell.Streams.Debug.DataAdded += (sender, args) => action(_powerShell.Streams.Debug[args.Index]);
    }

    public void SubscribeToInformationStream(Action<InformationRecord> action)
    {
        _powerShell.Streams.Information.DataAdded += (sender, args) => action(_powerShell.Streams.Information[args.Index]);
    }
    
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
    /// Determines whether a given PowerShell script contains an "exit" command
    /// </summary>
    /// <param name="tokens">An array of tokens representing the parsed components of a PowerShell script</param>
    /// <returns><c>true</c> if the script contains an 'exit' command; otherwise, <c>false</c>.</returns>
    private static bool ScriptContainsExitCommand(Token[] tokens) =>
        tokens.Any(token => token.Kind == TokenKind.Exit);
    
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