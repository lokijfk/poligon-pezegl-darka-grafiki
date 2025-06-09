
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class TreeViewExtensions
{
    public static TreeViewItem GetItem(this TreeView treeView, System.Windows.Point position)
    {
        DependencyObject item = VisualTreeHelper.HitTest(treeView, position).VisualHit;
        while (item != null && !(item is TreeViewItem))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as TreeViewItem;

    }

}
