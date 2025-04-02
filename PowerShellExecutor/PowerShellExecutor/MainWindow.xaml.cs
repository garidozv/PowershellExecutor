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
        DataContext = new MainWindowViewModel(
            _powerShellService, this, new CommandHistory());
    }

    public void SetCommandResultForeground(Brush brush) => 
        CommandResultTextBox.Foreground = brush;

    public void CloseMainWindow() => Close();

    public void SetCommandInputCaretIndex(int? index = null) =>
        CommandInputTextBox.CaretIndex = index ?? CommandInputTextBox.Text.Length;

    public int GetCommandInputCaretIndex() => CommandResultTextBox.CaretIndex;

    protected override void OnClosed(EventArgs e)
    {
        _powerShellService.Dispose();
        base.OnClosed(e);
    }
}