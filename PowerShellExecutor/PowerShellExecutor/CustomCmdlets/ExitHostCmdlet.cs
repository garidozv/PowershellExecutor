using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;

/// <summary>
/// Represents a custom PowerShell cmdlet used to exit the host application.
/// </summary>
/// <remarks>
/// This cmdlet interacts with the <see cref="ViewModels.MainWindowViewModel"/> to invoke its clear mechanism for host content
/// </remarks>
[Cmdlet(VerbsCommon.Exit, "Host")]
public class ExitHostCmdlet : PSCmdlet
{
    private MainWindowViewModel? MainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(ViewModels.MainWindowViewModel)).Value as MainWindowViewModel;
    
    protected override void ProcessRecord()
    {
        if (MainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(ViewModels.MainWindowViewModel)} has not been set");

        MainWindowViewModel.ExitHost();
    }
}