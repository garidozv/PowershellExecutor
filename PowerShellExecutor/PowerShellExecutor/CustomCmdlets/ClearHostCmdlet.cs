using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;

/// <summary>
/// Represents a custom PowerShell cmdlet to clear the host display.
/// </summary>
/// <remarks>
/// This cmdlet interacts with the <see cref="ViewModels.MainWindowViewModel"/> to invoke its clear mechanism for host content
/// </remarks>
[Cmdlet(VerbsCommon.Clear, "Host")]
public class ClearHostCmdlet : PSCmdlet
{
    private MainWindowViewModel? MainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(ViewModels.MainWindowViewModel)).Value as MainWindowViewModel;
    
    protected override void ProcessRecord()
    {
        if (MainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(ViewModels.MainWindowViewModel)} has not been set");
        
        MainWindowViewModel.ClearHost();
    }
}