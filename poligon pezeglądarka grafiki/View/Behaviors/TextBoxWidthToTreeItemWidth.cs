

using Microsoft.Xaml.Behaviors;
using poligon_pezeglądarka_grafiki.View.ext;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace poligon_pezeglądarka_grafiki.View.Behaviors;

public class TextBoxWidthToTreeItemWidth : Behavior<TextBox>
{
   public static readonly DependencyProperty BoxVisiblityProperty =
       DependencyProperty.Register("VisiblityProperty", typeof(Visibility), typeof(TextBoxWidthToTreeItemWidth), new PropertyMetadata(Visibility.Visible, OnVisiblityChanged));
    public Visibility VisiblityProperty
    { 
        get => (Visibility)GetValue(BoxVisiblityProperty); 
        set => SetValue(BoxVisiblityProperty, value);
    }

    private static void OnVisiblityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //Debug.WriteLine("OnVisiblityChanged called");
        if (d is TextBoxWidthToTreeItemWidth behavior && behavior.AssociatedObject != null)
        {
            //Debug.WriteLine($"Visibility changed to: {e.NewValue}");
            if (behavior.AssociatedObject is TextBox)
            {
                //Debug.WriteLine("AssociatedObject is TextBox");
                var textBox = behavior.AssociatedObject as TextBox;
                var treeViewItem = textBox.GetTreeViewItem();
                if (treeViewItem != null)
                {
                    //Debug.WriteLine("TreeViewItem found");
                    textBox.Width = treeViewItem.ActualWidth - 50; // Adjust width based on TreeViewItem width
                    textBox.Height = treeViewItem.ActualHeight; // Adjust height based on TreeViewItem height
                    textBox.MaxHeight = textBox.FontSize * 1.5;
                    textBox.Margin = new Thickness(0, 0, 0, 0);
                    textBox.Padding = new Thickness(0, 0, 0, 0);
                    textBox.BorderThickness = new Thickness(0);
                }

            }
        }
    }


}
