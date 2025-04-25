namespace PowerShellExecutor.Interfaces;

/// <summary>
/// Represents a dispatcher interface used to execute code.
/// Its main purpose is to enable asier testing of the code that uses dispatcher for UI-related tasks
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Executes the specified callback on the dispatcher
    /// </summary>
    /// <param name="callback">The callback action to be executed</param>
    void Invoke(Action callback);
}