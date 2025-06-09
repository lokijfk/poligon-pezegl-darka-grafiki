using Microsoft.Win32;
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;


namespace poligon_pezeglądarka_grafiki;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        //DataContext = new MainWindowViewModel();
        InitializeComponent();
    }

    #region Window
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
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                GetMonitorInfo(monitor, ref monitorInfo);
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
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
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
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
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

    #region zdarzenia
/*
    private void treeView_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        //Debug.WriteLine("SelectionChanged");
        (this.DataContext as MainWindowViewModel)?.SetSelectedItem(e.NewValue);
    }

    
    private void MenuOpen_Click(object sender, RoutedEventArgs e)
    {
        DrawerHostSettings.IsLeftDrawerOpen = false;
        if (ActualWidth > 1600)
        {
            //NavRail.Visibility = Visibility.Collapsed;
            //MenuToggleButton.Visibility = Visibility.Visible;
        }

    }

    private void MenuToggleButton_OnClick(object sender, RoutedEventArgs e)
    {
        //DemoItemsSearchBox.Focus();
        //MenuOpen.IsChecked = true;
        if (ActualWidth > 1600)
        {
            //NavRail.Visibility = Visibility.Collapsed;
            //MenuToggleButton.Visibility = Visibility.Collapsed;
        }

    }
   

    private void ListBoxItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        //tu wywołanie dodatkowego okna z poziomu MainWindowViewModel do zarządzania katalogami
        //(this.DataContext as MainWindowViewModel)?.SetSelectedItem(e.NewValue);
    }
*/
    private void Button_AddFolder(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFolderDialog();
        bool? result = ofd.ShowDialog();
        if (result == true)
        {
            string path = ofd.FolderName;
            //Debug.WriteLine (path);
            (this.DataContext as MainWindowViewModel).AddFolder(path);
        }
    }


    /*
    private void TVI_MouseWheel(object sender, MouseWheelEventArgs e)
    {

        TreeViewItem tvi = sender as TreeViewItem;
        DependencyObject item = tvi;
        while (item != null && !(item is ScrollViewer))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        if ((item is ScrollViewer)&&(item != null)) 
        {
            ScrollViewer scrollViewer = item as ScrollViewer;
            
            if (e.Delta > 0)
                scrollViewer.LineUp();
                //SCVX.LineUp();
            else
                scrollViewer.LineDown();
                //SCVX.LineDown();
            
            Debug.WriteLine(sender.GetType()+" Delta: "+e.Delta+" scv: "+scrollViewer.Name+" item: "+item.GetType());

        }
        //tu pokazuje na etykietę lub na element drzewa i trzeba to przekierować wyżej
        // jak pokazuję na pasek przewijania to przewija 
    }*/

    #endregion zdarzenia

    #region Widoki

    //to będzie do przerobienia, przenieść wykonanie  do mv
    private void Button_Click_ViewGrid(object sender, RoutedEventArgs e)
    {
        var mv = (this.DataContext as MainWindowViewModel);
        mv.SelectedView = "Gallery";
        mv.SelectionChangedCommand.Execute(this);
    }

    private void Button_Click_ViewList(object sender, RoutedEventArgs e)
    {
        //["Hello", "FDataGrid", "FList","Gallery"];

        var mv = (this.DataContext as MainWindowViewModel);
        mv.SelectedView = "FList";
        mv.SelectionChangedCommand.Execute(this);
    }

    private void Button_Click_SettingsFolder(object sender, RoutedEventArgs e)
    {
        var mv = (this.DataContext as MainWindowViewModel);
        mv.SelectedView = "SettingdFolder";
        mv.SelectionChangedCommand.Execute(this);
    }


    #endregion widoki


    #region Tree DragDrop
    Point _startPoint;
    bool _IsDragging = false;
    private void treeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {        
        _startPoint = e.GetPosition(null);        
        var tree = sender as TreeView;
        TreeViewItem item = tree.GetItem(e.GetPosition(tree));        
        var DC = item.DataContext as TreeModel; 
        //Debug.WriteLine(DC.Path); // działa !!! EUREKA  
    }


    private void treeView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
       // Debug.WriteLine("PreviewMouseMove");

        if (e.LeftButton == MouseButtonState.Pressed ||
    e.RightButton == MouseButtonState.Pressed && !_IsDragging)
        {

            var tree = sender as TreeView;
            TreeViewItem item = tree.SelectedItem as TreeViewItem;
            var DC = item.DataContext as TreeModel;
           // Debug.WriteLine(DC.Path); // działa !!! EUREKA
            //var val = tree.selectedvalue;
            //Debug.WriteLine(tree.SelectedItem.GetType().Name.ToString());
            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - _startPoint.X) >
                    SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _startPoint.Y) >
                    SystemParameters.MinimumVerticalDragDistance)
            {
               // StartDrag(e);//sender to treeView a jak wydobyć item ??
               /*
               Debug.WriteLine(sender.GetType().Name);
               if(sender is TreeViewItem)
                {
                    var tVI = sender as TreeViewItem;
                    Debug.WriteLine("w drzewie");
                }
               */
            }
        }
    }




    #endregion Tree DragDrop


}