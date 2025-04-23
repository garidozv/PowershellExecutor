using System.Collections.Immutable;
using System.IO;
using System.Management.Automation;
using PowerShellExecutor.Helpers;
using PowerShellExecutor.Interfaces;

namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// Provides PowerShell command completion functionality. Implements the <see cref="ICompletionProvider{TInput, TCompletion}"/>
/// interface to retrieve command completions based on user input and cursor position
/// </summary>
public class PowerShellCompletionProvider : ICompletionProvider<string, string>
{
    private readonly IPowerShell _powerShell;

    /// <summary>
    /// Creates an instance of <see cref="PowerShellCompletionProvider"/>
    /// </summary>
    /// <param name="powerShell">The <see cref="IPowerShell"/> instance to use for command completion</param>
    public PowerShellCompletionProvider(IPowerShell powerShell)
    {
        _powerShell = powerShell;
    }
    
    /// <summary>
    /// Retrieves the list of command completions based on the given input and cursor position
    /// </summary>
    /// <param name="input">The input string for which completions are to be generated</param>
    /// <param name="position">The cursor position in the input string</param>
    /// <returns>A <see cref="CommandCompletion"/> object containing the available completions</returns>
    public IReadOnlyList<CompletionElement<string>> GetCompletions(string input, int position)
    {
        var completions = _powerShell.GetCommandCompletion(input, position);
        return GenerateCompletionStrings(input, completions);
    }

    /// <summary>
    /// Generates a collection of PowerShell commands completions based on the provided input
    /// </summary>
    /// <param name="input">The user-provided input string for which completions are being generated</param>
    /// <param name="completion">The <see cref="CommandCompletion"/> object containing PowerShell completions</param>
    /// <returns>
    /// A read-only list of <see cref="CompletionElement{T}"/> objects representing the generated completions
    /// </returns>
    private IReadOnlyList<CompletionElement<string>> GenerateCompletionStrings(string input,
        CommandCompletion completion) =>
        completion.CompletionMatches.Select(match =>
        {
            var completionText = match.CompletionText;
            
            /*
             * PowerShell automatically appends directory separator at the end of directory completions.
             * Directory completions have the ProviderContainer completion result type, but they
             * are not the only completion type to have it, so we have to perform additional
             * check to make sure that the completion really represents a directory, after which
             * we can append the directory separator.
             */
            if (match.ResultType == CompletionResultType.ProviderContainer &&
                IsDirectoryCompletion(match.CompletionText) &&
                !match.CompletionText.EndsWith(Path.DirectorySeparatorChar))
                completionText += Path.DirectorySeparatorChar;

            return new CompletionElement<string>()
            {
                Completion = input.ReplaceSegment(
                    completion.ReplacementIndex,
                    completion.ReplacementLength,
                    completionText
                ),
                Position = completion.ReplacementIndex + completionText.Length,
            };
        }).ToImmutableArray();

    /// <summary>
    /// Determines whether the given completion corresponds to a directory path.
    /// If the provided completion is a relative path, it is combined with the
    /// current working directory to form the full path.
    /// </summary>
    /// <param name="completion">The completion string to check</param>
    /// <returns><c>true</c> if the completion corresponds to a directory; otherwise, <c>false</c>.</returns>
    private bool IsDirectoryCompletion(string completion)
    {
        ArgumentNullException.ThrowIfNull(completion);
        
        var fullPath = Path.IsPathRooted(completion) ? completion : Path.Combine(_powerShell.WorkingDirectoryPath, completion);
        return Directory.Exists(fullPath);
    }
}