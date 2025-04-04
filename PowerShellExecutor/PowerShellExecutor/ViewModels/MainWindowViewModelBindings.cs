using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace PowerShellExecutor.ViewModels;

public class MainWindowViewModelBindings  : INotifyPropertyChanged
{
    private string _commandInput = string.Empty;
    private string _commandResult = string.Empty;
    private string _workingDirectoryPath = string.Empty;
    private Brush _resultForeground = Brushes.White;
    private int _commandInputCaretIndex = 0;
    private bool _isResultTextBoxReadOnly = true;
    private bool _isInputTextBoxReadOnly = false;

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputEnterKeyCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when the Up key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputUpKeyCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when the Down key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputDownKeyCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when the Escape key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputEscapeKeyCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when the Tab key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputTabKeyCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when input text is changed
    /// </summary>
    public ICommand InputTextChangedCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandResultTextBox
    /// </summary>
    public ICommand CommandResultEnterKeyCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandResultTextBox
    /// </summary>
    public ICommand CommandResultControlCCommand { get; init; }

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