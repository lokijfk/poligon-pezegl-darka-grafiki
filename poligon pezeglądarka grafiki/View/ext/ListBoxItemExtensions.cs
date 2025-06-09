using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class ListBoxItemExtensions
{
    public static TextBox GetTexBox(this ListBoxItem listBoxItem, System.Windows.Point position)
    {
        //kliknięty element wizualny
        DependencyObject item = VisualTreeHelper.HitTest(listBoxItem, position).VisualHit;

       // DependencyObject item = VisualTreeHelper.HitTest(listBoxItem, position).VisualHit;
        while (item != null && !(item is TextBox))
        {

            item = VisualTreeHelper.GetParent(item);
        }
        return item as TextBox;

        //return null;
    }
}
