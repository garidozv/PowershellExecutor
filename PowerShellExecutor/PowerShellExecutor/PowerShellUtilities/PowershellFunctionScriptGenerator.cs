using System.Reflection;
using System.Text;

/// <summary>
/// Provides functionality to generate PowerShell function scripts based on method metadata.
/// </summary>
/// <remarks>
/// Currently, it only supports mandatory function parameters
/// </remarks>
public static class PowershellFunctionScriptGenerator
{
    private const string ParameterAttribute = "[Parameter(ValueFromPipeline=$true)]";

    /// <summary>
    /// Generates a PowerShell function script based on the provided method information and function name
    /// </summary>
    /// <param name="functionName">The name of the PowerShell function to be generated</param>
    /// <param name="sourceMethod">The <see cref="MethodInfo"/> object representing the method used to generate the script</param>
    /// <returns>A string containing the generated PowerShell script for the function</returns>
    public static string Generate(string functionName, MethodInfo sourceMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ArgumentNullException.ThrowIfNull(sourceMethod);

        var parameters = sourceMethod.GetParameters();
        var scriptBuilder = new StringBuilder();
        scriptBuilder.AppendLine($"function {functionName} {{");
        
        // Generate param() block
        scriptBuilder.AppendLine("param(");
        
        for (var i = 0; i < parameters.Length; i++)
        {
            scriptBuilder
                .AppendLine(ParameterAttribute)
                .Append($"[{parameters[i].ParameterType.Name}] ${parameters[i].Name}");
            if (i != parameters.Length - 1)
                scriptBuilder.AppendLine(", ");
        }
        
        scriptBuilder.AppendLine().AppendLine(")");

        if (sourceMethod.ReturnType != typeof(void))
            scriptBuilder.Append("$returnValue = ");

        // Generate C# method call
        scriptBuilder.Append($"[{sourceMethod.ReflectedType?.FullName}]::{sourceMethod.Name}(");
        for (var i = 0; i < parameters.Length; i++)
        {
            scriptBuilder.Append($"${parameters[i].Name}");
            if (i != parameters.Length - 1)
                scriptBuilder.Append(", ");
        }
        scriptBuilder.AppendLine(")");

        if (sourceMethod.ReturnType != typeof(void))
            scriptBuilder.AppendLine("return $returnValue");

        scriptBuilder.Append('}');
        
        return scriptBuilder.ToString();
    }
}