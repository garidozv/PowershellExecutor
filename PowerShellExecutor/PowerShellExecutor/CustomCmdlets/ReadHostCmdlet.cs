using PowerShellExecutor.ViewModels;
using System.Management.Automation;

namespace PowerShellExecutor.CustomCmdlets;

[Cmdlet(VerbsCommunications.Read, "Host")]
public class ReadHostCmdlet : PSCmdlet
{
    private MainWindowViewModel _mainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(MainWindowViewModel)).Value as MainWindowViewModel;
    
    [Parameter(Mandatory = false, Position = 0)]
    public string Prompt { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter AsSecureString { get; set; }
    
    protected override void ProcessRecord()
    {
        if (_mainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(MainWindowViewModel)} has not been set");

        if (AsSecureString.IsPresent)
            throw new NotSupportedException("AsSecureString parameter is not supported yet.");
        
        var res = _mainWindowViewModel.ReadHost(Prompt, AsSecureString.IsPresent);
        WriteObject(res);
    }
}