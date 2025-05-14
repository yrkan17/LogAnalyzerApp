using System;

namespace LogAnalyzerApp.Models;

public class SyslogEntry
{
    public DateTime Timestamp { get; set; }
    public string? Host { get; set; }
    public string? Source { get; set; }
    public string? Message { get; set; }
    public string? Severity { get; set; }
}