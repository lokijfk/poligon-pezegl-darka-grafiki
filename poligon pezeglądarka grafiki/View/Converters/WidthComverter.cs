using System.Globalization;
using System.Windows.Data;

namespace poligon_pezeglądarka_grafiki.View.Converters
{
    class WidthComverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            
            if(v == 0 || value == null || v<= 200)
            {
                return 200;
            }
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
