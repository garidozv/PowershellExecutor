using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;

[Cmdlet(VerbsCommon.Get, "History")]
public class GetHistoryCmdlet : PSCmdlet
{
    private MainWindowViewModel? MainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(ViewModels.MainWindowViewModel)).Value as MainWindowViewModel;
    
    [Parameter(Position = 1)]
    [ValidateRange(0, 32767)]
    public int? Count { get; set; }

    protected override void ProcessRecord()
    {
        if (MainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(ViewModels.MainWindowViewModel)} has not been set");

        var res = MainWindowViewModel.GetHistory(Count);
        WriteObject(res);
    }
}