using poligon_pezeglądarka_grafiki.DEP;
using poligon_pezeglądarka_grafiki.Model;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
            if (item is TextBox tb)
            {
                Debug.WriteLine("GetCHildTextBox: item is TextBox");
                return tb;
            }
            else
            {
                Debug.WriteLine($"GetCHildTextBox for {item.GetType().Name}, item != null");

                int childCount = VisualTreeHelper.GetChildrenCount(item);
                if (childCount == 0)
                {
                    Debug.WriteLine("GetCHildTextBox: child count is 0, returning null");

                    return null;
                }
                //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(item, i);
                    Debug.WriteLine($"GetCHildTextBox:{item.GetType().ToString()}  child: {child.GetType().ToString()}");
                    if (child is TextBox)
                    {
                        return (TextBox)child;
                    }
                    DependencyObject childItem = GetCHildTextBox(child);
                    if (childItem is TextBox TBi) return TBi;
                }
            }
        }
        Debug.WriteLine("beak TextBox");
        return null;
    }

    public static DependencyObject GetCHildTextBoxEx(this DependencyObject item)
    {
        if (item != null)
        {
            if (item is TextBox tb)
            {
                Debug.WriteLine("GetCHildTextBoxEx: item is TextBox");
                return tb;
            }
            else
            {
                Debug.WriteLine($"GetCHildTextBoxEx for {item.GetType().Name}, item != null");

                int childCount = VisualTreeHelper.GetChildrenCount(item);
                if (childCount == 0)
                {
                    Debug.WriteLine("GetCHildTextBoxEx: child count is 0, returning null");

                    return null;
                }
                //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(item, i);
                    Debug.WriteLine($"GetCHildTextBoxEx:{item.GetType().ToString()}  child: {child.GetType().ToString()}");
                    if (child is TextBox)
                    {
                        return (TextBox)child;
                    }
                    if (child is Grid grid)
                    {
                        Debug.WriteLine("GetCHildTextBoxEx: child is Grid, searching its children for TextBox");
                        foreach (var gridChild in grid.Children)
                        {
                            if (gridChild is TextBox tbGrid)
                            {
                                return tbGrid;
                            }
                        }
                    }
                    DependencyObject childItem = GetCHildTextBoxEx(child);
                    if (childItem is TextBox TBi) return TBi;
                }
            }
        }
        Debug.WriteLine("beak TextBox");
        return null;
    }


    public static DependencyObject GetElementByText(this DependencyObject item, string text)
    {
        if (item != null)
        {   
            

            if (item is TextBox tb && tb.Text == text)
            {
                Debug.WriteLine("GetElementByText: item is TextBox with matching text");
                return tb;
            }
            else
            {
                Debug.WriteLine($"GetElementByText for {item.GetType().Name}, item != null");
                int childCount = VisualTreeHelper.GetChildrenCount(item);
                if (childCount == 0)
                {
                    Debug.WriteLine("GetElementByText: child count is 0, returning null");
                    return null;
                }
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(item, i);
                    Debug.WriteLine($"GetElementByText:{item.GetType().ToString()}  child: {child.GetType().ToString()}");
                    if (child is TextBox tbChild && tbChild.Text == text)
                    {
                        return tbChild;
                    }
                    DependencyObject childItem = GetElementByText(child, text);
                    if (childItem is TextBox TBi) return TBi;
                }
            }
        }

        return null;
    }
}
