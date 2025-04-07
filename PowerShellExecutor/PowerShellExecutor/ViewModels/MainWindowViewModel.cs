using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PowerShellExecutor.CustomCmdlets;
using PowerShellExecutor.Helpers;
using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display
/// </summary>
public class MainWindowViewModel
{
    private record ColorScheme(Color Foreground, Color Background);
    private static class CommandOutputColors
    {
        public static readonly ColorScheme Default = new(Colors.White, Colors.Transparent);
        public static readonly ColorScheme Error = new(Colors.Red, Colors.Black);
        public static readonly ColorScheme Verbose = new(Colors.Yellow, Colors.Black);
        public static readonly ColorScheme Debug = new(Colors.Yellow, Colors.Black);
        public static readonly ColorScheme Warning = new(Colors.Yellow, Colors.Black);
        public static readonly ColorScheme Information = new(Colors.White, Colors.Transparent);
    }
    
    private const char SecureStringMaskChar = '*';
    private static readonly Color DefaultColor = Colors.White;
    
    private readonly PowerShellService _powerShellService;
    private readonly CommandHistory _commandHistory;

    private CommandCompletion? _currentCompletion;
    private string _originalCompletionInput;
    private bool _reactToInputTextChange = true;
    private bool _commandResultDisplayHandled;
    private bool _commandExecutionStopped;
    
    private readonly AutoResetEvent _readTextBoxSubmitted = new(false);
    private Task<PSObject?>? _commandExecutionTask;
    
    private Action _focusInputTextBoxAction;
    private Action _focuReadTextBoxAction;
    
    private RichTextBox _commandResultRichTextBox;
    
    /// <summary>
    /// Gets the bindings instance that contains properties and commands for data binding in the main window ViewModel
    /// </summary>
    public MainWindowViewModelBindings Bindings { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class
    /// </summary>
    /// <param name="powerShellService">The service for working with PowerShell</param>
    /// <param name="commandHistory">The <see cref="CommandHistory"/> object used for PowerShell command history</param>
    /// <param name="closeWindowAction">An action to close the main application window.</param>
    /// <param name="focusInputTextBoxAction">An action to set focus on the command input text box.</param>
    /// <param name="focuReadTextBoxAction">An action to set focus on the read-only text box displaying command results.</param>
    /// <param name="commandResultRichTextBox">The rich text box used for displaying command results in the main window.</param>
    public MainWindowViewModel(PowerShellService powerShellService, CommandHistory commandHistory,
        Action closeWindowAction, Action focusInputTextBoxAction, Action focuReadTextBoxAction,
        RichTextBox commandResultRichTextBox)
    {
        _powerShellService = powerShellService;
        _commandHistory = commandHistory;
        _focusInputTextBoxAction = focusInputTextBoxAction;
        _focuReadTextBoxAction = focuReadTextBoxAction;
        _commandResultRichTextBox = commandResultRichTextBox;

        Bindings = new MainWindowViewModelBindings()
        {
            CommandInputEnterKeyCommand = new AsyncRelayCommand(ExecuteCommand),
            ReadTextBoxEnterKeyCommand = new RelayCommand(SubmitReadTextBox),
            CommandInputUpKeyCommand = new RelayCommand(SetCommandToHistoryNext),
            CommandInputDownKeyCommand = new RelayCommand(SetCommandToHistoryPrev),
            CommandInputEscapeKeyCommand = new RelayCommand(ResetCommandInput),
            CommandInputTabKeyCommand = new RelayCommand(GetNextCompletion),
            InputTextChangedCommand = new RelayCommand(OnInputTextChanged),
            ReadTextBoxControlCCommand = new RelayCommand(StopReadHost)
        };
        
        _powerShellService.RegisterCustomCmdlet<WriteHostCmdlet>();
        _powerShellService.RegisterCustomCmdlet<ClearHostCmdlet>();
        _powerShellService.RegisterCustomCmdlet<ReadHostCmdlet>();
        _powerShellService.ExitCommandHandler = closeWindowAction;
        _powerShellService.SetVariable(nameof(MainWindowViewModel), this);
        
        _powerShellService.SubscribeToErrorStream(HandleErrorStreamInput);
        _powerShellService.SubscribeToDebugStream(HandleDebugStreamInput);
        _powerShellService.SubscribeToVerboseStream(HandleVerboseStreamInput);
        _powerShellService.SubscribeToWarningStream(HandleWarningStreamInput);
        _powerShellService.SubscribeToInformationStream(HandleInformationStreamInput);

        // Set initial working directory path
        Bindings.WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
    }

    private void HandleErrorStreamInput(ErrorRecord errorRecord)
    {
        CommandResultAddLine(errorRecord.ToSingleLineString(), CommandOutputColors.Error);
    }

    private void HandleWarningStreamInput(WarningRecord warningMessage)
    {
        CommandResultAddLine(warningMessage.ToSingleLineString(), CommandOutputColors.Warning);
    }

    private void HandleVerboseStreamInput(VerboseRecord verboseMessage)
    {
        CommandResultAddLine(verboseMessage.ToSingleLineString(), CommandOutputColors.Verbose);
    }

    private void HandleDebugStreamInput(DebugRecord debugMessage)
    {
        CommandResultAddLine(debugMessage.ToSingleLineString(), CommandOutputColors.Debug);
    }

    private void HandleInformationStreamInput(InformationRecord informationMessage)
    {
        CommandResultAddLine(informationMessage.ToSingleLineString(), CommandOutputColors.Information);
    }

    /// <summary>
    /// Writes the specified objects to the host output using the specified colors and format options
    /// </summary>
    /// <param name="objects">The objects to write to the host output</param>
    /// <param name="foregroundColor">The color of the text to display. If null, uses the default foreground color</param>
    /// <param name="backgroundColor">The color of the background to display. If null, uses the default background color</param>
    /// <param name="separator">The separator string to use when concatenating the objects</param>
    /// <param name="noNewLine">A flag indicating whether to suppress the automatic newline at the end of the output</param>
    public void WriteHost(object[] objects, string? foregroundColor, string? backgroundColor, string separator,
        bool noNewLine)
    {
        var text = string.Join(separator, objects);
        var foregroundColorValue = CommandOutputColors.Default.Foreground;
        var backgroundColorValue = CommandOutputColors.Default.Background;
    
        if (foregroundColor is not null)
            foregroundColorValue = ConvertStringToColor(foregroundColor);
        if (backgroundColor is not null)
            backgroundColorValue = ConvertStringToColor(backgroundColor);
    
        if (!noNewLine)
            text += Environment.NewLine;
    
        CommandResultAddLine(text, new ColorScheme(foregroundColorValue, backgroundColorValue));
        //_commandResultDisplayHandled = true;
    }
    
    

    /// <summary>
    /// Handles the Read-Host command by waiting for user input in the command result text box
    /// </summary>
    /// <returns>The string entered by the user into the result text box</returns>
    /// <remarks>This is a blocking method</remarks>
    public string ReadHost(string? prompt, bool asSecureString)
    {
        Bindings.IsInputTextBoxReadOnly = true;
        Bindings.ReadText = string.Empty;
        Bindings.PromptText = prompt ?? string.Empty;
        Bindings.ReadTextBoxVisibility = Visibility.Visible;
        _focuReadTextBoxAction();
        
        _readTextBoxSubmitted.WaitOne();
        
        var res = Bindings.ReadText;
        
        Bindings.ReadTextBoxVisibility = Visibility.Collapsed;
        Bindings.IsInputTextBoxReadOnly = false;
        _focusInputTextBoxAction();
        
        return res;
    }

    /// <summary>
    /// Handles the Clear-Host command by clearing the command result text box
    /// </summary>
    public void ClearHost()
    {
        CommandResultClear();
        _commandResultDisplayHandled = true;
    }

    /// <summary>
    /// Cleans up resources and ensures any ongoing command execution is completed
    /// </summary>
    public async Task Cleanup()
    {
        if (_commandExecutionTask is not null)
        {
            _readTextBoxSubmitted.Set();
            await _commandExecutionTask.ConfigureAwait(false);
        }
    }
    
    /// <summary>
    /// Executes the PowerShell command represented by the current input and updates the UI with the result
    /// </summary>
    private async Task ExecuteCommand(CancellationToken cancellationToken)
    {
        _commandHistory.AddCommand(Bindings.CommandInput);

        CommandResultClear();

        _commandExecutionTask = Task.Run(() => _powerShellService.ExecuteScript(Bindings.CommandInput));
        var executionResult = await _commandExecutionTask;
        _commandExecutionTask = null;
        
        Bindings.WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
        Bindings.CommandInput = string.Empty;

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

        if (executionResult is not null)
            CommandResultAddLine(executionResult.ToSingleLineString(), CommandOutputColors.Default);
    }

    /// <summary>
    /// Sets the command input to the next command in the history.
    /// If at the start of history, the command will be cleared
    /// </summary>
    private void SetCommandToHistoryPrev()
    {
        var prevCommand = _commandHistory.PrevCommand();
        
        /*
         * When end of the history is reached first PrevCommand invocation
         * will return the last command in history, which is already displayed.
         * So we have to do one additional call to PrevCommand to get the right
         * command
         */
        if (prevCommand is not null && prevCommand.Equals(Bindings.CommandInput))
            prevCommand = _commandHistory.PrevCommand();
        
        Bindings.CommandInput = prevCommand ?? string.Empty;
        Bindings.CommandInputCaretIndex = Bindings.CommandInput.Length;
    }
    
    /// <summary>
    /// Sets the command input to the previous command in the history.
    /// If the end of history is reached, the command will not be changed
    /// </summary>
    private void SetCommandToHistoryNext()
    {
        var nextCommand = _commandHistory.NextCommand();
        
        if (nextCommand is not null)
        {
            Bindings.CommandInput = nextCommand;
            Bindings.CommandInputCaretIndex = Bindings.CommandInput.Length;
        }
    }

    /// <summary>
    /// Clears the command input and resets any existing state related
    /// to the input
    /// </summary>
    private void ResetCommandInput()
    {
        _commandHistory.MoveToStart();
        Bindings.CommandInput = string.Empty;
    }
    
    /// <summary>
    /// Handles the next completion suggestion for the command input
    /// </summary>
    private void GetNextCompletion()
    {
        if (_currentCompletion is null) 
        {
            _currentCompletion = _powerShellService.GetCommandCompletions(
                Bindings.CommandInput, Bindings.CommandInputCaretIndex);
            _originalCompletionInput = Bindings.CommandInput;
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
        
        Bindings.CommandInput = _originalCompletionInput.ReplaceSegment(
            _currentCompletion.ReplacementIndex, 
            _currentCompletion.ReplacementLength,
            completionText);
        Bindings.CommandInputCaretIndex = _currentCompletion.ReplacementIndex + completionText.Length;

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
    private void OnInputTextChanged()
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
    /// Signals that input to the read textbox is ready to be processed.
    /// </summary>
    private void SubmitReadTextBox()
    {
        _readTextBoxSubmitted.Set();
    }
    
    /// <summary>
    /// Stops the execution of the Read-Host command
    /// </summary>
    private void StopReadHost()
    {
        _commandExecutionStopped = true;
        _readTextBoxSubmitted.Set();
    }

    /// <summary>
    /// Adds a line of text with specified formatting to the <see cref="CommandResultRichTextBox"/>
    /// </summary>
    /// <param name="text">The text to add to the result display</param>
    /// <param name="textBoxBrushes">The brushes used to format the foreground and background of the text</param>
    private void CommandResultAddLine(string text, ColorScheme colorScheme)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(colorScheme);
    
        _commandResultRichTextBox.Dispatcher.Invoke(() =>
        {
            var paragraph = new Paragraph(new Run(text)
            {
                Background = new SolidColorBrush(colorScheme.Background),
                Foreground = new SolidColorBrush(colorScheme.Foreground)
            })
            {
                Margin = new Thickness(0)
            };
    
            _commandResultRichTextBox.Document.Blocks.Add(paragraph);
        });
    }

    /// <summary>
    /// Clears the content of the command result text box by clearing all blocks
    /// in its document.
    /// </summary>
    private void CommandResultClear()
    {
        _commandResultRichTextBox.Dispatcher.Invoke(() =>
        {
            _commandResultRichTextBox.Document?.Blocks.Clear();
        });
    }

    /// <summary>
    /// Converts a string representing a color name or value to a <see cref="Brush"/>.
    /// </summary>
    /// <param name="colorName">The name or hexadecimal value of the color to convert</param>
    /// <returns>A <see cref="Brush"/> corresponding to the provided color name or value. If the conversion fails, returns a default brush</returns>
    private Color ConvertStringToColor(string colorName)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(colorName);
        }
        catch
        {
            return DefaultColor;
        }
    }
}