using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PowerShellExecutor.Helpers;

/// <summary>
/// Represents an asynchronous command interface that extends <see cref="ICommand"/>
/// Defines a contract for commands that execute asynchronously
/// </summary>
public interface IAsyncCommand : ICommand
{
    /// <summary>
    /// Executes an asynchronous operation associated with the command.
    /// </summary>
    /// <param name="parameter"> An optional parameter to be passed to the command during execution </param>
    /// <returns> A task representing the asynchronous operation </returns>
    Task ExecuteAsync(object parameter);
}

/// <summary>
/// Represents an asynchronous command that can be executed, with an optional condition for whether it can be executed
/// </summary>
/// <seealso cref="IAsyncCommand"/>
public class AsyncRelayCommand : IAsyncCommand
{
    private bool _isExecuting;
    private readonly Func<object, Task> _executeAsync;
    private readonly Predicate<object>? _canExecute;

    /// <summary>
    /// Gets the <see cref="Dispatcher"/> associated with the application.
    /// </summary>
    private Dispatcher Dispatcher { get; }

    /// <summary>
    /// Occurs when changes affect whether the command can execute.
    /// This event is typically raised to notify when the current state of the command's executability has changed
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class
    /// </summary>
    /// <param name="executeAsync">The action to execute asynchronously when the command is invoked</param>
    /// <param name="canExecute">An optional function that determines if the command can be executed</param>
    public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
        Dispatcher = Application.Current.Dispatcher;
    }

    /// <summary>
    /// Forces the <see cref="CommandManager"/> to raise the RequerySuggested event,
    /// which notifies bound commands to reevaluate their executability status
    /// </summary>
    /// <remarks>
    /// This method ensures that the UI is updated to reflect changes in the command's
    /// CanExecute state when triggered. If invoked from a thread other than the UI thread,
    /// this method dispatches the update call to the UI thread
    /// </remarks>
    private void InvalidateRequerySuggested()
    {
        if (Dispatcher.CheckAccess())
            CommandManager.InvalidateRequerySuggested();
        else
            Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
    }

    /// <summary>
    /// Determines if the command can be executed based on the current conditions
    /// </summary>
    /// <param name="parameter">An optional parameter used by the command</param>
    /// <returns>Returns <c>true</c> if the command can execute; otherwise, returns <c>false</c></returns>
    public bool CanExecute(object parameter) => !_isExecuting && (_canExecute == null || _canExecute(parameter));

    /// <summary>
    /// Executes the asynchronous operation associated with the command.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execute action</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation</returns>
    public async Task ExecuteAsync(object parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                InvalidateRequerySuggested();
                await _executeAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                _isExecuting = false;
                InvalidateRequerySuggested();
            }
        }
    }

    /// Executes the command with the specified parameter. This is a synchronous wrapper
    /// for the asynchronous execution, primarily used to trigger the execution of the
    /// asynchronous task defined in ExecuteAsync method.
    /// <param name="parameter">
    /// The parameter passed to the command execution.
    /// </param>
    public void Execute(object parameter) => _ = ExecuteAsync(parameter);
}