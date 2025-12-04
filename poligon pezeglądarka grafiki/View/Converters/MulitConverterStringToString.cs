
using System.Globalization;
using System.Windows.Data;

namespace poligon_pezeglądarka_grafiki.View.Converters;

[ValueConversion(typeof(string[]), typeof(string))]
public class MulitConverterStringToString : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string name;
         if((values.Length == 3) &&  (values[2].ToString().ToLower() == "true") )
        {
            //Debug.WriteLine("3: " + values[2].ToString());
            if(values[1] != null && values[1].ToString() != "")
            {
                name = $"{values[0]} [{values[1]}]";
            }
            else
            {
                name = $"{values[0]} [0]";
            }
        }
        else
        {
            name = $"{values[0]}";
        }
        return name;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
