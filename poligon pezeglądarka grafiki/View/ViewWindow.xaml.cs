using System;
using System.Windows;
using poligon_pezeglądarka_grafiki.ViewModel;

namespace poligon_pezeglądarka_grafiki.View;

/// <summary>
/// 
/// </summary>
public partial class ViewWindow : Window //czemu musi być partial ??
{
    public ViewWindow()
    {
        //DataContext = new ViewWindowViewModel();
        InitializeComponent();
    }
}
