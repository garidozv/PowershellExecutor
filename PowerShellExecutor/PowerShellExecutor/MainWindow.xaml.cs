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
        viewModel.CloseWindowAction = Close;
        
        DataContext = viewModel;
    }

    protected override void OnClosed(EventArgs e)
    {
        _powerShellService.Dispose();
        base.OnClosed(e);
    }
}