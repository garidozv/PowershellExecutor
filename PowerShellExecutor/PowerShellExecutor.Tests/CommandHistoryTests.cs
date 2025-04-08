
using System.Collections.Immutable;
using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.Tests;

public class CommandHistoryTests
{
    private const string SingleCommand = "c";
    private const string FirstCommand = "c1";
    private const string SecondCommand = "c2";
    private const string ThirdCommand = "c3";
    private readonly CommandHistory _commandHistory = new();
    
    [Fact]
    public void AddCommand_NullCommand_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _commandHistory.AddCommand(null));
    }

    [Fact]
    public void AddCommand_EmptyCommand_CommandNotAdded()
    {
        // Arrange
        var expectedCommandCount = _commandHistory.Count;
        
        // Act
        _commandHistory.AddCommand(string.Empty);
        
        // Assert
        Assert.Equal(expectedCommandCount, _commandHistory.Count);
    }

    [Fact]
    public void AddCommand_SubsequentDuplicateCommands_OnlyOneCommandAdded()
    {
        // Arrange
        var expectedCommandCount = _commandHistory.Count + 1;
        
        // Act
        _commandHistory.AddCommand(SingleCommand);
        _commandHistory.AddCommand(SingleCommand);
        
        // Assert
        Assert.Equal(expectedCommandCount, _commandHistory.Count);
    }

    [Fact]
    public void AddCommand_ValidCommand_CommandAdded()
    {
        // Arrange
        var expectedCommandCount = _commandHistory.Count + 1;
        
        // Act
        _commandHistory.AddCommand(SingleCommand);
        
        // Assert
        Assert.Equal(expectedCommandCount, _commandHistory.Count);
    }

    [Fact]
    public void AddCommand_MoveToStart_FirstCommandReturned()
    {
        // Arrange

        // Act
        _commandHistory.AddCommand(FirstCommand);
        _commandHistory.NextCommand();
        _commandHistory.AddCommand(SecondCommand);
        var returnedCommand = _commandHistory.NextCommand();

        // Assert
        Assert.Equal(SecondCommand, returnedCommand);
    }

    [Fact]
    public void MoveToStart_HistoryNotEmpty_FirstCommandReturned()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(SingleCommand);
        _commandHistory.NextCommand();
        _commandHistory.MoveToStart();
        var returnedCommand = _commandHistory.NextCommand();

        // Assert
        Assert.Equal(SingleCommand, returnedCommand);
    }

    [Fact]
    public void NextCommand_EmptyHistory_NullReturned()
    {
        Assert.Null(_commandHistory.NextCommand());
    }

    /// <summary>
    /// All commands have been read, we are at the end of the history
    /// </summary>
    [Fact]
    public void NextCommand_EndReached_NullReturned()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(SingleCommand);
        _commandHistory.NextCommand();
        
        // Assert
        Assert.Null(_commandHistory.NextCommand());
    }

    /// <summary>
    /// At least one command has been read, and at least one command is left
    /// </summary>
    [Fact]
    public void NextCommand_InTheMiddle_NextCommandReturned()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(FirstCommand);
        _commandHistory.AddCommand(SecondCommand);
        _commandHistory.AddCommand(ThirdCommand);
        _commandHistory.NextCommand(); // Read Third command
        _commandHistory.NextCommand(); // Read Second command
        var returnedCommand = _commandHistory.NextCommand(); // Next command should be First command

        // Assert
        Assert.Equal(FirstCommand, returnedCommand);
    }

    /// <summary>
    /// No commands have been read yet, we are at the start of history
    /// </summary>
    [Fact]
    public void NextCommand_AtStart_FirstCommandReturned()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(FirstCommand);
        
        // Assert
        Assert.Equal(FirstCommand, _commandHistory.NextCommand());
    }

    [Fact]
    public void PreviousCommand_EmptyHistory_NullReturned()
    {
        Assert.Null(_commandHistory.PrevCommand());
    }

    /// <summary>
    /// No commands have been read yet, we are at the start of the history
    /// </summary>
    [Fact]
    public void PreviousCommand_AtStart_NullReturned()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(SingleCommand);
        
        // Assert
        Assert.Null(_commandHistory.PrevCommand());
    }

    /// <summary>
    /// All commands have been read, we are at the end of the history
    /// </summary>
    [Fact]
    public void PreviousCommand_EndReached_LastCommandReturned()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(SingleCommand);
        _commandHistory.NextCommand();
        _commandHistory.NextCommand();
        
        // Assert
        Assert.Equal(SingleCommand, _commandHistory.PrevCommand());
    }

    /// <summary>
    /// At least one command has been read, and at least one command is left
    /// </summary>
    [Fact]
    public void PreviousCommand_InTheMiddle_ReturnsPreviousCommand()
    {
        // Arrange
        
        // Act
        _commandHistory.AddCommand(FirstCommand);
        _commandHistory.AddCommand(SecondCommand);
        _commandHistory.AddCommand(ThirdCommand);
        
        _commandHistory.NextCommand(); // Read Third command
        _commandHistory.NextCommand(); // Read Second command
        var returnedCommand = _commandHistory.PrevCommand(); // Previous command be Third command

        // Assert
        Assert.Equal(ThirdCommand, returnedCommand);
    }
}