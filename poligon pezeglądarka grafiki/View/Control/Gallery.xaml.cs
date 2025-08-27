
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace poligon_pezeglądarka_grafiki.View.Control;

/// <summary>
/// Logika interakcji dla klasy Gallery.xaml
/// </summary>
public partial class Gallery : UserControl
{

    //private decimal znacznik = 0;
    private bool edit = false;    
    private ListBoxItem? selectedItem = null;
    
    public Gallery()
    {
        InitializeComponent();       
       /*
        ((INotifyCollectionChanged)Lista.ItemsSource).CollectionChanged +=
        new NotifyCollectionChangedEventHandler(List1CollectionChanged);
       //*/
    }



    /*
    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ListBoxItem listBoxItem = sender as ListBoxItem;
        var ph = listBoxItem.DataContext as Photo;        
        var index = (this.DataContext as MainWindowViewModel).Photos.IndexOf(ph);
        var p = (this.DataContext as MainWindowViewModel)?.Photos[index].Path;
        ViewWindow viewWindow = new()
        {
            DataContext = new ViewWindowViewModel(index,
            (this.DataContext as MainWindowViewModel).Photos)
        };
        viewWindow.Show();
    }*/

    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        
        if (!edit)
        {
            ListBox List = sender as ListBox;
            Photo ph = List.SelectedItem as Photo;
            int curentIndex = 0;
            var y = List.GetListBoxItem(e.GetPosition(List));            
            if ((ph != null) && (y != null) && (ph == y.DataContext))
            {                
                // jak tu wpleść dyrektywe using tak żeby zwalniał obiekt automatycznie?
                //musi mieć interfejs idisposable
                var index = (this.DataContext as MainWindowViewModel).Photos.IndexOf(ph);
                ViewWindow viewWindow = new()
                {
                    DataContext = new ViewWindowViewModel(index,
                    (this.DataContext as MainWindowViewModel).Photos)
                };
                //viewWindow.Owner = Window.GetWindow(this); // ustawienie właściciela okna
                viewWindow.ShowDialog();//Show nie czeka na zamknięcia okna, ShowDialog czeka na zamknięcie okna
                viewWindow.Activate(); // aktywacja okna
                curentIndex = (viewWindow.DataContext as ViewWindowViewModel).currentImageIndex;
                if (curentIndex >= 0)
                {
                    //Debug.WriteLine("index: " + curentIndex);
                    List.SelectedIndex = curentIndex; // ustawienie zaznaczenia na liście
                    List.ScrollIntoView(List.SelectedItem); // przewinięcie do zaznaczonego elementu
                }
            }
        }
    }


   /*
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {        
        //Debug.WriteLine(" ok - key down text box");    
        if (e.Key == Key.Enter)
        {
            TextBox textBox = sender as TextBox;
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();//GetChildrenTBO(textBox.GetParentAsGrid());
            lb.Visibility = System.Windows.Visibility.Visible;
            edit = false;
            //if((textBox.DataContext as Photo).Name != textBox.Text)
            //(DataContext as MainWindowViewModel).RenameFile(textBox.DataContext as Photo, textBox.Text);
        }else if (e.Key == Key.Escape)
        {
            TextBox textBox = sender as TextBox;
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();//GetChildrenTBO(textBox.GetParentAsGrid());
            lb.Visibility = System.Windows.Visibility.Visible;
            edit = false;
        }
    }
   

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine(" ok - text box lost focus");
        TextBox textBox = sender as TextBox;
        if (textBox.Visibility == Visibility.Visible)//tu zamiast edit sprawdzać czy jest widoczny zamiasr edit, edit wywalić
        {
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();
            lb.Visibility = System.Windows.Visibility.Visible;
            textBox.Text = (textBox.DataContext as Photo).Name; // reset nazwy do oryginalnej, jeżeli nie zmieniono
        }
    }
   



    private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {        
       // Debug.WriteLine(" ok - text blok mouse up 1");
        decimal milliseconds = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
        if ((znacznik > 0) && (milliseconds - znacznik >= 1000) && (milliseconds - znacznik <= 3500))
        {
            if ((sender != null) && (sender is TextBlock) && (edit == false))
            {
                edit = true;
               // Debug.WriteLine(" ok - text blok mouse up 2");
                TextBlock label = sender as TextBlock;
                ListBoxItem lbi = label.GetParentAsListBoxItem();
                ListBox LB = lbi.GetParentAsListBox();
                if ((lbi != null) && (LB != null))
                {
                    Photo photo = lbi.DataContext as Photo;
                    Photo select = LB.SelectedItem as Photo;
                    if (photo == select)
                    {
                        label.Visibility = System.Windows.Visibility.Collapsed;
                        TextBox tb = label.GetSisTexBox();
                        TextBoxActivate(tb);
                    }
                }
            }
        }        
        else
        {
            znacznik = milliseconds;
        }
    }
   */
    private void TextBoxActivate(TextBox textBox)
    {
        //Debug.WriteLine(" ok - text box activate");
        if (textBox != null)
        {
            TextBlock textBlock = textBox.GetSisTextBlock();
            if (textBlock != null) textBlock.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.Select(0, textBox.Text.Length-4); // ustawia kursor na końcu tekstu
        }
    }


    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine("klik menu: "+ (selectedItem.DataContext as Photo).Name);
        TextBox textBox = (selectedItem as DependencyObject).GetCHildTextBox() as TextBox;
        if (textBox != null) TextBoxActivate(textBox);
    }

    private void MenuItem_Delete(object sender, RoutedEventArgs e)
    {
        ListBoxItem listBoxItem = selectedItem as ListBoxItem;
        if (listBoxItem != null)
        {
            Photo photo = listBoxItem.DataContext as Photo;
            if (photo != null)
            {
                //Debug.WriteLine("usuwanie: " + photo.Name);
                (DataContext as MainWindowViewModel).DeleteFile(photo.Path);
            }
        }

    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {

    }

    private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        //to chyba nie będzie potrzebne lista automatycznie zaznacza element
        // i mo zna go uzyskać jako selecteditem?
        selectedItem = sender as ListBoxItem;
    }

    private void ListBox_MouseMove(object sender, MouseEventArgs e)
    {   
        bool EditMode = (DataContext as MainWindowViewModel).EditMode;
        //Debug.WriteLine("EditMode: " + EditMode);
        if (e.LeftButton == MouseButtonState.Pressed && !edit && !EditMode)
        {
            Debug.WriteLine("przeciąganie elementu listy");
            ListBox listBox = sender as ListBox;
            ListBoxItem item = listBox.GetListBoxItem(e.GetPosition(listBox));
            if (item != null)
            {
                //Debug.WriteLine("wybrano element: " + (item.DataContext as Photo).Name);
                DragDrop.DoDragDrop(item, (item.DataContext as Photo).Path,
                    DragDropEffects.Copy | DragDropEffects.Move);

            }
        }
    }

    private void Lista_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Check if Modifiers for Selection modes are pressed
        if (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
        {
            ListBox parent = (ListBox)sender;
 
        }
    }




}
