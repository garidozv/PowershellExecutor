using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PowershellExecutor.Helpers;
using PowerShellExecutor.Helpers;
using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    private static readonly Brush DefaultResultForeground = Brushes.White;
    private static readonly Brush CommandSuccessResultForeground = Brushes.White;
    private static readonly Brush CommandErrorResultForeground = Brushes.Red;
    private static readonly Brush ParseErrorResultForeground = Brushes.Red;
    private static readonly Brush ExceptionResultForeground = Brushes.DarkRed;
    
    private readonly PowerShellService _powerShellService;
    private readonly CommandHistory _commandHistory;

    private CommandCompletion? _currentCompletion;
    private string _originalCompletionInput;
    private bool _reactToInputTextChange = true;
    private bool _commandResultDisplayHandled = false;
    private bool _commandExecutionStopped = false;
    
    private string _commandInput = string.Empty;
    private string _commandResult = string.Empty;
    private string _workingDirectoryPath = string.Empty;
    private Brush _resultForeground = Brushes.White;
    private int _commandInputCaretIndex = 0;
    private bool _isResultTextBoxReadOnly = true;
    private bool _isInputTextBoxReadOnly = false;
    
    private readonly AutoResetEvent _resultTextBoxInputReady = new(false);

    /// <summary>
    /// Gets or sets the action that will be invoked to close the main application window
    /// </summary>
    public Action CloseWindowAction { get; set; }
    /// <summary>
    /// Gets or sets the action that will be invoked to focus the input text box within the main application window
    /// </summary>
    public Action FocusInputTextBoxAction { get; set; }
    /// <summary>
    /// Gets or sets the action that will be invoked to focus the result text box within the main application window
    /// </summary>
    public Action FocusResultTextBoxAction { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class
    /// </summary>
    /// <param name="powerShellService">The service for working with PowerShell</param>
    /// <param name="commandHistory">The <see cref="CommandHistory"/> object used for PowerShell command history</param>
    public MainWindowViewModel(PowerShellService powerShellService, CommandHistory commandHistory)
    {
        _powerShellService = powerShellService;
        _commandHistory = commandHistory;

        PowerShellCommandOverrides.ViewModelInstance = this;
        _powerShellService.RegisterCommandOverrides<PowerShellCommandOverrides>();

        // Set initial working directory path
        WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
    }

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputEnterKeyCommand => new AsyncRelayCommand(ExecuteCommand);
    /// <summary>
    /// Gets the command that triggers when the Up key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputUpKeyCommand => new RelayCommand(SetCommandToHistoryNext);
    /// <summary>
    /// Gets the command that triggers when the Down key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputDownKeyCommand => new RelayCommand(SetCommandToHistoryPrev);
    /// <summary>
    /// Gets the command that triggers when the Escape key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputEscapeKeyCommand => new RelayCommand(ResetCommandInput);
    /// <summary>
    /// Gets the command that triggers when the Tab key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputTabKeyCommand => new RelayCommand(GetNextCompletion);
    /// <summary>
    /// Gets the command that triggers when input text is changed
    /// </summary>
    public ICommand InputTextChangedCommand => new RelayCommand(OnInputTextChanged);
    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandResultTextBox
    /// </summary>
    public ICommand CommandResultEnterKeyCommand => new RelayCommand(ReadHostInputSubmitted);
    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandResultTextBox
    /// </summary>
    public ICommand CommandResultControlCCommand => new RelayCommand(StopReadHost);

    /// <summary>
    /// Gets or sets the result text foreground color
    /// </summary>
    public Brush ResultForeground
    {
        get => _resultForeground;
        set
        {
            if (value != _resultForeground)
            {
                _resultForeground = value;
                OnPropertyChanged(nameof(ResultForeground));
            }
        }
    }

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
    /// Gets or sets the command input text caret index
    /// </summary>
    public int CommandInputCaretIndex
    {
        get => _commandInputCaretIndex;
        set
        {
            if (value != _commandInputCaretIndex)
            {
                _commandInputCaretIndex = value;
                OnPropertyChanged(nameof(CommandInputCaretIndex));
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the result text box IsReadOnly property
    /// </summary>
    public bool IsResultTextBoxReadOnly
    {
        get => _isResultTextBoxReadOnly;
        set
        {
            if (value != _isResultTextBoxReadOnly)
            {
                _isResultTextBoxReadOnly = value;
                OnPropertyChanged(nameof(IsResultTextBoxReadOnly));
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the input text box IsReadOnly property
    /// </summary>
    public bool IsInputTextBoxReadOnly
    {
        get => _isInputTextBoxReadOnly;
        set
        {
            if (value != _isInputTextBoxReadOnly)
            {
                _isInputTextBoxReadOnly = value;
                OnPropertyChanged(nameof(IsInputTextBoxReadOnly));
            }
        }
    }
    
    /// <summary>
    /// Executes the PowerShell command represented by the current input and updates the UI with the result
    /// </summary>
    private async Task ExecuteCommand(object? parameter)
    {
        _commandHistory.AddCommand(CommandInput);
        
        CommandResult = string.Empty;
        ResultForeground = DefaultResultForeground;
        
        var executionResult = await Task.Run(() => _powerShellService.ExecuteScript(CommandInput));
        
        WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
        CommandInput = string.Empty;

        if (_commandExecutionStopped)
        {
            _commandExecutionStopped = false;
            return;
        }

        if (_commandResultDisplayHandled)
        {
            _commandResultDisplayHandled = false;
            return;
        }
        
        CommandResult = executionResult.Output;
    
        ResultForeground = executionResult.OutputSource switch
        {
            ResultOutputSource.ExecutionError => CommandErrorResultForeground,
            ResultOutputSource.ParseError => ParseErrorResultForeground,
            ResultOutputSource.Exception => ExceptionResultForeground,
            ResultOutputSource.SuccessfulExecution => CommandSuccessResultForeground,
            _ => DefaultResultForeground
        };
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
        CommandInputCaretIndex = CommandInput.Length;
    }
    
    /// <summary>
    /// Sets the command input to the previous command in the history.
    /// If the end of history is reached, the command will not be changed
    /// </summary>
    private void SetCommandToHistoryNext(object? parameter)
    {
        var nextCommand = _commandHistory.NextCommand();
        
        if (nextCommand is not null)
        {
            CommandInput = nextCommand;
            CommandInputCaretIndex = CommandInput.Length;
        }
    }

    /// <summary>
    /// Clears the command input and resets any existing state related
    /// to the input
    /// </summary>
    private void ResetCommandInput(object? parameter)
    {
        _commandHistory.MoveToStart();
        CommandInput = string.Empty;
    }
    
    /// <summary>
    /// Handles the next completion suggestion for the command input
    /// </summary>
    private void GetNextCompletion(object? parameter)
    {
        if (_currentCompletion is null) 
        {
            _currentCompletion = _powerShellService.GetCommandCompletions(
                CommandInput, CommandInputCaretIndex);
            _originalCompletionInput = CommandInput;
        }
        
        if (_currentCompletion.CompletionMatches.Count == 0) return;
            
        _reactToInputTextChange = false;
        
        var nextCompletion = _currentCompletion.GetNextResult(true);
        var completionText = nextCompletion.CompletionText;
        
        /*
         * PowerShell automatically appends directory separator at the end of directory completions.
         * Directory completions have the ProviderContainer completion result type, but they
         * are not the only completion type to have it, so we have to perform additional
         * check to make sure that the completion really represents a directory, after which
         * we can append the directory separator.
         */
        if (nextCompletion.ResultType == CompletionResultType.ProviderContainer &&
            _powerShellService.IsDirectoryCompletion(completionText) &&
            !completionText.EndsWith(Path.DirectorySeparatorChar))
            completionText += Path.DirectorySeparatorChar;
        
        CommandInput = _originalCompletionInput.ReplaceSegment(
            _currentCompletion.ReplacementIndex, 
            _currentCompletion.ReplacementLength,
            completionText);
        CommandInputCaretIndex = _currentCompletion.ReplacementIndex + completionText.Length;

        /*
         * If there is only one completion match, reset the completions.
         * This allows the user to progressively complete the path with multiple Tab presses.
         *
         * For example, if the user types './De' and there is a single completion './Desktop',
         * the first Tab press will complete it to './Desktop'. A subsequent Tab press will append further completions
         * like './Desktop/someFile', similar to how PowerShell behaves.
         *
         * However, if there are multiple completions ('./De' could complete to './Desktop' and './Documents'),
         * Tab will cycle through these completions, and the next completion will not be generated until the input changes.
         */
        if (_currentCompletion.CompletionMatches.Count == 1)
            _currentCompletion = null;
    }

    /// <summary>
    /// Handles changes to the input text if change is not internal
    /// </summary>
    private void OnInputTextChanged(object parameter)
    {
        if (!_reactToInputTextChange)
        {
            _reactToInputTextChange = true;
            return;
        }

        // If the change was user input, reset the current completion state
        _currentCompletion = null;
    }
    
    /// <summary>
    /// Handles input submission for Read-Host command
    /// </summary>
    private void ReadHostInputSubmitted(object obj) => _resultTextBoxInputReady.Set();

    /// <summary>
    /// Stops the execution of the Read-Host command
    /// </summary>
    private void StopReadHost(object obj)
    {
        _commandExecutionStopped = true;
        _resultTextBoxInputReady.Set();
    }

    /// <summary>
    /// Handles the Write-Host command by updating the command result text boc with the provided text
    /// </summary>
    /// <param name="text">The text to display in the command result output</param>
    public void WriteHost(string text)
    {
        CommandResult = text;
        _commandResultDisplayHandled = true;
    }

    /// <summary>
    /// Handles the Read-Host command by waiting for user input in the command result text box
    /// </summary>
    /// <returns>The string entered by the user into the result text box</returns>
    /// <remarks>This is a blocking method</remarks>
    public string ReadHost()
    {
        CommandResult = string.Empty;
        IsResultTextBoxReadOnly = false;
        IsInputTextBoxReadOnly = true;
        FocusResultTextBoxAction();
        
        _resultTextBoxInputReady.WaitOne();
        
        var res = CommandResult;
        
        CommandResult = string.Empty;
        IsResultTextBoxReadOnly = true;
        IsInputTextBoxReadOnly = false;
        FocusInputTextBoxAction();
        
        return res;
    }

    /// <summary>
    /// Handles the Clear-Host command by clearing the command result text box
    /// </summary>
    public void ClearHost()
    {
        CommandResult = string.Empty;
        _commandResultDisplayHandled = true;
    }

    /// <summary>
    /// Handles the Exit-Host command by invoking the <see cref="CloseWindowAction"/>
    /// </summary>
    public void ExitHost() => CloseWindowAction();

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