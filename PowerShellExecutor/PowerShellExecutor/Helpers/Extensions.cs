using System.Text;

namespace PowerShellExecutor.Helpers;

public static class Extensions
{
    /// <summary>
    /// Converts the string representation of an object to a single line, optionally prefixed, without a trailing newline.
    /// </summary>
    /// <param name="obj">The object to convert to a single-line string.</param>
    /// <param name="linePrefix">An optional prefix to prepend to the string. Default is <c>null</c>.</param>
    /// <returns>A single-line string representation of the object without a trailing newline.</returns>
    public static string ToSingleLineString(this object obj, string? linePrefix = null)
    {
        var strBuilder = new StringBuilder();
        strBuilder.Append(obj);
        
        var str = strBuilder.ToString();
        
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
        ArgumentNullException.ThrowIfNull(replacement);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)startIndex, (uint)source.Length, nameof(startIndex));

        var builder = new StringBuilder(source.Length - length + replacement.Length);
        builder.Append(source.AsSpan(0, startIndex))
            .Append(replacement)
            .Append(source.AsSpan(startIndex + length));

        return builder.ToString();
    }
}