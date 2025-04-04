using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.PowerShellUtilities;

public class PowerShellCommandOverrides
{
    public static MainWindowViewModel? ViewModelInstance;
    
    private static void EnsureViewModelIsInitialized()
    {
        if (ViewModelInstance is null)
            throw new InvalidOperationException($"The {nameof(ViewModelInstance)} has not been initialized.");
    }
    
    [PowerShellCommand("Clear-Host")]
    public static void ClearHostCommandHandler()
    {
        EnsureViewModelIsInitialized();
        
        ViewModelInstance!.ClearHost();
    }
    
    [PowerShellCommand("Write-Host")]
    public static void WriteHostCommandHandler(string param)
    {
        EnsureViewModelIsInitialized();
        
        ViewModelInstance!.WriteHost(param);
    }
    
    [PowerShellCommand("exit")]
    public static void ExitCommandHandler()
    {
        EnsureViewModelIsInitialized();
        
        ViewModelInstance!.ExitHost();
    }
    
    [PowerShellCommand("Read-Host")]
    public static string ReadHostCommandHandler()
    {
        EnsureViewModelIsInitialized();

        return ViewModelInstance!.ReadHost();
    }
}