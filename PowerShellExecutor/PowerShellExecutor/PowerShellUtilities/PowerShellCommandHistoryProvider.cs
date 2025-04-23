using System.IO;
using PowerShellExecutor.Interfaces;

namespace PowerShellExecutor.PowerShellUtilities;

/// <summary>
/// Manages a history of executed PowerShell commands, and allows navigation through the history
/// </summary>
public class PowerShellCommandHistoryProvider : IHistoryProvider<string>
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
    
    private readonly string? _historyFilePath;
    private int _sessionHistoryStartIndex = 0;

    /// <summary>
    /// Create an instance of <see cref="PowerShellCommandHistoryProvider"/>
    /// </summary>
    public PowerShellCommandHistoryProvider()
    {
        
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PowerShellCommandHistoryProvider"/> class and loads existing
    /// command history from the specified file
    /// </summary>
    /// <param name="filePath">The path to the file used for storing and loading command history</param>
    /// <remarks>
    /// If the file does not exist, it is created as an empty file. If the file cannot be read due to an I/O error,
    /// a warning is written to standard error, and the command history will not be persisted during this session
    /// </remarks>
    public PowerShellCommandHistoryProvider(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                File.Create(filePath).Dispose();

            using var sr = new StreamReader(filePath);
            
            while (!sr.EndOfStream)
            {
                var command = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(command)) continue;
                
                _history.Add(command);
            }
            
            MoveToStart();
            _sessionHistoryStartIndex = _history.Count + 1;
            _historyFilePath = filePath;
        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"WARNING: Failed to load the command history from '{filePath}': {e.Message}");
            Console.Error.WriteLine("Command history will not be saved or loaded.");
            _history.Clear();
            _historyFilePath = null;
        }
    }

    public int Count => _history.Count;

    /// <summary>
    /// Adds a command to the history if it's not empty and not a duplicate of the last entry.
    /// Moves the navigation to the start of the history
    /// </summary>
    /// <param name="command">The command to add to history</param>
    public void AddEntry(string command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if ((_history.Count == 0 || _history[^1] != command) && command.Length > 0)
            _history.Add(command);
        
        MoveToStart();
    }
    
    /// <summary>
    /// Retrieves the next command in the history relative to the last read command,
    /// or the first command, if positioned at the start of the history.
    /// </summary>
    /// <returns>The next command, or <c>null</c> if unavailable</returns>
    public string? NextEntry()
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
    /// or the last command, if positioned at the end of the history.
    /// </summary>
    /// <returns>The previous command, or <c>null</c> if unavailable</returns>
    public string? PrevEntry()
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
    /// Saves the current command history to the associated history file, if available
    /// </summary>
    /// <remarks>
    /// If an I/O error occurs while writing to the file the application continues without crashing.
    /// </remarks>
    public void SaveHistory()
    {
        if (_historyFilePath is null) return;

        try
        {
            File.WriteAllLines(_historyFilePath, _history);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine($"WARNING: Failed to save command history: {e.Message}");
        }
    }
    
    public bool IsEmpty() => _history.Count == 0;

    public void MoveToStart() => _lastFetchedIndex = _history.Count;
}