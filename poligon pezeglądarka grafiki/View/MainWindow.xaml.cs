
using Microsoft.Win32;
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;
//using static MaterialDesignThemes.Wpf.Theme;





namespace poligon_pezeglądarka_grafiki;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /*NOTATKI
     * !!! dodać startowe ustawienie w ini oraz w ustawieniach programu przycisk przywracania do ustawień starowych
     * niby są ale każde jest poustawiane gdzieś indziej, można je zostawić jako awaryjne ale nadal się przydzadzą opowiednio zbudowane
     * dodać resztę skrótów klawiszowych do tego okna
     * trzeba dodać obsługę błędów przy dodawaniu folderu do drzewa
     * dodać obsługę błędów przy przenoszeniu plików
     * dodać obsługę błędów przy zmianie nazwy folderu
     * dodać obsługę błędów przy usuwaniu folderu
     * dodać możliwość edytowania nazwy  przy dodawaniu nowego folderu
     * 
     * 
     */
    private TreeViewItem? menuSelectedItem = null;
    private System.Windows.Media.Brush temp;

    public MainWindow()
    {
        InitializeComponent();
    }

    #region Window

    /// <summary>
    /// występuje tylko w oknie i jest wywoływane po utworzeniu okna, ale przed jego pokazaniem
    /// tylko tu da się ją wywołać, opcjonalnie można zrobić własne okno dziedziczące po Window
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
    }

    public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETMINMAXINFO)
        {
            // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
            // including the task bar.
            MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            //MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf<MONITORINFO>();
                _ = GetMonitorInfo(monitor, ref monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        return IntPtr.Zero;
    }

    private const int WM_GETMINMAXINFO = 0x0024;

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT(int left, int top, int right, int bottom)
    {
        public int Left = left;
        public int Top = top;
        public int Right = right;
        public int Bottom = bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT(int x, int y)
    {
        public int X = x;
        public int Y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    #endregion Window

    #region zdarzenia textbox   

    /// <summary>
    /// aktywuje textbox na potrzeby zmiany nazwy w podanym treeviewitem
    /// </summary>
    /// <param name="textBox"></param>
    private void TextBoxActivate(TextBox textBox)
    {
        Debug.WriteLine("TextBoxActivate");
        try
        {
            if (textBox != null)
            {
                TextBlock textBlock = textBox.GetSisTextBlock();
                //if (textBlock != null) 
                if (menuSelectedItem == null) menuSelectedItem = textBlock.GetTreeViewItem();
                if ((menuSelectedItem != null) && (textBlock != null))
                {
                    Debug.WriteLine("TextBoxActivate: " + (menuSelectedItem.DataContext as TreeModel).Name);
                    textBlock.Visibility = Visibility.Collapsed;
                    textBox.Height = menuSelectedItem.ActualHeight;//a jak to idzie z double click?
                    textBox.MaxHeight = textBox.FontSize * 1.5;
                    textBox.Width = menuSelectedItem.ActualWidth;
                    textBox.Margin = new Thickness(0, 0, 0, 0);
                    textBox.Padding = new Thickness(0, 0, 0, 0);
                    textBox.BorderThickness = new Thickness(0);//obramowanie jest z treViewItem i na razie nie mam na to wpływu
                    textBox.Visibility = Visibility.Visible;
                    _ = textBox.Focus();
                    textBox.Select(TabIndex, textBox.Text.Length); // ustawia kursor na końcu tekstu
                    //tu nie wiem czemu nie zanznacza tekstu, tylko ustawia kursor na końcu
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        //Debug.WriteLine(" ok - key down text box");
        TextBox textBox = sender as TextBox;
        if (e.Key == Key.Enter)
        {
            Debug.WriteLine(" ok - key down text box - enter");
            /*
             textBox.Visibility = System.Windows.Visibility.Collapsed;
             TextBlock lb = textBox.GetSisTextBlock();
             lb.Visibility = System.Windows.Visibility.Visible;
            */
            // ma odbierać info z MainWindowViewModel i jak jest błąd to go wyświetlić w oknie
            // jeżeli zwróci true to ok a okno wyświetlać po stonie MainWindowViewModel

            if ((textBox.DataContext as TreeModel).Name != textBox.Text)
            {
                Debug.WriteLine(" ok - key down text box - enter - rename folder: " + textBox.Text);
                //(DataContext as MainWindowViewModel).RenameFolder(textBox.DataContext as TreeModel, textBox.Text);
                (DataContext as MainWindowViewModel).RenameFolder(textBox.DataContext as TreeModel, textBox.Text);
            }

            TextBlock lb = textBox.GetSisTextBlock();
            //lb.UpdateLayout();
        }
        if ((e.Key == Key.Escape) || (e.Key == Key.Enter))
        {
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = textBox.GetSisTextBlock();
            lb.Visibility = System.Windows.Visibility.Visible;
            //Debug.WriteLine("tree model: " + (routed.SelectedItem as TreeModel).Name);// nie ma zaznaczenia
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
            textBox.Text = (textBox.DataContext as TreeModel).Name; // reset nazwy do oryginalnej, jeżeli nie zmieniono
        }
    }

    #endregion zdarzenia

    #region Widoki i przyciski główne
    /// <summary>
    /// wywołuje okno edycji
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditWindow_Click(object sender, RoutedEventArgs e)
    {
        //MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
        //string x = vm.SelectedTreeItem;// to jest katalog
        //vm.Photos.Where(static p => p.IsSelected).Select(static p => p.Path).ToList().ForEach(p => Debug.WriteLine("EditWindow_Click: " + p));
        //string y  = vm.Photos.Where(static p => p.IsSelected).Select(static p => p.Path).First();
        //Debug.WriteLine("EditWindow_Click: " +y);
        if (DataContext is MainWindowViewModel vm)
        {
            //dodać wykrywanie czy coś jest zaznaczone
            if (vm.Photos.Where(static p => p.IsSelected).Select(static p => p.Path).First() is string photo 
                && photo != String.Empty)
            {


                //string photo = vm.Photos.Where(static p => p.IsSelected).Select(static p => p.Path).First();

                EditWindow editWindow = new()
                {
                    DataContext = new EditWindowViewModel(photo),//(vm.Photos.Where(static p => p.IsSelected).Select(static p => p.Path).First()),
                    Owner = this
                };
                editWindow.ShowDialog();
                _ = editWindow.Activate();
                if (editWindow.isSaved)
                {
                    //tu trzeba dodać odświeżenie galerii, ale to chyba już jest w vm editwindow
                    vm.RefreshGalleryAfterEdit(photo);
                    //Debug.WriteLine("EditWindow_Click - isSaved: " + editWindow.isSaved);
                }
                //else
                //{
                //    Debug.WriteLine("EditWindow_Click - isSaved: " + editWindow.isSaved);
                //}
            }
        }
    }


    /// <summary>
    /// zdażenie kliknięcia na elementy menu sortowania na pasku narzędziowym
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ClickSort(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem MI)
        {
            (DataContext as MainWindowViewModel).SortAD(MI.DataContext);
        }
    }

    /// <summary>
    /// obsługa przycisku dodawania folderu do drzewa
    /// przycisk w oknie drzewa i na pasku narzędziowym
    /// otwiera okno systemowe wyboru folderu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_AddFolder(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFolderDialog();
        bool? result = ofd.ShowDialog();
        if (result == true)
        {
            string path = ofd.FolderName;
            //Debug.WriteLine (path);
            _ = (DataContext as MainWindowViewModel).AddRootFolderToTree(path);
        }
    }
    #endregion widoki


    #region Tree DragDrop


    private void TreeViewItem_Drop(object sender, DragEventArgs e)
    {
        var treeViewItem = sender as TreeViewItem;

        treeViewItem.Foreground = temp;
    }


    private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
    private TreeViewItem treeViewItemToEx = null;
    /// <summary>
    /// zmiana kolory napisu podczas przeciągania pliku nad elementem drzewa
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeViewItem_DragEnter(object sender, DragEventArgs e)
    {
        //Debug.WriteLine("TreeViewItem_DragEnter");
        var treeViewItem = sender as TreeViewItem;
        var mv = (this.DataContext as MainWindowViewModel);       
        if (!mv.GetDropCollor().Equals(treeViewItem.Foreground))
            temp = treeViewItem.Foreground;
        treeViewItem.Foreground = mv.GetDropCollor();
        TreeModel model = (TreeModel)treeViewItem.DataContext;
        if(model.Children.Count > 0)
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                treeViewItemToEx = treeViewItem;
                dispatcherTimer.Tick += new EventHandler(TreeExpand);                
                dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                dispatcherTimer.Start();
            }
        }
    }

    private void TreeExpand(object sender, EventArgs e)
    {
        treeViewItemToEx.IsExpanded = true;
        dispatcherTimer.Stop();
        dispatcherTimer = null;
    }

    /// <summary>
    /// sprzątaie, czyli przywraca kolor napisu po opuszczeniu elementu drzewa
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeViewItem_DragLeave(object sender, DragEventArgs e)
    {
        var treeViewItem = sender as TreeViewItem;        
        treeViewItem.Foreground = temp;
        if(dispatcherTimer != null)
        {
            dispatcherTimer.Stop();
            dispatcherTimer = null;
        }
        
    }

    

    /// <summary>
    /// zwraca informację o tym czy można upuścić plik w dane miejsce
    /// ale nie ogranicza tego co się stanie przy upuszczeniu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeView_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] dataStrings = (string[])e.Data.GetData(DataFormats.FileDrop);            
            foreach (var dataString in dataStrings)
            {
                if(System.IO.File.Exists(dataString) || Directory.Exists(dataString))
                {
                    //var mv = (this.DataContext as MainWindowViewModel);

                    if (System.IO.File.Exists(dataString))
                    {
                        if (DataContext is MainWindowViewModel mv)
                        {
                            if (Path.HasExtension(dataString) && !mv.ExtO(Path.GetExtension(dataString).ToLower()))
                            {
                                return;
                            }
                        }else return;
                    }
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else //if(e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey))
                        e.Effects = DragDropEffects.Move;
                    //e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                }

                //if (System.IO.File.Exists(dataString) && Path.HasExtension(dataString))
                //{
                //    string ext = Path.GetExtension(dataString).ToLower();
                //    var mv = (this.DataContext as MainWindowViewModel);
                //    //to jakoś trzeba zamienić na rozszeżenia brane z ustawień
                //    //if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".tiff" || ext == ".webp")
                //    if(mv.ExtO(ext))//sięganie do mv, żeby sprawdzić czy rozszerzenie jest obsługiwane, globalne ustawienia dla progrmu
                //    {
                //        if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                //        {
                //            e.Effects = DragDropEffects.Copy;
                //        }
                //        else //if(e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey))
                //            e.Effects = DragDropEffects.Move;
                //        //e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                //    }
                //}
                //else if (Directory.Exists(dataString) && !Path.HasExtension(dataString))
                //{
                //    //e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                //    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                //    {
                //        e.Effects = DragDropEffects.Copy;
                //    }
                //    else //if (e.KeyStates.HasFlag(DragDropKeyStates.ShiftKey))
                //        e.Effects = DragDropEffects.Move;
                //}

            }
        }
    }


    /// <summary>
    /// to jest efekt upuszczenia pliku na drzewo
    /// metoda wykonawcza znajduje się w MainWindowViewModel
    /// tam też jest sprawdzenie czy plik jest obrazem
    /// tu sprawdzam czy efekt jest różny od null
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeView_Drop(object sender, DragEventArgs e)
    {
        //Debug.WriteLine("TreeView_Drop");
        //Debug.WriteLine()
        //tu gzieś dodać znacznik że ma odświeżyć elementy interfejsu
        if (e.Data == null || (e.Effects == DragDropEffects.None))
        {
            Debug.WriteLine("e.data == null or TreeView_Drop brak efektu");
            return;
        }

        //znacznik = 0; // reset znacznik po upuszczeniu pliku
        TreeView treeView = (TreeView)sender;
        TreeViewItem treeViewItem = treeView.GetItem(e.GetPosition(treeView));
        treeViewItem.Foreground = temp;
        
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] dataStrings = (string[])e.Data.GetData(DataFormats.FileDrop);
            bool copy = false;
            var vm = (MainWindowViewModel)this.DataContext;
            foreach (var dataString in dataStrings)
            {
                if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                {
                    copy = true;
                }else copy = false;
                if (!File.Exists(dataString) && Directory.Exists(dataString))
                {
                    Debug.WriteLine("przenoszenie katalogu");
                    vm.MoveFoderToFolder(dataString, (TreeModel)treeViewItem.DataContext);
                }
                //dodać sprawdzanie czy to plik czy też katalog, katalog wysyłać do innej metody
                //na katalog można też tutaj inaczej zareagować niż w vm


                //tu by się przydało żeby moveFileToFolder przyjmowało TreeModel jako drugi parametr
                //może wtedy by sie unikneło niepotrzebnego wyszukiwania tego elementu w drzewie
                //((MainWindowViewModel)this.DataContext).MoveFileToFolder(dataString, ((TreeModel)treeViewItem.DataContext).Path, true);
                vm.MoveFileToFolder(dataString, (TreeModel)treeViewItem.DataContext, copy);
            }
                       
            //tu muszę dodać jakoś odświeżenie galerii o ile dodaję do katalogu który jest aktualnie wyświetlany
            /*
             //ten element w zasadzie już nie istnieje po upuszczeniu i tu generuje błąd
            if(selectedItem != null)
            {   
                Debug.WriteLine("TreeView_Drop - bring into view: " + (selectedItem.DataContext as TreeModel).Name);
                selectedItem.IsExpanded = true;
                selectedItem.BringIntoView();
            }
            */
        }
    }
    private void TreeView_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Debug.WriteLine("TreeView_MouseMove");
                TreeView treeView = sender as TreeView;
                //treeView.SelectedItem as TreeModel;
                //Debug.WriteLine("TreeView_MouseMove selectedItem: " + (treeView.SelectedItem != null ? (treeView.SelectedItem as TreeModel).Name : "null"));
                if ((treeView != null))//&&(selectedItem != null))
                {
                    //Debug.WriteLine("rozpoczęto przeciąganie: "+(selectedItem.DataContext as TreeModel).Name);
                    //ListBoxItem item = listBox.GetListBoxItem(e.GetPosition(listBox));
                    TreeViewItem item = treeView.GetItem(e.GetPosition(treeView));
                    TreeModel? dc = treeView.SelectedItem as TreeModel;
                    string[] paths;
                    if ((dc != null) && (item != null))
                    {
                        paths = [dc.Path];
                        var effect = DragDrop.DoDragDrop(item, new DataObject(DataFormats.FileDrop, paths),
                            DragDropEffects.Copy | DragDropEffects.Move);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}");
        }
    }

    
    #endregion Tree DragDrop


    #region TreeView

    /// <summary>
    /// potrzebne do zaznaczenia elementu do metod wywoływanych z menu kontekstowego
    /// zmiana nazwy, usuwanie folderu, dodawanie folderu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        menuSelectedItem = (sender as TreeViewItem);
        (DataContext as MainWindowViewModel).MenuSelectedItem((TreeModel)menuSelectedItem.DataContext);

    }

    /// <summary>
    /// obsługa zmiany nazwy folderu przez podwójne kliknięcie
    /// blokuje rozwijanie się drzewa przy podwójnym kliknięciu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /*
    private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true; // to powinno zablokować dalsze przetwarzanie
        if (sender is TreeViewItem treeViewItem) { 
            if (treeViewItem.DataContext == TreeViewX.SelectedItem)
            {            
                if (treeViewItem.GetCHildTextBox() is TextBox textBox)
                {
                    TextBoxActivate(textBox);
                }
                else Debug.WriteLine("none");
            }
        }         
    }*/

    /*
    private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        selectedItem = (sender as DependencyObject).GetTreeViewItem();   
  
        
        if(e.ClickCount == 2)
        {
            var org = (e.OriginalSource as TextBlock).Text;
            Debug.WriteLine("xxx TreeViewItem_PreviewMouseLeftButtonDown double click: " + org);
            e.Handled = true; // to powinno zablokować dalsze przetwarzanie , ale nie blokuje wywołania MouseDoubleClick !!

            if ((sender as TreeViewItem).DataContext != TreeViewX.SelectedItem)
            {
                Debug.WriteLine("xxx TreeViewItem_PreviewMouseLeftButtonDown double click - different selected item");
                Debug.WriteLine(" selected: " + (TreeViewX.SelectedItem as TreeModel).Name);
                Debug.WriteLine(" sender: " + ((sender as TreeViewItem).DataContext as TreeModel).Name);
                return;
            }
            //e.Handled = false;// to nic nie daje
                TreeViewItem treeViewItem = sender as TreeViewItem;
                TextBox textBox = treeViewItem.GetCHildTextBox() as TextBox;
                if (textBox != null)
                {                    
                    TextBoxActivate(textBox);
                }else Debug.WriteLine("none");            

        }
    }*/

    #endregion TreeView

    #region Menu Context

    /// <summary>
    /// jeszcze nie dokończone
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_AddDir(object sender, RoutedEventArgs e)
    {
    #pragma warning disable CS8602 // Wyłuskanie odwołania, które może mieć wartość null.
        
        if (menuSelectedItem.DataContext is TreeModel TM)
        {
            var newItem = ((MainWindowViewModel)this.DataContext).AddFolder(TM);
            //Debug.WriteLine($"menuSelectedItem: {(menuSelectedItem.DataContext as TreeModel).Name} - new item: {(newItem as TreeModel).Name}");
            if (menuSelectedItem.Items.Count > 0)
            {
                menuSelectedItem.IsExpanded = true;
            }

            RefreshAfterAddFolder(menuSelectedItem);
            //int index = menuSelectedItem.Items.IndexOf(newItem);            
            //if (menuSelectedItem.ItemContainerGenerator.ContainerFromIndex(index) is TreeViewItem tvi)
            var item = menuSelectedItem.ItemContainerGenerator.ContainerFromItem(newItem);
            RefreshAfterAddFolder(menuSelectedItem);
            menuSelectedItem.UpdateLayout();
            //TreeViewItem tvm = (TreeViewItem)(TreeViewX.ItemContainerGenerator.ContainerFromItem(item));
            //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(tvm);
            //if (myContentPresenter == null)
            //{

            //     myContentPresenter =
            //    (ContentPresenter)tvm.Template.FindName("ItemsHost", tvm);
            //    //if (itemsPresenter != null)
            //    //{
            //    //    itemsPresenter.ApplyTemplate();
            //    //}
            //}

            //DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            //TextBlock myTextBlock = (TextBlock)myDataTemplate.FindName("TextBoxRename", myContentPresenter);

            //MessageBox.Show("The text of the TextBlock of the selected list item: "
            //+ myTextBlock.Text);
            //problem występuje gdy dodaję do pustego folderu, wtedy item jest null
            //trzeba wymusić wygenerowanie kontenera
            
            
            if (item != null)
                //Debug.WriteLine("item: null");
            //else
            {
                //Debug.WriteLine($"item: {item.GetType().ToString()}");

                //if (menuSelectedItem.ItemContainerGenerator.ContainerFromItem(newItem) is TreeViewItem tvi)
                if (item is TreeViewItem tvi)
                {
                   // Debug.WriteLine($"menuSelectedItem: {(menuSelectedItem.DataContext as TreeModel).Name} - new item: {(newItem as TreeModel).Name}");
                    //Debug.WriteLine($"index: {index} - new item: {(newItem as TreeModel).Name}");


                    //tvi.IsSelected = true;
                    tvi.BringIntoView();
                    //tvi.UpdateLayout();                
                    RefreshAfterAddFolder(tvi);                    
                    if (tvi.GetCHildTextBox() is TextBox textBox)                
                    {
                        Debug.WriteLine("MenuItem_AddDir - TextBoxActivate");
                        TextBoxActivate(textBox);
                    }
                    else
                    {
                        if (tvi.GetCHildTextBoxEx() is TextBox textBox1)
                        {
                            Debug.WriteLine("MenuItem_AddDir - TextBoxActivate");
                            TextBoxActivate(textBox1);
                        }else
                            Debug.WriteLine("MenuItem_AddDir - Brak TextBox");
                    }
                    
                }
            }
        }
    #pragma warning restore CS8602 // Wyłuskanie odwołania, które może mieć wartość null.        
    }

    private childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);
            if (child != null && child is childItem)
            {
                return (childItem)child;
            }
            else
            {
                childItem childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
        }
        return null;
    }

    private void RefreshAfterAddFolder(TreeViewItem treeViewItem)
    {
        treeViewItem.ApplyTemplate();
        ItemsPresenter itemsPresenter =
            (ItemsPresenter)treeViewItem.Template.FindName("ItemsHost", treeViewItem);
        if (itemsPresenter != null)
        {
            itemsPresenter.ApplyTemplate();
        }
        else
        {
            itemsPresenter = FindVisualChild<ItemsPresenter>(treeViewItem);
            if (itemsPresenter == null)
            {
                treeViewItem.UpdateLayout();
                itemsPresenter = FindVisualChild<ItemsPresenter>(treeViewItem);
            }
        }
        Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);
        UIElementCollection children = itemsHostPanel.Children;
        //if (children != null)
        //{
        //    foreach (UIElement child in children)
        //    {
        //        if (child is TreeViewItem tvi)
        //        {
        //            tvi.ApplyTemplate();
        //        }
        //    }
        //}
        //else
        //{
        //    Debug.WriteLine("RefreshAfterAddFolder: children is null");
        //}
        }


    
    /// <summary>
    /// Search for an element of a certain type in the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of element to find.</typeparam>
    /// <param name="visual">The parent element.</param>
    /// <returns></returns>
    private T FindVisualChild<T>(Visual visual) where T : Visual
    {
        if (visual == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
        {
            Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
            if (child != null)
            {
                T correctlyTyped = child as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                T descendent = FindVisualChild<T>(child);
                if (descendent != null)
                {
                    return descendent;
                }
            }
        }

        return null;
    }
    
    /*
    private void MenuItem_DeleteDir(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("MenuItem_DeleteDir");
        //var parent = menuSelectedItem.Parent as TreeModel;
        //Debug.WriteLine("MenuItem_DeleteDir: " + (menuSelectedItem.DataContext as TreeModel).Path);
        bool x =((MainWindowViewModel)this.DataContext).DeleteFolder((TreeModel)menuSelectedItem.DataContext);

        //if(menuSelectedItem)
    }
    */
    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {
        var t = menuSelectedItem.GetCHildTextBox();
        TextBox textBox;// = (menuSelectedItem as DependencyObject).GetCHildTextBox() as TextBox;
    #pragma warning disable IDE0019
        if (t is TextBox x)
        {
            textBox = x;
            TextBoxActivate(textBox);
        }
    #pragma warning restore IDE0019
        menuSelectedItem = null; // reset zaznaczenia po aktywacji TextBoxa
        /*
        TextBlock textBlock = textBox.GetSisTextBlock();
        textBlock.Visibility = Visibility.Collapsed;
        textBox.Height = menuSelectedItem.ActualHeight;
        textBox.MaxHeight = textBox.FontSize * 1.5;
        textBox.Width = menuSelectedItem.ActualWidth;
        textBox.Margin = new Thickness(0, 0, 0, 0);
        textBox.Padding = new Thickness(0, 0, 0, 0);
        textBox.Visibility = Visibility.Visible;
        textBox.Focus();
        textBox.Select(TabIndex, textBox.Text.Length); // ustawia kursor na końcu tekstu
        //*/
    }

    private void MenuItem_Paste(object sender, RoutedEventArgs e)
    {
        //czy to da się zastąpić komendą w mv? na przykład PreviewMouseRightButtonDown
        (DataContext as MainWindowViewModel).MenuSelectedItem((TreeModel)menuSelectedItem.DataContext);
        _ = (DataContext as MainWindowViewModel).RefreshClipboardListenerResoult();
    }










    #endregion Menu Context

    #region EndGame



    #endregion

    
}