
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class ListBoxItemExtensions
{
    public static TextBox GetTexBox(this ListBoxItem listBoxItem, System.Windows.Point position)
    {
        //kliknięty element wizualny
        if (listBoxItem == null)
        {
            //Debug.WriteLine("GetTexBox - listBoxItem is null");
            return null;
        }
        DependencyObject item = null;
        try
        {
            HitTestResult itemHit = VisualTreeHelper.HitTest(listBoxItem, position);
            if (itemHit == null)
            {
                //Debug.WriteLine("GetTexBox - HitTestResult is null");
                return null;
            }
            item = itemHit.VisualHit;
            // DependencyObject item = VisualTreeHelper.HitTest(listBoxItem, position).VisualHit;
            while (item != null && !(item is TextBox))
            {
                //Debug.WriteLine("item TextBox: " + item.GetType().ToString());
                item = VisualTreeHelper.GetParent(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetTexBox - exception: " + ex.Message);
            return null;
        }
        return item as TextBox;

        //return null;
    }

    public static TextBlock GetTexBlock(this ListBoxItem listBoxItem, System.Windows.Point position)
    {
        //kliknięty element wizualny
        Debug.WriteLine("GetTexBlock - test kliknięcia");
        DependencyObject item = VisualTreeHelper.HitTest(listBoxItem, position).VisualHit;
        //Debug.WriteLine("HitTest: " + item.GetType().ToString());   
        // DependencyObject item = VisualTreeHelper.HitTest(listBoxItem, position).VisualHit;

        while (item != null && !(item is TextBlock))
        {
            Debug.WriteLine("item TextBlock: " + item.GetType().ToString());

            item = VisualTreeHelper.GetParent(item);
        }

        //item = VisualTreeHelper.GetParent(item);

        /*
        if(item != null)
        {
            //var children = LogicalTreeHelper.GetChildren(item);
            //Debug.WriteLine("item != null, childrem count: "+children.GetType().ToString());
            //foreach(var child in children)
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
            {
                var child = VisualTreeHelper.GetChild(item, i);
                Debug.WriteLine("child: " + child.GetType().ToString());
                if (child is TextBlock)
                {
                    Debug.WriteLine("TextBlock: " + (child as TextBlock).Text);
                    return child as TextBlock;
                }
            }
        }*/
        return item as TextBlock;

        //return null;
    }
}
