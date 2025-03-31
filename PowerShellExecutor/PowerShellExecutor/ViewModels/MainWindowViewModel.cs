using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PowershellExecutor.Helpers;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    private string _commandInput = string.Empty;
    private string _commandResult = string.Empty;

    /// <summary>
    /// Command that triggers when the Enter key is pressed
    /// </summary>
    public ICommand EnterKeyCommand => new RelayCommand(ExecuteCommand);
    
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
    /// Handles the execution of the command when Enter is pressed
    /// </summary>
    /// <param name="parameter">Optional parameter</param>
    private void ExecuteCommand(object? parameter)
    {
        CommandResult = $"Command: '{CommandInput}' executed!";
        CommandInput = string.Empty;
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