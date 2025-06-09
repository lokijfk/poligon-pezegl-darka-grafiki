using poligon_pezeglądarka_grafiki.View;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Windows;


namespace poligon_pezeglądarka_grafiki;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {

        if((e.Args.Length > 0) && (File.Exists(e.Args[0])))
        {
            ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(e.Args[0]) };
            viewWindow.Show();
        }
        else
        {
            MainWindow mainView = new MainWindow { DataContext = new MainWindowViewModel() };
            mainView.Show();
        }
        /*
        if (e.Args.Length == 0)
        {
            MainWindow mainView = new MainWindow { DataContext = new MainWindowViewModel()};
            mainView.Show();
        }
        else
        {
            
            for (int i = 0; i != e.Args.Length; ++i)
            {
                Debug.WriteLine("e.Args["+i+"]:" + e.Args[i]);
            }
            
            if (File.Exists(e.Args[0]))
            {
                ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(e.Args[0]) };
                viewWindow.Show();
                //
            }
            else
            {
                Debug.WriteLine(" plik "+ e.Args[1] + " nie istnieje !!");
                System.Windows.Application.Current.Shutdown();
                // tu można żucić jakimś błędem 
            }
        }
        */
        /*        //add some bootstrap or startup logic 
        var identity = AuthService.Login();
        if (identity == null)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
        }
        else
        {
            MainWindow mainView = new MainWindow();
            mainView.Show();
        }*/

        /*
        // Application is running
        // Process command line args
        bool startMinimized = false;
        for (int i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "/StartMinimized")
            {
                startMinimized = true;
            }
        }

        // Create main application window, starting minimized if specified
        MainWindow mainWindow = new MainWindow();
        if (startMinimized)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }
        mainWindow.Show();
        */
    }
}

