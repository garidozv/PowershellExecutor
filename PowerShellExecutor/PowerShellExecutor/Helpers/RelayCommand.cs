using System.Windows.Input;

namespace PowershellExecutor.Helpers;

/// <summary>
/// Represents a command that can be executed, with an optional condition for whether it can be executed
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool>? _canExecute;
    
    /// <summary>
    /// Occurs when the ability to execute the command has changed
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Initializes a new instance of the RelayCommand class
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked</param>
    /// <param name="canExecute">An optional function that determines if the command can be executed</param>
    public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    /// <summary>
    /// Determines if the command can be executed based on the current conditions
    /// </summary>
    /// <param name="parameter">The parameter to pass to the canExecute condition</param>
    /// <returns>true if the command can be executed; otherwise, false</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Executes the command
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execute action</param>
    public void Execute(object? parameter)
    {
        _execute.Invoke(parameter);
    }
}