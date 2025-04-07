using System.Text;

namespace PowerShellExecutor.Helpers;

public static class Extensions
{
    /// <summary>
    /// Converts an enumerable collection of objects into a single string, with each object's
    /// string representation on a new line, optionally prefixed with a specified line prefix
    /// </summary>
    /// <param name="enumerable">The collection of objects to convert to a string</param>
    /// <param name="linePrefix">An optional string to prepend to each line. Default is <c>null</c></param>
    /// <returns>A formatted string with one item per line, optionally prefixed by <paramref name="linePrefix"/></returns>
    public static string ToItemListString(this IEnumerable<object> enumerable, string? linePrefix = null)
    {
        var strBuilder = new StringBuilder();
        foreach (var item in enumerable)
        {
            strBuilder.Append(linePrefix).AppendLine(item.ToString());
        }
        if (strBuilder.Length >= Environment.NewLine.Length) 
            strBuilder.Remove(strBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        
        return strBuilder.ToString();
    }
    

    /// <summary>
    /// Converts an object to its string representation and removes the trailing new line if there is one
    /// </summary>
    /// <param name="obj">The object to be converted to a string</param>
    /// <param name="linePrefix">An optional prefix to prepend to the string. Default is <c>null</c>.</param>
    /// <returns>The string representation of the object without a trailing newline</returns>
    public static string ToSingleLineString(this object obj, string? linePrefix = null)
    {
        var strBuilder = new StringBuilder();
        strBuilder.Append(obj);
        if (strBuilder.Length >= Environment.NewLine.Length)
            strBuilder.Remove(strBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        
        return strBuilder.ToString();
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