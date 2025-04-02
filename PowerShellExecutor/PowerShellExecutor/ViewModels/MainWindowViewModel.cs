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
    private readonly CommandHistory _commandHistory;
    private readonly IMainWindow _mainWindow;
    
    private string _commandInput = string.Empty;
    private string _commandResult = string.Empty;
    private string _workingDirectoryPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class
    /// </summary>
    /// <param name="powerShellService">The service for working with PowerShell</param>
    /// <param name="mainWindow">The main window interface for UI interaction</param>
    public MainWindowViewModel(PowerShellService powerShellService, IMainWindow mainWindow, CommandHistory commandHistory)
    {
        _powerShellService = powerShellService;
        _mainWindow = mainWindow;
        _commandHistory = commandHistory;

        // Set initial working directory path
        WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
    }

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed
    /// </summary>
    public ICommand EnterKeyCommand => new RelayCommand(ExecuteCommand);
    /// <summary>
    /// Gets the command that triggers when the Up key is pressed
    /// </summary>
    public ICommand UpKeyCommand => new RelayCommand(SetCommandToHistoryNext);
    /// <summary>
    /// Gets the command that triggers when the Down key is pressed
    /// </summary>
    public ICommand DownKeyCommand => new RelayCommand(SetCommandToHistoryPrev);
    
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
        _commandHistory.AddCommand(CommandInput);
        
        if (HandleSpecialCommands())
            return;
        
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
    /// Handles special commands that require custom behavior instead of standard execution.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if the command was handled as a special case, preventing further execution; 
    /// otherwise, returns <c>false</c>.
    /// </returns>
    private bool HandleSpecialCommands()
    {
        switch (CommandInput.ToLower())
        {
            case "clear":
                CommandInput = string.Empty;
                CommandResult = string.Empty;
                return true;
            case "exit":
                _mainWindow.CloseMainWindow();
                return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the command input to the next command in the history.
    /// If at the start of history, the command will be cleared
    /// </summary>
    private void SetCommandToHistoryPrev(object? parameter)
    {
        var prevCommand = _commandHistory.PrevCommand();
        
        /*
         * When end of the history is reached first PrevCommand invocation
         * will return the last command in history, which is already displayed.
         * So we have to do one additional call to PrevCommand to get the right
         * command
         */
        if (prevCommand is not null && prevCommand.Equals(CommandInput))
            prevCommand = _commandHistory.PrevCommand();
        
        CommandInput = prevCommand ?? string.Empty;
    }
    
    /// <summary>
    /// Sets the command input to the previous command in the history.
    /// If the end of history is reached, the command will not be changed
    /// </summary>
    private void SetCommandToHistoryNext(object? parameter)
    {
        var nextCommand = _commandHistory.NextCommand();
        
        if (nextCommand is not null)
            CommandInput = nextCommand;
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