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
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
            {
                var child = VisualTreeHelper.GetChild(item, i);
                if (child is TextBox)
                {
                    return (TextBox)child;
                }                
                DependencyObject childItem = GetCHildTextBox(child);
                if (childItem is TextBox TBi) return TBi;
            }
        }
        return null;
    }
}
