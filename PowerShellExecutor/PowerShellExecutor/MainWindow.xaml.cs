﻿using System.Windows;
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
        _viewModel = new MainWindowViewModel(_powerShellService, new CommandHistory())
        {
            CloseWindowAction = () => Dispatcher.Invoke(Close),
            FocusInputTextBoxAction = () => Dispatcher.Invoke(() => CommandInputTextBox.Focus()),
            FocusResultTextBoxAction = () => Dispatcher.Invoke(() => CommandResultRichTextBox.Focus()),
            CommandResultRichTextBox = CommandResultRichTextBox
        };
        
        DataContext = _viewModel.Bindings;
        CommandInputTextBox.Focus();
    }

    protected override async void OnClosed(EventArgs e)
    {
        await _viewModel.Cleanup();
        _powerShellService.Dispose();
        base.OnClosed(e);
    }
}