using System;
using Avalonia.Controls;
using LogAnalyzerApp.ViewModels;

namespace LogAnalyzerApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        this.Opened += (_, _) =>
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.RequestChartsRefresh += () =>
                {
                    SeverityChart.IsVisible = false;
                    SeverityChart.IsVisible = true;
                    
                    SourceChart.IsVisible = false;
                    SourceChart.IsVisible = true;
                };
            }
        };
    }
}