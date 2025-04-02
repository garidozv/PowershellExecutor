namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// Manages a history of executed commands, and allows navigation through the history
/// </summary>
public class CommandHistory
{
    /*
     * Easy way to visualize the way the iteration works is like you have two dummy
     * entries at the beginning and at the end, at indexes _history.Count and -1 respectfully.
     * 
     * To reach the last dummy entry, NextCommand needs to be called after fetching the
     * last command, and to reach the first dummy entry, either MoveToStart needs to be
     * called or PrevCommand needs to be called after fetching the first command.
     * Moving to these dummy entries, causes NextCommand and PrevCommand to return null.
     *
     * If PrevCommand is called while positioned at the last dummy entry, the last entry
     * will be returned, and if NextCommand is called while positioned at the first dummy
     * entry, the first entry will be returned.
     */
    
    private readonly List<string> _history = [];
    private int _lastFetchedIndex;
    
    /// <summary>
    /// Gets the number of commands contained in <see cref="CommandHistory"/>
    /// </summary>
    public int Count => _history.Count;

    /// <summary>
    /// Adds a command to the history if it's not empty and not a duplicate of the last entry.
    /// Moves the navigation to the start of the history
    /// </summary>
    /// <param name="command">The command to add to history</param>
    public void AddCommand(string command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if ((_history.Count == 0 || _history[^1] != command) && command.Length > 0)
            _history.Add(command);
        
        MoveToStart();
    }
    
    /// <summary>
    /// Retrieves the next command in the history relative to the last read command,
    /// or the last command, if positioned at the end of the history.
    /// </summary>
    /// <returns>The next command, or <c>null</c> if unavailable</returns>
    public string? NextCommand()
    {
        if (IsEmpty() || _lastFetchedIndex == -1) 
            return null;

        if (_lastFetchedIndex == 0)
        {
            _lastFetchedIndex = -1;
            return null;
        }
        
        return _history[--_lastFetchedIndex];
    }

    /// <summary>
    /// Retrieves the previous command in the history relative to the last read command,
    /// or the first command, if positioned at the start of the history.
    /// </summary>
    /// <returns>The previous command, or <c>null</c> if unavailable</returns>
    public string? PrevCommand()
    {
        if (IsEmpty() || _lastFetchedIndex == _history.Count) 
            return null;

        if (_lastFetchedIndex == _history.Count - 1)
        {
            _lastFetchedIndex = _history.Count;
            return null;
        }
   
        return _history[++_lastFetchedIndex];
    }
    
    /// <summary>
    /// Checks if the history is empty
    /// </summary>
    /// <returns><c>true</c> if history contains no commands, otherwise <c>false</c></returns>
    public bool IsEmpty() => _history.Count == 0;

    /// <summary>
    /// Sets the current position to the start of the history
    /// </summary>
    public void MoveToStart() => _lastFetchedIndex = _history.Count;
}