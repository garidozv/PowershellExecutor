using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PowershellExecutor.Helpers;
using PowerShellExecutor.Interfaces;
using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly PowerShellService _powerShellService;
    private readonly IMainWindow _mainWindow;
    
    private string _commandInput = string.Empty;
    private string _commandResult = string.Empty;
    private string _workingDirectoryPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class
    /// </summary>
    /// <param name="powerShellService">The service for working with PowerShell</param>
    /// <param name="mainWindow">The main window interface for UI interaction</param>
    public MainWindowViewModel(PowerShellService powerShellService, IMainWindow mainWindow)
    {
        _powerShellService = powerShellService;
        _mainWindow = mainWindow;
        
        // Set initial working directory path
        WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
    }

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed
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
    /// Gets or sets the working directory path
    /// </summary>
    public string WorkingDirectoryPath
    {
        get => _workingDirectoryPath;
        set
        {
            if (value != _workingDirectoryPath)
            {
                _workingDirectoryPath = value;
                OnPropertyChanged(nameof(WorkingDirectoryPath));
            }
        }
    }

    /// <summary>
    /// Executes the PowerShell command represented by the current input and updates the UI with the result
    /// </summary>
    private void ExecuteCommand(object? parameter)
    {
        var executionResult = _powerShellService.ExecuteCommand(CommandInput);
        CommandResult = executionResult.Output;
        CommandInput = string.Empty;
        WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;

        var commandResultForeground = executionResult.OutputSource switch
        {
            ResultOutputSource.ExecutionError => Brushes.Red,
            ResultOutputSource.ParseError => Brushes.Red,
            ResultOutputSource.Exception => Brushes.DarkRed,
            ResultOutputSource.SuccessfulExecution => Brushes.White,
            _ => Brushes.White
        };
        
        _mainWindow.SetCommandResultForeground(commandResultForeground);
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