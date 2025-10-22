
using poligon_pezeglądarka_grafiki.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class SelectorBehavior
{
    #region bool ShouldSelectItemOnMouseUp

    //sam pomysł z dołączeniem zachowania nie jest zły ale wymaga modyfikacji

    public static readonly DependencyProperty ShouldSelectItemOnMouseUpProperty =
        DependencyProperty.RegisterAttached(
            "ShouldSelectItemOnMouseUp", typeof(bool), typeof(SelectorBehavior),
            new PropertyMetadata(default(bool), HandleShouldSelectItemOnMouseUpChange));

    public static void SetShouldSelectItemOnMouseUp(DependencyObject element, bool value)
    {
        element.SetValue(ShouldSelectItemOnMouseUpProperty, value);
    }

    public static bool GetShouldSelectItemOnMouseUp(DependencyObject element)
    {
        return (bool)element.GetValue(ShouldSelectItemOnMouseUpProperty);
    }

    private static Photo element = null;

    private static void HandleShouldSelectItemOnMouseUpChange(
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

        if (e.ChangedButton == MouseButton.Left && e.OriginalSource is Visual source)
        {
            var container = selector.ContainerFromElement(source);
            if (container != null)
            {                
                if (selector is ListBox listBox)
                {                        
                    if ((listBox.SelectionMode == SelectionMode.Multiple) || (listBox.SelectionMode == SelectionMode.Extended))
                    {                            
                        var index = selector.ItemContainerGenerator.IndexFromContainer(container);
                        if ((index >= 0) &&listBox.SelectedItems.Contains(listBox.Items[index]))
                        {                          
                            element = null;
                            e.Handled = true;
                        }
                        else element = (Photo)listBox.Items[index];
                    }
                }
            }
        }
    }

    #endregion

}
