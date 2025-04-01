using System.Windows.Media;

namespace PowerShellExecutor.Interfaces;

/// <summary>
/// Defines the contract for the main window, providing methods to interact with the UI
/// </summary>
public interface IMainWindow
{
    /// <summary>
    /// Sets the foreground color of the command result text box
    /// </summary>
    /// <param name="brush">The brush color to apply</param>
    public void SetCommandResultForeground(Brush brush); 
    
    /// <summary>
    /// Closes the main window
    /// </summary>
    public void CloseMainWindow();
}