using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;

[Cmdlet(VerbsCommunications.Write, "Host")]
public class WriteHostCmdlet : PSCmdlet
{
    private MainWindowViewModel _mainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(MainWindowViewModel)).Value as MainWindowViewModel;
    
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
    public object[] Object { get; set; }

    [Parameter]
    [ValidateSet("Black", "DarkBlue", "DarkGreen", "DarkCyan", "DarkRed", "DarkMagenta", 
                 "DarkYellow", "Gray", "DarkGray", "Blue", "Green", "Cyan", "Red", 
                 "Magenta", "Yellow", "White")]
    public ConsoleColor ForegroundColor { get; set; }

    [Parameter]
    [ValidateSet("Black", "DarkBlue", "DarkGreen", "DarkCyan", "DarkRed", "DarkMagenta", 
                 "DarkYellow", "Gray", "DarkGray", "Blue", "Green", "Cyan", "Red", 
                 "Magenta", "Yellow", "White")]
    public ConsoleColor BackgroundColor { get; set; }

    [Parameter]
    public SwitchParameter NoNewline { get; set; }

    [Parameter]
    public string Separator { get; set; } = " ";

    protected override void ProcessRecord()
    {
        if (_mainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(MainWindowViewModel)} has not been set");

        _mainWindowViewModel.WriteHost(Object, ForegroundColor, BackgroundColor, Separator, NoNewline.IsPresent);
    }
}

