using System.Management.Automation;

namespace PowerShellExecutor.Tests.Comparers;

/// <summary>
/// Custom comparer used for testing ErrorRecords generated in ExecuteString method
/// of PowerShellService
/// </summary>
public class ErrorRecordComparer : IEqualityComparer<ErrorRecord>
{
    public bool Equals(ErrorRecord? x, ErrorRecord? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return Equals(x.Exception.Message, y.Exception.Message) && Equals(x.TargetObject, y.TargetObject)
                && Equals(x.CategoryInfo.Category, y.CategoryInfo.Category) &&
               x.FullyQualifiedErrorId == y.FullyQualifiedErrorId;
    }

    public int GetHashCode(ErrorRecord obj)
    {
        return HashCode.Combine(obj.Exception.Message, obj.TargetObject, 
            obj.CategoryInfo.Category, obj.FullyQualifiedErrorId);
    }
}