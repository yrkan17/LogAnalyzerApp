namespace LogAnalyzerApp.Models;

public class LogSeverityFilter
{
    public string DisplayName { get; set; }  // отображается в ComboBox (по-русски)
    public string? SeverityValue { get; set; } // используется для фильтрации (null = все)

    public override string ToString() => DisplayName; // для правильного отображения
}