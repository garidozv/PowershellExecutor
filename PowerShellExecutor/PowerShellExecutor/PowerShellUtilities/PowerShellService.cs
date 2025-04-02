using System.IO;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using PowerShellExecutor.Helpers;

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
        
        // Set the current runspace location to the home directory
        var homeDirectoryPath = 
            $"{Environment.GetEnvironmentVariable("HOMEDRIVE")}{Environment.GetEnvironmentVariable("HOMEPATH")}";
        _powerShell.Runspace.SessionStateProxy.Path.SetLocation(homeDirectoryPath);
    }
    
    /// <summary>
    /// Gets the path to current working directory of the PowerShell runspace
    /// </summary>
    public string WorkingDirectoryPath => _powerShell.Runspace.SessionStateProxy.Path.CurrentLocation.Path;

    // TODO: Improve the command execution
    /// <summary>
    /// Executes the given PowerShell command and returns the result
    /// </summary>
    /// <param name="command">The PowerShell command to execute</param>
    /// <returns>A <see cref="PowerShellCommandResult"/> containing the output and the output source</returns>
    /// <remarks>
    /// This method executes the command as a script. While this allows you to easily handle complex
    /// commands, for example piped commands, it also makes you unable to handle some commands in a
    /// special way, for example host interaction commands like clear, exit, etc.
    /// </remarks>
    public PowerShellCommandResult ExecuteCommand(string command)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        if (string.IsNullOrEmpty(command)) 
            return new(string.Empty, ResultOutputSource.SuccessfulExecution);
        
        // Try to parse the command to detect syntax errors
        Parser.ParseInput(command, out _, out var errors);

        if (errors.Length > 0)
            return new(errors.ToItemListString(), ResultOutputSource.ParseError);
        
        string resultOutput;
        ResultOutputSource resultOutputSource;
        
        try
        {
            // Execute the command
            _powerShell.AddScript(command);
            var invocationResult = _powerShell.Invoke();

            if (_powerShell.HadErrors)
            {
                resultOutputSource = ResultOutputSource.ExecutionError;
                resultOutput = _powerShell.Streams.Error.ToItemListString();
            }
            else
            {
                resultOutputSource = ResultOutputSource.SuccessfulExecution;
                resultOutput = invocationResult.ToItemListString();
            }
        }
        catch (Exception ex)
        {
            resultOutputSource = ResultOutputSource.Exception;
            resultOutput = $"Error executing command: {ex.Message}";
        }
        finally
        {
            _powerShell.Commands.Clear();
            _powerShell.Streams.ClearStreams();
        }

        return new(resultOutput, resultOutputSource);
    }

    public CommandCompletion GetCommandCompletions(string command, int cursorIndex) =>
        CommandCompletion.CompleteInput(command, cursorIndex, null, _powerShell);

    public bool IsDirectoryCompletion(string completion)
    {
        ArgumentNullException.ThrowIfNull(completion);
        
        var fullPath = Path.IsPathRooted(completion) ? completion : Path.Combine(WorkingDirectoryPath, completion);
        return Directory.Exists(fullPath);
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