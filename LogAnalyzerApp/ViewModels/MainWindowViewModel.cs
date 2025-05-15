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

/// <summary>
/// Основная модель представления для главного окна приложения.
/// Содержит данные, команды и логику отображения/фильтрации/обновления логов и графиков.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SyslogParserService _parser = new();

    private readonly string logFilePath = "/var/log/syslog"; // путь к syslog

    /// <summary>
    /// Доступные фильтры по уровню серьезности логов.
    /// </summary>
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

    /// <summary>
    /// Все загруженные записи логов.
    /// </summary>
    private ObservableCollection<SyslogEntry> allEntries = new();

    /// <summary>
    /// Отфильтрованные записи, отображаемые в UI.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SyslogEntry> entries = new();

    /// <summary>
    /// Серии данных для круговой диаграммы по серьезности логов.
    /// </summary>
    [ObservableProperty]
    private ISeries[] severitySeries;

    /// <summary>
    /// Серии данных для круговой диаграммы по источникам логов.
    /// </summary>
    [ObservableProperty]
    private ISeries[] sourceSeries;

    /// <summary>
    /// Серии данных для столбчатой диаграммы по часам.
    /// </summary>
    [ObservableProperty]
    private ISeries[] timeFrequencySeries;

    /// <summary>
    /// Оси X для диаграммы частоты логов по времени.
    /// </summary>
    [ObservableProperty]
    private Axis[] timeFrequencyXAxes;

    /// <summary>
    /// Оси Y для диаграммы частоты логов по времени.
    /// </summary>
    [ObservableProperty]
    private Axis[] timeFrequencyYAxes;

    /// <summary>
    /// Конструктор. Устанавливает значения по умолчанию и загружает логи.
    /// </summary>
    public MainWindowViewModel()
    {
        SelectedSeverity = SeverityOptions.First();
        LoadLogCommand.Execute(null); // автоматическая загрузка при запуске
    }

    /// <summary>
    /// Загружает лог-файл и парсит его в фоне.
    /// </summary>
    [RelayCommand]
    private async Task LoadLog()
    {
        if (File.Exists(logFilePath))
        {
            var parsed = await Task.Run(() =>
            {
                var result = _parser.ParseSyslog(logFilePath);
                result.Reverse();
                SimpleLogger.Instance.Info("Логи загружены");
                return result;
            });

            allEntries = new ObservableCollection<SyslogEntry>(parsed);
            ApplyFilters();
            UpdateAllCharts();
        }
    }

    /// <summary>
    /// Обновляет данные при изменении текста поиска.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
        UpdateAllCharts();
    }

    /// <summary>
    /// Обновляет данные при изменении выбранного фильтра серьезности.
    /// </summary>
    partial void OnSelectedSeverityChanged(LogSeverityFilter value)
    {
        ApplyFilters();
        UpdateAllCharts();
    }

    /// <summary>
    /// Применяет фильтрацию по серьезности и тексту.
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = allEntries.Where(entry =>
            (SelectedSeverity?.SeverityValue == null || entry.Severity == SelectedSeverity.SeverityValue) &&
            (string.IsNullOrWhiteSpace(SearchText) ||
             (entry.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
              entry.Source?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true))
        );

        Entries = new ObservableCollection<SyslogEntry>(filtered);
        SimpleLogger.Instance.Info("Фмльтр применен");
    }

    /// <summary>
    /// Событие запроса на перерисовку графиков.
    /// </summary>
    public event Action? RequestChartsRefresh;

    /// <summary>
    /// Обновляет все графики и вызывает событие перерисовки.
    /// </summary>
    private void UpdateAllCharts()
    {
        UpdateSeverityChart();
        UpdateSourceChart();
        UpdateTimeFrequencyChart();
        RequestChartsRefresh?.Invoke();
        SimpleLogger.Instance.Info("Графики обновлены");
    }

    /// <summary>
    /// Обновляет круговую диаграмму по уровням серьезности логов.
    /// </summary>
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

    /// <summary>
    /// Обновляет круговую диаграмму по источникам логов.
    /// </summary>
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

    /// <summary>
    /// Обновляет столбчатую диаграмму частоты логов по часам.
    /// </summary>
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
