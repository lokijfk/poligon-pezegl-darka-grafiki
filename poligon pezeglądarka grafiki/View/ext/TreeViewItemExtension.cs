
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace poligon_pezeglądarka_grafiki.View.ext;

public static class TreeViewItemExtension
{
    /// <summary>
    /// Zwraca pierwszy znaleziony TextBox z child TreeViewItem.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static DependencyObject GetCHildTextBox(this DependencyObject item)
    {
        if (item != null)
        {
            //if (item is TextBox tb)return tb;
            //else
            //{
            //    int childCount = VisualTreeHelper.GetChildrenCount(item);
            //    if (childCount == 0)return null;
            //    for (int i = 0; i < childCount; i++)
            //    {
            //        var child = VisualTreeHelper.GetChild(item, i);
            //        if (child is TextBox)return (TextBox)child;
            //        DependencyObject childItem = GetCHildTextBox(child);
            //        if (childItem is TextBox TBi) return TBi;
            //    }
            //}
            return GetCHildTextBox(item);
        }
        return null;
    }

}
