using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellExecutor.PowerShellUtilities;

public class PowerShellWrapper : IPowerShell, IDisposable
{
    private bool _isDisposed;
    
    private readonly Runspace _runspace;
    private readonly PowerShell _powerShell;
    
    /// <summary>
    /// Creates and opens a dedicated <see cref="Runspace"/>.
    /// Uses the created <see cref="Runspace"/> to create a <see cref="PowerShell"/> instance
    /// </summary>
    public PowerShellWrapper()
    {
        _runspace = RunspaceFactory.CreateRunspace();
        _runspace.Open();
        _powerShell = PowerShell.Create(_runspace);
    }

    public string WorkingDirectoryPath
    {
        get => _powerShell.Runspace.SessionStateProxy.Path.CurrentLocation.Path;
        set => _powerShell.Runspace.SessionStateProxy.Path.SetLocation(value);
    }
    
    public PSDataCollection<ErrorRecord> ErrorStream => _powerShell.Streams.Error;
    public PSDataCollection<WarningRecord> WarningStream => _powerShell.Streams.Warning;
    public PSDataCollection<VerboseRecord> VerboseStream => _powerShell.Streams.Verbose;
    public PSDataCollection<DebugRecord> DebugStream => _powerShell.Streams.Debug;
    public PSDataCollection<InformationRecord> InformationStream => _powerShell.Streams.Information;
    
    public void SetVariable(string name, object value) => _powerShell.Runspace.SessionStateProxy.SetVariable(name, value);
        
    public object GetVariable(string name) => _powerShell.Runspace.SessionStateProxy.GetVariable(name);

    public IPowerShell AddScript(string script)
    {
        _powerShell.AddScript(script);
        return this;
    }

    public IPowerShell AddCommand(string cmdlet)
    {
        _powerShell.AddCommand(cmdlet);
        return this;
    }

    public IPowerShell AddParameter(string paramName, object value)
    {
        _powerShell.AddParameter(paramName, value);
        return this;
    }

    public IPowerShell AddParameter(string paramName)
    {
        _powerShell.AddParameter(paramName);
        return this;
    }

    public Collection<PSObject> Invoke() => _powerShell.Invoke();
    
    public CommandCompletion GetCommandCompletion(string commandName, int commandPosition) =>
        CommandCompletion.CompleteInput(commandName, commandPosition, null, _powerShell);

    public void Clear()
    {
        _powerShell.Commands.Clear();
        _powerShell.Streams.ClearStreams();
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