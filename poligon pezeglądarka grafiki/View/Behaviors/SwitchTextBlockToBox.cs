using Microsoft.Xaml.Behaviors;
using poligon_pezeglądarka_grafiki.View.ext;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace poligon_pezeglądarka_grafiki.View.Behaviors;

public class SwitchTextBlockToBox : Behavior<FrameworkElement>
{
    //może zrobić tak żeby to było dodawane  do grid i z tego gridu pobierać textblock i textbox
    public static readonly DependencyProperty EditModeProperty =
        DependencyProperty.Register("EditMode", typeof(bool), typeof(SwitchTextBlockToBox));

    public bool EditMode
    {
        get => (bool)GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(SwitchTextBlockToBox), new PropertyMetadata(null));
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        this.AssociatedObject.Visibility = Visibility.Visible;
        //Debug.WriteLine("SwitchTextBlockToBox: OnAttached");
        if (AssociatedObject is TextBox textBox)
        {
            textBox.Visibility = Visibility.Collapsed;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.KeyDown += TextBox_KeyDown;
        }
        else if (AssociatedObject is TextBlock textBlock)
        {
            textBlock.MouseLeftButtonUp += TextBlock_MouseLeftButtonUp;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        this.AssociatedObject.Visibility = Visibility.Collapsed;


        if (AssociatedObject is TextBox textBox)
        {
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.KeyDown -= TextBox_KeyDown;
        }
        else if (AssociatedObject is TextBlock textBlock)
        {
            textBlock.MouseLeftButtonUp -= TextBlock_MouseLeftButtonUp;
        }
    }


    private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //dodać sprawdzanie czy jest już zaznaczony, jeżeli nie to nie robić nic
        //Debug.WriteLine("TextBlock_MouseLeftButtonUp: " + AssociatedObject.GetType().Name);
        if (AssociatedObject is TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Collapsed;
            var textBox = textBlock.GetSisTexBox();
            if (textBox != null)
            {
                EditMode = true;
                textBox.Visibility = Visibility.Visible;
                textBox.Focus();
                //tu dodać sprawdzanie czy jest rozszeżenie i zaznaczać bez rozszeżenia
                textBox.SelectAll();
                //Debug.WriteLine("TextBox_MouseLeftButtonUp: " + textBox.Text);
            }
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject is TextBox textBox)
        {
            textBox.Visibility = Visibility.Collapsed;
            var textBlock = textBox.GetSisTextBlock();
            EditMode = false;
            if (textBlock != null)
            {
                textBlock.Visibility = Visibility.Visible;
                textBlock.Text = textBox.Text;
            }
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {   
        // żeby nie łądować obiektów przy kazdym naciśnięciu klawisza
        if (e.Key == Key.Escape || e.Key == Key.Enter)
        {
            TextBox textBox = sender as TextBox;
            TextBlock textBlock = textBox.GetSisTextBlock();
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            textBlock.Visibility = System.Windows.Visibility.Visible;
            EditMode = false;
            if (e.Key == Key.Enter)
            {
                /*
                textBox.Visibility = System.Windows.Visibility.Collapsed;
                textBlock.Visibility = System.Windows.Visibility.Visible;
                EditMode = false;
                //*/
                /*
                string Name = String.Empty;
                object DC = null;
                //tu zrezygnować z sprawdzania typu i przesyłać jako object do komendy tam sprawdzać czy są zmiany
                // to komenda na sobie sprawdzi\c poprawność typu

                if (textBox.DataContext is Photo)
                {
                    Name = (textBox.DataContext as Photo).Name;
                    DC = textBox.DataContext as Photo;
                }
                if (textBox.DataContext is TreeModel)
                {
                    Name = (textBox.DataContext as TreeModel).Name;
                    DC = textBox.DataContext as TreeModel;
                }
                if (textBox.DataContext is FilesIO)
                {
                    Name = (textBox.DataContext as FilesIO).Name;
                    DC = textBox.DataContext as FilesIO;
                }
                if (Name != textBox.Text)
                {                
                    object[] args = { DC, textBox.Text };
                    Command?.Execute(args);//wywołanie jest zależne od metody i tu nie ma problemu                
                }
                //*/
                //to Command ma sobie sprawdzić kai to obiekt i czy jest zmiana
                Command?.Execute(new object[] { textBox.DataContext, textBox.Text });
            }
            /*
            else if (e.Key == Key.Escape)
            {
                //TextBox textBox = sender as TextBox;
                textBox.Visibility = System.Windows.Visibility.Collapsed;
                //TextBlock textBlock = textBox.GetSisTextBlock();//GetChildrenTBO(textBox.GetParentAsGrid());
                textBlock.Visibility = System.Windows.Visibility.Visible;
                EditMode = false;
            }
            //*/
        }
    }

    private void TextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
         
        //Debug.WriteLine("TextBox_MouseLeftButtonUp: " + AssociatedObject.GetType().Name);
        if (AssociatedObject is TextBox textBox)
        {
            textBox.Visibility = Visibility.Collapsed;
            var textBlock = textBox.GetSisTextBlock();
            if (textBlock != null)
            {
                EditMode = false;
                textBlock.Visibility = Visibility.Visible;
                textBlock.Text = textBox.Text;
            }
        }
    }
}
