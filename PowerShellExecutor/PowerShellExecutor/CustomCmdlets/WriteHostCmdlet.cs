using System.Management.Automation;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.CustomCmdlets;

/// <summary>
/// Represents a custom PowerShell cmdlet that enables writing text to the host application's output display
/// </summary>
/// <remarks>
/// This cmdlet interacts with the <see cref="ViewModels.MainWindowViewModel"/> to invoke its clear mechanism for host content
/// </remarks>
[Cmdlet(VerbsCommunications.Write, "Host")]
public class WriteHostCmdlet : PSCmdlet
{
    private MainWindowViewModel? MainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(ViewModels.MainWindowViewModel)).Value as MainWindowViewModel;
    
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
    public object[] Object { get; set; }

    [Parameter]
    [ValidateSet("Black", "DarkBlue", "DarkGreen", "DarkCyan", "DarkRed", "DarkMagenta", 
                 "DarkYellow", "Gray", "DarkGray", "Blue", "Green", "Cyan", "Red", 
                 "Magenta", "Yellow", "White")]
    public ConsoleColor? ForegroundColor { get; set; }

    [Parameter]
    [ValidateSet("Black", "DarkBlue", "DarkGreen", "DarkCyan", "DarkRed", "DarkMagenta", 
                 "DarkYellow", "Gray", "DarkGray", "Blue", "Green", "Cyan", "Red", 
                 "Magenta", "Yellow", "White")]
    public ConsoleColor? BackgroundColor { get; set; }

    [Parameter]
    public SwitchParameter NoNewline { get; set; }

    [Parameter]
    public string Separator { get; set; } = " ";

    protected override void ProcessRecord()
    {
        if (MainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(ViewModels.MainWindowViewModel)} has not been set");

        MainWindowViewModel.WriteHost(Object, ForegroundColor, BackgroundColor, Separator, NoNewline.IsPresent);
    }
}

