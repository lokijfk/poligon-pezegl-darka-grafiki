
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
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
            {
                var child = VisualTreeHelper.GetChild(item, i);
                if (child is TextBox)
                {
                    //Debug.WriteLine("Found TextBox: " + ((TextBox)child).Text);
                    return (TextBox)child;
                }

                DependencyObject childItem = GetCHildTextBox(child);
                if (childItem != null) return childItem as TextBox;
            }
        }
        return null;




        /*
        //DependencyObject item = block;
        //var parent = VisualTreeHelper.GetParent(item);
        TreeViewItem treeViewItem = item as TreeViewItem;
        var child = VisualTreeHelper.GetChild(treeViewItem, 0);
        if (child is TextBox textBox)
        {
            Debug.WriteLine("Found TextBox in TreeViewItem: " + textBox.Name);
            return textBox;
        }else if (child is Grid grid)
        {
            foreach (var gridChild in LogicalTreeHelper.GetChildren(grid))
            {
                if (gridChild is TextBox textBox2)
                {
                    Debug.WriteLine("Found TextBox2 in TreeViewItem: " + textBox2.Name);
                    return textBox2;
                }
            }
        }
        return null;
        */

    }


}
