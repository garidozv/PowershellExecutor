using System.Windows.Media;
using System.Windows;
using PowerShellExecutor.PowerShellUtilities;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly PowerShellService _powerShellService;
    private readonly MainWindowViewModel _viewModel;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _powerShellService = new PowerShellService();
        _viewModel = new MainWindowViewModel(_powerShellService, new CommandHistory());
        
        _viewModel.CloseWindowAction = new Action(() => Dispatcher.Invoke(Close));
        _viewModel.FocusInputTextBoxAction = new Action(() => Dispatcher.Invoke(() => CommandInputTextBox.Focus()));
        _viewModel.FocusResultTextBoxAction = new Action(() => Dispatcher.Invoke(() => CommandResultTextBox.Focus()));
        
        DataContext = _viewModel;
        CommandInputTextBox.Focus();
    }

    protected override async void OnClosed(EventArgs e)
    {
        await _viewModel.Cleanup();
        _powerShellService.Dispose();
        base.OnClosed(e);
    }
}