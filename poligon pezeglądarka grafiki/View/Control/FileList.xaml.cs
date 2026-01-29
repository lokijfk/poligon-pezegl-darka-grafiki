using Microsoft.Win32;
using poligon_pezeglądarka_grafiki.Model;
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
    public string Title { get; set; } = "File List Control";
    public FileList()
    {
        //Debug.WriteLine("-- jest: FileList");
        //DataContext = new MainWindowViewModel();
        InitializeComponent();
    }

    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (((ListView)sender).SelectedItem is Photo photo)
        {
            ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(photo.Path) };
            viewWindow.Show();
        }
    }

    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {

    }
}
