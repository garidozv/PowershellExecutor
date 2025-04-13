using System.IO;
using System.Reflection;
using System.Windows;
using PowerShellExecutor.PowerShellUtilities;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string AppName = "PowerShellExecutor";
    private const string CommandHistoryFileName = "command_history.txt";
    
    private readonly PowerShellWrapper _powerShellWrapper;
    private readonly MainWindowViewModel _viewModel;
    private readonly CommandHistory _commandHistory;

    public MainWindow()
    {
        InitializeComponent();
        
        var commandHistoryFilePath = GetCommandHistoryFilePath();
        _commandHistory = new CommandHistory(commandHistoryFilePath);
        
        _powerShellWrapper = new PowerShellWrapper();
        var powerShellService = new PowerShellService(_powerShellWrapper);
        _viewModel = new MainWindowViewModel(powerShellService, _commandHistory, () => Dispatcher.Invoke(Close));

        DataContext = _viewModel;
        CommandInputTextBox.Focus();
    }
    
    protected override async void OnClosed(EventArgs e)
    {
        _commandHistory.SaveHistory();
        await _viewModel.Cleanup();
        _powerShellWrapper.Dispose();
        base.OnClosed(e);
    }

    /// <summary>
    /// Gets the file path to command history file
    /// </summary>
    /// <returns>The file path to command history file</returns>
    /// <remarks>
    /// This method creates an app data directory for this app if it doesn't already exist
    /// </remarks>
    private static string GetCommandHistoryFilePath()
    {
        var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? AppName;
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            appName);
        
        Directory.CreateDirectory(appDataFolder);

        return Path.Combine(appDataFolder, CommandHistoryFileName);
    }

    /*
     * This approach doesn't follow MVVM, but was the only way to make swapping
     * focus between CommandInputTextBox and ReadTextBox work as intended
     */
    private void ReadTextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            ReadTextBox.Focus();
        }
        else
        {
            CommandInputTextBox.Focus();
        }
    }
}