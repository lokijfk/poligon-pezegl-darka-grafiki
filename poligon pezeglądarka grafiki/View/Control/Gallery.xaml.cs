
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
    private bool isSelected = false;
    //private ListBoxItem? 
    public Gallery()
    {
        InitializeComponent();
       // ListBox List1;
       /*
        ((INotifyCollectionChanged)Lista.ItemsSource).CollectionChanged +=
        new NotifyCollectionChangedEventHandler(List1CollectionChanged);
       //*/
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

    /*
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


    private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("preview mouse down text block");
        TextBlock textBlock = sender as TextBlock;
        ListBoxItem item = textBlock.GetParentAsListBoxItem();
        if (item != null)
        {
            ListBox listBox = item.GetParentAsListBox();
            if (listBox != null)
            {
                if (listBox.SelectionMode == SelectionMode.Single)
                {                    
                    isSelected = true;                    
                }
                else
                {
                    isSelected = false;                    
                }
            }
        }
    }

    private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("mouse down text block");

        if (isSelected == true)
        {
          //  Debug.WriteLine("isSelected == true");
            if ((sender != null) && (sender is TextBlock) && (edit == false))
            {
                //    Debug.WriteLine("(sender != null) && (sender is TextBlock) && (edit == false)");

                // Debug.WriteLine(" ok - text blok mouse up 2");
                //if (selectedItem != null)
                //{

                //tu ze wszystkim ma problem i nie wiem czemu, czy to nie jest kolizja z innymi eventami?
                // np double click w liście?
                edit = true;
                    
                    //if ((selectedItem != null) && (selectedItem is ListBoxItem))
                    //{
                        //Debug.WriteLine("selectedItem != null && selectedItem is ListBoxItem: "
                        //+(selectedItem.DataContext as Photo).Name);
                        
                        //TextBox textBox = (Lista.SelectedItem as DependencyObject).GetCHildTextBox() as TextBox;
                        TextBlock label = sender as TextBlock;
                        Debug.WriteLine("label: " + label.Text);
                    TextBox textBox = ((sender as TextBlock).GetSisTexBox()) as TextBox;
                    if (textBox != null) TextBoxActivate(textBox);

                    //}
                //}
            }
            
            /*
            else
            {
                Debug.WriteLine("(sender != null) && (sender is TextBlock) && (edit == false)");
                Debug.WriteLine("sender: " + (sender != null) + " sender is TextBlock: " + (sender is TextBlock)
                        + " edit == false: " + (edit == false) + " edit : " + edit
                    );
            }
        }
    }*/
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
        //znacznik = 0; //znacznik do zmiany nazwy powoduje problemy
        //tu można dodać jescze Keyboard.IsKeyDown żeby sprawdzić czy ctrl nie jest wciśnięty
        //Debug.WriteLine("double click w liście, edit: "+edit.ToString()+ " Lista.SelectionMode: "+ Lista.SelectionMode.ToString());
        if ((!edit) && (Lista.SelectionMode == SelectionMode.Single))
        {
            //Debug.WriteLine("double click po teście 1");
            //e.Handled = true;
            ListBox List = sender as ListBox;
            Photo ph = List.SelectedItem as Photo;
            int curentIndex = 0;// = List.SelectedIndex;
            var listitem = List.GetListBoxItem(e.GetPosition(List));
            //Debug.WriteLine(x.GetType().ToString()+" : "+y.GetType().ToString());
            if ((ph != null) && (listitem != null) && (ph == listitem.DataContext))
            {

                    //to działa
                TextBox textBox = listitem.GetTexBox(e.GetPosition(listitem));
                if (textBox != null)
                {
                    //Debug.WriteLine("TextBox: " + textBox.Text);
                    edit = true;
                    TextBoxActivate(textBox);
                }
                else
                {
                    //Debug.WriteLine("TextBox is NULL");

                var index = (DataContext as MainWindowViewModel).Photos.IndexOf(ph);
                //Debug.WriteLine("start index: " + index);
                //ViewWindow viewWindow = new() { DataContext = new ViewWindowViewModel((y.DataContext as Photo).Path) };
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
    }

    private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        //to chyba nie będzie potrzebne lista automatycznie zaznacza element
        // i mozna go uzyskać jako selecteditem?
        selectedItem = sender as ListBoxItem;
    }

    private void ListBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            //Debug.WriteLine("przeciąganie elementu listy");
            ListBox listBox = sender as ListBox;
            ListBoxItem item = listBox.GetListBoxItem(e.GetPosition(listBox));
            string[] paths; 
            if (item != null)
            {
                //if (Lista.SelectionMode == SelectionMode.Single)
                if(listBox.SelectedItems.Count == 0)
                {
                    Debug.WriteLine("przeciąganie pojedyńczego elementu listy, Selecteditems: "+Lista.SelectedItems.Count);
                    paths = [((Photo)item.DataContext).Path];
                }
                else
                {
                    Debug.WriteLine("przeciąganie wielu elementów listy, Selecteditems: " + Lista.SelectedItems.Count);
                    var selectedPhotos = Lista.SelectedItems.Cast<Photo>().ToList();
                    Debug.WriteLine("Liczba zaznaczonych elementów: " + selectedPhotos.Count);
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

    /*
    private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //to nie jest wywoływane, możliwe że zostało przejęte przez inne eventy
        //Debug.WriteLine("kliknięto element listy");

        //e.Handled = true;//przejęcie obsługi zdarzenia - nie działa
        ListBoxItem item = sender as ListBoxItem;
        ListBox listBox = item.GetParentAsListBox();
        Debug.Assert(item == null, "item == null");
        Debug.Assert(item.IsSelected != false, " item zaznaczony");
        Debug.Assert(listBox.SelectionMode == SelectionMode.Single, "tryb single");
        Debug.Assert(Lista.SelectionMode == SelectionMode.Multiple, "tryb multiple Lista");
        Debug.WriteLine("listboxitem");

        if (e.ClickCount >= 1)
        {
            
            Debug.WriteLine("klik - pomijam");
            e.Handled = true; //przejęcie obsługi zdarzenia
            //Debug.WriteLine("double click - pomijam");
            return; //pomijanie podwójnego kliknięcia
        }
    }

    private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("preview kliknięto element listy");

        selectedItem = sender as ListBoxItem;
        /*
        ListBoxItem item = sender as ListBoxItem;
        if (item != null)
        {

            ListBox listBox = item.GetParentAsListBox();
            Debug.WriteLine("previewMLBD: "+(item.DataContext as Photo).Name);
            //if (item.IsSealed == true) item.IsSealed = false;
        }
        //Debug.Assert(item == null, "item == null");
        //Debug.Assert(item.IsSelected != false, " item zaznaczony");
        //Debug.Assert(listBox.SelectionMode == SelectionMode.Single, "tryb single");
        //Debug.Assert(Lista.SelectionMode == SelectionMode.Multiple, "tryb multiple Lista");
        Debug.WriteLine("listboxitem preview");
        
    }*/

    private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //trzeba to zmienić bo generuje problemy i błędy
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
