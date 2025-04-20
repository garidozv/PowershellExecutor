using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Language;
using Moq;
using PowerShellExecutor.PowerShellUtilities;
using PowerShellExecutor.Tests.Comparers;

namespace PowerShellExecutor.Tests;

public class PowerShellServiceTests
{
    private class InvalidTestCmdlet : PSCmdlet { }

    private const string OutStringCommand = "Out-String";
    private const string Script = "script";

    private readonly PowerShellService _powerShellService;
    private readonly Mock<IPowerShell> _powerShellWrapperMock;

    public PowerShellServiceTests()
    {
        _powerShellWrapperMock = new Mock<IPowerShell>();
        _powerShellService = new PowerShellService(_powerShellWrapperMock.Object);
    }

    private static ErrorRecord CreateErrorRecord(ParseError error) =>
        new(new Exception(error.Message), "Parse error", ErrorCategory.ParserError, null);
    
    private void SetupExecuteScriptCommonMockMethods(bool asStringOutput = true)
    {
        _powerShellWrapperMock
            .Setup(ps => ps.AddScript(Script))
            .Returns(_powerShellWrapperMock.Object);
        if (asStringOutput) 
            _powerShellWrapperMock
                .Setup(ps =>
                    ps.AddCommand(It.Is<string>(cmd =>
                        string.Equals(cmd, OutStringCommand, StringComparison.OrdinalIgnoreCase))))
                .Returns(_powerShellWrapperMock.Object);
        _powerShellWrapperMock
            .Setup(ps => ps.Clear())
            .Verifiable();
    }

    [Fact]
    public void ExecuteScript_NullScript_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _powerShellService.ExecuteScript(null));
    }

    [Fact]
    public void ExecuteScript_StringObjectResult_ResultReturned()
    {
        // Arrange
        var expectedResult = new PSObject("result");
        var resultCollection = new Collection<PSObject> { expectedResult };

        SetupExecuteScriptCommonMockMethods();
        _powerShellWrapperMock.Setup(ps => ps.Invoke()).Returns(resultCollection);

        // Act
        var result = _powerShellService.ExecuteScript(Script, true);

        // Assert
        _powerShellWrapperMock.Verify(ps => ps.Clear(), Times.Once);
        Assert.NotNull(result);
        var firstElement = result.First();
        Assert.IsType<string>(firstElement.BaseObject);
        Assert.Equal(expectedResult, firstElement);
    }
    
    [Fact]
    public void ExecuteScript_NonStringObjectResult_ResultReturned()
    {
        // Arrange
        var expectedResult = new Collection<PSObject>()
        {
            new PSObject("obj1"),
            new PSObject("obj2")
        };

        SetupExecuteScriptCommonMockMethods(false);
        _powerShellWrapperMock.Setup(ps => ps.Invoke()).Returns(expectedResult);

        // Act
        var result = _powerShellService.ExecuteScript(Script);

        // Assert
        _powerShellWrapperMock.Verify(ps => ps.Clear(), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void ExecuteScript_ExecutionStopped_NullReturned()
    {
        // Arrange
        SetupExecuteScriptCommonMockMethods();
        _powerShellWrapperMock.Setup(ps => ps.Invoke())
            .Callback(_powerShellHostService.StopExecution)
            .Returns([]);
        
        // Act
        var result = _powerShellService.ExecuteScript(Script);

        // Assert
        _powerShellWrapperMock.Verify(ps => ps.Clear(), Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void ExecuteScript_InvalidScript_NullReturned()
    {
        // Arrange
        var errorStream = new PSDataCollection<ErrorRecord>();

        SetupExecuteScriptCommonMockMethods();
        _powerShellWrapperMock.Setup(ps => ps.Invoke()).Throws<ParseException>();
        _powerShellWrapperMock.Setup(ps => ps.ErrorStream).Returns(errorStream);

        // Act
        var result = _powerShellService.ExecuteScript(Script);

        // Assert
        _powerShellWrapperMock.Verify(ps => ps.Clear(), Times.Once);
        Assert.Null(result);
    }

    [Fact]
    public void ExecuteScript_InvalidScriptParseErrorsGenerated_ParseErrorsAddedToErrorStream()
    {
        // Arrange
        var errorStream = new PSDataCollection<ErrorRecord>();
        var parseErrors = new[] { 
            new ParseError(null, "id1", "message1"), 
            new ParseError(null, "id2", "message2") };
        var exception = new ParseException(parseErrors);
        var expectedErrorRecords = parseErrors.Select(CreateErrorRecord).ToArray();

        SetupExecuteScriptCommonMockMethods();
        _powerShellWrapperMock.Setup(ps => ps.Invoke()).Throws(() => exception);
        _powerShellWrapperMock.Setup(ps => ps.ErrorStream).Returns(errorStream).Verifiable();

        // Act
        var result = _powerShellService.ExecuteScript(Script);

        // Assert
        _powerShellWrapperMock.Verify(ps => ps.Clear(), Times.Once);
        _powerShellWrapperMock.Verify(ps => ps.ErrorStream, Times.Exactly(2));
        Assert.Equal(expectedErrorRecords, errorStream.ToArray(), new ErrorRecordComparer());
    }

    [Fact]
    public void ExecuteScript_InvalidScriptNoParseErrorsGenerated_ErrorRecordAddedToErrorStream()
    {
        // Arrange
        var errorStream = new PSDataCollection<ErrorRecord>();
        var exception = new ParseException("message");
        var expectedErrorRecords = new[] { exception.ErrorRecord };

        SetupExecuteScriptCommonMockMethods();
        _powerShellWrapperMock.Setup(ps => ps.Invoke()).Throws(() => exception);
        _powerShellWrapperMock.Setup(ps => ps.ErrorStream).Returns(errorStream).Verifiable();

        // Act
        var result = _powerShellService.ExecuteScript(Script);

        // Assert
        _powerShellWrapperMock.Verify(ps => ps.Clear(), Times.Once);
        _powerShellWrapperMock.Verify(ps => ps.ErrorStream, Times.Once);
        Assert.Equal(expectedErrorRecords, errorStream.ToArray());
    }

    [Fact]
    public void IsDictionaryCompletion_NullCompletion_ArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(() => _powerShellService.IsDirectoryCompletion(null));
    }

    [Fact]
    public void RegisterCustomCmdlet_TypeWithoutCmdletAttribute_InvalidOperationExceptionThrown()
    {
        Assert.Throws<InvalidOperationException>(() => _powerShellService.RegisterCustomCmdlet<InvalidTestCmdlet>());
    }
}