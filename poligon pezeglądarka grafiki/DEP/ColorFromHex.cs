
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;
using System.Diagnostics;

namespace poligon_pezeglądarka_grafiki.View.Converters;

class ColorFromHex : IValueConverter
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
