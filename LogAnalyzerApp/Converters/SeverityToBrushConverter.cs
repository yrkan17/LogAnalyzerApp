using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Media;

namespace LogAnalyzerApp.Converters;

/// <summary>
/// Конвертер типа лога в цвет
/// </summary>
public class SeverityToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "error" => new SolidColorBrush(Color.Parse("#C62828")),
            "warning" => new SolidColorBrush(Color.Parse("#EF6C00")),
            "info" => new SolidColorBrush(Color.Parse("#1565C0")),
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}