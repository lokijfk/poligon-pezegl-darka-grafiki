
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Diagnostics;
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

    public static TreeViewItem GetTreeViewItem(this DependencyObject item)
    {
        //DependencyObject item = block;
        while (item != null && !(item is TreeViewItem))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as TreeViewItem;
    }

    /*
    public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                {
                    return (T)child;
                }

                T childItem = FindVisualChild<T>(child);
                if (childItem != null) return childItem;
            }
        }
        return null;
    }*/

    public static TextBox GetSisTexBox(this DependencyObject item)
    {
        //DependencyObject item = block;

        var parent = VisualTreeHelper.GetParent(item);
        foreach (var child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is TextBox textBox)
            {
                return textBox;
            }
        }
        return null;
    }


    public static TextBlock GetSisTextBlock(this DependencyObject item)
    {
        //DependencyObject item = block;
        var parent = VisualTreeHelper.GetParent(item);
        foreach (var child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is TextBlock textBlock)
            {
                return textBlock;
            }
        }
        return null;
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
