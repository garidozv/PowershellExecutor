using System.Windows.Documents;
using PowerShellExecutor.UI.Tests.Comparers;

namespace PowerShellExecutor.UI.Tests.Asserts;

public static class DocumentAssert
{
    public static void Empty(FlowDocument document) => Assert.Empty(document.Blocks);
    
    public static void ContainsRuns(IList<Run> expectedRuns, FlowDocument document)
    {
        Assert.Equal(expectedRuns.Count, document.Blocks.Count);
        
        var actualRuns = document.Blocks
            .OfType<Paragraph>()
            .Select(p => p.Inlines.FirstInline)
            .OfType<Run>()
            .ToList();

        Assert.Equal(expectedRuns, actualRuns, new RunComparer());
    }
}