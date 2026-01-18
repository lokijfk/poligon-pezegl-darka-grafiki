
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;




namespace poligon_pezeglądarka_grafiki.View.Control;

/// <summary>
/// Logika interakcji dla klasy Gallery.xaml
/// </summary>
public partial class Gallery : UserControl
{
    /*NOTATKI
     * dodać obsługę klawiszy np: enter przy zaznaczonych elementach do otwierania okna podglądu
     * delete i shift+delete, 
     * 
     * 
     * 
     * 
     */
    public bool edit = false;
    private ListBoxItem? selectedItem = null;



    public Gallery()
    {
        InitializeComponent();

    }


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
            if ((textBox.DataContext as Photo).Name != textBox.Text)
                (DataContext as MainWindowViewModel).RenameFile(textBox.DataContext as Photo, textBox.Text);
            e.Handled = true;

        }
        else if (e.Key == Key.Escape)
        {
            TextBox textBox = sender as TextBox;
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();//GetChildrenTBO(textBox.GetParentAsGrid());
            lb.Visibility = System.Windows.Visibility.Visible;
            edit = false;
            e.Handled = true;
        }
        if (edit)
        {
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
                if (textBox.IsVisible)
                {
                    //Debug.WriteLine("TextBoxActivate - textBox.IsVisible");
                    _ = textBox.Focus();
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
            selectedItem = null;
            if (textBox != null) TextBoxActivate(textBox);
        }
    }


    #endregion

    #region ListBox & ListBoxItem events
    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox list)
        {
            if ((!edit) && (list.SelectionMode == SelectionMode.Single))
            {
                if (list.SelectedItem is Photo ph)
                {
                    var listitem = list.GetListBoxItem(e.GetPosition(list));
                    if (e.OriginalSource.GetType().Name == "Image")
                    {

                        if (listitem != null && ph == listitem.DataContext)
                        {

                            ViewSelectedImage(list, ph);
                        }
                    }
                    else if (e.OriginalSource.GetType().Name == "TextBlock")
                    {
                        TextBox textBox = (TextBox)listitem.GetCHildTextBox();
                        if (textBox != null)
                            TextBoxActivate(textBox);
                    }
                    //Debug.WriteLine("orginal source: " + e.OriginalSource.GetType().Name);
                }
            }
        }
    }

    // Zmiana przekazania kolekcji Photos do ViewWindowViewModel - przypisz do zmiennej przed przekazaniem
    /// <summary>
    /// wyświetla wybrane image na pełnym oknie
    /// </summary>
    /// <param name="List"></param>
    /// <param name="ph"></param>
    private void ViewSelectedImage(ListBox List, Photo ph)
    {
        int curentIndex = 0;
        var mainWindowViewModel = DataContext as MainWindowViewModel;
        if (ph != null && mainWindowViewModel != null && mainWindowViewModel.Photos != null)
        {
            var index = mainWindowViewModel.Photos.IndexOf(ph);
            var photos = mainWindowViewModel.Photos; // przypisz do zmiennej, aby spełnić wymagania ref readonly
            ViewWindow viewWindow = new()
            {
                DataContext = new ViewWindowViewModel(index, in photos) // <-- poprawka: dodaj "in"
            };
            _ = viewWindow.ShowDialog();
            _ = viewWindow.Activate();
            var viewWindowViewModel = viewWindow.DataContext as ViewWindowViewModel;
            if (viewWindowViewModel != null)
            {
                curentIndex = viewWindowViewModel.currentImageIndex;
                if (curentIndex >= 0)
                {
                    List.SelectedIndex = curentIndex;
                    List.ScrollIntoView(List.SelectedItem);
                }
            }
        }
    }

    private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        selectedItem = sender as ListBoxItem;
        if (Lista.SelectedItems.Count > 1)
        {
            //Debug.WriteLine("więcej niż jeden element zaznaczony");
            Lista.SelectionMode = SelectionMode.Extended;
        }
        _ = (DataContext as MainWindowViewModel).RefreshClipboardListenerResoult();
    }

    private void ListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _ = (DataContext as MainWindowViewModel).RefreshClipboardListenerResoult();

    }

    /*private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        
    }*/

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
        else if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
        {
            //Debug.WriteLine("shift wciśnięty");
            Lista.SelectionMode = SelectionMode.Extended;
        }
        else
        {
            if (sender is ListBox lisBox)
            {
                if (lisBox.SelectedItems.Count <= 1)
                {
                    Lista.SelectionMode = SelectionMode.Single;
                }
            }
        }
    }
    private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("preview mouse left button up");
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
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
        if ((Lista.SelectionMode == SelectionMode.Multiple) || (Lista.SelectionMode == SelectionMode.Extended))
        {
            //Debug.WriteLine("klik poza elementami listy, czyszczenie zaznaczenia");
            Lista.SelectedItems.Clear();
        }

        Lista.SelectionMode = SelectionMode.Single;
        Lista.SelectedItem = null;
        if (edit)
        {
            edit = false;
            _ = Lista.Focus();
        }
        //Debug.WriteLine("lista");
    }

    // Poprawka dla IDE0019 i CS8600 w metodzie ListBox_KeyDown


    private void ListBox_KeyDown(object sender, KeyEventArgs e)
    {
        //Debug.WriteLine("naciśnięto klawisz w ListBox: " + e.Key.ToString());
        //to omija InputBindings tam mam problem z tym bo to jest wywoływane tylko po stronie interfejsu
        // i nie ma nic wspólnego z danymi, no oprócz elementu wybranego
        if ((e.Key == Key.Enter) && (edit == false))
        {
            if (sender is ListBox list)
            {
                if ((!edit) && (list.SelectionMode == SelectionMode.Single))
                {
                    if (list.SelectedItem is Photo ph)
                    {
                        ViewSelectedImage(list, ph);
                    }
                }
                //tu dodać jeszcze tryb z zaznaczonymi wieloma obrazami
            }
        }

    }

    #endregion ListBox & ListBoxItem events

    #region DragDrop
    private void ListBox_MouseMove(object sender, MouseEventArgs e)
    {
        //tu powinno być dodane że jakiś element jest zaznaczony 
        // i że mysz nie jest nad paskiem przewijania, czy obszarem przewijania        
        try
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //ListBox listBox = sender as ListBox;
                if (sender is ListBox listBox)
                {
                    ListBoxItem item = listBox.GetListBoxItem(e.GetPosition(listBox));
                    string[] paths;
                    //Photo[] selectedPhotos = [];
                    List<Photo> selectedPhotos = new List<Photo>();
                    if (item != null && listBox != null)
                    {
                        if (listBox.SelectedItems.Count == 0)
                        {
                            //Debug.WriteLine("przeciąganie pojedyńczego elementu listy, Selecteditems: "+Lista.SelectedItems.Count);
                            paths = [((Photo)item.DataContext).Path];
                        }
                        else
                        {
                            //Debug.WriteLine("przeciąganie wielu elementów listy, Selecteditems: " + Lista.SelectedItems.Count);
                            selectedPhotos = Lista.SelectedItems.Cast<Photo>().ToList();
                            //selectedPhotos = Lista.SelectedItems.Cast<Photo>().ToArray();
                            //Debug.WriteLine("Liczba zaznaczonych elementów: " + selectedPhotos.Count);
                            paths = [.. selectedPhotos.Select(static p => p.Path)];
                        }
                        //jak zrobię zaznaczanie kilku elementów to trzeba będzie tu dodać dodawanie ich do tablicy

                        var effect = DragDrop.DoDragDrop(listBox, new DataObject(DataFormats.FileDrop, paths),
                            DragDropEffects.Copy | DragDropEffects.Move);

                        //var effect = DragDrop.DoDragDrop(item, new DataObject(DataFormats.FileDrop, paths),
                        //    DragDropEffects.Copy | DragDropEffects.Move);
                        if (effect == DragDropEffects.Move)
                        {
                            //to jest potrzebne jak przenosimy plik do innego folderu lub poza program
                            // i ma zresetować kolejkę plików, czyli usunąć te przeniesione
                            //ale usuwa również jak nie powinny być przeniesione,
                            //to trzeba poprawić lub dać to w innym miejscu
                            if (DataContext is MainWindowViewModel viewModel)
                            {
                                //ok to jest błąd bo odświeża cały katalog a powinien usuwać tylko obiekty reprezentacji plików z kolejki
                                //do przerobienia
                                //Debug.WriteLine("ListBox_MouseMove(object sender, MouseEventArgs e) -- przeniesiono plik");
                                viewModel.RemoveFileFromPhotos(selectedPhotos.ToList());
                                //viewModel.MoveFileToFolder();//??
                                                             
                            }
                        }
                        //Debug.WriteLine("efekt przeciągania:" + effect);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// zmiana kursora w trakcie przeciągania określa jaka akcja będzie zrobiona
    ///  podgląd efektów operacji przeciągania i upuszczania
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_DragEnter(object sender, DragEventArgs e)
    {
        //Debug.WriteLine("ListBox_DragEnter");
        /*
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {

            if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        */
    }

    /// <summary>
    /// efekt upuszczenia tutaj pliku, wykonanie czegoś...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_Drop(object sender, DragEventArgs e)
    {

        //na początek trzeba określić w jakim katalogu jesteśmy
        //defakto jest to wyciąganie na siłę ściezki z MV którą będziemy do niego zwracali
        //może zrobić odpowiednią metodę która będzie to uwzględniałą
        //string path = "";
        /*
        //skopiowane z TreeView z MainWindow
        TreeView treeView = (TreeView)sender;
        TreeViewItem treeViewItem = treeView.GetItem(e.GetPosition(treeView));
        */
        //Debug.WriteLine("ListBox_Drop 1 - upuszczono plik(effects != none): " + e.Effects);
        //tu nie wiem co zrobić, jak rozróżnić z kąd to idzie ;(
        //if ((sender is ListBox listBox) && (e.Source is ListBox listBox2) && (listBox == listBox2))
        //{
        //    if (listBox == Lista && listBox2 == Lista)
        //    {
        //        Debug.WriteLine("źródło = przeznaczenia: "+e.Source.GetType());

        //        //return;
        //    }
        //}

        //tu nie wiem jak rozwiązać problem z wrzucaniem grafiki

        if (e.Data.GetDataPresent(DataFormats.FileDrop) && (e.Effects != DragDropEffects.None))
        {
            //Debug.WriteLine("orginal source: "+e.Source.GetType().Name);
            //Debug.WriteLine("ListBox_Drop - upuszczono plik(effects != none): "+e.Effects.ToString());
            string[] dataStrings = (string[])e.Data.GetData(DataFormats.FileDrop);
            MainWindowViewModel mv = (MainWindowViewModel)this.DataContext;
            foreach (var dataString in dataStrings)
            {
                //tu dodać sprawdzenie czy dany plik już nie znajduje się w tym folderze o ile to jest przenoszenie
                //jak to jest kopiwanie to też bo trzeba zmienić nazwę
                // w taki sposób uniknę niechcianych zmian

                if (mv.FileIsFolder(dataString)) return;

                if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                {
                    //Debug.WriteLine("TreeView_Drop Copy: ");
                    //foreach (var dataString in dataStrings)//kopiowanie
                    //{
                    mv.MoveFileToFolder(dataString, true);
                    //}
                }
                else if (e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey))
                {
                    //przenoszenie 
                    mv.MoveFileToFolder(dataString);
                }
            }
            //tu muszę dodać jakoś odświeżenie galerii o ile dodaję do katalogu który jest aktualnie wyświetlany
            //dodawać na sam koniec a nie odświeżać, to zajmuje czas
        }

    }
    private void ListBox_DragOver(object sender, DragEventArgs e)
    {
        //Debug.WriteLine("ListBox_DragOver -pre efekt: " + e.Effects.ToString());
        e.Effects = DragDropEffects.None;//??

        //Debug.WriteLine("ListBox_DragOver");
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] dataStrings = (string[])e.Data.GetData(DataFormats.FileDrop);
            //pobieranie wzorców rozszezeń z wspólnego punktu
            string pattern = ((MainWindowViewModel)this.DataContext).pattern;
            Match m;
            foreach (var dataString in dataStrings)
            {
                //jeżeli istnieje taki plik
                if (System.IO.File.Exists(dataString) && Path.HasExtension(dataString))
                {
                    //pobiera rozszeżenie przenoszonego pliku
                    string ext = Path.GetExtension(dataString).ToLower();
                    m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {

                        if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                        {
                            e.Effects = DragDropEffects.Copy;
                        }
                        else e.Effects = DragDropEffects.Move;
                        //e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                        //Debug.WriteLine("ListBox_DragOver - plik graficzny: "+dataString);
                    }
                    else e.Effects = DragDropEffects.None;
                }
                //jeżeli istnieje taki katalog
                else if (Directory.Exists(dataString) && !Path.HasExtension(dataString))
                {
                    e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                    //Debug.WriteLine("ListBox_DragOver - katalog: " + dataString);
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }

            }
        }
        //Debug.WriteLine("ListBox_DragOver - after efekt: " + e.Effects.ToString());
    }







    #endregion DragDrop

}
