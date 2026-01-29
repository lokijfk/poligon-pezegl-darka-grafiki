using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace poligon_pezeglądarka_grafiki.View.Converters;

internal class SizeConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        
        if (value is string stringValue)
        {
            if(long.TryParse(stringValue, out long size))
            {
                return convert(size);
            }
        }else if (value is long longValue)
        {
            return convert(longValue);
        }
        
        return string.Empty;
    }


    private string convert(long sizeInBytes)
    {
        double kb = 0.0;
        string og = string.Empty;
        if (sizeInBytes > 1000)
        {
            kb = sizeInBytes / 1024.0;
            og = " KB";
        }
        if (kb > 1000)
        {
            kb = kb / 1024.0;
            og = " MB";
        }
        if (kb > 1000)
        {
            kb = kb / 1024.0;
            og = " GB";
        }
        return kb.ToString("F2") + og;
    }


    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
