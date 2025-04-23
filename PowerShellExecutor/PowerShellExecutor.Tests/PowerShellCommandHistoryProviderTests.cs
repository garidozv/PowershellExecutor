using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.Tests;

public class PowerShellCommandHistoryProviderTests
{
    private const string SingleCommand = "c";
    private const string FirstCommand = "c1";
    private const string SecondCommand = "c2";
    private const string ThirdCommand = "c3";
    private readonly PowerShellCommandHistoryProvider _powerShellCommandHistoryProvider = new();
    
    [Fact]
    public void AddEntry_NullCommand_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _powerShellCommandHistoryProvider.AddEntry(null));
    }

    [Fact]
    public void AddEntry_EmptyCommand_CommandNotAdded()
    {
        // Arrange
        var expectedCommandCount = _powerShellCommandHistoryProvider.Count;
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(string.Empty);
        
        // Assert
        Assert.Equal(expectedCommandCount, _powerShellCommandHistoryProvider.Count);
    }

    [Fact]
    public void AddEntry_SubsequentDuplicateCommands_OnlyOneCommandAdded()
    {
        // Arrange
        var expectedCommandCount = _powerShellCommandHistoryProvider.Count + 1;
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        
        // Assert
        Assert.Equal(expectedCommandCount, _powerShellCommandHistoryProvider.Count);
    }

    [Fact]
    public void AddEntry_ValidCommand_CommandAdded()
    {
        // Arrange
        var expectedCommandCount = _powerShellCommandHistoryProvider.Count + 1;
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        
        // Assert
        Assert.Equal(expectedCommandCount, _powerShellCommandHistoryProvider.Count);
    }

    [Fact]
    public void AddEntry_MoveToStart_FirstCommandReturned()
    {
        // Arrange

        // Act
        _powerShellCommandHistoryProvider.AddEntry(FirstCommand);
        _powerShellCommandHistoryProvider.NextEntry();
        _powerShellCommandHistoryProvider.AddEntry(SecondCommand);
        var returnedCommand = _powerShellCommandHistoryProvider.NextEntry();

        // Assert
        Assert.Equal(SecondCommand, returnedCommand);
    }

    [Fact]
    public void MoveToStart_HistoryNotEmpty_FirstCommandReturned()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        _powerShellCommandHistoryProvider.NextEntry();
        _powerShellCommandHistoryProvider.MoveToStart();
        var returnedCommand = _powerShellCommandHistoryProvider.NextEntry();

        // Assert
        Assert.Equal(SingleCommand, returnedCommand);
    }

    [Fact]
    public void NextEntry_EmptyHistory_NullReturned()
    {
        Assert.Null(_powerShellCommandHistoryProvider.NextEntry());
    }

    /// <summary>
    /// All commands have been read, we are at the end of the history
    /// </summary>
    [Fact]
    public void NextEntry_EndReached_NullReturned()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        _powerShellCommandHistoryProvider.NextEntry();
        
        // Assert
        Assert.Null(_powerShellCommandHistoryProvider.NextEntry());
    }

    /// <summary>
    /// At least one command has been read, and at least one command is left
    /// </summary>
    [Fact]
    public void NextEntry_InTheMiddle_NextEntryReturned()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(FirstCommand);
        _powerShellCommandHistoryProvider.AddEntry(SecondCommand);
        _powerShellCommandHistoryProvider.AddEntry(ThirdCommand);
        _powerShellCommandHistoryProvider.NextEntry(); // Read Third command
        _powerShellCommandHistoryProvider.NextEntry(); // Read Second command
        var returnedCommand = _powerShellCommandHistoryProvider.NextEntry(); // Next command should be First command

        // Assert
        Assert.Equal(FirstCommand, returnedCommand);
    }

    /// <summary>
    /// No commands have been read yet, we are at the start of history
    /// </summary>
    [Fact]
    public void NextEntry_AtStart_FirstCommandReturned()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(FirstCommand);
        
        // Assert
        Assert.Equal(FirstCommand, _powerShellCommandHistoryProvider.NextEntry());
    }

    [Fact]
    public void PreviousCommand_EmptyHistory_NullReturned()
    {
        Assert.Null(_powerShellCommandHistoryProvider.PrevEntry());
    }

    /// <summary>
    /// No commands have been read yet, we are at the start of the history
    /// </summary>
    [Fact]
    public void PreviousCommand_AtStart_NullReturned()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        
        // Assert
        Assert.Null(_powerShellCommandHistoryProvider.PrevEntry());
    }

    /// <summary>
    /// All commands have been read, we are at the end of the history
    /// </summary>
    [Fact]
    public void PreviousCommand_EndReached_LastCommandReturned()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(SingleCommand);
        _powerShellCommandHistoryProvider.NextEntry();
        _powerShellCommandHistoryProvider.NextEntry();
        
        // Assert
        Assert.Equal(SingleCommand, _powerShellCommandHistoryProvider.PrevEntry());
    }

    /// <summary>
    /// At least one command has been read, and at least one command is left
    /// </summary>
    [Fact]
    public void PreviousCommand_InTheMiddle_ReturnsPreviousCommand()
    {
        // Arrange
        
        // Act
        _powerShellCommandHistoryProvider.AddEntry(FirstCommand);
        _powerShellCommandHistoryProvider.AddEntry(SecondCommand);
        _powerShellCommandHistoryProvider.AddEntry(ThirdCommand);
        
        _powerShellCommandHistoryProvider.NextEntry(); // Read Third command
        _powerShellCommandHistoryProvider.NextEntry(); // Read Second command
        var returnedCommand = _powerShellCommandHistoryProvider.PrevEntry(); // Previous command be Third command

        // Assert
        Assert.Equal(ThirdCommand, returnedCommand);
    }
}