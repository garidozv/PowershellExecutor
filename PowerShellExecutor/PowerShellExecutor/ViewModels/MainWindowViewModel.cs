using System.IO;
using System.Management.Automation;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using PowershellExecutor.Helpers;
using PowerShellExecutor.Helpers;
using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display
/// </summary>
public class MainWindowViewModel
{
    private record TextBoxBrushes(Brush Foreground, Brush Background);
    private static class CommandOutputBrushes
    {
        public static readonly TextBoxBrushes Default = new(Brushes.White, Brushes.Transparent);
        public static readonly TextBoxBrushes Error = new(Brushes.Red, Brushes.Black);
        public static readonly TextBoxBrushes Verbose = new(Brushes.Yellow, Brushes.Black);
        public static readonly TextBoxBrushes Debug = new(Brushes.Yellow, Brushes.Black);
        public static readonly TextBoxBrushes Warning = new(Brushes.Yellow, Brushes.Black);
        public static readonly TextBoxBrushes Information = new(Brushes.White, Brushes.Transparent);
    }
    
    private readonly PowerShellService _powerShellService;
    private readonly CommandHistory _commandHistory;

    private CommandCompletion? _currentCompletion;
    private string _originalCompletionInput;
    private bool _reactToInputTextChange = true;
    private bool _commandResultDisplayHandled;
    private bool _commandExecutionStopped;
    
    private readonly AutoResetEvent _resultTextBoxInputReady = new(false);
    private Task<PowerShellExecutionResult>? _commandExecutionTask;

    /// <summary>
    /// Gets the RichTextBox control used for displaying and interacting with the command execution results
    /// </summary>
    public RichTextBox CommandResultRichTextBox { get; init; }
    
    /// <summary>
    /// Gets the bindings instance that contains properties and commands for data binding in the main window ViewModel
    /// </summary>
    public MainWindowViewModelBindings Bindings { get; }
    
    /// <summary>
    /// Gets or sets the action that will be invoked to close the main application window
    /// </summary>
    public Action CloseWindowAction { get; init; }
    /// <summary>
    /// Gets or sets the action that will be invoked to focus the input text box within the main application window
    /// </summary>
    public Action FocusInputTextBoxAction { get; init; }
    /// <summary>
    /// Gets or sets the action that will be invoked to focus the result text box within the main application window
    /// </summary>
    public Action FocusResultTextBoxAction { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class
    /// </summary>
    /// <param name="powerShellService">The service for working with PowerShell</param>
    /// <param name="commandHistory">The <see cref="CommandHistory"/> object used for PowerShell command history</param>
    public MainWindowViewModel(PowerShellService powerShellService, CommandHistory commandHistory)
    {
        _powerShellService = powerShellService;
        _commandHistory = commandHistory;

        Bindings = new MainWindowViewModelBindings()
        {
            CommandInputEnterKeyCommand = new AsyncRelayCommand(ExecuteCommand),
            CommandInputUpKeyCommand = new RelayCommand(SetCommandToHistoryNext),
            CommandInputDownKeyCommand = new RelayCommand(SetCommandToHistoryPrev),
            CommandInputEscapeKeyCommand = new RelayCommand(ResetCommandInput),
            CommandInputTabKeyCommand = new RelayCommand(GetNextCompletion),
            InputTextChangedCommand = new RelayCommand(OnInputTextChanged),
            CommandResultEnterKeyCommand = new RelayCommand(ReadHostInputSubmitted),
            CommandResultControlCCommand = new RelayCommand(StopReadHost)
        };

        PowerShellCommandOverrides.ViewModelInstance = this;
        _powerShellService.RegisterCommandOverrides<PowerShellCommandOverrides>();

        // Set initial working directory path
        Bindings.WorkingDirectoryPath = _powerShellService.WorkingDirectoryPath;
    }
    
    /// <summary>
    /// Handles the Write-Host command by updating the command result text boc with the provided text
    /// </summary>
    /// <param name="text">The text to display in the command result output</param>
    public void WriteHost(string text)
    {
        CommandResultAddLine(text, CommandOutputBrushes.Default);
        _commandResultDisplayHandled = true;
    }

    /// <summary>
    /// Handles the Read-Host command by waiting for user input in the command result text box
    /// </summary>
    /// <returns>The string entered by the user into the result text box</returns>
    /// <remarks>This is a blocking method</remarks>
    public string ReadHost()
    {
        CommandResultClear();
        Bindings.IsResultTextBoxReadOnly = false;
        Bindings.IsInputTextBoxReadOnly = true;
        FocusResultTextBoxAction();
        
        _resultTextBoxInputReady.WaitOne();
        
        var res = new TextRange(CommandResultRichTextBox.Document.ContentStart, CommandResultRichTextBox.Document.ContentEnd).Text.Trim();
        
        CommandResultClear();
        Bindings.IsResultTextBoxReadOnly = true;
        Bindings.IsInputTextBoxReadOnly = false;
        FocusInputTextBoxAction();
        
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
    /// Handles the Exit-Host command by invoking the <see cref="CloseWindowAction"/>
    /// </summary>
    public void ExitHost() => CloseWindowAction();

    /// <summary>
    /// Cleans up resources and ensures any ongoing command execution is completed
    /// </summary>
    public async Task Cleanup()
    {
        if (_commandExecutionTask is not null)
        {
            _resultTextBoxInputReady.Set();
            await _commandExecutionTask.ConfigureAwait(false);
        }
    }
    
    /// <summary>
    /// Executes the PowerShell command represented by the current input and updates the UI with the result
    /// </summary>
    private async Task ExecuteCommand(object? parameter)
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

        GenerateResultOutput(executionResult);
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
        if (prevCommand is not null && prevCommand.Equals(Bindings.CommandInput))
            prevCommand = _commandHistory.PrevCommand();
        
        Bindings.CommandInput = prevCommand ?? string.Empty;
        Bindings.CommandInputCaretIndex = Bindings.CommandInput.Length;
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
            Bindings.CommandInput = nextCommand;
            Bindings.CommandInputCaretIndex = Bindings.CommandInput.Length;
        }
    }

    /// <summary>
    /// Clears the command input and resets any existing state related
    /// to the input
    /// </summary>
    private void ResetCommandInput(object? parameter)
    {
        _commandHistory.MoveToStart();
        Bindings.CommandInput = string.Empty;
    }
    
    /// <summary>
    /// Handles the next completion suggestion for the command input
    /// </summary>
    private void GetNextCompletion(object? parameter)
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
    /// Generates and formats the output result based on the specified execution result data
    /// </summary>
    /// <param name="executionResult">The result of the PowerShell script execution</param>
    private void GenerateResultOutput(PowerShellExecutionResult executionResult)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (CommandResultRichTextBox.Document is null) return;

            CommandResultRichTextBox.Document.Blocks.Clear();
            
            if (executionResult.CommandResults is not null)
                CommandResultAddLine(executionResult.CommandResults.ToItemListString(), CommandOutputBrushes.Default);
            if (executionResult.ParseErrors is not null)
                CommandResultAddLine(executionResult.ParseErrors.ToItemListString(), CommandOutputBrushes.Error);
            if (executionResult.Errors is not null)
                CommandResultAddLine(executionResult.Errors.ToItemListString(), CommandOutputBrushes.Error);
            if (executionResult.Warnings is not null)
                CommandResultAddLine(executionResult.Warnings.ToItemListString("WARNING: "), CommandOutputBrushes.Warning);
            if (executionResult.VerboseMessages is not null)
                CommandResultAddLine(executionResult.VerboseMessages.ToItemListString("VERBOSE: "), CommandOutputBrushes.Verbose);
            if (executionResult.DebugMessages is not null)
                CommandResultAddLine(executionResult.DebugMessages.ToItemListString("DEBUG: "), CommandOutputBrushes.Debug);
            if (executionResult.InformationMessages is not null)
                CommandResultAddLine(executionResult.InformationMessages.ToItemListString(), CommandOutputBrushes.Information);
        });
    }

    /// <summary>
    /// Adds a line of text with specified formatting to the <see cref="CommandResultRichTextBox"/>
    /// </summary>
    /// <param name="text">The text to add to the result display</param>
    /// <param name="textBoxBrushes">The brushes used to format the foreground and background of the text</param>
    private void CommandResultAddLine(string text, TextBoxBrushes textBoxBrushes)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(textBoxBrushes);
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            var paragraph = new Paragraph(new Run(text)
            {
                Background = textBoxBrushes.Background,
                Foreground = textBoxBrushes.Foreground
            })
            {
                Margin = new Thickness(0)
            };

            CommandResultRichTextBox.Document.Blocks.Add(paragraph);
        });
    }

    /// <summary>
    /// Clears the content of the command result text box by clearing all blocks
    /// in its document.
    /// </summary>
    private void CommandResultClear()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            CommandResultRichTextBox.Document?.Blocks.Clear();
        });
    }
}