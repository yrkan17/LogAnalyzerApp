using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LogAnalyzerApp.Converters;

public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateTime dateTime ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DateTime.TryParse(value?.ToString(), out var result) ? result : default;
    }
}