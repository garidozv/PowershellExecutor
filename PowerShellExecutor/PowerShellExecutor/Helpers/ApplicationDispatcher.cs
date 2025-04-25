using System.Windows;
using PowerShellExecutor.Interfaces;

namespace PowerShellExecutor.Helpers;

/// <summary>
/// Provides an implementation of the <see cref="IDispatcher"/> interface by using the
/// application's main dispatcher to execute code on the UI thread
/// </summary>
public class ApplicationDispatcher : IDispatcher
{
    public void Invoke(Action callback) => Application.Current.Dispatcher.Invoke(callback);
}