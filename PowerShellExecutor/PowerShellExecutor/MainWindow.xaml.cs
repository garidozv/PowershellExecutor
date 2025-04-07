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
        _viewModel = new MainWindowViewModel(_powerShellService, new CommandHistory(),
            () => Dispatcher.Invoke(Close), CommandResultRichTextBox);
        
        DataContext = _viewModel.Bindings;
        CommandInputTextBox.Focus();
    }
    
    protected override async void OnClosed(EventArgs e)
    {
        await _viewModel.Cleanup();
        _powerShellService.Dispose();
        base.OnClosed(e);
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