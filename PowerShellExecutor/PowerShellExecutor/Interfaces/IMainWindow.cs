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
    
    /// <summary>
    /// Sets the caret index to the specified value if provided; otherwise, moves the caret to the end of the text
    /// </summary>
    /// <param name="index">Optional caret index value</param>
    void SetCommandInputCaretIndex(int? index = null);
    
    /// <summary>
    /// Gets the current caret index value
    /// </summary>
    int GetCommandInputCaretIndex();
}