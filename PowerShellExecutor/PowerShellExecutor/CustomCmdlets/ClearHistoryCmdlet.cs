using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;

[Cmdlet(VerbsCommon.Clear, "History")]
public class ClearHistoryCmdlet : PSCmdlet
{
    private MainWindowViewModel? MainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(ViewModels.MainWindowViewModel)).Value as MainWindowViewModel;
    
    [Parameter(Mandatory = false, Position = 1, HelpMessage = "Clears the specified number of history entries")]
    [ValidateRange(1, 2147483647)]
    [AllowNull]
    public int? Count { get; set; }

    protected override void ProcessRecord()
    {
        if (MainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(ViewModels.MainWindowViewModel)} has not been set");
        
        MainWindowViewModel.ClearHistory(Count);
    }
}