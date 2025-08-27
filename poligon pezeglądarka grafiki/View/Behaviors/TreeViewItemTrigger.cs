using Microsoft.Xaml.Behaviors;
using poligon_pezeglądarka_grafiki.View.ext;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.Behaviors;

public abstract class TreeViewItemTrigger : TriggerBase<DependencyObject>
{
    public static readonly DependencyProperty TreeViewItemProperty =
        DependencyProperty.Register(nameof(TreeViewItem), typeof(TreeViewItem), typeof(TreeViewItemTrigger),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTreeViewItemChanged)));//new PropertyMetadata(true));

    public TreeViewItem TreeViewItem
    {
        get => (TreeViewItem)GetValue(TreeViewItemProperty);
        set => SetValue(TreeViewItemProperty, value);
    }

    //no ale  on się raczej nie będzie zmieniał a zostanie aktywowany przy kliknięciu na TreeView
    // trzeba jakoś tu wpiąć wyłuskiwanie treeviewitem na który kliknięto .. 
    private static void OnTreeViewItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TreeViewItemTrigger treeViewItemTrigger = d as TreeViewItemTrigger;
        if (treeViewItemTrigger != null)
        {
            treeViewItemTrigger.OnTreeViewItemChanged(e);
        }
    }

    protected virtual void OnTreeViewItemChanged(DependencyPropertyChangedEventArgs args)
    {
    }
}

public  class TreeViewItemSelectedTrigger : TreeViewItemTrigger
{
    protected override void OnTreeViewItemChanged(DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is TreeViewItem treeViewItem)
        {
            // Do something with the TreeViewItem, e.g., set it as selected
            treeViewItem.IsSelected = true;
        }
    }

    //public AssociatedObject{ get => AssociatedObject as TreeView; }


    protected override void OnAttached()
    {
        base.OnAttached();
        // Additional initialization if needed

        if (AssociatedObject is TreeView treeView)
        {
            treeView.MouseLeftButtonDown += (sender, e) =>
            {
                // Get the TreeViewItem under the mouse cursor
                var position = e.GetPosition(treeView);
                var item = GetItem(treeView,position);
                if (item != null)
                {
                    // Set the TreeViewItem property to the clicked item
                    TreeViewItem = item;
                }
            };

        }
    }

    
    private static void OnMouseLeftButtonDown(DependencyObject sender, MouseButtonEventArgs e)
    {
        //TreeViewItemSelectedTrigger trigger = sender as TreeViewItemSelectedTrigger;// tu jest źle
        TreeView treeView = sender as TreeView;
        if (treeView != null)
        {
            // Get the TreeViewItem under the mouse cursor
            var position = e.GetPosition(treeView);
            //var item = treeView.GetItem(position);
            var item = GetItem(treeView, position);
            //metoda statyczna odwołuje siędo mestod statycznych i po kłopocie
            if (item != null)
            {
                // Set the TreeViewItem property to the clicked item
                //trigger.TreeViewItem = item;
            }
        }
    }
   
    private static TreeViewItem? GetItem(TreeView treeView, Point position)
    {
        HitTestResult hitTestResult = VisualTreeHelper.HitTest(treeView, position);
        if (hitTestResult != null)
        {

            DependencyObject hitObject = hitTestResult.VisualHit;
            while (hitObject != null && !(hitObject is TreeViewItem))
            {
                hitObject = VisualTreeHelper.GetParent(hitObject);
            }
            return hitObject as TreeViewItem;
        }
        return null;
    }
 

    protected override void OnDetaching()
    {
        base.OnDetaching();
        // Cleanup if needed
    }
}
