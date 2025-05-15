using Avalonia.Controls;
using TextCopy;
using LogAnalyzerApp.Models;
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

                    TimeFrequencyChart.IsVisible = false;
                    TimeFrequencyChart.IsVisible =  true;
                };
            }
        };
    }
    
    private async void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ListBox listBox && listBox.SelectedItem is SyslogEntry entry)
        {
            ClipboardService.SetText(entry.Message);
            
        }
    }
}