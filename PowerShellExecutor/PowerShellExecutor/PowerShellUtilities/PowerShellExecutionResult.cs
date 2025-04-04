using System.Management.Automation.Language;
using System.Management.Automation;

namespace PowerShellExecutor.PowerShellUtilities;

public class PowerShellExecutionResult
{
    /// <summary>
    /// Indicates whether the script executed successfully or failed
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// The executed script
    /// </summary>
    public string Script { get; set; } = string.Empty;

    /// <summary>
    /// The objects returned from the executed script
    /// </summary>
    public IEnumerable<PSObject>? CommandResults { get; set; }

    /// <summary>
    /// Error records captured during the execution of the script
    /// </summary>
    public IEnumerable<ErrorRecord>? Errors { get; set; }

    /// <summary>
    /// Contains parsing error information
    /// </summary>
    public IEnumerable<ParseError>? ParseErrors { get; set; }

    /// <summary>
    /// The warnings generated during script execution
    /// </summary>
    public IEnumerable<WarningRecord>? Warnings { get; set; }

    /// <summary>
    /// The verbose output generated during script execution
    /// </summary>
    public IEnumerable<VerboseRecord>? VerboseMessages { get; set; }

    /// <summary>
    /// The debug messages generated during script execution
    /// </summary>
    public IEnumerable<DebugRecord>? DebugMessages { get; set; }

    /// <summary>
    /// Represents informational messages generated during script execution
    /// </summary>
    public IEnumerable<InformationRecord>? InformationMessages { get; set; }
}