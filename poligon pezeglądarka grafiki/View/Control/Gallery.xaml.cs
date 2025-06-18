
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Collections.Specialized;
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

    private decimal znacznik = 0;
    private bool edit = false;
    //private TextBox TBed = null;
    private ListBoxItem? selectedItem = null;
    //private ListBoxItem? 
    public Gallery()
    {
        InitializeComponent();
       // ListBox List1;
       /*
        ((INotifyCollectionChanged)Lista.ItemsSource).CollectionChanged +=
    new NotifyCollectionChangedEventHandler(List1CollectionChanged);
       */
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
        //znacznik = 0; //znacznik do zmiany nazwy powoduje problemy
        if (!edit)
        {
            ListBox List = sender as ListBox;
            Photo ph = List.SelectedItem as Photo;

            var y = List.GetListBoxItem(e.GetPosition(List));
            //Debug.WriteLine(x.GetType().ToString()+" : "+y.GetType().ToString());
            if ((ph != null) && (y != null) && (ph == y.DataContext))
            {
                //Photo ph = x as Photo;
                var index = (this.DataContext as MainWindowViewModel).Photos.IndexOf(ph);
                //ViewWindow viewWindow = new() { DataContext = new ViewWindowViewModel((y.DataContext as Photo).Path) };
                ViewWindow viewWindow = new()
                {
                    DataContext = new ViewWindowViewModel(index,
                    (this.DataContext as MainWindowViewModel).Photos)
                };
                viewWindow.Show();
            }
        }
    }

    

    public void List1CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
    {
        // Your logic here
        Debug.WriteLine("Collection changed in Gallery");
    }


    /*
    private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        //dzia\\la ale jest wywoływane za każdym razem przed double klick
        // więc nie może być wykozystane
        Debug.WriteLine("mouse down z");
        ListBoxItem listBoxItem = sender as ListBoxItem;
        Photo photo = listBoxItem.DataContext as Photo;
        string path = photo.Path;
        ListBox listBox = listBoxItem.GetListBox(listBoxItem);
        Photo select = listBox.SelectedItem as Photo;
        if ((select != null) && (photo != null) && (photo == select))
            Debug.WriteLine("jest zgodność !!");
        Debug.WriteLine($"{path}");
    }//*/

    /*
    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //var timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmssffff");
        Debug.WriteLine(" ok - text blok mouse down 1");
        decimal milliseconds = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
        if ((znacznik > 0) && (milliseconds - znacznik >= 1000) && (milliseconds - znacznik <= 3500))
        {   
            edit = true;
            Debug.WriteLine(" ok - text blok mouse down 2");
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
                    Grid p = label.GetParentAsGrid();//to jest z DependencyObject                
                    if (p != null)
                    {
                        lastIndex = LB.SelectedIndex;
                        LB.SelectedIndex = -1; // to nic nie zmienia
                        //LB.Items[lastIndex].

                        TextBox tb = GetChildrenTB(p);
                        //LB.ClearSelectedItems();
                        tb.Visibility = System.Windows.Visibility.Visible;
                        // tu prawdopodobnie focus jest "kradziony" przez listboxa
                        // jeszcze nie wiem jak to rozwiązać,
                        // może zamiast zmiany na textbox tywoływać coś w rodzaju małego okna z tezboxem i je obcłużyć ??
                        bool test = tb.Focus();
                        Debug.WriteLine("foccus: "+test.ToString());
                        //tb.Select(tb.Text.Length, 0);
                        
                        //tb.CaretIndex = tb.Text.Length;
                        //tb.ScrollToEnd();
                        //tb.Focusable = true;
                        //tb.Focus();
                        //tb.Select(tb.Text.Length,0);
                        //tb.Select(0,tb.Text.Length);
                        
                        tb.SelectAll();

                    }

                }
            }
        }
        else
        {
            znacznik = milliseconds;
            Debug.WriteLine(" ok m: "+milliseconds+" m-z: "+(milliseconds-znacznik).ToString());
        }
    }//*/
    /*
    private TextBox GetChildrenTB(DependencyObject obj)
    {
        Grid p = obj as Grid;
        foreach (var x in p.Children)
        {
            if (x is TextBox)
            {
                return x as TextBox;
            }
        }
        return null;
    }
    

    private TextBlock GetChildrenTBO(DependencyObject obj)
    {
        Grid p = obj as Grid;
        foreach (var x in p.Children)
        {
            if (x is TextBlock)
            {
                return x as TextBlock;
            }
        }
        return null;
    }
    //*/
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
            if((textBox.DataContext as Photo).Name != textBox.Text)
            (DataContext as MainWindowViewModel).RenameFile(textBox.DataContext as Photo, textBox.Text);
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
        TextBox textBox = sender as TextBox;
        if (textBox.Visibility == Visibility.Visible)//tu zamiast edit sprawdzać czy jest widoczny zamiasr edit, edit wywalić
        {
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();
            lb.Visibility = System.Windows.Visibility.Visible;
            textBox.Text = (textBox.DataContext as Photo).Name; // reset nazwy do oryginalnej, jeżeli nie zmieniono
        }
    }


    private void TextBoxActivate(TextBox textBox)
    {
        if (textBox != null)
        {
            TextBlock textBlock = textBox.GetSisTextBlock();
            if (textBlock != null) textBlock.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.Select(0, textBox.Text.Length-4); // ustawia kursor na końcu tekstu
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
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Debug.WriteLine("przeciąganie elementu listy");
            ListBox listBox = sender as ListBox;
            ListBoxItem item = listBox.GetListBoxItem(e.GetPosition(listBox));
            if (item != null)
            {
                Debug.WriteLine("wybrano element: " + (item.DataContext as Photo).Name);
                //DataObject data = new DataObject();
                //data.SetText((selectedItem.DataContext as Photo).Path);
                //data.SetData(DataFormats.StringFormat, (selectedItem.DataContext as Photo).Path);
                //data.SetData("Photo", selectedItem.DataContext as Photo);//?

                //DragDrop.DoDragDrop(selectedItem, data, DragDropEffects.Copy | DragDropEffects.Move);
                DragDrop.DoDragDrop(item, (item.DataContext as Photo).Path,
                    DragDropEffects.Copy | DragDropEffects.Move);

            }
        }
    }



    /*
    private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        selectedItem = (sender as ListBox).GetListBoxItem(e.GetPosition(sender as ListBox));
        if (selectedItem != null)
        {
            Debug.WriteLine("wybrano element: " + (selectedItem.DataContext as Photo).Name);
            //DataObject data = new DataObject();
            //data.SetText((selectedItem.DataContext as Photo).Path);
            //data.SetData(DataFormats.StringFormat, (selectedItem.DataContext as Photo).Path);
            //data.SetData("Photo", selectedItem.DataContext as Photo);//?

            //DragDrop.DoDragDrop(selectedItem, data, DragDropEffects.Copy | DragDropEffects.Move);
            DragDrop.DoDragDrop(selectedItem, (selectedItem.DataContext as Photo).Path, DragDropEffects.Copy | DragDropEffects.Move);
        }

    }//*/
}
