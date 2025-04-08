using PowerShellExecutor.Helpers;

namespace PowerShellExecutor.Tests;


public class ExtensionsTests
{
    // ToSingleLineString fields
    private const string Prefix = "Prefix";
    private const string TextWithoutNewLine = "Text";
    private static readonly string TextWithNewLine = $"{TextWithoutNewLine}{Environment.NewLine}";
    
    
    [Fact]
    public void ToSingleLineString_NullPrefix_TextWithoutPrefixReturned()
    {
        // Arrange

        // Act
        var result = TextWithoutNewLine.ToSingleLineString(null);

        // Assert
        Assert.Equal(TextWithoutNewLine, result);
    }

    [Fact]
    public void ToSingleLineString_PrefixAndTextWithoutNewLine_PrefixedTextReturned()
    {
        // Arrange

        // Act
        var result = TextWithoutNewLine.ToSingleLineString(Prefix);

        // Assert
        Assert.Equal($"{Prefix}{TextWithoutNewLine}", result);
    }

    [Fact]
    public void ToSingleLineString_PrefixAndTextWithNewLine_PrefixedTextWithoutNewLineReturned()
    {
        // Arrange

        // Act
        var result = TextWithNewLine.ToSingleLineString(Prefix);

        // Assert
        Assert.Equal($"{Prefix}{TextWithoutNewLine}", result);
    }
    
    [Fact]
    public void ToSingleLineString_NullObject_ShouldReturnNullString()
    {
        // Arrange
        object obj = null!;

        // Act
        
        // Assert
        Assert.Throws<ArgumentNullException>(() => obj.ToSingleLineString(Prefix));
    }

    [Fact]
    public void ReplaceSegment_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        string source = null!;

        // Act
        
        // Assert
        Assert.Throws<ArgumentNullException>(() => source.ReplaceSegment(0, 0, string.Empty));
    }
    
    [Fact]
    public void ReplaceSegment_NullReplacement_ThrowsArgumentNullException()
    {
        // Arrange
        const string source = "source";
        string replacement = null!;

        // Act
        
        // Assert
        Assert.Throws<ArgumentNullException>(() => source.ReplaceSegment(0, 0, replacement));
    }
    
    [Fact]
    public void ReplaceSegment_NegativeStartIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string source = "source", replacement = "replacement";
        
        // Act
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => source.ReplaceSegment(-1, 0, replacement));
    }
    
    [Fact]
    public void ReplaceSegment_StartIndexGreaterOrEqualToSourceLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string source = "source", replacement = "replacement";
        
        // Act
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => source.ReplaceSegment(source.Length, 0, replacement));
    }
    
    [Fact]
    public void ReplaceSegment_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string source = "source", replacement = "replacement";
        
        // Act
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => source.ReplaceSegment(0, -1, replacement));
    }
    
    [Fact]
    public void ReplaceSegment_LengthGreaterThanAvailable_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const string source = "source", replacement = "replacement";
        
        // Act
        
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => source.ReplaceSegment(1, source.Length, replacement));
    }
    
    [Fact]
    public void ReplaceSegment_ReplacementGreaterThanLength_ReturnsReplacedSource()
    {
        // Arrange
        const string source = "source", replacement = "REPLACEMENT", expectedResult = "sREPLACEMENTrce";
        const int length = 2, index = 1;
        
        // Act
        var result = source.ReplaceSegment(index, length, replacement);
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
    
    [Fact]
    public void ReplaceSegment_ReplacementGreaterThanLengthAtSourceEnd_ReturnsReplacedSource()
    {
        // Arrange
        const string source = "source", replacement = "REPLACEMENT", expectedResult = "sourREPLACEMENT";
        const int length = 2, index = 4;
        
        // Act
        var result = source.ReplaceSegment(index, length, replacement);
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
    
    [Fact]
    public void ReplaceSegment_ReplacementShorterThanLength_ReturnsReplacedSource()
    {
        // Arrange
        const string source = "source", replacement = "REP", expectedResult = "REPce";
        const int length = 4, index = 0;
        
        // Act
        var result = source.ReplaceSegment(index, length, replacement);
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
    
    [Fact]
    public void ReplaceSegment_ReplacementShorterThanLengthAtSourceEnd_ReturnsReplacedSource()
    {
        // Arrange
        const string source = "source", replacement = "REP", expectedResult = "soREP";
        const int length = 4, index = 2;
        
        // Act
        var result = source.ReplaceSegment(index, length, replacement);
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
}
