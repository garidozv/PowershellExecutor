using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display.
/// Implements INotifyPropertyChanged to support data binding.
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    private string _commandInput = string.Empty;
    private string _commandResult = string.Empty;
    
    /// <summary>
    /// Gets or sets the command input text
    /// </summary>
    public string CommandInput
    {
        get => _commandInput;
        set
        {
            if (value != _commandInput)
            {
                _commandInput = value;
                OnPropertyChanged(nameof(CommandInput));
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the command result text
    /// </summary>
    public string CommandResult
    {
        get => _commandResult;
        set
        {
            if (value != _commandResult)
            {
                _commandResult = value;
                OnPropertyChanged(nameof(CommandResult));
            }
        }
    }
    
    /// <summary>
    /// Event triggered when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for a given property
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}