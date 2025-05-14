using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogAnalyzerApp.Models;
using LogAnalyzerApp.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

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

    public MainWindowViewModel()
    {
        SelectedSeverity = SeverityOptions.First();
        LoadLog(); // автоматическая загрузка при запуске
    }

    [RelayCommand]
    private void LoadLog()
    {
        if (File.Exists(logFilePath))
        {
            var parsed = _parser.ParseSyslog(logFilePath);
            parsed.Reverse();
            allEntries = new ObservableCollection<SyslogEntry>(parsed);
            ApplyFilters();
            UpdateAllCharts();
        }
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
            .GroupBy(e => e.Source)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Source = g.Key ?? "Неизвестно", Count = g.Count() })
            .ToList();

        SourceSeries = grouped
            .Select(g => new PieSeries<double>
            {
                Values = new[] { (double)g.Count },
                Name = g.Source
            })
            .ToArray();
    }

}
