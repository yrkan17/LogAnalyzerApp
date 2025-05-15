using System;

namespace LogAnalyzerApp.Models;

/// <summary>
/// Модель лога
/// </summary>
public class SyslogEntry
{
    /// <summary>
    /// Время
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Хост
    /// </summary>
    public string? Host { get; set; }
    
    /// <summary>
    /// Источник
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// Сообщение
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Тип лога
    /// </summary>
    public string? Severity { get; set; }
}