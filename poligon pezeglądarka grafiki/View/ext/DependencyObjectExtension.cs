
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class DependencyObjectExtension
{
    public static Grid GetParentAsGrid(this DependencyObject item)
    {
        //DependencyObject item = block;
        while (item != null && !(item is Grid))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as Grid;
    }

    public static ListBoxItem GetParentAsListBoxItem(this DependencyObject item)
    {
        //DependencyObject item = block;
        while (item != null && !(item is ListBoxItem))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as ListBoxItem;
    }

    public static ListBox GetParentAsListBox(this DependencyObject item)
    {
        //DependencyObject item = block;
        while (item != null && !(item is ListBox))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as ListBox;
    }
    //dodać listbox i listboxitem
}
