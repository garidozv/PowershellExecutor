
using System.Collections.Immutable;
using PowerShellExecutor.PowerShellUtilities;

namespace PowerShellExecutor.Tests;

public class CommandHistoryTests
{
    private static readonly IEnumerable<string> SampleCommands =
    [
        "cd", "ls | Select-Object Name", "Select-Object", "Get-Alias -Name clear", "dir" 
    ];
    private readonly CommandHistory _commandHistory = new();
    
    [Fact]
    public void AddCommand_NullCommand_ArgumentNullExceptionThrown()
    {
        // Arrange
        
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => _commandHistory.AddCommand(null));
    }
    
    [Fact]
    public void AddCommand_EmptyCommand_NoCommandAdded()
    {
        // Arrange
        var expectedCount = _commandHistory.Count;
        
        // Act
        _commandHistory.AddCommand(string.Empty);
        
        // Assert
        Assert.Equal(expectedCount, _commandHistory.Count);   
    }
    
    [Fact]
    public void AddCommand_SubsequentDuplicateCommands_OnlyOneCommandAdded()
    {
        // Arrange
        const string command = "c";
        var expectedCount = _commandHistory.Count + 1;
        
        // Act
        _commandHistory.AddCommand(command);
        _commandHistory.AddCommand(command);
        
        // Assert
        Assert.Equal(expectedCount, _commandHistory.Count);   
    }
    
    [Fact]
    public void AddCommand_ValidCommand_CommandAdded()
    {
        // Arrange
        const string command = "c";
        var expectedCount = _commandHistory.Count + 1;
        
        // Act
        _commandHistory.AddCommand(command);
        
        // Assert
        Assert.Equal(expectedCount, _commandHistory.Count);   
    }
    
    [Fact]
    public void AddCommand_MoveToStartAfterAdd_NewestCommandReturned()
    {
        // Arrange
        string[] commands = ["c1", "c2"];
        
        // Act
        _commandHistory.AddCommand(commands[0]);
        _commandHistory.NextCommand();
        _commandHistory.AddCommand(commands[1]);
        var returned = _commandHistory.NextCommand();
        
        // Assert
        Assert.Equal(commands[1], returned);
    }
    
    [Fact]
    public void MoveToStart_NotAtTheStart_FirstCommandReturned()
    {
        // Arrange
        const string command = "c";
        
        // Act
        _commandHistory.AddCommand(command);
        _commandHistory.NextCommand();
        _commandHistory.MoveToStart();
        var returned = _commandHistory.NextCommand();
        
        // Assert
        Assert.Equal(command, returned);
    }
    
    [Fact]
    public void NextCommand_EmptyHistory_NullReturned()
    {
        // Arrange
        
        // Act
        var returned = _commandHistory.NextCommand();
        
        // Assert
        Assert.Null(returned);   
    }
    
    [Fact]
    public void NextCommand_EndReached_NullReturned()
    {
        // Arrange
        const string command = "test";
        
        // Act
        _commandHistory.AddCommand(command);
        // Move to end
        _commandHistory.NextCommand();
        var returned = _commandHistory.NextCommand();
        
        // Assert
        Assert.Null(returned);   
    }
    
    /// <summary>
    /// At least one command has been read, and at least one command is left
    /// </summary>
    [Fact]
    public void NextCommand_InTheMiddle_NextCommandReturned()
    {
        // Arrange
        string[] commands = ["c1", "c2", "c3"];
        
        // Act
        _commandHistory.AddCommand(commands[0]);
        _commandHistory.AddCommand(commands[1]);
        _commandHistory.AddCommand(commands[2]);
        _commandHistory.NextCommand(); // read command [2]
        _commandHistory.NextCommand(); // read command [1]
        var returned = _commandHistory.NextCommand();
        
        // Assert
        // Next command relative to the last read command [1] is command [0]
        Assert.Equal(commands[0], returned);   
    }
    
    /// <summary>
    /// No commands have been read yet, we are at the start of history
    /// </summary>
    [Fact]
    public void NextCommand_AtTheStart_FirstCommandReturned()
    {
        // Arrange
        const string command = "c1";
        
        // Act
        _commandHistory.AddCommand(command);
        var returned = _commandHistory.NextCommand();
        
        // Assert
        Assert.Equal(command, returned);   
    }
    
    [Fact]
    public void PreviousCommand_EmptyHistory_NullReturned()
    {
        // Arrange
        
        // Act
        var returned = _commandHistory.PrevCommand();
        
        // Assert
        Assert.Null(returned);   
    }
    
    /// <summary>
    /// No commands have been read yet, we are at the start of the history
    /// </summary>
    [Fact]
    public void PreviousCommand_AtTheStart_NullReturned()
    {
        // Arrange
        const string command = "c";
        
        // Act
        _commandHistory.AddCommand(command);
        var returned = _commandHistory.PrevCommand();
        
        // Assert
        Assert.Null(returned);   
    }
    
    /// <summary>
    /// All commands have been read, we are at the end of the history
    /// </summary>
    [Fact]
    public void PreviousCommand_EndReached_LastCommandReturned()
    {
        // Arrange
        const string command = "c";
        
        // Act
        _commandHistory.AddCommand(command);
        _commandHistory.NextCommand();
        _commandHistory.NextCommand();
        var returned = _commandHistory.PrevCommand();
        
        // Assert
        Assert.Equal(command, returned);   
    }
    
    [Fact]
    public void PreviousCommand_InTheMiddle_PreviousCommandReturned()
    {
        // Arrange
        string[] commands = ["c1", "c2", "c3"];
        
        // Act
        _commandHistory.AddCommand(commands[0]);
        _commandHistory.AddCommand(commands[1]);
        _commandHistory.AddCommand(commands[2]);
        _commandHistory.NextCommand(); // read command [2]
        _commandHistory.NextCommand(); // read command [1]
        var returned = _commandHistory.PrevCommand();
        
        // Assert
        // Previous command relative to the last read command [1] is command [2]
        Assert.Equal(commands[2], returned);   
    }
}