using System.Management.Automation;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using PowerShellExecutor.Helpers;
using PowerShellExecutor.Interfaces;

namespace PowerShellExecutor.ViewModels;

/// <summary>
/// ViewModel for the main window, handling command input and result display
/// </summary>
public partial class MainWindowViewModel
{
    private const string BannerMessage = """
                                         Welcome to PowerShell Executor! 
                                         - Made by garido :) 
                                         """;
    
    private readonly IPowerShellHostService _powerShellHostService;
    private readonly IHistoryProvider<string> _historyProvider;
    private readonly ICompletionProvider<string, string> _completionProvider;
    
    private IReadOnlyList<CompletionElement<string>>? _currentCompletions;
    private int _currentCompletionIndex;

    private bool _reactToInputTextChange = true;
    private bool _commandResultDisplayHandled;
    private bool _commandExecutionStopped;
    
    private readonly AutoResetEvent _readTextBoxSubmitted = new(false);
    private Task<IEnumerable<PSObject>?>? _commandExecutionTask;
    
    private readonly Action _closeWindowAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class
    /// </summary>
    /// <param name="powerShellHostService">The service for working with PowerShell</param>
    /// <param name="historyProvider">The command history provider</param>
    /// <param name="completionProvider">The command completion provider</param>
    /// <param name="closeWindowAction">An action to close the main application window.</param>
    public MainWindowViewModel(IPowerShellHostService powerShellHostService, IHistoryProvider<string> historyProvider,
       ICompletionProvider<string, string> completionProvider, Action closeWindowAction)
    {
        _powerShellHostService = powerShellHostService;
        _historyProvider = historyProvider;
        _completionProvider = completionProvider;
        _closeWindowAction = closeWindowAction;
        
        CommandInputEnterKeyCommand = new AsyncRelayCommand(ExecuteCommand);
        ReadTextBoxEnterKeyCommand = new RelayCommand(SubmitReadTextBox);
        CommandInputUpKeyCommand = new RelayCommand(SetCommandToHistoryNext);
        CommandInputDownKeyCommand = new RelayCommand(SetCommandToHistoryPrev);
        CommandInputEscapeKeyCommand = new RelayCommand(ResetCommandInput);
        CommandInputTabKeyCommand = new RelayCommand(GetNextCompletion);
        CommandInputControlCCommand = new RelayCommand(StopCommandExecution);
        InputTextChangedCommand = new RelayCommand(OnInputTextChanged);
        ReadTextBoxControlCCommand = new RelayCommand(StopReadHost);
        
        _powerShellHostService.SubscribeToErrorStream(HandleErrorStreamInput);
        _powerShellHostService.SubscribeToDebugStream(HandleDebugStreamInput);
        _powerShellHostService.SubscribeToVerboseStream(HandleVerboseStreamInput);
        _powerShellHostService.SubscribeToWarningStream(HandleWarningStreamInput);
        _powerShellHostService.SubscribeToInformationStream(HandleInformationStreamInput);

        // Set initial working directory path
        WorkingDirectoryPath = _powerShellHostService.WorkingDirectoryPath;
        
        // Set banner message
        AddTextToResultDocument(BannerMessage, 
            new OutputColorScheme(Colors.Gold, Colors.Transparent));
    }

    /// <summary>
    /// Writes the specified objects to the host output using the specified colors and format options
    /// </summary>
    /// <param name="objects">The objects to write to the host output</param>
    /// <param name="foregroundColor">The color of the text to display. If null, uses the default foreground color</param>
    /// <param name="backgroundColor">The color of the background to display. If null, uses the default background color</param>
    /// <param name="separator">The separator string to use when concatenating the objects</param>
    /// <param name="noNewLine">A flag indicating whether to suppress the automatic newline at the end of the output</param>
    public void WriteHost(object[] objects, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, string separator,
        bool noNewLine)
    {
        var text = string.Join(separator, objects);
        var foregroundColorValue = OutputColorScheme.Default.Foreground;
        var backgroundColorValue = OutputColorScheme.Default.Background;
    
        if (foregroundColor is not null)
            foregroundColorValue = foregroundColor.Value.ConvertConsoleColorToColor();
        if (backgroundColor is not null)
            backgroundColorValue = backgroundColor.Value.ConvertConsoleColorToColor();
    
        // noNewLine is ignored since the way output is displayed has a new line by default
        
        AddTextToResultDocument(text, new(foregroundColorValue, backgroundColorValue));
    }

    /// <summary>
    /// Handles the Read-Host command by waiting for user input in the read text box
    /// </summary>
    /// <returns>The string entered by the user into the result text box</returns>
    /// <remarks>This is a blocking method</remarks>
    public string ReadHost(object[]? prompt, bool asSecureString)
    {
        IsInputTextBoxReadOnly = true;
        ReadText = string.Empty;
        PromptText = prompt is null ? string.Empty : $"{string.Join(' ', prompt)}:";
        ReadTextBoxVisibility = Visibility.Visible;
        
        _readTextBoxSubmitted.WaitOne();
        
        var res = ReadText;
        
        ReadTextBoxVisibility = Visibility.Collapsed;
        IsInputTextBoxReadOnly = false;
        
        return res;
    }

    /// <summary>
    /// Handles the Clear-Host command by clearing the command result text box
    /// </summary>
    public void ClearHost()
    {
        ClearResultDocument();
        _commandResultDisplayHandled = true;
    }

    /// <summary>
    /// Terminates the host session and triggers an action to close the main application window.
    /// </summary>
    public void ExitHost() => _closeWindowAction();

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
        _historyProvider.AddEntry(CommandInput);

        ClearResultDocument();

        _commandExecutionTask = Task.Run(() => _powerShellHostService.ExecuteScript(CommandInput, outputAsString: true));
        var executionResult = await _commandExecutionTask;
        _commandExecutionTask = null;
        
        WorkingDirectoryPath = _powerShellHostService.WorkingDirectoryPath;
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

        if (executionResult is not null && executionResult.Any())
            AddTextToResultDocument(executionResult.First().ToSingleLineString(), OutputColorScheme.Default);
    }

    /// <summary>
    /// Sets the command input to the next command in the history.
    /// If at the start of history, the command will be cleared
    /// </summary>
    private void SetCommandToHistoryPrev()
    {
        var prevCommand = _historyProvider.PrevEntry();
        
        /*
         * When end of the history is reached first PrevCommand invocation
         * will return the last command in history, which is already displayed.
         * So we have to do one additional call to PrevCommand to get the right
         * command
         */
        if (prevCommand is not null && prevCommand.Equals(CommandInput))
            prevCommand = _historyProvider.PrevEntry();
        
        CommandInput = prevCommand ?? string.Empty;
        CommandInputCaretIndex = CommandInput.Length;
    }
    
    /// <summary>
    /// Sets the command input to the previous command in the history.
    /// If the end of history is reached, the command will not be changed
    /// </summary>
    private void SetCommandToHistoryNext()
    {
        var nextCommand = _historyProvider.NextEntry();
        
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
    private void ResetCommandInput()
    {
        _historyProvider.MoveToStart();
        CommandInput = string.Empty;
    }
    
    /// <summary>
    /// Handles the next completion suggestion for the command input
    /// </summary>
    private void GetNextCompletion()
    {
        if (_currentCompletions is null)
        {
            _currentCompletions = _completionProvider.GetCompletions(CommandInput, CommandInputCaretIndex);
            _currentCompletionIndex = 0;
        }
        
        if (!_currentCompletions.Any()) return;
            
        _reactToInputTextChange = false;
        
        CommandInput = _currentCompletions[_currentCompletionIndex].Completion;
        CommandInputCaretIndex = _currentCompletions[_currentCompletionIndex].Position;

        if (++_currentCompletionIndex == _currentCompletions.Count)
            _currentCompletionIndex = 0;

        /*
         * If there is only one completion match, reset the completions.
         * This allows the user to progressively complete the path with multiple Tab presses.
         *
         * For example, if the user types './De' and there is a single completion './Desktop',
         * the first Tab press will complete it to './Desktop'. A subsequent Tab press will append further completions
         * like './Desktop/someFile', similar to how PowerShell behaves.
         *
         * However, if there are multiple completions ('./D' could complete to './Desktop' and './Documents'),
         * Tab will cycle through these completions, and the next completion will not be generated until the input changes.
         */
        if (_currentCompletions.Count == 1)
            _currentCompletions = null;
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
        _currentCompletions = null;
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
    /// Stops the command execution
    /// </summary>
    private void StopCommandExecution() => _powerShellHostService.StopExecution();
    
    /// <summary>
    /// Handles input from the PowerShell's error stream
    /// </summary>
    /// <param name="errorRecord">
    /// The <see cref="ErrorRecord"/> object representing the error details from the PowerShell execution
    /// </param>
    private void HandleErrorStreamInput(ErrorRecord errorRecord) =>
        AddTextToResultDocument(errorRecord.ToSingleLineString(), OutputColorScheme.Error);

    /// <summary>
    /// Handles input from the PowerShell's warning stream
    /// </summary>
    /// <param name="warningRecord">
    /// The <see cref="WarningRecord"/> object representing the error details from the PowerShell execution
    /// </param>
    private void HandleWarningStreamInput(WarningRecord warningRecord) =>
        AddTextToResultDocument(warningRecord.ToSingleLineString("WARNING: "),  OutputColorScheme.Warning);

    /// <summary>
    /// Handles input from the PowerShell's verbose stream
    /// </summary>
    /// <param name="verboseRecord">
    /// The <see cref="VerboseRecord"/> object representing the error details from the PowerShell execution
    /// </param>
    private void HandleVerboseStreamInput(VerboseRecord verboseRecord) =>
        AddTextToResultDocument(verboseRecord.ToSingleLineString("VERBOSE: "), OutputColorScheme.Verbose);

    /// <summary>
    /// Handles input from the PowerShell's debug stream
    /// </summary>
    /// <param name="debugRecord">
    /// The <see cref="DebugRecord"/> object representing the error details from the PowerShell execution
    /// </param>
    private void HandleDebugStreamInput(DebugRecord debugRecord) =>
        AddTextToResultDocument(debugRecord.ToSingleLineString("DEBUG: "), OutputColorScheme.Debug);
    
    /// <summary>
    /// Handles input from the PowerShell's information stream
    /// </summary>
    /// <param name="informationRecord">
    /// The <see cref="InformationRecord"/> object representing the error details from the PowerShell execution
    /// </param>
    private void HandleInformationStreamInput(InformationRecord informationRecord) =>
        AddTextToResultDocument(informationRecord.ToSingleLineString(), OutputColorScheme.Information);
    
}