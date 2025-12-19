
using System.Diagnostics;
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
        try
        {
            if (listBox != null)
            {
                HitTestResult resultHit = VisualTreeHelper.HitTest(listBox, position);
                if (resultHit != null)
                {
                    DependencyObject item = resultHit.VisualHit as DependencyObject;
                    if (item != null)
                    {
                        while (item != null && !(item is ListBoxItem))
                        {
                            item = VisualTreeHelper.GetParent(item);
                        }
                    }
                    return item as ListBoxItem;
                }
            }

        }
        catch (System.Exception ex)
        {
            Debug.WriteLine(ex);
        }
        return null;
    }

}
