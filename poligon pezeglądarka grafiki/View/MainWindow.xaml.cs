
using Microsoft.Win32;
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
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
     * 
     * zrobić nowy widok z miniatruami, ma ładować miniatury w zależności od wielkości kolejnej
     * i sprawszać czy się zmieści, jak się zmiesci to dodaje a jak nie to przechodzi do kolejnej linijki 
     * i tam ją dodaje 
     * 
     */
    #region Zmienne prywatne
    private TreeViewItem? menuSelectedItem = null;
    private System.Windows.Media.Brush temp = null;


    /// <summary>
    /// zmienne pomocnicze do rozwijania drzewa katalogów
    /// zapobieganie rozwinięciu się natychmiast, tylko po pewnym czasie
    /// wspólny timer
    /// </summary>
    private System.Windows.Threading.DispatcherTimer? dispatcherTimer = null;
    private TreeViewItem? treeViewItemToEx = null;
    private bool TimerTest = false;

    #endregion Zmienne prywatne

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
            MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
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
        try
        {
            if (textBox != null)
            {
                TextBlock textBlock = textBox.GetSisTextBlock();                
                if (menuSelectedItem == null) menuSelectedItem = textBlock.GetTreeViewItem();
                if ((menuSelectedItem != null) && (textBlock != null))
                {
                    textBlock.Visibility = Visibility.Collapsed;
                    textBox.Height = menuSelectedItem.ActualHeight;//a jak to idzie z double click?
                    textBox.MaxHeight = textBox.FontSize * 1.5;
                    textBox.Width = menuSelectedItem.ActualWidth;
                    textBox.Margin = new Thickness(0, 0, 0, 0);
                    textBox.Padding = new Thickness(0, 0, 0, 0);
                    textBox.BorderThickness = new Thickness(0);//obramowanie jest z treViewItem i na razie nie mam na to wpływu
                    textBox.Visibility = Visibility.Visible;
                    _ = textBox.Focus();                    
                    textBox.SelectAll();
                    //textBox.Select(TabIndex, textBox.Text.Length); // ustawia kursor na końcu tekstu
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
        TextBox textBox = sender as TextBox;
        if (e.Key == Key.Enter)
        {
            // ma odbierać info z MainWindowViewModel i jak jest błąd to go wyświetlić w oknie
            // jeżeli zwróci true to ok a okno wyświetlać po stonie MainWindowViewModel

            if ((textBox.DataContext as TreeModel).Name != textBox.Text)
            {
                //Debug.WriteLine(" ok - key down text box - enter - rename folder: " + textBox.Text);
                //(DataContext as MainWindowViewModel).RenameFolder(textBox.DataContext as TreeModel, textBox.Text);
                (DataContext as MainWindowViewModel).RenameFolder(textBox.DataContext as TreeModel, textBox.Text);
            }

            //TextBlock lb = textBox.GetSisTextBlock();            
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

    private void SetForeground(object sender)
    {
        if ((sender is TreeViewItem treeViewItem) && temp != null)
        {
            treeViewItem.Foreground = temp;
        }
    }

    private void TreeViewItem_Drop(object sender, DragEventArgs e)
    {
        SetForeground(sender);
    }

    

    /// <summary>
    /// zmiana kolory napisu podczas przeciągania pliku nad elementem drzewa
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeViewItem_DragEnter(object sender, DragEventArgs e)
    {
        if(sender  is TreeViewItem treeViewItem)
        {            
            if (temp == null)
            {
                temp = treeViewItem.Foreground;
            }
            if (DataContext is MainWindowViewModel mv)
            {                
                treeViewItem.Foreground = mv.GetDropCollor();
            }
            TreeModel model = (TreeModel)treeViewItem.DataContext;
            if(model.Children.Count > 0)
            {
                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    treeViewItemToEx = treeViewItem;
                    dispatcherTimer.Tick += new EventHandler(TreeExpand);                
                    dispatcherTimer.Interval = TimeSpan.FromSeconds(1);//rozwija drzewo po sekundzie
                    dispatcherTimer.Start();
                }
            }
        }
    }

    private void TreeExpand(object sender, EventArgs e)
    {
        if (treeViewItemToEx != null && dispatcherTimer != null)
        {
            treeViewItemToEx.IsExpanded = true;
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= TreeExpand;
            dispatcherTimer = null;
            treeViewItemToEx = null;
        }
    }

    /// <summary>
    /// sprzątaie, czyli przywraca kolor napisu po opuszczeniu elementu drzewa
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TreeViewItem_DragLeave(object sender, DragEventArgs e)
    {
        SetForeground(sender);
        dispatcherTimer?.Stop();
        if (dispatcherTimer != null) dispatcherTimer.Tick -= TreeExpand;
        dispatcherTimer = null;

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
        if (e.Data == null || (e.Effects == DragDropEffects.None))
        {
            Debug.WriteLine("e.data == null or TreeView_Drop brak efektu");
            return;
        }        
        TreeView treeView = (TreeView)sender;
        TreeViewItem treeViewItem = treeView.GetItem(e.GetPosition(treeView));

        if (treeViewItem == null)
        {
            Debug.WriteLine("treeview_Drop: brak treeviewitem");// ok to się pokazuje
            //tu muszę podjąć inną akcję ale tylko wtedy jak jest przeciągany katalog
            //i to najlepiej z poza programu, bo przeciąganie z galerii jest tylko do katalogu a nie do pustego miejsca w drzewie
            return;
        }

        //if (treeViewItem != null && temp != null)
        //{
        //    treeViewItem.Foreground = temp;
        //}
        SetForeground(treeViewItem); //zmiana koloru napisu

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            //zostawiam zmienne dla przejżystości kodu, ale można je usunąć i użyć bezpośrednio w metodzie
            string[] dataStrings = (string[])e.Data.GetData(DataFormats.FileDrop);            
            var vm = (MainWindowViewModel)this.DataContext;

            vm.MoveFileToFolder(dataStrings, (TreeModel)treeViewItem.DataContext, e.KeyStates.HasFlag(DragDropKeyStates.ControlKey));
            /*
            foreach (var dataString in dataStrings)
            {                
                if (!File.Exists(dataString) && Directory.Exists(dataString))
                {
                    //Debug.WriteLine("przenoszenie katalogu");
                    if(dataString != target.Path)
                    vm.MoveFoderToFolder(dataString, target);
                    //if (temp != null)
                    //{
                    //    treeViewItem.Foreground = temp;
                    //}
                    SetForeground(treeViewItem);
                }
                else
                {
                    vm.MoveFileToFolder(dataString, target, copy);
                    //jak przenosimy plik do tego samego katalogu to zmienia jego nazwę zamast wstrzymać się
                }                
            }
            */
            SetForeground(treeViewItem);
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


    //private System.Windows.Threading.DispatcherTimer? dispatcherTimerPMLBD = null;
    //private TreeViewItem? treeViewItemToPMLBD = null;
    
    private void TreeView_MouseMove(object sender, MouseEventArgs e)
    {        
        try
        {
            if (e.LeftButton == MouseButtonState.Pressed && TimerTest)
            {
                TimerTest = false;
                if (sender is TreeView treeView)
                {   
                    TreeViewItem item = treeView.GetItem(e.GetPosition(treeView));
                    TreeModel? dc = treeView.SelectedItem as TreeModel;
                    string[] paths;
                    if ((dc != null) && (item != null))
                    {
                        paths = [dc.Path];
                        var effect = DragDrop.DoDragDrop(item, new DataObject(DataFormats.FileDrop, paths),
                          DragDropEffects.Copy | DragDropEffects.Move);
                    }
                    if ((dc != null) && (item == null))
                    {
                        Debug.WriteLine("TreeView_MouseMove - brak itemu do przeciągnięcia");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}");
        }
    }

    private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {        
        if((sender is TreeViewItem item) &&(e.LeftButton == MouseButtonState.Pressed))
        {                       
            if (dispatcherTimer == null)
            {
                //dispatcherTimerPMLBD = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                //treeViewItemToPMLBD = item; 
                dispatcherTimer.Tick += new EventHandler(mouseDownX);
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
                dispatcherTimer.Start();
            }
        }
    }

    private void StopTimer()
    {
        if (dispatcherTimer != null)
        {
            TimerTest = true;
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= mouseDownX;
            dispatcherTimer = null;
        }
    }


    /// <summary>
    /// wstrzymuje Timer i ustawia test na true
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mouseDownX(object sender, EventArgs e) 
    {
        StopTimer();
    }
    
    private void TreeViewItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        StopTimer();
        SetForeground(sender);
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

    #endregion TreeView

    #region Menu Context

    /// <summary>
    /// jeszcze nie dokończone
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_AddDir(object sender, RoutedEventArgs e)
    {
    //#pragma warning disable CS8602 // Wyłuskanie odwołania, które może mieć wartość null.
        
        if (menuSelectedItem.DataContext is TreeModel TM)
        {
            var newItem = ((MainWindowViewModel)this.DataContext).AddFolder(TM);            
            if (menuSelectedItem.Items.Count > 0)
            {
                menuSelectedItem.IsExpanded = true;
            }
            //zapewnia czas na utworzenie wszystkich potrzebnych elementów wizualnych
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += (sender, e) => { TickEvent(sender, e, newItem); };
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
                dispatcherTimer.Start();
            }
        }
    //#pragma warning restore CS8602 // Wyłuskanie odwołania, które może mieć wartość null.        
    }


    private void TickEvent(object sender, EventArgs e, TreeModel newItem)
    {
        if (dispatcherTimer != null)
        {            
            dispatcherTimer.Stop();
            dispatcherTimer = null;            
        }
        if (menuSelectedItem.ItemContainerGenerator.ContainerFromItem(newItem) is TreeViewItem tvi)
        {                            
            if (tvi.GetCHildTextBox() is TextBox textBox)TextBoxActivate(textBox);
        }
    }
    
    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {
        if (menuSelectedItem != null)
        {
            var t = menuSelectedItem.GetCHildTextBox();
            if (t is TextBox x) TextBoxActivate(x);
            menuSelectedItem = null;
        }
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