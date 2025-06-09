
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace poligon_pezeglądarka_grafiki.View.ext;

public static class ListBoxExtensions
{

    public static ListBoxItem GetListBoxItem(this ListBox listBox, System.Windows.Point position)
    {
        DependencyObject item = VisualTreeHelper.HitTest(listBox, position).VisualHit;
        while (item != null && !(item is ListBoxItem))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as ListBoxItem;
    }
    /*
    public static ListBoxItem GetListBoxItem(this ListBox listBox, DependencyObject item)
    {
        while (item != null && !(item is ListBoxItem))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as ListBoxItem;
    }
    public static ListBox GetListBox(this ListBoxItem listBox, DependencyObject item)
    {
        while (item != null && !(item is ListBox))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as ListBox;
    }*/
}
