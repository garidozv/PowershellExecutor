using System.Management.Automation;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Moq;
using PowerShellExecutor.Helpers;
using PowerShellExecutor.Interfaces;
using PowerShellExecutor.UI.Tests.Asserts;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor.UI.Tests;

using System;
public class MainWindowViewModelTests
{
    private class MockDispatcher : IDispatcher
    {
        public void Invoke(Action callback) => callback();
    }
    
    private readonly Mock<IPowerShellHostService> _powerShellServiceMock;
    private readonly Mock<IHistoryProvider<string>> _historyProviderMock;
    private readonly Mock<Action> _actionMock;
    private readonly Mock<ICompletionProvider<string, string>> _completionProviderMock;
    private readonly MainWindowViewModel _viewModel;
    private readonly AsyncRelayCommand _executeCommand;
    
    public MainWindowViewModelTests()
    {
        _powerShellServiceMock = new Mock<IPowerShellHostService>();
        _historyProviderMock = new Mock<IHistoryProvider<string>>();
        _completionProviderMock = new Mock<ICompletionProvider<string, string>>();
        _actionMock = new Mock<Action>();
        _viewModel = new MainWindowViewModel(_powerShellServiceMock.Object, _historyProviderMock.Object,
            _completionProviderMock.Object, new MockDispatcher(), _actionMock.Object);
        _executeCommand = (_viewModel.CommandInputEnterKeyCommand as AsyncRelayCommand)!;
        _viewModel.ClearResultDocument();
    }
 
    [WpfFact]
    public void ClearHost_HasContent_ContentCleared()
    {
        // Arrange
        var expectedRuns = new List<Run>();
        
        // Act
        _viewModel.WriteHost(["test"], null, null, string.Empty, false);
        _viewModel.ClearHost();
        
        // Assert
        DocumentAssert.Empty(_viewModel.ResultDocument);
    }
    
    [WpfFact]
    public void WriteHost_EmptyObjectList_NoContentCreated()
    {
        // Arrange
        var expectedRuns = new List<Run>();
        
        // Act
        _viewModel.ClearHost();
        _viewModel.WriteHost([], null, null, string.Empty, false);
        
        // Assert
        DocumentAssert.Empty(_viewModel.ResultDocument);;
    }
    
    [WpfFact]
    public void WriteHost_ListWithSeparator_SingleRunOfDefaultColorsCreated()
    {
        // Arrange
        var objects = new object[] {"first", "second", "third"};
        const string separator = "-";
        var expectedRuns = new List<Run>() { new()
        {
            Text = string.Join(separator, objects),
            Foreground = new SolidColorBrush(OutputColorScheme.Default.Foreground),
            Background = new SolidColorBrush(OutputColorScheme.Default.Background)
        }};
        
        // Act
        _viewModel.WriteHost(objects, null, null, separator, false);
        
        // Assert
        DocumentAssert.ContainsRuns(expectedRuns, _viewModel.ResultDocument);
    }

    [WpfFact]
    public void WriteHost_CustomColors_SingleRunOfSpecifiedColorsCreated()
    {
        // Arrange
        var objects = new object[] {"text"};
        const ConsoleColor foregroundColor = ConsoleColor.Red;
        const ConsoleColor backgroundColor = ConsoleColor.Blue;
        var expectedColorScheme = new OutputColorScheme(
            foregroundColor.ConvertConsoleColorToColor(), 
            backgroundColor.ConvertConsoleColorToColor());
        var expectedRuns = new List<Run>() { new()
        {
            Text = objects[0].ToString(),
            Foreground = new SolidColorBrush(expectedColorScheme.Foreground),
            Background = new SolidColorBrush(expectedColorScheme.Background)
        }};
        
        // Act
        _viewModel.WriteHost(objects, foregroundColor, backgroundColor, string.Empty, false);
        
        // Assert
        DocumentAssert.ContainsRuns(expectedRuns, _viewModel.ResultDocument);
    }
    
    [WpfFact]
    public void ExitHost_HostExited_CloseWindowActionInvoked()
    {
        // Arrange
        _actionMock.Setup(a => a.Invoke()).Verifiable();
        
        // Act
        _viewModel.ExitHost();
        
        // Assert
        _actionMock.Verify(a => a.Invoke(), Times.Once, "Close window action not invoked");;
    }

    [WpfFact]
    public async Task InputEnterKeyCommand_ExecuteCommand_CommandAddedToHistory()
    {
        // Arrange
        const string input = "command";
        _historyProviderMock.Setup(hp => hp.AddEntry(input)).Verifiable();
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns((IEnumerable<PSObject>?)null);
        
        // Act
        _viewModel.CommandInput = input;
        await _executeCommand.ExecuteAsync(null);

        // Assert
        _historyProviderMock.Verify(hp => hp.AddEntry(input), Times.Once);
    }
    
    [WpfFact]
    public async Task InputEnterKeyCommand_ExecuteCommand_InputCleared()
    {
        // Arrange
        const string input = "command";
        _historyProviderMock.Setup(hp => hp.AddEntry(input));
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns((IEnumerable<PSObject>?)null);
        
        // Act
        _viewModel.CommandInput = input;
        await _executeCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(_viewModel.CommandInput);
    }
    
    [WpfFact]
    public async Task InputEnterKeyCommand_ExecutionStopped_OutputCleared()
    {
        // Arrange
        const string input = "command";
        _historyProviderMock.Setup(hp => hp.AddEntry(input));
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns((IEnumerable<PSObject>?)null)
            .Callback(() => _viewModel.CommandInputControlCCommand.Execute(null));;
        
        // Act
        // Add some output so that we can test if it was cleared
        _viewModel.AddTextToResultDocument("random", OutputColorScheme.Default);
        _viewModel.CommandInput = input;
        await _executeCommand.ExecuteAsync(null);

        // Assert
        DocumentAssert.Empty(_viewModel.ResultDocument);
    }
    
    [WpfFact]
    public async Task InputEnterKeyCommand_ExecutionChangedWorkingDirectory_WorkingDirectoryChanged()
    {
        // Arrange
        const string input = "command";
        const string oldWorkingDirectory = "old", newWorkingDirectory = "new";
        _historyProviderMock.Setup(hp => hp.AddEntry(input));
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns((IEnumerable<PSObject>?)null);
        _powerShellServiceMock.Setup(pss => pss.WorkingDirectoryPath)
            .Returns(newWorkingDirectory);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.WorkingDirectoryPath = oldWorkingDirectory;
        await _executeCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(newWorkingDirectory, _viewModel.WorkingDirectoryPath);
    }
    
    [WpfFact]
    public async Task InputEnterKeyCommand_EmptyResult_OutputCleared()
    {
        // Arrange
        const string input = "command";
        _historyProviderMock.Setup(hp => hp.AddEntry(input));
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns([]);
        
        // Act
        // Add some output so that we can test if it was cleared
        _viewModel.AddTextToResultDocument("random", OutputColorScheme.Default);
        _viewModel.CommandInput = input;
        await _executeCommand.ExecuteAsync(null);

        // Assert
        DocumentAssert.Empty(_viewModel.ResultDocument);
    }
    
    [WpfFact]
    public async Task InputEnterKeyCommand_ResultOutputHandled_OutputClearedAndResultNotDisplayed()
    {
        // Arrange
        const string input = "command";
        var result = new List<PSObject> { new("result") };
        _historyProviderMock.Setup(hp => hp.AddEntry(input));
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns(result);
        
        // Act
        // Add some output so that we can test if it was cleared
        _viewModel.AddTextToResultDocument("random", OutputColorScheme.Default);
        _viewModel.CommandInput = input;
        // Call clear host so that the flag gets set
        _viewModel.ClearHost();
        await _executeCommand.ExecuteAsync(null);

        // Assert
        DocumentAssert.Empty(_viewModel.ResultDocument);
    }
    
    [WpfFact]
    public async Task InputEnterKeyCommand_CommandExecuted_ResultDisplayedWithDefaultColorScheme()
    {
        // Arrange
        const string input = "command";
        const string output = "result";
        var result = new List<PSObject> { new(output) };
        var expectedRuns = new List<Run> { new()
        {
            Text = output,
            Foreground = new SolidColorBrush(OutputColorScheme.Default.Foreground),
            Background = new SolidColorBrush(OutputColorScheme.Default.Background)
        }};
        _historyProviderMock.Setup(hp => hp.AddEntry(input));
        _powerShellServiceMock.Setup(pss => pss.ExecuteScript(input, true))
            .Returns(result);
        
        // Act
        _viewModel.CommandInput = input;
        await _executeCommand.ExecuteAsync(null);

        // Assert
        DocumentAssert.ContainsRuns(expectedRuns, _viewModel.ResultDocument);
    }

    [WpfFact]
    public void InputEscapeKeyCommand_HasInput_InputCleared()
    {
        // Arrange
        const string input = "command";
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputEscapeKeyCommand.Execute(null);
        
        // Assert
        Assert.Empty(_viewModel.CommandInput);;
    }
    
    [WpfFact]
    public void InputUpKeyCommand_EndOfHistory_InputUnchanged()
    {
        // Arrange
        const string input = "command";
        _historyProviderMock.Setup(hp => hp.NextEntry()).Returns((string?)null);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputUpKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(input, _viewModel.CommandInput);
    }
    
    [WpfFact]
    public void InputUpKeyCommand_MiddleOfHistory_InputSetToNextEntry()
    {
        // Arrange
        const string input = "command";
        const string nextEntry = "nextEntry";
        _historyProviderMock.Setup(hp => hp.NextEntry()).Returns(nextEntry);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputUpKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(nextEntry, _viewModel.CommandInput);
    }
    
    [WpfFact]
    public void InputDownKeyCommand_StartOfHistory_InputCleared()
    {
        // Arrange
        const string input = "command";
        _historyProviderMock.Setup(hp => hp.PrevEntry()).Returns((string?)null);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputDownKeyCommand.Execute(null);
        
        // Assert
        Assert.Empty(_viewModel.CommandInput);
    }
    
    [WpfFact]
    public void InputDownKeyCommand_MiddleOfHistory_InputSetToPrevEntry()
    {
        // Arrange
        const string command = "command";
        const string prevEntry = "nextEntry";
        _historyProviderMock.Setup(hp => hp.PrevEntry()).Returns(prevEntry);
        
        // Act
        _viewModel.CommandInput = command;
        _viewModel.CommandInputDownKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(prevEntry, _viewModel.CommandInput);
    }
    
    [WpfFact]
    public void InputDownKeyCommand_EndOfHistory_InputSetToEntryBeforeLast()
    {
        // Arrange
        const string command = "command";
        const string entryBeforeLast = "nextEntry";
        _historyProviderMock.SetupSequence(hp => hp.PrevEntry())
            .Returns(command)
            .Returns(entryBeforeLast);
        
        // Act
        _viewModel.CommandInput = command;
        _viewModel.CommandInputDownKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(entryBeforeLast, _viewModel.CommandInput);
    }

    [WpfFact]
    public void InputTabKeyCommand_NoCompletions_InputUnchanged()
    {
        // Arrange
        const string input = "command";
        const int caretPosition = 6;
        _completionProviderMock.Setup(cp => cp.GetCompletions(input, caretPosition)).Returns([]);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputCaretIndex = caretPosition;
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(input, _viewModel.CommandInput);
        Assert.Equal(caretPosition, _viewModel.CommandInputCaretIndex);
    }
    
    [WpfFact]
    public void InputTabKeyCommand_CompletionsExist_InputSetToNextCompletion()
    {
        // Arrange
        const string input = "command";
        const int caretPosition = 6;
        var completions = new List<CompletionElement<string>>
        {
            new() { Completion = "command1", Position = 7 },
            new() { Completion = "command23", Position = 8 }
        };
        _completionProviderMock.Setup(cp => cp.GetCompletions(input, caretPosition)).Returns(completions);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputCaretIndex = caretPosition;
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(completions[0].Completion, _viewModel.CommandInput);
        Assert.Equal(completions[0].Position, _viewModel.CommandInputCaretIndex);
    }
    
    [WpfFact]
    public void InputTabKeyCommand_CycledThroughAllCompletions_InputSetToFirstCompletion()
    {
        // Arrange
        const string input = "command";
        const int caretPosition = 6;
        var completions = new List<CompletionElement<string>>
        {
            new() { Completion = "command1", Position = 7 },
            new() { Completion = "command23", Position = 8 }
        };
        _completionProviderMock.Setup(cp => cp.GetCompletions(input, caretPosition)).Returns(completions);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputCaretIndex = caretPosition;
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(completions[0].Completion, _viewModel.CommandInput);
        Assert.Equal(completions[0].Position, _viewModel.CommandInputCaretIndex);
    }
    
    [WpfFact]
    public void InputTabKeyCommand_OneCompletionReturned_CompletionsReset()
    {
        // Arrange
        const string input = "command";
        const int caretPosition = 6;
        var firstCompletions = new List<CompletionElement<string>>
        {
            new() { Completion = "command1", Position = 7 },
        };
        var secondCompletions = new List<CompletionElement<string>>
        {
            new() { Completion = "command123", Position = 9 },
        };
        _completionProviderMock.SetupSequence(cp => cp.GetCompletions(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(firstCompletions)
            .Returns(secondCompletions);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputCaretIndex = caretPosition;
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        // First Tab will return single completion and reset completions, so the next one will fetch completions again
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        
        // Assert
        Assert.Equal(secondCompletions[0].Completion, _viewModel.CommandInput);
        Assert.Equal(secondCompletions[0].Position, _viewModel.CommandInputCaretIndex);
    }

    [WpfFact]
    public void InputChangedCommand_InputChangedByUser_CompletionsReset()
    {
        // Arrange
        const string input = "command", changedInput = "comm";
        const int caretPosition = 6,  changedCaretPosition = 3;
        var originalCompletions = new List<CompletionElement<string>>
        {
            new() { Completion = "command1", Position = 7 },
            new() { Completion = "command2", Position = 7 },
        };
        var newCompletions = new List<CompletionElement<string>>
        {
            new() { Completion = "commando", Position = 7 },
            new() { Completion = "common", Position = 5 },
        };
        _completionProviderMock.SetupSequence(cp => cp.GetCompletions(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(originalCompletions)
            .Returns(newCompletions);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputCaretIndex = caretPosition;
        // InputTextChangedCommand has to be invoked after Tab, since completion will be set and text changed
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        _viewModel.InputTextChangedCommand.Execute(null);
        // User input change
        _viewModel.CommandInput = changedInput;
        _viewModel.CommandInputCaretIndex = changedCaretPosition;
        _viewModel.InputTextChangedCommand.Execute(null);
        
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        _viewModel.InputTextChangedCommand.Execute(null);
        
        // Assert
        Assert.Equal(newCompletions[0].Completion, _viewModel.CommandInput);
        Assert.Equal(newCompletions[0].Position, _viewModel.CommandInputCaretIndex);
    }
    
    [WpfFact]
    public void InputChangedCommand_InputChangedByCompletion_CompletionsUnchanged()
    {
        // Arrange
        const string input = "command";
        const int caretPosition = 6;
        var completions = new List<CompletionElement<string>>
        {
            new() { Completion = "command1", Position = 7 },
            new() { Completion = "command2", Position = 7 },
        };
        _completionProviderMock.Setup(cp => cp.GetCompletions(input, caretPosition)).Returns(completions);
        
        // Act
        _viewModel.CommandInput = input;
        _viewModel.CommandInputCaretIndex = caretPosition;
        // InputTextChangedCommand has to be invoked after Tab, since completion will be set and text changed
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        _viewModel.InputTextChangedCommand.Execute(null);
        
        _viewModel.CommandInputTabKeyCommand.Execute(null);
        _viewModel.InputTextChangedCommand.Execute(null);
        
        // Assert
        Assert.Equal(completions[1].Completion, _viewModel.CommandInput);
        Assert.Equal(completions[1].Position, _viewModel.CommandInputCaretIndex);
    }
}