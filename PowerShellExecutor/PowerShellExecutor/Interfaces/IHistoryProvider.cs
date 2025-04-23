namespace PowerShellExecutor.Interfaces;

/// <summary>
/// Represents a provider for managing and navigating through a history of entries
/// </summary>
/// <typeparam name="T">The type of entries stored in the history</typeparam>
public interface IHistoryProvider<T>
{
    /// <summary>
    /// Gets the number of entries currently stored in the history
    /// </summary>
    /// <value>
    /// An integer representing the total count of entries in the history
    /// </value>
    public int Count { get; }

    /// <summary>
    /// Adds an entry to the history
    /// </summary>
    /// <param name="command">The entry to add to history</param>
    public void AddEntry(T entry);

    /// <summary>
    /// Retrieves the next item from the history relative to the last read entry,
    /// or the first entry, if positioned at the start of the history
    /// </summary>
    /// <returns>The next entry, or <c>null</c> if unavailable</returns>
    public T? NextEntry();

    /// <summary>
    /// Retrieves the previous item from the history relative to the last read entry,
    /// or the last entry, if positioned at the end of the history.
    /// </summary>
    /// <returns>The previous command, or <c>null</c> if unavailable</returns>
    public T? PrevEntry();

    /// <summary>
    /// Determines whether the history is empty
    /// </summary>
    /// <returns><c>true</c> if the history contains no entries; otherwise, <c>false</c>.</returns>
    public bool IsEmpty();

    /// <summary>
    /// Sets the current position to the start of the history
    /// </summary>
    public void MoveToStart();
}