using System.Globalization;
using System.Windows.Data;

namespace poligon_pezeglądarka_grafiki.View.Converters
{
    class MultibindingConvertertoArrayObject : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            
            return values;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }   
}
