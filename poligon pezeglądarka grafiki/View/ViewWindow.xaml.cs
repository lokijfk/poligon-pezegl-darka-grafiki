using System.Windows;
using poligon_pezeglądarka_grafiki.ViewModel;

namespace poligon_pezeglądarka_grafiki.View;

/// <summary>
/// Logika interakcji dla klasy ViewWindow.xaml
/// </summary>
public partial class ViewWindow : Window
{
    public ViewWindow()
    {
        DataContext = new ViewWindowViewModel();
        InitializeComponent();
    }
}
