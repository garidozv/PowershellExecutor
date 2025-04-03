namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// An attribute to define and register a PowerShell command override
/// </summary>
/// <remarks>
/// This attribute is used to associate a PowerShell command name with a static method.
/// The method must be public and static
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PowerShellCommandAttribute : Attribute
{
    public string CommandName { get; }

    public PowerShellCommandAttribute(string commandName)
    {
        CommandName = commandName;
    }
}