using LogAnalyzerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using LoggingLibrary;

namespace LogAnalyzerApp.Services;

/// <summary>
/// Парсер системнвх логов
/// </summary>
public class SyslogParserService
{
    private static readonly Regex SyslogRegex = new Regex(
        @"^(?<date>\w{3}\s+\d{1,2}\s+\d{2}:\d{2}:\d{2})\s+(?<host>\S+)\s+(?<source>[^\[:]+)(?:\[\d+\])?:\s(?<message>.+)$",
        RegexOptions.Compiled);

    /// <summary>
    /// Парсит файл расположенный по пути
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Список логов</returns>
    public List<SyslogEntry> ParseSyslog(string filePath)
    {
        var entries = new List<SyslogEntry>();

        foreach (var line in File.ReadLines(filePath))
        {
            var match = SyslogRegex.Match(line);
            if (match.Success)
            {
                try
                {
                    var dateString = match.Groups["date"].Value;
                    var timestamp = ParseSyslogDate(dateString);
                    var entry = new SyslogEntry
                    {
                        Timestamp = timestamp,
                        Host = match.Groups["host"].Value,
                        Source = match.Groups["source"].Value,
                        Message = match.Groups["message"].Value
                    };
                    entry.Severity = entry.Message.Contains("error", StringComparison.OrdinalIgnoreCase) ? "error"
                        : entry.Message.Contains("warn", StringComparison.OrdinalIgnoreCase) ? "warning"
                        : "info";
                    entries.Add(entry);
                }
                catch
                {
                    // Пропуск строки при ошибке разбора
                    SimpleLogger.Instance.Error("Ошибка парсинга строки логов");
                }
            }
        }

        return entries;
    }

    private DateTime ParseSyslogDate(string dateStr)
    {
        var currentYear = DateTime.Now.Year;
        var fullDateStr = $"{dateStr} {currentYear}";
        return DateTime.ParseExact(fullDateStr, "MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture);
    }
}
