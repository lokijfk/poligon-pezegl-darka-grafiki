using System.Globalization;
using System.Windows.Data;

namespace poligon_pezeglądarka_grafiki.View.Converters;

public class AutomationPropertiesNameConverter : IValueConverter
{
    public static readonly AutomationPropertiesNameConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue;
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}