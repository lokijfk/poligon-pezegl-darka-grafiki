
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;


namespace poligon_pezeglądarka_grafiki;

public partial class App : Application
{
    public readonly string Version = "1.0.0 beta";

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

    

    #region test DI
    /*
    
    public App()
    {
        Services = ConfigureServices();
        //Startup += Application_Startup;
    }

    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }
    
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        //services.AddSingleton<IFilesService, FilesService>();
        //services.AddSingleton<ISettingsService, SettingsService>();
        //services.AddSingleton<IClipboardService, ClipboardService>();
        //services.AddSingleton<IShareService, ShareService>();
        //services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<BrokerIni>();
        return services.BuildServiceProvider();
    }
    */

    #endregion
    /*
    /// <summary>
    /// sprawdza czy nie ma urochomionej innej instancji tego programu
    /// pozwala tylko na uruchomienie tylko jeden wersji
    /// </summary>
    /// <param name="e"></param>
    protected override void OnStartup(StartupEventArgs e)
    {
        Process thisProc = Process.GetCurrentProcess();
        if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
        {
            MessageBox.Show("Application is already running.");
            Application.Current.Shutdown();
            return;
        }

        base.OnStartup(e);
    }
    */
}

