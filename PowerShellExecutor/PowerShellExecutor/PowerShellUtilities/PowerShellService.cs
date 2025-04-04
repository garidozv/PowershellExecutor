using System.IO;
using System.Management.Automation;
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
    
    private MethodInfo? _exitCommandHandler;
    private readonly HashSet<string> _commandOverrides = [];
    
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
    /// Registers command overrides for a specified type, enabling the overriding
    /// of PowerShell commands based on static public methods in the type.
    /// A method must be public, static, and have the  <see cref="PowerShellCommandAttribute"/>
    /// applied to be considered for overriding.
    /// </summary>
    /// <typeparam name="T">
    /// A class defining static public methods decorated with <see cref="PowerShellCommandAttribute"/>
    /// to specify PowerShell command overrides.
    /// </typeparam>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a command name is empty, already overridden, if the specified command name is not valid
    /// in the PowerShell context, or in case of the 'exit' command, if the method signature is not valid.
    /// </exception>
    public void RegisterCommandOverrides<T>() where T : class
    {
        var methods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public);
        foreach (var method in methods)
        {
            var commandAttribute = method.GetCustomAttribute<PowerShellCommandAttribute>();
            if (commandAttribute is null) continue;

            var commandName = commandAttribute.CommandName.ToLowerInvariant();
            
            if (string.IsNullOrWhiteSpace(commandName))
                throw new InvalidOperationException("Command name cannot be empty");
            
            if (_commandOverrides.Contains(commandName))
                throw new InvalidOperationException(
                    $"Command '{commandName}' is already overridden.");
            
            // Exit command has to be handled separately, because 'exit' is a keyword, and not function, cmdlet or alias
            if (commandName.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                if (method.GetParameters().Length != 0 || method.ReturnType != typeof(void))
                    throw new InvalidOperationException("Exit command override must be a parameterless void method");
                    
                _exitCommandHandler = method;
            }
            else
            {
                if (!IsFunctionOrCmdlet(commandName))
                    throw new InvalidOperationException($"Command '{commandName}' is not a function or a cmdlet.");
            
                var script = PowershellFunctionScriptGenerator.Generate(commandName, method);
                ExecuteScript(script);
            }

            _commandOverrides.Add(commandName);
        }
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
    /// <returns>A <see cref="PowerShellExecutionResult"/> containing the details of the execution</returns>
    public PowerShellExecutionResult ExecuteScript(string script)
    {
        ArgumentNullException.ThrowIfNull(script);
        
        var res = new PowerShellExecutionResult
        {
            IsSuccessful = true,
            Script = script,
        };

        if (string.IsNullOrEmpty(script))
            return res;

        Parser.ParseInput(script, out var tokens, out var errors);

        if (errors.Length > 0)
        {
            res.IsSuccessful = false;
            res.ParseErrors = errors;
            return res;
        }
        
        ExecuteScriptInternal(script, tokens, res);

        return res;
    }

    /// <summary>
    /// Executes a PowerShell script after parsing and validating the input script.
    /// Updates the provided <see cref="PowerShellExecutionResult"/> with the outcome of the script execution
    /// </summary>
    /// <param name="script">The PowerShell script to be executed</param>
    /// <param name="tokens">The tokens parsed from the input script</param>
    /// <param name="result">An instance of <see cref="PowerShellExecutionResult"/> used to hold the execution results</param>
    private void ExecuteScriptInternal(string script, Token[] tokens, PowerShellExecutionResult result)
    {
        var containsExit = ScriptContainsExitCommand(tokens);

        try
        {
            _powerShell.Streams.ClearStreams();
            _powerShell.Commands.Clear();
            
            _powerShell.AddScript(script);
            var invocationResult = _powerShell.Invoke();

            result.IsSuccessful = !_powerShell.HadErrors;
            result.CommandResults = invocationResult.Count > 0 ? invocationResult : null;
            result.Errors = _powerShell.Streams.Error.Count > 0 ? _powerShell.Streams.Error : null;
            result.VerboseMessages = _powerShell.Streams.Verbose.Count > 0 ? _powerShell.Streams.Verbose : null;
            result.Warnings = _powerShell.Streams.Warning.Count > 0 ? _powerShell.Streams.Warning : null;
            result.DebugMessages = _powerShell.Streams.Debug.Count > 0 ? _powerShell.Streams.Debug : null;
            result.InformationMessages = _powerShell.Streams.Information.Count > 0 ? _powerShell.Streams.Information : null;
        }
        catch
        {
            // Possibly do not re-throw, but handle the exception by storing the exception message in the execution result
            throw;
        }

        if (result.IsSuccessful && containsExit && _exitCommandHandler is not null)
        {
            _exitCommandHandler.Invoke(null, null);
        }
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
    /// Determines whether the given name corresponds to a PowerShell function or cmdlet
    /// </summary>
    /// <param name="name">The name of the function or cmdlet to check</param>
    /// <returns><c>true</c> if the specified name is a function or cmdlet; otherwise, <c>false</c></returns>
    private bool IsFunctionOrCmdlet(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        
        var script = $"get-command {name} | where {{$_.CommandType -eq 'Function' -or $_.CommandType -eq 'Cmdlet'}}";
        var res = ExecuteScript(script);
        
        return res.IsSuccessful && (res.CommandResults?.Any() ?? false);
    }

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