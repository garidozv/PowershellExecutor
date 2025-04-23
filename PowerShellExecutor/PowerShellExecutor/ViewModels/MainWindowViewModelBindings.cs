using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using PowerShellExecutor.Helpers;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// Partial class containing all bindable properties and commands for the <see cref="MainWindowViewModel"/>
/// </summary>
public partial class MainWindowViewModel  : INotifyPropertyChanged
{
    private string _commandInput = string.Empty;
    private string _readText = string.Empty;
    private string _promptText = string.Empty;
    private string _workingDirectoryPath = string.Empty;
    private int _commandInputCaretIndex = 0;
    private bool _isInputTextBoxReadOnly = false;
    private Visibility _readTextBoxVisibility = Visibility.Collapsed;
    private readonly FlowDocument _resultDocument = new FlowDocument();

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
    /// Gets the command that triggers when Control-C is pressed on CommandInputTextBox
    /// </summary>
    public ICommand CommandInputControlCCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when input text is changed
    /// </summary>
    public ICommand InputTextChangedCommand { get; init; }

    /// <summary>
    /// Gets the command that triggers when Control-C is pressed on CommandResultTextBox
    /// </summary>
    public ICommand ReadTextBoxControlCCommand { get; init; }
    
    /// <summary>
    /// Gets the result document
    /// </summary>
    public FlowDocument ResultDocument => _resultDocument;

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
    /// Gets or sets the read text
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

    /// <summary>
    /// Gets or sets the read tet box Visibility property
    /// </summary>
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

    /// <summary>
    /// Gets or sets the prompt text
    /// </summary>
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

    /// <summary>
    /// Adds a new paragraph with the specified text and colors to the result document
    /// </summary>
    /// <param name="text">The text to add to the result document</param>
    /// <param name="outputColorScheme">The <see cref="OutputColorScheme"/> to be used for added text</param>
    /// <remarks>
    /// This method raises the <see cref="PropertyChanged"/> event for <see cref="ResultDocument"/>
    /// </remarks>
    public void AddTextToResultDocument(string text, OutputColorScheme outputColorScheme)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var paragraph = new Paragraph(new Run(text)
            {
                Background = new SolidColorBrush(outputColorScheme.Background),
                Foreground = new SolidColorBrush(outputColorScheme.Foreground)
            })
            {
                Margin = new Thickness(0)
            };
            
            _resultDocument.Blocks.Add(paragraph);
            OnPropertyChanged(nameof(ResultDocument));
        });
    }

    /// <summary>
    /// Clears the contents of result document
    /// </summary>
    /// <remarks>
    /// This method raises the <see cref="PropertyChanged"/> event for <see cref="ResultDocument"/>
    /// </remarks> 
    public void ClearResultDocument()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _resultDocument.Blocks.Clear();
            OnPropertyChanged(nameof(ResultDocument));
        });
    }

    /// <summary>
    /// Gets the visibility state of the prompt text box based on its contents
    /// </summary>
    public Visibility PromptTextBoxVisibility => 
        _promptText.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
    
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