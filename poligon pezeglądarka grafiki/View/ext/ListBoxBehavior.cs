
using poligon_pezeglądarka_grafiki.Model;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class ListBoxBehavior
{
    #region bool ShouldSelectItemOnMouseUp

    //sam pomysł z dołączeniem zachowania nie jest zły ale wymaga modyfikacji

    public static readonly DependencyProperty SelectItemOnMouseUpProperty =
        DependencyProperty.RegisterAttached(
            "SelectItemOnMouseUp",
            typeof(bool),
            typeof(ListBoxBehavior),
            new PropertyMetadata(default(bool), onSelectItemOnMouseUpChange)
        );

    public static void SetSelectItemOnMouseUp(DependencyObject element, bool value)
    {
        element.SetValue(SelectItemOnMouseUpProperty, value);
    }

    public static bool GetSelectItemOnMouseUp(DependencyObject element)
    {
        return (bool)element.GetValue(SelectItemOnMouseUpProperty);
    }

    private static Photo element = null;

    private static void onSelectItemOnMouseUpChange(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {        
        if (d is Selector selector)
        {
            selector.MouseDown -= HandleSelectMouseDown;
            selector.MouseUp -= HandleSelectMouseUp;

            if (Equals(e.NewValue, true))
            {
                selector.PreviewMouseDown += HandleSelectMouseDown;
                selector.MouseUp += HandleSelectMouseUp;
            }
        }
    }

    
    private static void HandleSelectMouseUp(object sender, MouseButtonEventArgs e)
    {
        var selector = (Selector)sender;

        if (e.ChangedButton == MouseButton.Left && e.OriginalSource is Visual source)
        {
            var container = selector.ContainerFromElement(source);
            if (container != null)
            {
                var index = selector.ItemContainerGenerator.IndexFromContainer(container);
                if (index >= 0)
                {
                    if (selector is ListBox listBox)
                    {
                        if ((listBox.SelectionMode == SelectionMode.Multiple) || (listBox.SelectionMode == SelectionMode.Extended))
                        {
                            if (listBox.SelectedItems.Contains(listBox.Items[index])&&(element == null))
                            {
                                listBox.SelectedItems.Remove(listBox.Items[index]);
                            }else element = null;
                        }
                    }
                }
            }
        }
    }
    

    private static void HandleSelectMouseDown(object sender, MouseButtonEventArgs e)
    {
        var selector = (Selector)sender;
        if (e.OriginalSource is Visual source)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var container = selector.ContainerFromElement(source);
                if (container != null)
                {
                    if (selector is ListBox listBox)
                    {
                        if ((listBox.SelectionMode == SelectionMode.Multiple) || (listBox.SelectionMode == SelectionMode.Extended))
                        {
                            var index = selector.ItemContainerGenerator.IndexFromContainer(container);
                            if ((index >= 0) && listBox.SelectedItems.Contains(listBox.Items[index]))
                            {
                                element = null;
                                e.Handled = true;
                            }
                            else element = (Photo)listBox.Items[index];
                        }
                    }
                }
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                var container = selector.ContainerFromElement(source);
                if (container != null)
                {
                    if (selector is ListBox listBox)
                    {
                        if ((listBox.SelectionMode == SelectionMode.Multiple) || (listBox.SelectionMode == SelectionMode.Extended))
                        {
                            var index = selector.ItemContainerGenerator.IndexFromContainer(container);
                            if ((index >= 0) && listBox.SelectedItems.Contains(listBox.Items[index]))
                            {
                                element = null;
                                //e.Handled = true;
                            }
                            else element = (Photo)listBox.Items[index];
                            //if((index >= 0) && !listBox.SelectedItems.Contains(listBox.Items[index])){
                            //    listBox.SelectedItems.Add(listBox.Items[index]);
                            //}
                        }
                    }
                }
            }
        }
    }

    #endregion


    #region ScrollToTopOnItemsSourceChange
    /*
    public static readonly DependencyProperty ScrollToTopProperty =
    DependencyProperty.RegisterAttached
    (
        "ScrollToTop",
        typeof(bool),
        typeof(ListBoxBehavior),
        new PropertyMetadata(default(bool), OnScrollToTopPropertyChanged)
    );
    public static bool GetScrollToTop(DependencyObject obj)
    {
        return (bool)obj.GetValue(ScrollToTopProperty);
    }
    public static void SetScrollToTop(DependencyObject obj, bool value)
    {
        obj.SetValue(ScrollToTopProperty, value);
    }
    private static void OnScrollToTopPropertyChanged(DependencyObject dpo,
                                                     DependencyPropertyChangedEventArgs e)
    {
        if ((dpo is ItemsControl itemsControl))
        {
            //Debug.WriteLine("OnScrollToTopPropertyChanged: not an ItemsControl");   

            //ItemsControl itemsControl = dpo as ItemsControl;
            //Debug.WriteLine("OnScrollToTopPropertyChanged triggered");//to działa
            if (itemsControl != null)
            {
                DependencyPropertyDescriptor dependencyPropertyDescriptor =
                        DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
                if (dependencyPropertyDescriptor != null)
                {
                    if ((bool)e.NewValue == true)
                    {
                        dependencyPropertyDescriptor.AddValueChanged(itemsControl, xItemsSourceChanged);
                    }
                    else
                    {
                        dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, xItemsSourceChanged);
                    }
                }
            }
        }
    }
    static void xItemsSourceChanged(object sender, EventArgs e)
    {
        if ((sender is ItemsControl itemsControl))
        {       
            //ItemsControl itemsControl = sender as ItemsControl;
            EventHandler eventHandler = null;
            Debug.WriteLine("ItemsSourceChanged triggered");//to działa
            eventHandler = new EventHandler(delegate
            {
                if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(itemsControl) as ScrollViewer;
                    if (scrollViewer == null) Debug.WriteLine("scrollViewer is null");
                    else Debug.WriteLine("scrollViewer is NOT null");
                    scrollViewer.ScrollToTop();
                    itemsControl.ItemContainerGenerator.StatusChanged -= eventHandler;
                }
            });
            itemsControl.ItemContainerGenerator.StatusChanged += eventHandler;
        }else Debug.WriteLine("ItemsSourceChanged: sender is not an ItemsControl");
    }

    static T GetVisualChild<T>(DependencyObject parent) where T : Visual
    {
        T child = default(T);
        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null)
            {
                child = GetVisualChild<T>(v);
            }
            if (child != null)
            {
                break;
            }
        }
        return child;
    }
    */
    #endregion ScrollToTopOnItemsSourceChange

    #region ScrollToTopOnItemsSourceChange Property

    public static readonly DependencyProperty ScrollToTopOnItemsSourceChangeProperty =
        DependencyProperty.RegisterAttached(
            "ScrollToTopOnItemsSourceChange",
            typeof(bool),
            typeof(ListBoxBehavior),
            new UIPropertyMetadata(false, OnScrollToTopOnItemsSourceChangePropertyChanged));

    public static bool GetScrollToTopOnItemsSourceChange(DependencyObject obj)
    {
        return (bool)obj.GetValue(ScrollToTopOnItemsSourceChangeProperty);
    }

    public static void SetScrollToTopOnItemsSourceChange(DependencyObject obj, bool value)
    {
        obj.SetValue(ScrollToTopOnItemsSourceChangeProperty, value);
    }

    static void OnScrollToTopOnItemsSourceChangePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var itemsControl = obj as ItemsControl;
        if (itemsControl == null)
        {
            //throw new Exception("ScrollToTopOnItemsSourceChange Property must be attached to an ItemsControl based control.");
            throw new Exception("ScrollToTopOnItemsSourceChange Właściwość musi być dołączona do kontrolki opartej na ItemsControl");
        }

        DependencyPropertyDescriptor descriptor =
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
        if (descriptor != null)
        {
            if ((bool)e.NewValue)
            {
                descriptor.AddValueChanged(itemsControl, ItemsSourceChanged);
            }
            else
            {
                descriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
            }
        }
    }

    static void ItemsSourceChanged(object sender, EventArgs e)
    {
        var itemsControl = sender as ItemsControl;
        Debug.WriteLine("sender: "+sender.GetType());
            DoScrollToTop(itemsControl);

        if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {           
            collection.CollectionChanged += (o, args) => DoScrollToTop(itemsControl);
        }
    }

    static void DoScrollToTop(ItemsControl itemsControl)
    {
        if (itemsControl.Items.Count < 10)
        {
            EventHandler eventHandler = null;
            eventHandler =
                delegate
                {
                    if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        var scrollViewer = xGetVisualChild<ScrollViewer>(itemsControl);
                        scrollViewer.ScrollToTop();
                        itemsControl.ItemContainerGenerator.StatusChanged -= eventHandler;
                    }
                };
            itemsControl.ItemContainerGenerator.StatusChanged += eventHandler;
        }
    }

    static T xGetVisualChild<T>(DependencyObject parent) where T : Visual
    {
        T child = default(T);
        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T ?? xGetVisualChild<T>(v);
            if (child != null)
            {
                break;
            }
        }
        return child;
    }

    #endregion
}
