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
    
    public MainWindow()
    {
        InitializeComponent();
        
        _powerShellService = new PowerShellService();
        var viewModel = new MainWindowViewModel(_powerShellService, new CommandHistory());
        
        viewModel.CloseWindowAction = new Action(() => Dispatcher.Invoke(Close));
        viewModel.FocusInputTextBoxAction = new Action(() => Dispatcher.Invoke(() => CommandInputTextBox.Focus()));
        viewModel.FocusResultTextBoxAction = new Action(() => Dispatcher.Invoke(() => CommandResultTextBox.Focus()));
        
        
        DataContext = viewModel;
        CommandInputTextBox.Focus();
    }

    protected override void OnClosed(EventArgs e)
    {
        _powerShellService.Dispose();
        base.OnClosed(e);
    }
}