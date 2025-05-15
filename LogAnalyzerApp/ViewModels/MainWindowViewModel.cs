using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogAnalyzerApp.Models;
using LogAnalyzerApp.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LoggingLibrary;

namespace LogAnalyzerApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SyslogParserService _parser = new();

    private readonly string logFilePath = "/var/log/syslog"; // путь к syslog

    public ObservableCollection<LogSeverityFilter> SeverityOptions { get; } = new()
    {
        new LogSeverityFilter { DisplayName = "Все", SeverityValue = null },
        new LogSeverityFilter { DisplayName = "Информация", SeverityValue = "info" },
        new LogSeverityFilter { DisplayName = "Предупреждение", SeverityValue = "warning" },
        new LogSeverityFilter { DisplayName = "Ошибка", SeverityValue = "error" }
    };

    [ObservableProperty]
    private LogSeverityFilter selectedSeverity;

    [ObservableProperty]
    private string searchText = string.Empty;

    private ObservableCollection<SyslogEntry> allEntries = new(); // все данные

    [ObservableProperty]
    private ObservableCollection<SyslogEntry> entries = new(); // отображаемые данные

    [ObservableProperty]
    private ISeries[] severitySeries;
    
    [ObservableProperty]
    private ISeries[] sourceSeries;
    
    [ObservableProperty]
    private ISeries[] timeFrequencySeries;

    [ObservableProperty]
    private Axis[] timeFrequencyXAxes;

    [ObservableProperty]
    private Axis[] timeFrequencyYAxes;

    public MainWindowViewModel()
    {
        SelectedSeverity = SeverityOptions.First();
        LoadLogCommand.Execute(null); // автоматическая загрузка при запуске
    }

    [RelayCommand]
    private async Task LoadLog()
    {
        if (File.Exists(logFilePath))
        {
            var parsed = await Task.Run(() =>
            {
                var result = _parser.ParseSyslog(logFilePath);
                result.Reverse();
                return result;
            });
            
            allEntries = new ObservableCollection<SyslogEntry>(parsed);
            ApplyFilters();
            UpdateAllCharts();
            
            SimpleLogger.Instance.Info("Логи загружены");
        }
    }
    
    [RelayCommand]
    private void SearchTextLostFocus()
    {
        UpdateAllCharts();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
        UpdateAllCharts();
    }

    partial void OnSelectedSeverityChanged(LogSeverityFilter value)
    {
        ApplyFilters();
        UpdateAllCharts();
    }

    private void ApplyFilters()
    {
        var filtered = allEntries.Where(entry =>
            (SelectedSeverity?.SeverityValue == null || entry.Severity == SelectedSeverity.SeverityValue) &&
            (string.IsNullOrWhiteSpace(SearchText) ||
             (entry.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
              entry.Source?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true))
        );

        Entries = new ObservableCollection<SyslogEntry>(filtered);
    }
    public event Action? RequestChartsRefresh;

    private void UpdateAllCharts()
    {
        UpdateSeverityChart();
        UpdateSourceChart();
        UpdateTimeFrequencyChart();
        RequestChartsRefresh?.Invoke();
    }
    
    private void UpdateSeverityChart()
    {
        var errorCount = Entries.Count(e => e.Severity == "error");
        var warningCount = Entries.Count(e => e.Severity == "warning");
        var infoCount = Entries.Count(e => e.Severity == "info");
        
        SeveritySeries = new ISeries[]
        {
            new PieSeries<double> { Values = new[] { (double)errorCount }, Name = "Ошибки" },
            new PieSeries<double> { Values = new[] { (double)warningCount }, Name = "Предупреждения" },
            new PieSeries<double> { Values = new[] { (double)infoCount }, Name = "Информация" },
        };
    }

    private void UpdateSourceChart()
    {
        var grouped = Entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Source))
            .GroupBy(e => e.Source!.Trim())
            .Where(g => g.Count() > 1)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Source = g.Key.Length > 20 ? g.Key.Substring(0, 20) + "..." : g.Key, Count = g.Count() })
            .ToList();

        SourceSeries = grouped
            .Select(g => new PieSeries<double>
            {
                Values = new[] { (double)g.Count },
                Name = g.Source
            })
            .ToArray();
    }
    
    private void UpdateTimeFrequencyChart()
    {
        var grouped = Entries
            .GroupBy(e => e.Timestamp.Hour)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        var labels = Enumerable.Range(0, 24).Select(h => h.ToString("D2") + ":00").ToArray();
        var values = Enumerable.Range(0, 24).Select(h => grouped.ContainsKey(h) ? grouped[h] : 0).Select(v => (double)v).ToArray();

        TimeFrequencySeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = values,
                Name = "Логи по часам"
            }
        };

        TimeFrequencyXAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = 45,
                TextSize = 10
                
            }
        };

        
        

        TimeFrequencyYAxes = new Axis[]
        {
            new Axis
            {
                TextSize = 10
            }
        };
    }

}
