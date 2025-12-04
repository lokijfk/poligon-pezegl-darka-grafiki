
using System.Windows.Controls;
using System.Windows.Data;


namespace poligon_pezeglądarka_grafiki.View.Converters;

internal class ListConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is ListBox list)
        {
            if (list.SelectedItem == null || list.SelectedItems.Count == 0)
            {
                return null;
            }
            else
            {
                return list.SelectedItems;
            }
        }
        //if(value is System.Windows.Controls.SelectedItemCollection)
        return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
