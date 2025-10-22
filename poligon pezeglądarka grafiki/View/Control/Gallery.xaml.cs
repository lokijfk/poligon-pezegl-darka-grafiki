
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
    private bool edit = false;    
    private ListBoxItem? selectedItem = null;
        
    public Gallery()
    {
        InitializeComponent();
    }
      

    #region Old code



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

    /*
    public void List1CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
    {
        // Your logic here
        Debug.WriteLine("Collection changed in Gallery");
        // to nie działa
        Lista.SelectedIndex = 0;
        Lista.ScrollIntoView(Lista.SelectedIndex);
    }
    */

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
    #endregion Old code

    #region TextBox events
    //tu gdzieś jest błąd który powoduje że po powrocie do textblock obrazy nie wyświetlają się na pełnym ekranie
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
            e.Handled = true;

        }else if (e.Key == Key.Escape)
        {
            TextBox textBox = sender as TextBox;
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();//GetChildrenTBO(textBox.GetParentAsGrid());
            lb.Visibility = System.Windows.Visibility.Visible;
            edit = false;
            e.Handled = true;
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
            edit = false;
        }
    }


    private void TextBoxActivate(TextBox textBox)
    {
        //Debug.WriteLine("TextBoxActivate");
        if (textBox != null)
        {
            //Debug.WriteLine("if TextBoxActivate - textBox != null");
            TextBlock textBlock = textBox.GetSisTextBlock();
            if (textBlock != null)
            {
                //Debug.WriteLine("TextBoxActivate - textBlock != null");
                textBlock.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                if (textBox.IsVisible) { 
                    //Debug.WriteLine("TextBoxActivate - textBox.IsVisible");
                    textBox.Focus();
                    if (textBox.IsFocused)
                    {
                        //Debug.WriteLine("TextBoxActivate - textBox.IsFocused");
                        textBox.Select(0, textBox.Text.Length - 4); // ustawia kursor na końcu tekstu
                    }
                }
            }
            else
            {
                edit = false;
                //Debug.WriteLine("TextBoxActivate - textBlock == null");
            }
        }
    }

    
    #endregion TextBox events

    #region menuitems
    // obsługa menu kontekstowego
    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine("klik menu: "+ (selectedItem.DataContext as Photo).Name);
        if (selectedItem != null)
        {
            edit = true;
            TextBox textBox = (selectedItem as DependencyObject).GetCHildTextBox() as TextBox;
            if (textBox != null) TextBoxActivate(textBox);
        }
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

    #endregion menuitems

    #region ListBox & ListBoxItem events

    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {        
        if ((!edit) && (Lista.SelectionMode == SelectionMode.Single))
        {
            ListBox List = sender as ListBox;
            Photo ph = List.SelectedItem as Photo;
            int curentIndex = 0;
            var listitem = List.GetListBoxItem(e.GetPosition(List));            
            if ((ph != null) && (listitem != null) && (ph == listitem.DataContext))
            {
                TextBox textBox = listitem.GetTexBox(e.GetPosition(listitem));
                if (textBox != null)
                { 
                    edit = true;
                    TextBoxActivate(textBox);
                }
                else
                {
                    var index = (DataContext as MainWindowViewModel).Photos.IndexOf(ph);
                    ViewWindow viewWindow = new()
                    {
                        DataContext = new ViewWindowViewModel(index,
                        (this.DataContext as MainWindowViewModel).Photos)
                    };                
                    viewWindow.ShowDialog();//Show nie czeka na zamknięcia okna, ShowDialog czeka na zamknięcie okna
                    viewWindow.Activate(); // aktywacja okna
                    curentIndex = (viewWindow.DataContext as ViewWindowViewModel).currentImageIndex;
                    if (curentIndex >= 0)
                    {
                        List.SelectedIndex = curentIndex; // ustawienie zaznaczenia na liście
                        List.ScrollIntoView(List.SelectedItem); // przewinięcie do zaznaczonego elementu
                    }
                }
            }
        }
    }

    private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        selectedItem = sender as ListBoxItem;
    }

    private void ListBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            ListBox listBox = sender as ListBox;
            ListBoxItem item = listBox.GetListBoxItem(e.GetPosition(listBox));
            string[] paths; 
            if (item != null)
            {
                if(listBox.SelectedItems.Count == 0)
                {
                    //Debug.WriteLine("przeciąganie pojedyńczego elementu listy, Selecteditems: "+Lista.SelectedItems.Count);
                    paths = [((Photo)item.DataContext).Path];
                }
                else
                {
                    //Debug.WriteLine("przeciąganie wielu elementów listy, Selecteditems: " + Lista.SelectedItems.Count);
                    var selectedPhotos = Lista.SelectedItems.Cast<Photo>().ToList();
                    //Debug.WriteLine("Liczba zaznaczonych elementów: " + selectedPhotos.Count);
                    paths = selectedPhotos.Select(p => p.Path).ToArray();
                }
                //jak zrobię zaznaczanie kilku elementów to trzeba będzie tu dodać dodawanie ich do tablicy
                var effect = DragDrop.DoDragDrop(item, new DataObject(DataFormats.FileDrop, paths),
                    DragDropEffects.Copy | DragDropEffects.Move);
                if (effect == DragDropEffects.Move)
                {
                    (DataContext as MainWindowViewModel).MoveFileToFolder();
                    //Debug.WriteLine("przeniesiono plik");
                }
                //Debug.WriteLine("efekt przeciągania:" + effect);
            }
        }
    }

    private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Debug.WriteLine("kliknięto element listy");


        ListBoxItem item = sender as ListBoxItem;
        if (item != null)
        {
            // Anuluj zaznaczenie elementu
            item.IsSelected = false;
            e.Handled = true; // Zapobiega dalszemu przetwarzaniu zdarzenia
        }
    }

    private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("preview kliknięto element listy");

        selectedItem = sender as ListBoxItem;
        Debug.WriteLine("listboxitem preview");
        
    }

    private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //trzeba to zmienić bo generuje problemy i błędy
        //Debug.WriteLine("preview mouse left button down");
        if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) 
            //&& ((Lista.SelectionMode != SelectionMode.Single)||(Lista.SelectionMode != SelectionMode.Extended)))
        {
            //Debug.WriteLine("ctrl wciśnięty");
            Lista.SelectionMode = SelectionMode.Multiple;
        }
        else if((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
        {
            //Debug.WriteLine("shift wciśnięty");
            Lista.SelectionMode = SelectionMode.Extended;
        }
    }
    private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("preview mouse left button up");
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)&& !Keyboard.IsKeyDown(Key.LeftShift)&& !Keyboard.IsKeyDown(Key.RightShift))
        {
            Lista.SelectionMode = SelectionMode.Single;
        }
    }
    /// <summary>
    /// uruchamiane się jak klikamy poza elementami listy ale na liście
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //uruchamia się jak klikamy poza elementami listy ale na liście
        if((Lista.SelectionMode == SelectionMode.Multiple)||(Lista.SelectionMode == SelectionMode.Extended))
        {
            //Debug.WriteLine("klik poza elementami listy, czyszczenie zaznaczenia");
            Lista.SelectedItems.Clear();
        }
        
        Lista.SelectionMode = SelectionMode.Single;
        Lista.SelectedItem = null;
        if (edit)
        {
            edit = false;
            Lista.Focus();
        }
        //Debug.WriteLine("lista");
    }

    private void ListBox_KeyDown(object sender, KeyEventArgs e)
    {
        //przewidywana obsługa klawisza enter do otwierania okna podglądu
        //dla jednego i wielu zaznaczonych elementów
        if ((e.Key == Key.Enter)&&(edit == false))
        {
            Debug.WriteLine("enter w liście");
        }
        else
        {
            //Debug.WriteLine("edycja");
        }
    }





    #endregion ListBox & ListBoxItem events

    
}
