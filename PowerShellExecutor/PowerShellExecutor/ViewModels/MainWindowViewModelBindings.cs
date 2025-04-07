using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PowerShellExecutor.ViewModels;

public class MainWindowViewModelBindings  : INotifyPropertyChanged
{
    private string _commandInput = string.Empty;
    private string _readText = string.Empty;
    private string _promptText = string.Empty;
    private string _workingDirectoryPath = string.Empty;
    private int _commandInputCaretIndex = 0;
    private bool _isInputTextBoxReadOnly = false;
    private Visibility _readTextBoxVisibility = Visibility.Collapsed;

    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputEnterKeyCommand { get; init; }
    
    /// <summary>
    /// Gets the command that triggers when the Enter key is pressed on CommandInputTextBox
    /// </summary>
    public ICommand ReadTextBoxEnterKeyCommand { get; init; }

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
    public ICommand ReadTextBoxControlCCommand { get; init; }

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
    /// Gets or sets the command input text
    /// </summary>
    public string ReadText
    {
        get => _readText;
        set
        {
            if (value != _readText)
            {
                _readText = value;
                OnPropertyChanged(nameof(ReadText));
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
    
    public Visibility ReadTextBoxVisibility
    {
        get => _readTextBoxVisibility;
        set
        {
            if (value != _readTextBoxVisibility)
            {
                _readTextBoxVisibility = value;
                OnPropertyChanged(nameof(ReadTextBoxVisibility));
            }
        }
    }
    
    public string PromptText
    {
        get => _promptText;
        set
        {
            if (value != _promptText)
            {
                _promptText = value;
                OnPropertyChanged(nameof(PromptText));
                OnPropertyChanged(nameof(PromptTextBoxVisibility));
            }
        }
    }
    
    public Visibility PromptTextBoxVisibility => _promptText.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
    
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