using LogAnalyzerApp.Services;
using System.IO;

namespace LogAnalyzerApp.Test;

public class SyslogParserServiceTests
{
    /// <summary>
    /// Проверяет, что парсер корректно разбирает строку syslog стандартного формата.
    /// </summary>
    [Fact]
    public void ParseSyslog_ParsesCorrectLine()
    {
        // Arrange — создаём временный файл с корректной строкой лога
        var parser = new SyslogParserService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "May 15 12:34:56 myhost myapp: System started");

        // Act — парсим содержимое файла
        var result = parser.ParseSyslog(tempFile);

        // Assert — убеждаемся, что результат корректный
        Assert.Single(result); // только одна запись
        var entry = result[0];
        Assert.Equal("myhost", entry.Host);
        Assert.Equal("myapp", entry.Source);
        Assert.Equal("System started", entry.Message);
        Assert.Equal("info", entry.Severity); // сообщение без ошибок — должна быть info
    }

    /// <summary>
    /// Проверяет, что парсер устанавливает уровень серьезности "error", если сообщение содержит "error".
    /// </summary>
    [Fact]
    public void ParseSyslog_SetsSeverityToError()
    {
        var parser = new SyslogParserService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "May 15 12:34:56 host app: Disk error occurred");

        var result = parser.ParseSyslog(tempFile);

        Assert.Single(result);
        Assert.Equal("error", result[0].Severity);
    }

    /// <summary>
    /// Проверяет, что парсер устанавливает уровень серьезности "warning", если сообщение содержит "warn".
    /// </summary>
    [Fact]
    public void ParseSyslog_SetsSeverityToWarning()
    {
        var parser = new SyslogParserService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "May 15 12:34:56 host app: Low memory warning");

        var result = parser.ParseSyslog(tempFile);

        Assert.Single(result);
        Assert.Equal("warning", result[0].Severity);
    }

    /// <summary>
    /// Проверяет, что парсер игнорирует строку, не соответствующую шаблону syslog.
    /// </summary>
    [Fact]
    public void ParseSyslog_IgnoresInvalidLine()
    {
        var parser = new SyslogParserService();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Invalid log line");

        var result = parser.ParseSyslog(tempFile);

        Assert.Empty(result); // строка должна быть проигнорирована
    }
}