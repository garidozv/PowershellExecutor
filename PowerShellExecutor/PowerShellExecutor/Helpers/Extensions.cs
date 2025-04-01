using System.Text;

namespace PowerShellExecutor.Helpers;

public static class Extensions
{
    public static string ToItemListString(this IEnumerable<Object> enumerable)
    {
        var strBuilder = new StringBuilder();
        foreach (var item in enumerable)
        {
            strBuilder.AppendLine(item.ToString());
        }
        return strBuilder.ToString();
    }
}