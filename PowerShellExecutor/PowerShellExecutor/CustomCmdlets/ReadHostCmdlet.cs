using PowerShellExecutor.ViewModels;
using System.Management.Automation;

namespace PowerShellExecutor.CustomCmdlets;

/// <summary>
/// Represents a custom PowerShell cmdlet that provides functionality to read input from the host
/// </summary>
/// <remarks>
/// This cmdlet interacts with the <see cref="ViewModels.MainWindowViewModel"/> to invoke its clear mechanism for host content.
/// If the "AsSecureString" parameter is provided, an exception will be thrown since secure string input is
/// not currently supported
/// </remarks>
[Cmdlet(VerbsCommunications.Read, "Host")]
public class ReadHostCmdlet : PSCmdlet
{
    
    private MainWindowViewModel? MainWindowViewModel =>
        SessionState.PSVariable.Get(nameof(ViewModels.MainWindowViewModel)).Value as MainWindowViewModel;
    
    [Parameter(Position = 0, ValueFromRemainingArguments = true)]
    [AllowNull]
    public object[] Prompt { get; set; }
    
    public SwitchParameter AsSecureString { get; set; }
    
    protected override void ProcessRecord()
    {
        if (MainWindowViewModel is null)
            throw new InvalidOperationException($"A reference to the {nameof(ViewModels.MainWindowViewModel)} has not been set");

        if (AsSecureString.IsPresent)
            throw new NotSupportedException("AsSecureString parameter is not supported yet.");
        
        var res = MainWindowViewModel.ReadHost(Prompt, AsSecureString.IsPresent);
        WriteObject(res);
    }
}