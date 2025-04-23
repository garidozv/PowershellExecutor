using System.Windows.Media;

namespace PowerShellExecutor.Helpers;

/// <summary>
/// Represents a color scheme composed of foreground and background colors,
/// used for defining the appearance of the output text
/// </summary>
public record OutputColorScheme(Color Foreground, Color Background)
{
    public static readonly OutputColorScheme Default = new(Colors.White, Colors.Transparent);
    public static readonly OutputColorScheme Error = new(Colors.Red, Colors.Black);
    public static readonly OutputColorScheme Verbose = new(Colors.Yellow, Colors.Black);
    public static readonly OutputColorScheme Debug = new(Colors.Yellow, Colors.Black);
    public static readonly OutputColorScheme Warning = new(Colors.Yellow, Colors.Black);
    public static readonly OutputColorScheme Information = new(Colors.White, Colors.Transparent);
}