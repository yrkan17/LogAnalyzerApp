using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LogAnalyzerApp.Models;
using LogAnalyzerApp.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LogAnalyzerApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SyslogParserService _parser = new();

    private readonly string logFilePath = "/var/log/syslog"; // путь к syslog

    [ObservableProperty]
    private ObservableCollection<SyslogEntry> entries = new ObservableCollection<SyslogEntry>();

    // Новый текст для вывода в TextBlock
    [ObservableProperty]
    private string entriesText;

    public MainWindowViewModel()
    {
        LoadLog(); // автоматическая загрузка при запуске
    }

    [RelayCommand]
    private void LoadLog()
    {
        if (File.Exists(logFilePath))
        {
            var parsed = _parser.ParseSyslog(logFilePath);
            Entries = new ObservableCollection<SyslogEntry>(parsed);
            UpdateEntriesText();
        }
    }

    private void UpdateEntriesText()
    {
        // Обновим EntriesText на основе данных в Entries
        EntriesText = string.Join("\n", Entries.Select(e => $"{e.Timestamp} - {e.Host} - {e.Source} - {e.Message}"));
    }
}
