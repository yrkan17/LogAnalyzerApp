namespace LogAnalyzerApp.Models;

/// <summary>
/// Вспомогательный класс модели для разделения названия типа и значения типа лога
/// </summary>
public class LogSeverityFilter
{
    /// <summary>
    /// Название типа лога
    /// </summary>
    public string DisplayName { get; set; }  // отображается в ComboBox (по-русски)
    
    /// <summary>
    /// Значение типа лога
    /// </summary>
    public string? SeverityValue { get; set; } // используется для фильтрации (null = все)
    
    public override string ToString() => DisplayName; // для правильного отображения
}