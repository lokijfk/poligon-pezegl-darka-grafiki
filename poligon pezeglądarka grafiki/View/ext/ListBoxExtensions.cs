
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace poligon_pezeglądarka_grafiki.View.ext;

public static class ListBoxExtensions
{
    /// <summary>
    /// zwraca ListBoxItem spod pozycji kursora 
    /// </summary>
    /// <param name="listBox"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static ListBoxItem GetListBoxItem(this ListBox listBox, System.Windows.Point position)
    {
            // tu jest problem jak jest urzywany suwak z boku to szaleje
            DependencyObject item = VisualTreeHelper.HitTest(listBox, position).VisualHit;
            while (item != null && !(item is ListBoxItem))
            {
                item = VisualTreeHelper.GetParent(item);
            }
            return item as ListBoxItem;
    }

}
