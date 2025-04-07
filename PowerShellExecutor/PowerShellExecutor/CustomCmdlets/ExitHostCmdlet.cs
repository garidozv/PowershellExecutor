using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;


[Cmdlet(VerbsCommon.Exit, "Host")]
public class ExitHostCmdlet : PSCmdlet
{
    private MainWindowViewModel _mainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(MainWindowViewModel)).Value as MainWindowViewModel;
    
    protected override void ProcessRecord()
    {
        if (_mainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(MainWindowViewModel)} has not been set");

        _mainWindowViewModel.ExitHost();
    }
}