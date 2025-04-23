namespace PowerShellExecutor.Interfaces;

/// <summary>
/// Represents an individual completion suggestion with its associated content and position
/// </summary>
/// <typeparam name="T">The type of the suggested completion content</typeparam>
public readonly struct CompletionElement<T>
{
    public T Completion { get; init; }
    public int Position { get; init; }
}

/// <summary>
/// Provides a mechanism to retrieve a collection of completion suggestions
/// based on a specified input and position
/// </summary>
/// <typeparam name="TInput">The type of the input which is used to generate completions</typeparam>
/// <typeparam name="TCompletion">The type of individual completions that are provided</typeparam>
public interface ICompletionProvider<in TInput, TCompletion>
{
    /// <summary>
    /// Retrieves a collection of completions for the given input and position
    /// </summary>
    /// <param name="input">The input value for which completions are generated</param>
    /// <param name="position">The position within the input to be considered for generating completions</param>
    /// <returns>
    /// A read-only list of <see cref="CompletionElement{TCompletion}"/> containing the generated completion suggestions.
    /// </returns>
    IReadOnlyList<CompletionElement<TCompletion>> GetCompletions(TInput input, int position);
}