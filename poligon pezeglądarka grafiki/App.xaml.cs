using poligon_pezeglądarka_grafiki.View;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Windows;


namespace poligon_pezeglądarka_grafiki;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        {
            if ((e.Args.Length > 0) && (File.Exists(e.Args[0])))
            {
                ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(e.Args[0]) };
                viewWindow.Show();
            }
            else
            {
                MainWindow mainView = new MainWindow { DataContext = new MainWindowViewModel() };
                mainView.Show();
            }
        }
    }
}

