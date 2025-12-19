using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Controls;
using Microsoft.Win32;
using poligon_pezeglądarka_grafiki.ViewModel;


namespace poligon_pezeglądarka_grafiki.View.Control;

/// <summary>
/// Logika interakcji dla klasy Directories.xaml
/// </summary>
public partial class Directories : UserControl
{
    //public ObservableCollection<SSS> list = []; 
    /*
    private string previousView = string.Empty;
    public Directories(string previousView)
    {        
        InitializeComponent();
        this.previousView = previousView;
    }

    public string PreviousView
    {
        get { return previousView; }
        set { previousView = value; }
    }*/

    //public static readonly string Identyfier = "Settings";

    public Directories()
    {
        //DataContext = new MainWindowViewModel();
        //string path = (this.DataContext as MainWindowViewModel).TreePath;
        //path.Split(';').ToList().ForEach(path => list.Add(path));
        InitializeComponent();
        //LList.ItemsSource = list;


    }


    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        /*
        string path = (this.DataContext as MainWindowViewModel).TreePath;

        foreach(var p in path.Split(';')) list.Add(new SSS { path=p});
        //path.Split(';').ToList().ForEach(path => list.Add(path));
        //var p = (this.DataContext as MainWindowViewModel).TreePath;
        Debug.WriteLine("wielkość: " + list.Count);
        //LList.ItemsSource = list;
        */

    }

    private void Button_Click_AddFolder(object sender, System.Windows.RoutedEventArgs e)
    {
        var ofd = new OpenFolderDialog();
        _ = ofd.ShowDialog();
        string path = ofd.FolderName;
        //Debug.WriteLine (path);
        _ = (this.DataContext as MainWindowViewModel).AddRootFolderToTree(path);
        /*if ((this.DataContext as MainWindowViewModel).AddFolder(path))
        {
            LList.UpdateLayout();
            Debug.WriteLine("update");
        }*/

    }

    private void Button_Click_ExcludeFolder(object sender, System.Windows.RoutedEventArgs e)
    {
        //do zmiany !! 
        var ofd = new OpenFolderDialog();
        _ = ofd.ShowDialog();
        string path = ofd.FolderName;
        //Debug.WriteLine (path);
        _ = (this.DataContext as MainWindowViewModel).AddRootFolderToTree(path);
    }

    private void Expander_Expanded(object sender, System.Windows.RoutedEventArgs e)
    {
        var expander = sender as Expander;
        var parent = expander.Parent;
        //if(parent != kontener)
        foreach (var ch in kontener.Children)
        {
            if ((ch is Expander) && (ch != expander))
            {
                (ch as Expander).IsExpanded = false;
            }

        }
        // expander.IsExpanded = true;
    }

    private void Button_Click_Usun(object sender, System.Windows.RoutedEventArgs e)
    {
        var button = sender as Button;
        //if(button.Parent != null)Debug.WriteLine(button.Parent.GetType().Name);
        if (button.Parent is WrapPanel)
        {
            var parent = button.Parent as WrapPanel;
            foreach (var ch in parent.Children)
            {
                if (ch is Label)
                {
                    (this.DataContext as MainWindowViewModel).RemoveFolder(ch.ToString().Substring(ch.ToString().IndexOf(':') + 1).Trim());

                }
            }
        }
        /*
        Debug.WriteLine(parent.ToString());
        if (parent != null)
        {
            (this.DataContext as MainWindowViewModel).RemoveFolder(parent.Content.ToString());

        }*/
    }

    private void Button_Click_UsunD(object sender, System.Windows.RoutedEventArgs e)
    {
        // Do zmiany !!
        var button = sender as Button;
        //if(button.Parent != null)Debug.WriteLine(button.Parent.GetType().Name);
        if (button.Parent is WrapPanel)
        {
            var parent = button.Parent as WrapPanel;
            foreach (var ch in parent.Children)
            {
                if (ch is Label)
                {
                    var cy = ch as Label;
                    (this.DataContext as MainWindowViewModel).RemoveFolder(cy.Content.ToString());
                }
            }
        }
        /*
        Debug.WriteLine(parent.ToString());
        if (parent != null)
        {
            (this.DataContext as MainWindowViewModel).RemoveFolder(parent.Content.ToString());

        }*/
    }

    private void DList_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = System.Windows.DragDropEffects.Copy;
    }
}
