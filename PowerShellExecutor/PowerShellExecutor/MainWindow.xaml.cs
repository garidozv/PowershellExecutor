using System.Windows.Media;
using System.Windows;
using PowerShellExecutor.Interfaces;
using PowerShellExecutor.PowerShellUtilities;
using PowerShellExecutor.ViewModels;

namespace PowerShellExecutor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IMainWindow
{
    private readonly PowerShellService _powerShellService;
    
    public MainWindow()
    {
        InitializeComponent();

        _powerShellService = new PowerShellService();
        DataContext = new MainWindowViewModel(_powerShellService, this);
    }

    public void SetCommandResultForeground(Brush brush) => CommandResultTextBox.Foreground = brush;

    public void CloseMainWindow() => Close();

    protected override void OnClosed(EventArgs e)
    {
        _powerShellService.Dispose();
        base.OnClosed(e);
    }
}