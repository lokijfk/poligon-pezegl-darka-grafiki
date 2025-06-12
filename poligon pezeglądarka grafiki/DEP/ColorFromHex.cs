
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.Converters;
[ValueConversion(typeof(string), typeof(Color))]
public class ColorFromHex : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Color color = (Color)ColorConverter.ConvertFromString(value.ToString());
        Debug.WriteLine(value.ToString());
        return color;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
