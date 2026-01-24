using Microsoft.Win32;
using poligon_pezeglądarka_grafiki.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace poligon_pezeglądarka_grafiki.View.Control;

/// <summary>
/// Logika interakcji dla klasy FileList.xaml
/// </summary>
public partial class FileList : UserControl
{
    public FileList()
    {
        //Debug.WriteLine("-- jest: FileList");
        //DataContext = new MainWindowViewModel();
        InitializeComponent();
    }

    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var index = (e.Source as ListBox).SelectedIndex;
        if (index < 0)
        {
            Debug.WriteLine("MouseDoubleClick: invalid index");
            return;
        }
        var FList = (this.DataContext as MainWindowViewModel)?.Photos;
        var p = FList[index].Path + "\\" + FList[index].Name;
        ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(p) };
        viewWindow.Show();
        //Debug.WriteLine("MouseDoubleClick:"+sender.ToString()+" event:"+(e.Source as ListBox).SelectedIndex );
    }
}
