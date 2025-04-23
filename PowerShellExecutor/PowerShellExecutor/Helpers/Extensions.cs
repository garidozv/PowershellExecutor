using System.Text;
using System.Windows.Media;

namespace PowerShellExecutor.Helpers;

public static class Extensions
{
    /// <summary>
    /// Converts the string representation of an object to a single line, optionally prefixed, without a trailing newline.
    /// </summary>
    /// <param name="obj">The object to convert to a single-line string</param>
    /// <param name="linePrefix">An optional prefix to prepend to the string. Default is <c>null</c></param>
    /// <returns>A single-line string representation of the object without a trailing newline</returns>
    public static string ToSingleLineString(this object obj, string? linePrefix = null)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var str = $"{linePrefix}{obj}";

        if (str.EndsWith(Environment.NewLine))
            str = str.Substring(0, str.Length - Environment.NewLine.Length);

        return str;
    }

    /// <summary>
    /// Replaces a portion of the string with the specified value, starting at a given index and spanning a defined length
    /// </summary>
    /// <param name="source">The original string</param>
    /// <param name="startIndex">The index at which the replacement begins</param>
    /// <param name="length">The number of characters to replace</param>
    /// <param name="replacement">The string to insert in place of the removed section</param>
    /// <returns>A new string with the specified portion replaced</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="replacement"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> is out of bounds</exception>
    public static string ReplaceSegment(this string source, int startIndex, int length, string replacement)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(replacement);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)startIndex, (uint)source.Length, nameof(startIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)length, (uint)source.Length - (uint)startIndex,
            nameof(length));

        var builder = new StringBuilder(source.Length - length + replacement.Length);
        builder
            .Append(source.AsSpan(0, startIndex))
            .Append(replacement)
            .Append(source.AsSpan(startIndex + length));

        return builder.ToString();
    }

    /// <summary>
    /// Converts a <see cref="ConsoleColor"/> to a <see cref="Color"/>
    /// </summary>
    /// <param name="consoleColor">The <see cref="ConsoleColor"/> to convert</param>
    /// <returns>The corresponding <see cref="Color"/></returns>
    public static Color ConvertConsoleColorToColor(this ConsoleColor consoleColor) => consoleColor switch
    {
        ConsoleColor.Black => Colors.Black,
        ConsoleColor.DarkBlue => Colors.DarkBlue,
        ConsoleColor.DarkGreen => Colors.DarkGreen,
        ConsoleColor.DarkCyan => Colors.DarkCyan,
        ConsoleColor.DarkRed => Colors.DarkRed,
        ConsoleColor.DarkMagenta => Colors.DarkMagenta,
        ConsoleColor.DarkYellow => Colors.Olive,
        ConsoleColor.Gray => Colors.Gray,
        ConsoleColor.DarkGray => Colors.DarkGray,
        ConsoleColor.Blue => Colors.Blue,
        ConsoleColor.Green => Colors.Green,
        ConsoleColor.Cyan => Colors.Cyan,
        ConsoleColor.Red => Colors.Red,
        ConsoleColor.Magenta => Colors.Magenta,
        ConsoleColor.Yellow => Colors.Yellow,
        ConsoleColor.White => Colors.White,
        _ => Colors.White
    };
}