using System.Windows.Documents;
using System.Windows.Media;

namespace PowerShellExecutor.UI.Tests.Comparers;

/// <summary>
/// Compares two <see cref="Run"/> objects for equality based on their
/// background, foreground, and text properties
/// </summary>
public class RunComparer : IEqualityComparer<Run>
{
    public bool Equals(Run? firstRun, Run? secondRun)
    {
        if (ReferenceEquals(firstRun, secondRun)) return true;
        if (firstRun is null || secondRun is null || firstRun.GetType() != secondRun.GetType()) return false;

        var firstBackgroundColor = GetBrushColor(firstRun.Background);
        var firstForegroundColor = GetBrushColor(firstRun.Foreground);
        var secondBackgroundColor = GetBrushColor(secondRun.Background);
        var secondForegroundColor = GetBrushColor(secondRun.Foreground);

        return firstRun.Text == secondRun.Text &&
               Equals(firstBackgroundColor, secondBackgroundColor) &&
               Equals(firstForegroundColor, secondForegroundColor);
    }

    public int GetHashCode(Run run)
    {
        var backgroundColor = GetBrushColor(run.Background);
        var foregroundColor = GetBrushColor(run.Foreground);
        return HashCode.Combine(backgroundColor, foregroundColor, run.Text);
    }

    private static Color? GetBrushColor(Brush? brush)
    {
        return (brush as SolidColorBrush)?.Color;
    }

}