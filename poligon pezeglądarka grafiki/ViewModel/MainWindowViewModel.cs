using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View.Control;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Path = System.IO.Path;


namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    #region notatki
    /*
     * poprawić przenoszenie katalogu na inny dysk, systemow move tego nie obsługuje
     * 
     * generowanie tokena 
     * - można go zapisac w pliku bin jako sól do hasła do szyfrowania bazy danych
    using System.Security.Cryptography;
    byte[] randomBytes = new byte[32]; // 256 bitów
    RandomNumberGenerator.Fill(randomBytes);
    string token = Convert.ToBase64String(randomBytes);
    - dokończyć przenoszenie ustawień domyślnych do pliku ini
    - dodać odczyt ustawień domyslnych, resert do domyślnych itd...
    - dokończyć skalowanie miniatur i napisów pod nimi

    - poprawić widok do wyświetlania pełnoekranowego
    - dodać widok z miniaturami w liście prewijanej na dole i dużym podglądem na górze

    - dodać obsługę wielu monitorów
    */
    #endregion notatki

    #region Properties and [observableProperty]
    #region Collection
    //private readonly BrokerIni iniFile = BrokerIni.GetBroker();  
    private readonly BrokerIni BrokerIni = BrokerIni.GetBroker();
    //BrokerIni BrokerIni = App.Services.
    private ImageSource BlinkIcom { get; set; } = PhotoHelper.CreateEmtpyBitmapSource();
    
    /// <summary>
    /// kolekcja zawierająca tablicę z osobnymi drzewami katalogów do przeglądu
    /// </summary>
    public ObservableCollection<TreeModel>? Tree { get; set; } = [];

    /// <summary>
    /// lista wyliczeniowa możliwości sortowania
    /// </summary>  
    public ObservableCollection<string> TypSotowania { get; set; } = ["Nazwa", "Data", "Wielkość"];

    /// <summary>
    /// kolekcja przechowująca kolekcją obrazów w katalogu do urzytku w widoku galerii
    /// </summary>
    public ObservableCollection<Photo> Photos { get; set; } = [];

    public ObservableCollection<MenuRadioButton> MenuSort { get; set; } = []; 
    
    public ObservableCollection<Extension> Extensions { get; set; } = [];

    #endregion Collection

    [ObservableProperty]
    private SelectionMode _CurSelectionMode = SelectionMode.Single;    

    //============================================
    [ObservableProperty]
    private int _ThumbnailHeight = 200;

    //============================================
    [ObservableProperty]
    private bool _Writings = true;
    [ObservableProperty]
    private string _SelectedTreeItem = string.Empty;

    [ObservableProperty]
    private TreeModel selectedItem;

    [ObservableProperty]
    private string _SelectedView = string.Empty;//do wywołania zmiany widoku

    /// <summary>
    /// objekt żródłowy dla aktualnie wyświetlanego widoku
    /// </summary>
    [ObservableProperty]
    private object _selectedViewModel = new();

    [ObservableProperty]
    private string _WindowTitle = "Poligon - Przeglądarka Grafiki";

    [ObservableProperty]
    private bool _SwitchTglButton;

    [ObservableProperty]
    private bool restoreButton = false;

    [ObservableProperty]
    private bool maximizeButton = false;

    [ObservableProperty]
    private double _MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

    [ObservableProperty]
    private int _FilesToLoad = 0;

    [ObservableProperty]
    private int _FileLoaded = 0;


    /// <summary>
    /// odnośniki do katalogów do wyswietlenia w ustawieniach
    /// to jest błędne, wskazuje miejsce z którego został uruchomiony program a nie jego katalog
    /// w krócie na pulpicie wystarczy dodać katalog startowy i działa
    /// </summary>
    public string ActualPath { get => Directory.GetCurrentDirectory(); }
    public string InstallPath { get => BrokerFile.GetUserAppDataPath; }

    //public readonly DateTime CreationTime = File.GetCreationTime(Assembly.GetExecutingAssembly().Location);
    /// <summary>
    /// wykożystywane do wyświetlenia wersji w module "Welcome"
    /// </summary>
    public string Version { get => $"{GetVersion()}"; }

    private string GetVersion()
    {
        //var assembly = Assembly.GetExecutingAssembly();
        //var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        //return versionAttribute?.InformationalVersion ?? "Unknown Version";

        //$"Wersja:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public string StatusVersion
    {
        get
        {
            string version = GetVersion();
            if (CampareVersion(BrokerIni.Version))
            {
                return $"Wersja: {version} - posiadasz najnowszą wersję programu!";
            }
            else
            {
                return $"Wersja: {version} - program można zaktualizować!";
            }
        }
    }
    private bool CampareVersion(string version)
    {
        Version currentVersion = new Version(GetVersion());
        Version otherVersion = new Version(version);
        return currentVersion.CompareTo(otherVersion) == 0;
    }

    #region Interface
    public bool FirstAdd
    {
        get => !(TreePath.Count() > 0);
    }

    public bool VisibleSeparator
    {
        get => !BrokerIni.VisibleToolBar;
    }

    public bool VisibleToolBar
    {
        get => BrokerIni.VisibleToolBar;
        set => SetProperty(BrokerIni.VisibleToolBar, value, BrokerIni, static (u, n) => u.VisibleToolBar = n);
    }


    public Brush DropCollor
    {
        get => BrokerIni.DropCollor;
        set => SetProperty(BrokerIni.DropCollor, value, BrokerIni, static (u, n) => u.DropCollor = n);//nie wiem czy to zadziała
    }
    public bool VisibleStatusBar
    {
        get => BrokerIni.VisibleStatusBar;
        set => SetProperty(BrokerIni.VisibleStatusBar, value, BrokerIni, static (u, n) => u.VisibleStatusBar = n);
    }

    public bool VisibleFilesInTree
    {
        get => BrokerIni.VisibleFilesInTree;
        set => SetProperty(BrokerIni.VisibleFilesInTree, value, BrokerIni, static (u, n) => u.VisibleFilesInTree = n);
    }

    #endregion Interface

    private bool _cut = false;
    
    public string Pattern { get => @"\.(" + string.Join("|", BrokerIni.Extension.Split(',')) + ")";}    
    private string[] patternArray => BrokerIni.Extension.Split(',').Select(e => e.Trim().ToLower()).ToArray();
    //public string[] PatternArray => patternArray;

    #region Private

    /// <summary>
    /// zmienna wspomagająca wklejanie plików do katalogu wybranego w menu kontekstowym drzewa
    /// </summary>
    private TreeModel? MenuSelectedTreeItem = null;
    private bool counter = false;
    private CancellationTokenSource? cts = null;
    private CancellationToken token;
    string[] GalleryView = ["Gallery", "Gallery2", "GaleryCan"];
    string[] FileView = ["FDataGrid", "FList"];
    #endregion Private
    #endregion Properties and [observableProperty]       

    #region RelayCommand menu

    #region settings button
    /// <summary>
    /// kopiowanie plików do katalogu docelowego w profilu urzytkownika
    /// dodać sprawdzanie czy przypadkiem ta wersja nie jest już uruchomiona z tego katalogu
    /// dodać zastępowanie plików które tam są, to jest aktualizacja
    /// albo zrobić osobny przycisk aktualizacji
    /// dodać automatyczne uruchomienie wersji z katalogu docelowego i zamknięcie tej wersji - dodane!!
    /// </summary>
    [RelayCommand(CanExecute = nameof(InstallCanExecute))]
    private void Install()
    {
         MovingFilesUI();
    }




    /// <summary>
    /// usuwanie plików z katalogu " deinstalacja
    /// </summary>
    [RelayCommand(CanExecute = nameof(DeinstallCanExecute))]
    private void Deinstall()
    {
        Debug.WriteLine("Deinstalacja");
        string currentDir = Directory.GetCurrentDirectory();
        string appDir = BrokerFile.GetUserAppDataPath;
        string[] dir = Directory.GetDirectories(appDir);
        string[] files = Directory.GetFiles(appDir);
        if (currentDir != appDir)
        {
            foreach (string directory in dir)
            {
                BrokerFile.DeleteDirectory(directory);
                // DeleteFolder(directory);
            }
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                //string fileName = Path.GetFileName(file);
                if (ext != ".ini")//to tylko na czas testów, później jest do usunięcia
                {
                    //BrokerFile.DeleteFile(file);
                    BrokerFile.DeleteFileStrong(file);
                }
            }
            Application.Current.Shutdown();
        }
    }


    /// <summary>
    /// kopiuje pliki miedzy wybranymi katalogami, tylko z wybranymi rozszeżeniami
    /// pomija resztę. Zwraca ścieżkę do ostatniego pliku wykonywalnego jaki natrafi
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destDir"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    private string CopyFiles(string sourceDir, string destDir, bool overwrite = false)
    {
        string result = string.Empty;
        string[] files = Directory.GetFiles(sourceDir);
        string[] extDest = [".exe", ".dll", ".json", ".ico", ".ini", ".png"];
        foreach (string file in files)
        {
            if (File.Exists(file))
            {
                string ext = Path.GetExtension(file);
                if (extDest.Contains(ext.ToLower()))
                {
                    _ = FileMove(file, destDir, true,overwrite);
                }
                if (ext == ".exe")
                {
                    result = Path.Combine(destDir, Path.GetFileName(file));
                }
            }
        }
        return result;
    }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    private bool _InstallCanExecute = true;

    [RelayCommand]
    private void InsatallCanExecuteTest()
    {
        //Debug.WriteLine("InsatallCanExecuteTest");
        string directoryApp = BrokerFile.GetUserAppDataPath;
        string[] files = Directory.GetFiles(directoryApp);
        string sourceDir = Directory.GetCurrentDirectory();
        if (files.Length > 1)
        {
            //Debug.WriteLine("tam są jakieś pliki");
            InstallCanExecute = false;
        }
        else
        {
            //Debug.WriteLine("brak plików");
            InstallCanExecute = true;
        }
        //UpdateCanExecute = !InstallCanExecute;

        if (Directory.GetCurrentDirectory() != BrokerFile.GetUserAppDataPath)
        {
            DeinstallCanExecute = !InstallCanExecute;
            ShortcutCanExecute = !InstallCanExecute;
        }
        else
        {
            DeinstallCanExecute = false;
            ShortcutCanExecute = false;
            //Debug.WriteLine("test: "+Directory.GetCurrentDirectory()+" != "+ BrokerFile.GetUserAppDataPath);
        }
        if (directoryApp != sourceDir)
        {
            UpdateCanExecute = !InstallCanExecute;
        }
        else
        {
            UpdateCanExecute = false;
            //Debug.WriteLine("test: "+ directoryApp+" != "+sourceDir);
        }
        //Debug.WriteLine("InstallCanExecute: " + InstallCanExecute);
    }

    /// <summary>
    /// zdarzenie wywoływane przez DispatcherTimer //po zamknięciu innych instancji programu, pozwala na odświeżenie stanu przycisków i przeniesienie plików
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="dispatcherTimer"></param>
    private void TickEvent(object sender, EventArgs e, DispatcherTimer dispatcherTimer)
    {
        dispatcherTimer.Stop();        
        MovingFilesUI(true);
    }

    /// <summary>
    /// metoda odpowiada za operacje install i update, czyli przenoszenie plików i katalogów
    /// </summary>
    /// <param name="update"></param>
    private void MovingFilesUI(bool update = false)
    {        
        BrokerIni.Version = GetVersion();
        string sourcedir = Directory.GetCurrentDirectory();
        string destinyDir = BrokerFile.GetUserAppDataPath;
        ClearDirectory(destinyDir);
        string pathExe = CopyFiles(sourcedir, destinyDir);
        string[] subdirectories = Directory.GetDirectories(sourcedir);
        foreach (string directory in subdirectories)
        {
            if (Directory.GetDirectories(directory).Length > 0 || Directory.GetFiles(directory).Length > 0)
            {
                string dirNam = directory.Substring(directory.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                if (dirNam == "Img" || dirNam == "Config")
                {
                    string destSubDir = Path.Combine(destinyDir, dirNam);
                    _ = Directory.CreateDirectory(destSubDir);
                    _ = CopyFiles(directory, destSubDir);
                }
            }
        }
        if (pathExe != string.Empty && !update)
        {
            StartExe(pathExe);
            Application.Current.Shutdown();
        }
    }

    private bool ClouseAnotherVersion()
    {
        Process thisProc = Process.GetCurrentProcess();
        var procs = Process.GetProcessesByName(thisProc.ProcessName);
        //if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
        if (procs.Length > 1)
        {
            //MessageBox.Show("Application is already running.");// działa
            foreach (var proc in procs)
            {
                if (proc.Id != thisProc.Id)
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error closing process: {ex.Message}");
                    }
                }
            }
            //Application.Current.Shutdown();
            return true;
        }
        return false;
    }

    /// <summary>
    /// czyszczenie wybranego katalogu, pozostawia tylko pliki ini w katalogu głównym
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    private bool ClearDirectory(string directory)
    {
        string[] files = Directory.GetFiles(directory);
        foreach (string file in files)
        {
            if (Path.GetExtension(file) != ".ini") File.Delete(file);
        }
        string[] dirs = Directory.GetDirectories(directory);
        foreach (string dir in dirs)
        {
            Directory.Delete(dir, true);
        }
        return false;
    }

    /// <summary>
    /// Wykonuje operację aktualizacji poprzez zamknięcie innej wersji aplikacji i uruchomienie licznika czasu, aby kontynuować
    /// proces aktualizacji.
    /// </summary>
    /// <remarks>Ta metoda jest przeznaczona do obsługi poleceń i może zostać wykonana tylko wtedy, gdy
    /// powiązana metoda CanExecute zwróci wartość true. Należy ją wywołać w scenariuszach, w których aplikacja musi
    /// upewnić się, że żadna inna wersja nie jest uruchomiona przed kontynuowaniem aktualizacji.</remarks>
    [RelayCommand(CanExecute = nameof(UpdateCanExecute))]
    private void Update()
    {
        //zamknięcie innych instancji programu, które mogą być uruchomione, a które blokują aktualizację
        ClouseAnotherVersion();
        //opóźnienie działania w celu umożliwienia zamknięcia innych instancji programu, które mogą być uruchomione, a które blokują aktualizację
        DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        dispatcherTimer.Tick += (sender, e) => { TickEvent(sender, e, dispatcherTimer); };//wywołanie metody po upływie czasu, która wykona aktualizację
        dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
        dispatcherTimer.Start();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    private bool _UpdateCanExecute = false;

    private void StartExe(string path, string param = "")
    {
        Debug.WriteLine($"StartExe called with path: {path}, param: {param}");
        //if (File.Exists(path))
        //to powoduje ryzyko powstania błędu
        if (!string.IsNullOrEmpty(path))
        { 
            try
            {
                if (!string.IsNullOrEmpty(param))
                {
                    //to jest po to żeby można było wywołać komendy systemu pn explorer
                    _ = Process.Start(path, param);
                }
                else
                {
                    if (File.Exists(path))
                    {
                        //Debug.WriteLine($"StrtExe:{path}");
                        //ale tu to naprawiam
                        _ = Process.Start(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wystąpił błąd: {ex.Message}");
                Log.Write(LogLevel.Error, $"Wystąpił błąd: {ex.Message}");
            }
        }
    }



    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeinstallCommand))]
    private bool _DeinstallCanExecute = false;

    /// <summary>
    /// otwiera katalog programu w explorerze
    /// katalog programu to katalog dzie jest plik ini i gdzie zostanie zainstalowany program
    /// </summary>
    [RelayCommand]
    private void OpenExplorer(string path)
    {
        path = BrokerFile.GetUserAppDataPath;
        //Debug.WriteLine($"OpenExplorer called with path: {path}");
        StartExe("explorer", path);
    }

    /// <summary>
    /// tworzy skrót na pulpicie o ile program jest zainstalowany
    /// </summary>
    [RelayCommand(CanExecute = nameof(ShortcutCanExecute))]
    private void ShortcutCall()
    {
        string env = BrokerFile.GetUserAppDataPath;
        string PathToExe = Path.Combine(env, "poligon pezeglądarka grafiki.exe");
        string PathToIco = Path.Combine(env, @"poligon pezeglądarka grafiki.exe");
        string lnkFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Poligon PG.lnk");
        if (File.Exists(PathToExe) && File.Exists(PathToIco))
        {
            Shortcut.Create(lnkFileName, PathToExe, null, env, "Poligon Przeglądarka Grafiki", string.Empty, PathToIco);
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShortcutCallCommand))]
    private bool _ShortcutCanExecute = false;


    #endregion

    [RelayCommand]
    private void ShowDialog()
    {
        //Debug.WriteLine($"ShowDialog called");
        ShowDialogProgresBarr();
    }

    [RelayCommand]
    private void ReloadView()
    {
        //Debug.WriteLine($"reload view - selectView: {SelectedViewWindow}");
        //ChangeView(SelectedViewWindow);
        SelectedView = SelectedViewWindow;//to na wszelki wypadek
        SelectionChanged(SelectedView);
    }

    public void RefreshGalleryAfterEdit(string PathToPhoto)
    {
        if (PathToPhoto != string.Empty)
        {
            //załadowanie pierwszego?? po co 
            var photo = Photos.Where(p => p.Path.Equals(PathToPhoto)).First();
            if (photo != null)
            {
                photo.Load(token);
            }

            var directory = Directory.GetCurrentDirectory().Count();
            if (directory > Photos.Count)
            {
                var path = System.IO.Path.GetDirectoryName(PathToPhoto);
                if (Directory.Exists(path))
                {
                    string newFile = BrokerFile.GetNewFile(path, patternArray);
                    Photo elem = Photos.FirstOrDefault(p => p.Path == PathToPhoto);
                    int pos = IndexOf(elem);
                    //Debug.WriteLine($"index:{pos}");
                    newPhoto(newFile, pos + 1);  //tu dodawanie elementu do Photos                
                }
                //Debug.WriteLine(newFile);//ok
            }
        }
    }


    public int IndexOf(object item)
    {
        var e = Photos.GetEnumerator();
        var idx = 0;
        while (e.MoveNext())
        {
            if (Equals(item, e.Current))
                return idx;
            else
                idx++;
        }
        //if we've got this far it means that the item is either filtered out
        //or is not in the source collection
        return -1;
    }

    /// <summary>
    /// obsługa zmiany widoków, polecenie z parametrem
    /// </summary>
    /// <param name="sel">nazwa widoku, wielkość liter jest ważna</param>
    [RelayCommand]
    private void SelectionChanged(object sel)
    {
        if ((sel is string sx) && (!string.IsNullOrEmpty(sx)))
        {
             //Debug.WriteLine($"Selection Changed: {sx} ");
            //SelectedView = sx;
            if (!sx.Equals("Settings") && !sx.Equals("Welcome"))
            {                
                SelectedViewWindow = sx;//zapis do ini
            }
            if (SelectedView == sx)
            {
                SelectedView = SelectedViewWindow;
                SelectedViewModel = CallMethod(SelectedView);
            }
            else
            {
                if (SelectedViewModel != null) SelectedViewModel = null;
                SelectedView = sx;
                SelectedViewModel = CallMethod(sx);
            }
            
            //if (SelectedViewWindow == "Gallery")
            ReloadFileList(SelectedItem);
        }
    }

    /*
    /// <summary>
    /// zmienia widok na podany
    /// </summary>
    /// <param name="view"></param>
    private void ChangeView(string view)
    {
        if(view != SelectedView && view != string.Empty)
        {
            SelectedView = view;
            SelectedViewWindow = SelectedView;//zapis do ini
            SelectedViewModel = CallMethod(SelectedView);
            //Debug.WriteLine("ChangeView to: " + view +" ret: "+SelectedViewModel.GetType().ToString());
            ReloadFileList(SelectedItem);
        }
    }*/

    [RelayCommand]
    private void DeleteFolderTreeItem()
    {
        if (MenuSelectedTreeItem != null)
        {
            DeleteFolder(MenuSelectedTreeItem);
            /*
            var parent = MenuSelectedTreeItem.Parent;
            Debug.WriteLine($"Parent: {parent} Parent children count: " + parent.Children.Count);
            if (parent.Children.Count == 0)
            {
                parent.IsExpanded = false;
            }*/
            MenuSelectedItem(null);
        }
    }

    #region Menu snd InputBindings
    [RelayCommand(CanExecute = nameof(ClipboardListenerResoult))]
    private void MenuPaste()
    {
        if (Clipboard.ContainsFileDropList())
        {
            var fileList = Clipboard.GetFileDropList();//zwraca StringCollection i taką kolekcję trzeba tam podawać
            foreach (var file in fileList)
            {
                if ((File.Exists(file))&&(System.IO.Path.GetExtension(file) is string ext))
                {
                    if(patternArray.Contains(ext.ToLower()))
                    {
                        if (MenuSelectedTreeItem != null)
                        {
                            //to do wklejania do katalogu wskazanego poprzez context menu w drzewie
                            MoveFileToFolder(file, MenuSelectedTreeItem, !_cut);
                            //MenuSelectedTreeItem = null;
                        }
                        else
                            //to do wklejania do aktualnie wybranego katalogu w drzewie
                            MoveFileToFolder(file, SelectedTreePath, !_cut);
                    }
                }
            }
            MenuSelectedTreeItem = null;
            Clipboard.Clear();
            _ = RefreshClipboardListenerResoult();
        }
    }

    public void MenuSelectedItem(object parameter)
    {
        if (parameter is TreeModel ti)
        {
            MenuSelectedTreeItem = ti;            
        }
    }

    /// <summary>
    /// wklejanie ze skrótów klawiaturowych (CTRL+V) z schowka systemowego
    /// </summary>
    [RelayCommand]
    private void MenuPasteInputBindings()
    {
        MenuPaste();
    }

    /// <summary>
    /// kopiowanie do schowka systemowego zaznaczonych elementów w galerii (CTRL+C)
    /// </summary>
    /// <param name="parameter">lista zaznaczonych elementów podan z XAML</param>
    [RelayCommand]
    private void MenuCopy(object parameter)
    {
        _=CopyX(GetCollection(parameter));
    }

    /// <summary>
    /// Wycinanie do schowka systemowego zaznaczonych elementów w galerii (CTRL+X)
    /// </summary>
    /// <param name="parameter">lista zaznaczonych elementów podan z XAML</param>
    [RelayCommand]
    private void MenuCut(object parameter)
    {
        _cut = CopyX(GetCollection(parameter));
    }

    private StringCollection?  GetCollection(object parameter)
    {
        if (parameter is System.Collections.IList ph)
        {
            return [.. ph.Cast<Photo>().Select(static p => p.Path)];
        }
        return null;
    }


    /// <summary>
    /// metoda wykonawcza dla kopiowani i wycinania elementów do schowka systemowego
    /// </summary>
    /// <param name="param">lista zaznaczonych elementów podan z XAML</param>
    /// <returns></returns>
    private bool CopyX(StringCollection? param = null)
    {
        StringCollection paths;
        if (param == null)
        {
            paths = [.. Photos.Where(static p => p.IsSelected).Select(static p => p.Path)];
        }
        else
        {
            paths = param;
        }

        if (paths.Count > 0)
        {
            //StringCollection paths = [.. pathsFx];
            Clipboard.SetFileDropList(paths);
            _cut = false;
            _ = RefreshClipboardListenerResoult();
            return true;
        }
        _ = RefreshClipboardListenerResoult();
        return false;
    }

    /// <summary>
    /// (F5) odświeżanie listy plików w aktualnie wybranym katalogu
    /// </summary>
    [RelayCommand]
    private void MenuRefresh()
    {
        //Debug.WriteLine("MenuRefresh called.");
        RefreshFileList();
    }

    /// <summary>
    /// blokada wklejania jeżeli w schowku nie ma odpowiednich plików
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MenuPasteCommand))]
    private bool _ClipboardListenerResoult = false;

    /// <summary>
    /// odświerza stan schowka systemowego pod kątem czy są tam pliki które można wkleić
    /// </summary>
    /// <returns></returns>
    public bool RefreshClipboardListenerResoult()
    {
        //Debug.WriteLine("RefreshClipboardListenerResoult called.");
        //Match m;
        if (Clipboard.ContainsFileDropList())
        {
            //Debug.WriteLine("Clipboard contains FileDropList data.");//to jest
            try
            {
                var fileList = Clipboard.GetFileDropList();//zwraca StringCollection i taką kolekcję trzeba tam podawać
                foreach (var file in fileList)
                {
                    if (System.IO.Path.GetExtension(file) is string ext)
                    {
                        if (ExtO(ext)) { ClipboardListenerResoult = true; return true; }
                    }
                }
            } catch(Exception ex) 
            {
                Log.Write(LogLevel.Error, "Error occurred while processing clipboard data: " + ex.Message);
                Debug.WriteLine("Error occurred while processing clipboard data: " + ex.Message);
            }
        }
        else
        {
            ClipboardListenerResoult = false;
            //return false;
        }
        return false;
    }

    /// <summary>
    /// do menu delete z menu kontekstowego galerii &&(DELETE)
    /// </summary>
    /// <param name="parameter">lista zaznaczonych elementów podan z XAML</param>
    [RelayCommand]
    private void DeleteThumbnails(object parameter)
    {
        if (parameter is System.Collections.IList ph)
        {
            DeleteFile([.. ph.Cast<Photo>()]);
        }
        /*
        else
        {            
            DeleteFile([.. Photos.Where(static p => p.IsSelected)]);
        } */
    }

    /// <summary>
    /// usuwa plik poza kosz
    /// </summary>
    /// <param name="parameter"></param>
    [RelayCommand]
    private void DeleteFileStrong(object parameter)
    {
        if (parameter is System.Collections.IList ph)
        {
            Photo[] photos = [.. ph.Cast<Photo>()];
            foreach (Photo p in photos)
            {
                if (BrokerFile.DeleteFileStrong(p.Path))
                {
                    if (Photos.Remove(p))
                    {
                        string pathFile = Path.GetDirectoryName(p.Path);
                        int x = GetCountFiles(pathFile);
                        SelectedItem.CountFiles = x;
                        //można zrobić metodę ToolBarStatusRefresh() i tam to wszystko wrzucić
                        // ze sprawdzeniem ilości plików w aktualnie wyświetlanym katalogu
                        //FilesToLoad = x.ToString();
                        //FileLoaded = x.ToString();
                        RefreshStatusBarFileCount();
                    }
                }
            }
        }
    }

    [RelayCommand]
    private void SelectAll(object parameter)
    {
        //to będzie zaznaczanie wszystkich elementów w widoku galerii lub liście
        //Debug.WriteLine("SelectAll");
        if (Photos != null && Photos.Count > 0)
        {
            bool sel = true;
            if ((parameter is System.Collections.IList ph) && (ph != null))
            {
                if (ph.Count == Photos.Count)
                {
                    sel = false;
                }
            }
            else
            {
                //Debug.WriteLine("SelectAll: parameter is null or not IList");
                sel = GetCountSelectedItem() != Photos.Count;
                //Debug.WriteLine("SelectAll: sel set to: " + sel + ", GetCountSelectedItem: "+GetCountSelectedItem());
            }
            CurSelectionMode = SelectionMode.Multiple;
            foreach (var photo in Photos)
            {
                photo.IsSelected = sel;
            }
        }
    }

    [RelayCommand]
    private void TestMethod(object parameter)
    {
        Debug.WriteLine("TestMethod klik: " + parameter.ToString());

        foreach (var item in parameter as System.Collections.IList)
        {
            if (item is Photo p)
            {
                Debug.WriteLine("path: " + p.Path);
            }
        }
    }
    #endregion

    #region toolbar menuitem


    public async void SortAD(object DC)
    {
        if (DC is MenuRadioButton par)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
            string grupa = par.Grupa;
            MenuSort.Where(q => q.Grupa == grupa).ToList().ForEach(x => x.IsChecked = false);
            par.IsChecked = true;

            string kryterium = MenuSort.Where(q => q.IsChecked && q.Grupa == "kryterium").Select(q => q.Name).ToArray()[0].ToString();
            string kierunek = MenuSort.Where(predicate: q => q.IsChecked && q.Grupa == "kierunek").Select(q => q.Name).ToArray()[0].ToString();
            //tu dodać jeszcze zapisywanie do ini
            // Debug.WriteLine($"kryterium: {kryterium} + kierunek: {kierunek}");
            Sortowanie(kryterium, kierunek, patternArray);
            //tu dodać wznowienie łądowania miniaturek o ile nie były załadowane wszystkie
            //Debug.WriteLine($"SortAD - FilesToLoad: {FilesToLoad}, FileLoaded: {FileLoaded}");
            if (FileLoaded < FilesToLoad)
            {
                cts = new CancellationTokenSource();
                token = cts.Token;                
                await PhotosLoadImae(token);                
            }
        }
    }

    #endregion

    //[RelayCommand]
    //private void DataGridLDoubleClick(object parameter)
    //{
    //    Debug.WriteLine("LBM klik: " + parameter.ToString());
    //}



    /// <summary>
    /// zwraca ilość zaznaczonych elementów w Photos
    /// </summary>
    /// <returns></returns>
    private int GetCountSelectedItem()
    {
        //Photo[] photos = [.. Photos.Where(static p => p.IsSelected)];
        //return photos.Length;
        return ((Photo[]) [.. Photos.Where(static p => p.IsSelected)]).Length;
    }



    /// <summary>
    /// odśwież ilość plików w pasku statusu (po przenoszeniu, kopiowaniu, wycinaniu)
    /// </summary>
    private void RefreshStatusBarFileCount()
    {
        if (SelectedItem != null)
        {
            int x = GetCountFiles(SelectedItem.Path);
            //selectedItem.CountFiles = x;
            FilesToLoad = x;
            FileLoaded = x;
        }
    }

    #endregion

    #region Konstruktory
    public MainWindowViewModel()
    {
        _init(String.Empty);
        //Debug.WriteLine("MainWindowViewModel constructor without parameters called.");
        //foreach (string ext in patternArray) 
        //{   
        //    object obj = null;
        //    if (BrokerRegistry.RegistryValueExists("HKCU",
        //    @"Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts",
        //    @"Applications\poligon pezeglądarka grafiki.exe_" + ext))
        //    {
        //        obj = null;
        //        obj = BrokerRegistry.RegistryGetValue("HKCU",
        //            @"Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts",
        //            @"Applications\poligon pezeglądarka grafiki.exe_" + ext);
        //        if (obj != null)
        //        {
        //            Debug.WriteLine($"Registry value for extension {ext}: {obj.GetType()}");
        //        }
        //    }
                
        //    Debug.WriteLine($"Pattern extension: {ext} in register : {BrokerRegistry.RegistryValueExists("HKCU",
        //    @"Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts",
        //    @"Applications\poligon pezeglądarka grafiki.exe_"+ext)}");
        //}
       // Brokerregistry.RegistryGetSubKeyNames("HKCU", @"Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts").ToList().ForEach(static s => Debug.WriteLine($"Registry subkey: {s}"));
       //BrokerRegistry.RegistrySetValue("HKCU",
       //     @"Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts",
       //     @"Applications\poligon pezeglądarka grafiki.exe_.png", 0, RegistryValueKind.DWord);

        //Debug.WriteLine($"rejestr: {BrokerRegistry.RegistryValueExists("HKCU", 
        //    @"Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts",
        //    @"Applications\poligon pezeglądarka grafiki.exe_.png")}");//działa
    }

    //na razie zostawiam ale raczej będzie do wyżucenia
    //public DateTime GetBuildDateFromVersion()
    //{
    //    Version version = Assembly.GetExecutingAssembly().GetName().Version;
    //    // Wersja 1.0.* używa formatu: 
    //    // Major.Minor.DaysSinceBaseDate.SecondsSinceMidnight/2
    //    DateTime buildDate = new DateTime(2026, 4, 3).AddDays(version.Build).AddSeconds(version.Revision * 2);
    //    return buildDate;
    //}

    public MainWindowViewModel(string path)
    {
        _init(path);
    }

    /// <summary>
    /// uzupełnia listę źródłową menu sortowania
    /// </summary>
    /// <param name="kryterium"></param>
    /// <param name="kierunek"></param>
    private void LoadSortowanie(string kryterium, string kierunek)
    {
        MenuSort.Add(new("Nazwa", (kryterium == "Nazwa"),  "kryterium"));
        MenuSort.Add(new("Data", (kryterium == "Data"),  "kryterium"));
        MenuSort.Add(new("Wielkość", (kryterium == "Wielkość"),  "kryterium"));
        MenuSort.Add(new("Rosnąco", (kierunek == "Rosnąco"),  "kierunek"));
        MenuSort.Add(new("Malejąco", (kierunek == "Malejąco"),  "kierunek"));
    }
    private void _init(string path)
    {
        

        if (CurMainWindowState == WindowState.Minimized)
            CurMainWindowState = WindowState.Normal;

        // MenuSort.Add(new { NAme="Nazwa", IsChecked= false,Value= 0 });

        //uzupełnianie menu sortowania       
        LoadSortowanie(Sortowaniekryterium, Sortowaniekierunek);

        SwitchTglButton = SwitchToggleButton;
        SwitchThemeMode();
        ButtonRefresh();
        //tu upewnić się że katalog wyświetlany jak nie ma dzieci  to nie jest expand
        BuildTree(Path.GetDirectoryName(path));//to tu wskazuje katalog ostatnio urzyty i tu jest problem

        SelectedView = SelectedViewWindow;
        //if (SelectedView == String.Empty)
        //{
        //    SelectedView = "Welcome";// to tylko zabezpieczenie nie powinno dojść do takiej sytuacji
        //    // w sumie to można tu wywołać wyjątek
        //    //SelectedView = DefaultSettings.SelectedView;
        //    //SelectedViewWindow = SelectedView;
        //}
        Debug.Assert(!string.IsNullOrEmpty(SelectedView), "SelectedView is null or empty in MainWindowViewModel _init");
        SelectedViewModel = CallMethod(SelectedView);
        //Debug.WriteLine("SelectedView:" + SelectedView);
        try
        {
            cts = new CancellationTokenSource();
            token = cts.Token;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string pathFile = Path.GetDirectoryName(path);
                //Debug.WriteLine($"ściezka do katalogu: {pathFile}");
                FileListLoad(pathFile, token);
            }
            else
            {
                //Debug.WriteLine("nie znaleziono pliku: " + path);
                FileListLoad(SelectedTreePath, token);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }

        
        //DropCollor = Brushes.Red;
        //Debug.WriteLine($"DropCollor set to: {DropCollor.ToString()}");

    }

    #endregion

    #region wywołania Control

    private object CallMethod(string p, object?[]? x = null)
    {
        Type thisType = GetType();
        if (thisType != null)
        {
            if ((thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo theMethod)
                && (theMethod != null))
            {
                var ret = theMethod?.Invoke(this, x);
                if (ret != null)
                {
                    return ret;
                }
            }
        }
        else
        {
            Debug.WriteLine("CallMethod - tu jest problem Type == null");
        }

        return new();
    }

    private object Welcome() => new Welcome();

    private object FDataGrid() => new FileDataGrid();

    private object FList() => new FileList();

    private object Gallery() => new Gallery();

    private object Settings() => new Settings();

    private object Gallery2() => new FileList2();
    private object GalleryCan() => new GalleryCan();
    //to też jest wyświetlanie miniatur ale nie ma ich łądowania asynchronicznie tak jak w gallery


    #endregion wywołania Control

    #region Folders and Files
    #region Sortowanie
    private string Sortowaniekryterium
    {
        get => BrokerIni.Sortowaniekryterium;
        set => SetProperty(BrokerIni.Sortowaniekryterium, value, BrokerIni, static (u,n) => u.Sortowaniekryterium = n);
    }

    private string Sortowaniekierunek
    {
        get =>BrokerIni.Sortowaniekierunek;
        set => SetProperty(BrokerIni.Sortowaniekierunek, value, BrokerIni, static (u,n) => u.Sortowaniekierunek = n);
    }
    private void Sortowanie(string kryterium, string kierunek, string[] patternArray)
    {
        List<Photo> PhotosKopia =[];// = Photos.ToList();
        
        if(kierunek == "Rosnąco")
        {
            if(kryterium == "Nazwa")
            {
                PhotosKopia = [.. Photos.OrderBy(p => p.Name)];
            }
            else if(kryterium == "Data")
            {
                PhotosKopia = [.. Photos.OrderBy(p => p.DateModified)];
            }
            else if(kryterium == "Wielkość")
            {
                PhotosKopia = [.. Photos.OrderBy(p => p.Size)];
            }
        }
        else if(kierunek == "Malejąco")
        {
            if (kryterium == "Nazwa")
            {
                PhotosKopia = [.. Photos.OrderByDescending(p => p.Name)];
            }
            else if (kryterium == "Data")
            {
                PhotosKopia = [.. Photos.OrderByDescending(p => p.DateModified)];
            }
            else if (kryterium == "Wielkość")
            {
                PhotosKopia = [.. Photos.OrderByDescending(p => p.Size)];
            }
        }

        if (PhotosKopia.Count == Photos.Count)
        {            
            //string ViewPath = SelectedTreePath;            
            //var files = BrokerFile.IGetFiles(ViewPath, kryterium, kierunek, patternArray);  
            
            Photos.Clear();
            //foreach (var file in files)
            foreach (var file in PhotosKopia)
            {
                //Photos.Add(PhotosKopia.First(x => x.Path == file));
                Photos.Add(file);
            }        
            PhotosKopia.Clear();
            Sortowaniekierunek = kierunek;
            Sortowaniekryterium = kryterium;
        }
    }
    #endregion
       
    public void RenameFile(Photo photo, string newName)
    {
        newName = photo.Path.Substring(0, photo.Path.LastIndexOf('\\') + 1) + newName;
        bool x = BrokerFile.RenameFile(photo.Path, newName);
        if (x)
        {
            PhotoHelper.Rename(photo,newName);
        }
    }

    public void RenameFolder(TreeModel treeModel, string newName)
    {
        //Debug.WriteLine("RenameFolder: " + treeModel.Path + " newName: " + newName);
        if (treeModel != null)
        {
            string newNameX = treeModel.Path.Substring(0, treeModel.Path.LastIndexOf('\\') + 1) + newName;
            //Debug.WriteLine("RenameFolder new path: newNameX: " + newNameX);
            if (cts != null)
            {
                cts.Cancel();
                while (!cts.IsCancellationRequested && counter)
                {
                    //Thread.SpinWait(50000);
                    Debug.WriteLine("RenameFolder - nadal czekam ...");
                    Thread.SpinWait(5000);
                }
                //cts.Dispose();
            }
            if (cts == null || (cts.IsCancellationRequested && !counter))
            {
                var xuz = treeModel.GetSelfFromParent();
                bool x = BrokerFile.RenameFilDirectory(treeModel.Path, newNameX);
                if (x && xuz != null)
                {
                    xuz.Name = newName;
                    xuz.Path = newNameX;
                    xuz.Children.Clear();
                    xuz.Addchild(ScanPath(newNameX, "").Children);
                }
            }
            else
            {
                //na razie zostawiam do czasu zrobienia komunikatu w oknie
                Debug.WriteLine("RenameFolder: Canceled");

            }
        }
    }

    /// <summary>
    /// sprawdza czy podany plik istnieje w  Photos
    /// czyli aktualnie wyświetlanym katalogu
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool FileIsFolder(string file)
    {
        if (File.Exists(file))
        {
            foreach (var photo in Photos)
            {
                if (photo.Path == file) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// co ja tu miałem na myśli?? chyba jest błędne
    /// </summary>
    /// <param name="treeModel"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public bool DirectroyExists(TreeModel treeModel, string newName)
    {
        string path = Path.Combine(treeModel.Path, newName);
        if (File.Exists(path)) { return true; }
        return false;
    }


    /// <summary>
    /// dodawanie folderu na dysku
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string AddFolder(string path)
    {
        //tu ma być robiony katalog o nazwie "nowy katalog"
        //najpierw zrobić sprawdzanie czy taki istnieje jak istnieje to dodajemy (1) itd

        string cat = "Nowy katalog";
        var newPath = System.IO.Path.Combine(path, cat);
        int i = 1;
        while (Directory.Exists(newPath))
        {
            cat = "Nowy katalog (" + i.ToString() + ")";
            newPath = System.IO.Path.Combine(path, cat);
            i++;
        }
        try
        {            
            BrokerFile.CreateDirectory(newPath);            
            return cat;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return String.Empty;
    }

    /// <summary>
    /// dodawanie folderu do aktywnego drzewa, generowane automatycznie, nie przetestowane
    /// </summary>
    /// <param name="treeModel"></param>
    /// <returns></returns>
    public TreeModel AddFolder(TreeModel treeModel)
    {
        if (treeModel != null)
        {
            string x = AddFolder(treeModel.Path);//dodaje "nowy folder" do podanej ścieżki
            if (x != string.Empty)// jeżeli się udało to...
            {                
                treeModel.Children.Clear();
                treeModel.Addchild(ScanPath(treeModel.Path).Children);
                treeModel.IsExpanded = true;
                var yuz = treeModel.Children.FirstOrDefault(f => f.Name == x);
               //var index = treeModel.Children.IndexOf(yuz);
                //Debug.WriteLine("AddFolder - new folder index: " + index.ToString());
                return yuz;
            }
            //return null;
        }
        return null;
    }

    /// <summary>
    /// inicjuje usuwanie folderu z dysku do kosza, aktualizuje drzewo
    /// </summary>
    /// <param name="treeModel"></param>
    public bool DeleteFolder(TreeModel treeModel)
    {
        if (treeModel != null)
        {
            bool x = DeleteFolder(treeModel.Path);
            if (x)
            {
                var xuz = treeModel.GetSelfFromParent();
                if (xuz != null)
                {   var parent = xuz.Parent;
                    parent = parent.GetSelfFromParent();
                    _ = (xuz.Parent?.Children.Remove(xuz));
                    if (parent != null && parent.Children.Count == 0)
                    {                        
                        parent.IsExpanded = false;
                    }
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Usuwanie folderu z dysku do kosza
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    private bool DeleteFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder)) return false;
        if (!Directory.Exists(folder)) return false;
        try
        {
            BrokerFile.DeleteDirectory(folder);            
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return false;
    }

    public void DeleteFile(Photo[] photos)
    {
        if (photos == null) return;
        foreach (var photo in photos)
            DeleteFile(photo.Path);
    }
    private void DeleteFile(string file)
    {
        if (string.IsNullOrEmpty(file)) return;
        if (!File.Exists(file)) return;
        try
        {
            if (BrokerFile.DeleteFile(file))
            {
                if (Photos.FirstOrDefault(p => p.Path == file) is Photo x)
                {
                    _ = Photos.Remove(x);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// sparwdza czy podana ext jest w patternArray
    /// sprawdza czy obsługiwane rozszerzenie pliku jest obsługiwane
    /// </summary>
    /// <param name="Ext"></param>
    /// <returns></returns>
    public bool ExtO(string Ext)
    {
        if(patternArray.Contains(Ext.ToLower()))
        {
            return true;
        }
        return false;        
    }

    #endregion Folders and Files

    #region okna dialogowe - nie wykożystane
    /*
    private async Task<string> ShowDialog(SimpledialogViewModel DC)
    {
        //tak wszystko działa, pozostaje pododawać metody żeby mozna było wywołać jedno okno do różnych celów
        // DC = new SimpledialogViewModel { Name = string.Empty, WindowName = "Podaj nazwę nowej bazy", Hint = "Nazwa bazy" };
        object? view = new SimpleDialog
        {
            DataContext = DC
        };

        object? result = await DialogHost.Show(view, "RootDialog", null, null, ClosedEventHandler);

        //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL")+" - " + DC.Name);

        if ((result != null) && (bool)result)
        {
            return DC.Name;
        }
        return "";
    }
    */

    private async Task<string> ShowDialogAddFolder(TreeModel treeModel, string newName)
    {
        object? view = new SimpleDialog();
        object? result = await DialogHost.Show(view, "RootDialog", null, null, ClosedEventHandler);
        return "";
    }

    private async Task<string> ShowDialogProgresBarr()
    {
        //Debug.WriteLine($"ShowDialogProgresBarr called  ");//to jest wywoływane ale nie wyświetla okna dialogowego
        object? view = new ProgresDialog { WindowName = "Copy Or Move File To Folder" };// dodać resztę...
        object? result = await DialogHost.Show(view, "WindowDialogHost", OpenDialogOpenedEventArgs_SDPB, null, ClosedEventHandler_SDPB);
        var x = result as DialogHost;
        //var session = x.CurrentSession;
        //if (session != null)
        //{
        //    session.Close();
        //}
        
        return "";
    }

    //tego urzyć żeby sprawdzić czy okno jest jeszcze otwarte
    // i zamknąć je z poziomu kodu jeżeli jest otwarte
    // Podsumowanie:
    //     Retrieve the current dialog session for a DialogHost
    //
    // Parametry:
    //   dialogIdentifier:
    //     The identifier to use to retrieve the DialogHost
    //
    // Zwraca:
    //     The DialogSession if one is in process, or null
    //public static DialogSession? GetDialogSession(object? dialogIdentifier)
    //{
    //    return GetInstance(dialogIdentifier).CurrentSession;
    //}

    private void OpenDialogOpenedEventArgs_SDPB(object sender, DialogOpenedEventArgs eventArgs)
    {
        //Debug.WriteLine("OpenDialogOpenedEventArgs_SDPB called");
        var x = eventArgs.Session;

        if (x != null)
        {
            Debug.WriteLine("OpenDialogOpenedEventArgs_SDPB - Session is not null");
            //x.Close();// ok zamyka okno z poziomu kodu, trzeba przesłać sesję gdzieś na zewnątrz do wykożystania
            //DialogHost.Close("WindowDialogHost");//to działa, nie muszę niczego przekazywać !!!
        }
    }

    private void ClosedEventHandler_SDPB(object sender, DialogClosedEventArgs eventArgs)
    {
        //ok - to działa i po anulowaniu można próbować zatrzymać kopiowanie / przenoszenie plików
        //jak okno jest zamknięte z poziomu kodu to zwraca null w eventArgs.Parameter, jeżeli z poziomu przycisku to zwraca true/false
        Debug.WriteLine("You can intercept the closed event here (1)." + eventArgs.Parameter);
        if(eventArgs.Parameter is bool param)
        {
            Debug.WriteLine("ClosedEventHandler_SDPB - parameter: " + param);//ok mamy bool 
            //jeżeli jest false to można próbować anulować operację
        }
    }

    private void ClosedEventHandler(object sender, DialogClosedEventArgs eventArgs)
    {
        //to jest opcjonalne, na razie zostawiam może jeszcze wykozystam
        Debug.WriteLine("You can intercept the closed event here (1)." + eventArgs.Parameter);
    }

    private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
    {
        //to jest opcjonalne, na razie zostawiam może jeszcze wykozystam
        Debug.WriteLine("You can intercept the closed event here (10)." + eventArgs.Parameter);
    }
    #endregion okna

    #region DragDrop

    public void MoveFileToFolder(string[] dataStrings, bool copy = false)
    {
        foreach (var dataString in dataStrings)
        {
            if (!FileIsFolder(dataString))
            {
                string newDestinyPath = Path.Combine(SelectedItem.Path, Path.GetFileName(dataString));
                if (copy)
                {
                    //BrokerFile.CopyFile(dataString, newDestinyPath);
                    MoveFileToFolder(dataString, SelectedTreePath, true);
                }
                else
                {
                    //BrokerFile.MoveFile(dataString, newDestinyPath);
                    MoveFileToFolder(dataString, SelectedTreePath, false);
                }
            }
        }
    }

    /// <summary>
    /// przenoszenie plików  i katalogów a w drzewie
    /// </summary>
    /// <param name="dataStrings"></param>
    /// <param name="target"></param>
    /// <param name="copy"></param>
    public void MoveFileToFolder(string[] dataStrings, TreeModel target, bool copy = false)
    {
        foreach (var dataString in dataStrings)
        {
            //jeżeli nie jest to plik a jest katalogiem to przenosimy katalog do katalogu docelowego
            if (!File.Exists(dataString) && Directory.Exists(dataString))
            {
                if (dataString != target.Path)
                {
                    MoveFoderToFolder(dataString, target.Path);
                    Log.Write(LogLevel.Info, $"MoveFoderToFolder - przenoszenie katalogu {dataString} do {target.Path}");
                }else Log.Write(LogLevel.Warning, $"MoveFoderToFolder - katalog źródłowy i docelowy są takie same: {dataString} == {target.Path}");
                //Log.Write(LogLevel.Info, "MoveFileToFolder: przenoszenie katalogów z poziomu przenoszenia plików jest wyłączone");

            }
            else
            {
                Log.Write(LogLevel.Info, $"MoveFileToFolder(string[] dataStrings, TreeModel target, bool copy = false) - {dataString}");
                MoveFileToFolder(dataString, target, copy);

                //jak przenosimy plik do tego samego katalogu to zmienia jego nazwę zamast wstrzymać się
            }
        }
    }


    /// <summary>
    /// zwraca kolor tła dla elementu docelowego przy przeciąganiu plików
    /// </summary>
    /// <returns></returns>
    public Brush GetDropCollor()
    {
        //Debug.WriteLine($"GetDropCollor called: {DropCollor}");
        //return DefaultSettings.DropCollor;
        return DropCollor;
    }

    /// <summary>
    /// przenosi katalog do innego katalogu, aktualizuje drzewo
    /// </summary>
    /// <param name="folder">Katalog źródłowy, przenoszony</param>
    /// <param name="DestinyPath">Katalog docelowy</param>
    /// <exception cref="FileNotFoundException">wywoływane jeżeli któryś z katalogów nie istnieje</exception>
    private void MoveFoderToFolder(string folder, string DestinyPath)
    {
        Debug.WriteLine("Move Foder To Folder - przenoszenie KATALOGU do katalogu");
        if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(DestinyPath))
        {   
            Log.Write(LogLevel.Warning, "MoveFoderToFolder: Katalog żródłowy lub docelowy nie istnieje");
            throw new FileNotFoundException("Katalog żródłowy lub docelowy nie istnieje");
        }
        if (folder == DestinyPath)
        {
            Log.Write(LogLevel.Warning, "MoveFoderToFolder: ścieżka źródłowa i docelowa są takie same");
            Debug.WriteLine("MoveFoderToFolder: ścieżka źródłowa i docelowa są takie same");
            return;
        }
        string newDestinyPath = Path.Combine(DestinyPath, folder.Substring(folder.LastIndexOf('\\') + 1));

        //jeżeli katalog źródłowy nie jest plikiem i jest katalogiem i katalog docelowy istnieje
        //i nie istnieje katalog docelowy z tą samą nazwą co przenoszony katalog
        if ((!File.Exists(folder)) && (Directory.Exists(folder)) && (Directory.Exists(DestinyPath))
            && (!Directory.Exists(newDestinyPath)))
        {
            try
            {
                //Debug.WriteLine($"deftiny: {newDestinyPath}" + $" Source: {folder}");
                //tu pewnie trzeba zatrzymać wątek ładujący pliki z katalogu źródłowego
                if (cts != null)
                {
                    cts.Cancel();
                    //Debug.WriteLine("MoveFoderToFolder: Waiting for cancellation");
                    //oczekujemy aż wątek się zatrzyma, jeżeli nie to przenosimy katalog i aktualizujemy drzewo
                    while (!cts.IsCancellationRequested)
                    {
                        //Debug.WriteLine("MoveFoderToFolder: still waiting..."); 
                        Thread.SpinWait(5000);
                    }
                }
                //BrokerFile.MoveDirectory(folder, newDestinyPath);
                try
                {
                    Directory.Move(folder, newDestinyPath);
                }
                catch (IOException)
                {
                    //przy większej ilości jest zwiecha jak to przenieść do innego wątku ??
                    // albo dać informację o przenoszeniu plików??
                    BrokerFile.MoveDirectory(folder, newDestinyPath);
                }
                //nie odświeżył drzewa !!! i trwa za długo ;(

                //Tree.Flatten()

                TreeModel? TreeFoldr = null, TreeDestinyPath = null;
                foreach (var tree in Tree)//wybór właściwego drzwa
                {
                    //ograniczamy niepotrzebe szukanie w drzewach
                    if (TreeDestinyPath == null)
                        TreeDestinyPath = tree.GetElementByPath(DestinyPath);
                    if (TreeFoldr == null)
                        TreeFoldr = tree.GetElementByPath(folder);
                    //jeżeli oba zostały znalezione to przerywamy pętlę
                    if ((TreeFoldr != null) && (TreeDestinyPath != null)
                        && (TreeFoldr.Path == folder) && (TreeDestinyPath.Path == DestinyPath)
                        ) break;
                    else
                    {
                        TreeFoldr = null;
                        TreeDestinyPath = null;
                    }
                }
                if ((TreeFoldr != null) && (TreeDestinyPath != null) && (TreeFoldr.Path != string.Empty)
                    && (TreeDestinyPath.Path != string.Empty))
                {
                    var xuz = TreeFoldr.GetParent();
                    Debug.Assert(xuz != null);
                    TreeDestinyPath.Children.Clear();
                    TreeDestinyPath.Addchild(ScanPath(TreeDestinyPath.Path, newDestinyPath).Children);
                    TreeDestinyPath.IsExpanded = true;
                    TreeDestinyPath.IsSelected = true;
                    SelectedItem = TreeDestinyPath;

                    if (xuz != null)
                    {
                        xuz.Children.Clear();
                        xuz.IsExpanded = true;
                        xuz.IsSelected = true;// to powodowało problemy z zaznaczeniem i był zaznaczany i ładowany katalog główny
                        xuz.Addchild(ScanPath(xuz.Path, "").Children);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "MoveFoderToFolder: błąd przenoszenia katalogu: " + ex.Message);
                Debug.WriteLine(ex.ToString());
            }
            //return selectedItem;
        }
        else if ((Directory.Exists(newDestinyPath))) Log.Write(LogLevel.Warning, "MoveFoderToFolder: katalog już istnieje: " + newDestinyPath);//tu przydało by się okno z zapytaniem czy scalić?? czy anulować??
    }


     /// <summary>
    /// usuwa przeniesione pliki z kolekcji Photos
    /// </summary>
    /// <param name="photos">lista plików do usunięcia</param>
    public void RemoveFileFromPhotos(List<Photo> photos)//zmienić nawę na list
    {
        //wszystkie operacje na Photos zatrzymują generowanie miniatur, trzeba ustawiać znacznik i wznawiać w jakiś sposób
        //ale wcześniej warto przerywać tak żeby nie było błędu, albo raczej go przewidzieć !!!
        //odejmować też pliki z "kolejki" znacznika plików na pasku statusu - statusbar
        // zwrócić uwagę na zmiany ilości plików

        //urzywane  tylko w module galerii
        //może da sie przerobić jakoś?? do przemyslenia
        if (cts != null)
        {            
            cts.Cancel();            
        }
        photos.ForEach(photo => { Photos.Remove(photo); });
    }


    
    /// <summary>
    /// to jest do wrzucania plików do aktualnmie wyświetlanego folderu w listbox
    /// </summary>
    /// <param name="file"></param>
    /// <param name="copy"></param>
    //public void MoveFileToFolder(string file, bool copy = false)
    //{        
    //    if (string.IsNullOrEmpty(file)) return;
    //    if (!File.Exists(file)) return;
    //    string ViewPath = SelectedTreePath;//katalog zaznaczony w drzewie katalogów, galeria nie ma do niego dostępu
    //    MoveFileToFolder(file, ViewPath, copy);
    //}

    /// <summary>
    /// przenosi plik do katalogu docelowego, aktualizuje kolekcję Photos i liczbę plików w katalogu docelowym
    /// </summary>
    /// <param name="file">plik do przeniesienia</param>
    /// <param name="path">katalog docelowy</param>
    /// <param name="copy">czy kopiować, domyślnie False</param>
    private void MoveFileToFolder(string file, TreeModel path, bool copy = false)
    {
        //Debug.WriteLine("Move File To Folder - przenoszenie PLIKU do katalogu");
        //jak przenosimy plik do tego samego katalogu to zmienia jego nazwę zamast wstrzymać się

        //odświeża listę plików jeżeli nie podano pliku i katalogu docelowego i nie kopiujemy
        if ((file == String.Empty) && (path == null) && !copy)
        {            
            RefreshFileList();
            return;
        }

        //to mi się nie podoba,przemysleć i sprawdzić
        //czy to nie miało na celu kopiowania drzewa katalogów?
        //if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Directory.Exists(path.Path) || !Path.HasExtension(file))
        //{
        //    Debug.WriteLine("MoveFileToFolder: niepoprawny plik lub katalog docelowy");

        //    return;
        //}
        //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
        if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Directory.Exists(path.Path) || !Path.HasExtension(file))
        {
            //Debug.WriteLine("MoveFileToFolder: niepoprawny plik lub katalog docelowy"); 
            Log.Write(LogLevel.Warning, "MoveFileToFolder: nieobsługiwany format pliku: " + Path.GetExtension(file).ToLower()+" lub nie istniejący katalog docelowy: " + path.Path);
            return;
        }

        //to poniżej poprawić na metodęz brokerfile, ale metoda z brokerfile musi i tak pobierać tablicę rozszeżeń z mv to błędne koło        
        //tu zrobić sprawdzanie a tam wysyłać już do kopiowania/przenoszenia pliku do katalogu docelowego
        if (ExtO(Path.GetExtension(file).ToLower()))
        {
            string newFilePath = FileMove(file, path.Path, copy);
            string pathFile = System.IO.Path.GetDirectoryName(file);
            path.CountFiles = GetCountFiles(path.Path);//dodaje liczbę plików w katalogu docelowym
            SelectedItem.CountFiles = GetCountFiles(SelectedItem.Path);

            RefreshStatusBarFileCount();
            if (!copy)
                _ = Photos.Remove(Photos.FirstOrDefault(i => i.Path == file));

                        
            if (SelectedTreePath == path.Path)
            {

                if (!newPhoto(newFilePath)) ReloadFileList(SelectedItem);//to jako ostateczność
            }
        }
        else Log.Write(LogLevel.Warning, "MoveFileToFolder: nieobsługiwany format pliku: " + Path.GetExtension(file).ToLower());
    }

    /// <summary>
    /// metoda wykożystywana w menu paste i w listbox drop
    /// </summary>
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <param name="copy"></param>
    /// <returns></returns>
    /// 
    public bool MoveFileToFolder(string file, string path, bool copy = false)
    {
        if(GetTreeModel(path) is TreeModel treeModel)
        {
            MoveFileToFolder(file, treeModel, copy);
            return true;
        }

        return false;
    }

    //public bool MoveFileToFolder(string file, string path, bool copy = false)
    //{

    //    if ((file == String.Empty) && (path == String.Empty) && !copy)
    //    {
    //        //czyszczenie kolekcji po przeniesieniu pliku do np explorera plików
    //        RefreshFileList();
    //        return false;
    //    }


    //    if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Directory.Exists(path) || !Path.HasExtension(file))
    //    {
    //        Log.Write("MoveFileToFolder: brak pliku lub katalogu docelowego: " + Path.GetExtension(file).ToLower());
    //        return false;
    //    }
    //    //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];

    //    if (ExtO(Path.GetExtension(file).ToLower()))
    //    {
    //        string newFilePath = FileMove(file, path, copy);//przenoszenie/kopiowanie pliku
    //        string pathFile = System.IO.Path.GetDirectoryName(file);
    //        //to dlatego że Tree jest tablicą kilku drzew
    //        //wyszukiwanie właściwego drzewa


    //        //jeżeli to przeniosę do innych metod to będzie można inaczej to zorganizować
    //        foreach (var treeItem in Tree)
    //        {
    //            TreeModel? item = treeItem.GetElementByPath(path);
    //            if (item != null) item.CountFiles = GetCountFiles(item.Path);//dodaje liczbę plików w katalogu docelowym
    //            item = treeItem.GetElementByPath(pathFile);
    //            if (item != null) item.CountFiles = GetCountFiles(item.Path);//odejmuje liczbę plików w katalogu źródłowym
    //            RefreshStatusBarFileCount();
    //        }


    //        //jeżeli nie kopiujemy to usuwamy go z Photos o ile tam istnieje
    //        if (!copy) _ = Photos.Remove(Photos.FirstOrDefault(i => i.Path == file));

    //        if (SelectedTreePath == path)
    //        {
    //            if (!newPhoto(newFilePath))
    //            {
    //                //Debug.WriteLine("MoveFileToFolder -- ReloadFileList");
    //                ReloadFileList(SelectedItem);
    //            }//to jako ostateczność
    //        }
    //        return true;
    //    }
    //    else
    //    {
    //        Log.Write(LogLevel.Warning, "MoveFileToFolder: nieobsługiwany format pliku: " + Path.GetExtension(file).ToLower());
    //        //Debug.WriteLine("MoveFileToFolder: nieobsługiwany format pliku: " + Path.GetExtension(file).ToLower());
    //    }

    //    return false;
    //}


    private TreeModel GetTreeModel(string path)
    {
        foreach (var treeItem in Tree)
        {
            TreeModel? item = treeItem.GetElementByPath(path);
            if (item != null) return item;
        }
        return null;
    }

    /// <summary>
    /// przenoszenie pliku do innego katalogu
    /// </summary>
    /// <param name="path">katalog docelowy</param>
    /// <param name="file">plik ze ścieżką</param>
    public void MoveFileToFolder(string file, object pathX, bool copy = false)
    {
        //czy da się zamienic string na object i testować jaki to jest typ ??
        // dalej podejmować działania zgodne z typem ojektu czyli string lub treeModel??
        string path = string.Empty;
        if(pathX is string)
        {
            path = (string)pathX;
        }
        else if (pathX is TreeModel)
        {
            path = ((TreeModel)pathX).Path;
        }
        else
        {
            Log.Write("MoveFileToFolder: nieobsługiwany typ katalogu docelowego");
            return;
        }



        //Debug.WriteLine("MoveFileToFolder(string file, string path, bool copy");
        if ((file == String.Empty) && (path == String.Empty) && !copy)
        {
            //czyszczenie kolekcji po przeniesieniu pliku do np explorera plików
            RefreshFileList();
            return;
        }


        if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Directory.Exists(path) || !Path.HasExtension(file))
        {
            if (!File.Exists(file) && Directory.Exists(file))
            {
                //MoveFoderToFolder(file, path); //przenieść jako osobną metodę
                Log.Write("MoveFileToFolder: przenoszenie katalogu do katalogu docelowego nie jest obsługiwane");
                return;
            }
            Log.Write("MoveFileToFolder: brak pliku lub katalogu docelowego: " + Path.GetExtension(file).ToLower());
            return;
        }
        //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];

        if (ExtO(Path.GetExtension(file).ToLower()))
        {            
            string newFilePath = FileMove(file, path, copy);//przenoszenie/kopiowanie pliku
            string pathFile = System.IO.Path.GetDirectoryName(file);
            //to dlatego że Tree jest tablicą kilku drzew
            //wyszukiwanie właściwego drzewa


            //jeżeli to przeniosę do innych metod to będzie można inaczej to zorganizować
            foreach (var treeItem in Tree)
            {
                TreeModel? item = treeItem.GetElementByPath(path);
                if (item != null) item.CountFiles = GetCountFiles(item.Path);//dodaje liczbę plików w katalogu docelowym
                item = treeItem.GetElementByPath(pathFile);
                if (item != null) item.CountFiles = GetCountFiles(item.Path);//odejmuje liczbę plików w katalogu źródłowym
                RefreshStatusBarFileCount();
            }
            //jeżeli nie kopiujemy to usuwamy go z Photos o ile tam istnieje
            if (!copy) _ = Photos.Remove(Photos.FirstOrDefault(i => i.Path == file));

            if (SelectedTreePath == path)
            {
                //to trzeba przerobić bo przy 1000 plików jest to bardzo widoczne i uciążliwe
                //tu trzeba wygenerować nową ścieżkę pliku, chyba że dostaniemy ją z przeniesienia
                //if (newFilePath != String.Empty && cts != null)
                //{
                //    var token = cts.Token;
                //    if (!token.IsCancellationRequested)
                //    {
                //        //Debug.WriteLine("load file: " + imFile.value);

                //        Photo p = new Photo(newFilePath);
                //        p.Image = new BitmapImage(new Uri(@"pack://application:,,,/img/g1.png")); 
                //        //Photos.Add(p);//dodaje na końcu
                //        Photos.Insert(0, p);//dodaje na początku albo w wyznaczonym miejscu
                //        //p.AddToken(token);
                //        //jak zrobić żeby to było odpalane przez interfejs? a nie tutaj
                //        _ = p.Load(token);
                //    }
                //}
                //else ReloadFileList(SelectedItem);//to jako ostateczność
                if (!newPhoto(newFilePath))
                {
                    //Debug.WriteLine("MoveFileToFolder -- ReloadFileList");
                    ReloadFileList(SelectedItem);
                }//to jako ostateczność
            }
        }
        else
        {
            Log.Write("MoveFileToFolder: nieobsługiwany format pliku: " + Path.GetExtension(file).ToLower());
            //Debug.WriteLine("MoveFileToFolder: nieobsługiwany format pliku: " + Path.GetExtension(file).ToLower());
        }
    }

    /// <summary>
    /// tworzy nowy obiekt Photo, uzupełnia go i dodaje do Photos
    /// </summary>
    /// <param name="path">ścieżka do obrazu</param>
    /// <param name="pos">wskazana pozycja w liscie: 0 to pierwszy, -1 to ostatni, kazdy inny int to wskazana pozycja</param>
    /// <param name="pathBacground">ścieżka do obrazu zastępczego "tła"</param>
    private bool newPhoto(string path, int pos = -1, string pathBacground = "")
    {
        if (path != String.Empty && cts != null)
        {
            var token = cts.Token;
            if (!token.IsCancellationRequested)
            {
                //Debug.WriteLine("load file: " + imFile.value);

                Photo p = new Photo(path);
                if (pathBacground != "")
                {
                    //to trzeba zamienić na odbierany image a nie ścieżkę tak żeby go nie robić za każdym razem
                    p.Image = new BitmapImage(new Uri(pathBacground));
                }
                else
                {
                    p.Image = new BitmapImage(new Uri(@"pack://application:,,,/img/g1.png"));
                }
                if (pos == -1)
                {
                    Photos.Add(p);//dodaje na końcu
                }
                else
                {
                    Photos.Insert(pos, p);//dodaje na początku albo w wyznaczonym miejscu
                                          //p.AddToken(token);
                                          //jak zrobić żeby to było odpalane przez interfejs? a nie tutaj
                }
                _ = p.Load(token);
                //_= PhotoHelper.Load(p,token);
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// sprawdzenie czy path istnieje i skopiowanie lub przeniesienie tam pliku
    /// </summary>
    /// <param name="file">ścieżka żródłowa do przenoszonego pliku</param>
    /// <param name="path">ścieżka docelowa</param>
    /// <param name="copy">true - kopiowanie, false - przenoszenie</param>
    /// <returns>string: zwraca nowąścieżkę pliku z nazwą lub String.Empty jak pojawią się błędy</returns>
    private string FileMove(string file, string path, bool copy = false, bool overwrite = false)
    {
        //przenieść do BrokerFile??
        //tu dodać wywoływanie wyjątków w razie błędów
        if (string.IsNullOrEmpty(path))
        {
            //Debug.WriteLine("MoveFileToFolder: path is null or empty");
            return String.Empty;
        }
        if (file == null)
        {
            //Debug.WriteLine("MoveFileToFolder: file is null");
            return String.Empty;
        }
        if (!Directory.Exists(path))
        {
            //Debug.WriteLine("MoveFileToFolder: path does not exist: " + path);
            return String.Empty;
        }
        //Debug.WriteLine("MoveFileToFolder: path: " + path + " , file: " + file);
        string fileName = System.IO.Path.GetFileName(file);
        string newPath = System.IO.Path.Combine(path, fileName);
        if (File.Exists(newPath) && !overwrite)
        {
            string ext = Path.GetExtension(file);
            string filenameX = Path.GetFileNameWithoutExtension(file);
            // dodać sprawdzenie czy istniejący plik nie ma nazwy z (x)
            // i jeżeli ma to zwiększyć x o 1 i sprawdzić czy istnieje plik o takiej nazwie

            int i = 1;
            // to o ile dobrze pamiętam to jest zmiana nazwy pliku o ile istnieje plik o tekiej samej nazwie
            // w katalogu docelowym i o ile nie ma zaznaczonego nadpisywania pliku docelowego
            while (File.Exists(newPath))
            {
                fileName = filenameX + "(" + i.ToString() + ")" + ext;
                newPath = System.IO.Path.Combine(path, fileName);
                i++;
            }

        }
        //Debug.WriteLine("MoveFileToFolder: newPath: " + newPath);
        try
        {
            //tu dodać sprawdzanie czy w miejscu docelowym istnieje już pliko podanej nazwie
            //jak istnieje to modyfikujemy ścieżkę (1) lub (x) i przenosimy wskazany
            if (copy)
            {
                Log.Write(LogLevel.Info, $"Copy file: {file} to {newPath}");
                File.Copy(file, newPath, overwrite);
            }
            else 
            {
                Log.Write(LogLevel.Info, $"Move file: {file} to {newPath}");
                File.Move(file, newPath);
            }
            return newPath;
        }
        catch (Exception ex)
        {
            Log.Write(LogLevel.Error, $"Error occurred while moving file: {ex.Message}");
            return String.Empty;
        }
        //return String.Empty;
    }

    /**
     * odświeżenie listy plików w aktualnie przeglądanym katalogu
     * wykożystywane w dragdrop przy przenoszeniu plików
     */
    private void RefreshFileList()
    {
        ReloadFileList(SelectedItem);
        //to dlatego że Tree jest tablicą kilku drzew
        foreach (var treeItem in Tree)
        {
            TreeModel? item = treeItem.GetElementByPath(SelectedItem.Path);
            if (item != null) item.CountFiles = GetCountFiles(item.Path);//odejmuje liczbę plików w katalogu
        }
    }

    #endregion DragDrop

    #region Clipboard

    public void CopyToClipboard()
    {
        Clipboard.SetData(DataFormats.FileDrop, "A String");
        //Clipboard.SetText(ToString());
    }


    #endregion

    #region Tree and View

    [RelayCommand]
    /// <summary>   
    /// wywoływane przy kliknięciu LBM na drzewie - zmiana przeglądanego katalogu
    /// </summary>
    /// <param name="parameter"></param>
    private void TreeModelLBMClick(TreeModel parameter)
    {       
        string View = SelectedViewModel.ToString().Split('.').Last();        
        if (parameter != null)
        {
            //zmiana katalogu tylko jeżeli jest inny niż aktualny
            //SelectedItemChanged
            if (SelectedItem != parameter)
            {
                SelectedItem = parameter;
                ReloadFileList(parameter);
            }else if (SelectedViewWindow != View)
            {
                //może to rozbudować tak żeby pominąć reload ??
                //albo tam dodać parametr wykluczający reload
                SelectionChanged(SelectedViewWindow);//aaa bo tu też jest reload :/
            }
        }
    }

    [RelayCommand]
    private void ThumbnailHeightMinus() { ThumbnailHeight--; }

    [RelayCommand]
    private void ThumbnailHeightPlus() { ThumbnailHeight++; }

    /// <summary>
    /// ma obliczać ilość plików możliwych do wyświetlenia ale tu jest jakiś bubel, bo źle liczy
    /// </summary>
    /// <param name="path"></param>
    /// <returns>ilość plików możliwych do wyświetlenia</returns>
    private int GetCountFiles(string path)
    {
        if (Directory.Exists(path))
        {
            SelectedTreeItem = path;//??
            var imFiles = Directory.EnumerateFiles(path);            
            int i = 0;
            i = imFiles.Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower())).Aggregate(0, (count, f) =>  count +1 );            
            return i;
        }
        return 0;
    }

    private void ReloadFileList(TreeModel treeModel)
    {
        //Debug.WriteLine("ReloadFileList");
        if (cts != null)
        {
            try
            {
                cts.Cancel();
                while (!cts.IsCancellationRequested && counter)
                {
                    Debug.WriteLine("ReloadFileList: still waiting...");
                    Thread.SpinWait(5000);
                }
                cts.Dispose();
                cts = null;
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
            }
        }
        try
        {
            if (treeModel != null)
            {
                string path = treeModel.Path;
                SelectedTreePath = path;
                if (cts == null) cts = new CancellationTokenSource();
                FileListLoad(path, cts.Token);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }


    }

    /// <summary>
    /// ładuje kolekcje FilesList i Photos według podanej ścieżki
    /// dodać znacznik określający czy Photos jest potrzebne
    /// PRZEBUDOWAĆ - połączyć FilesIO i Photo
    /// </summary>
    /// <param name="path"></param>
    /// <param name="token"></param>
    private async void FileListLoad(string path, CancellationToken token)
    {        
        Photos.Clear();
        if (Directory.Exists(path))
        {
            SelectedTreeItem = path;
            var imFiles = BrokerFile.IGetFiles(path, Sortowaniekryterium, Sortowaniekierunek, patternArray);
            FileInfo finfo;
            var back = new BitmapImage(new Uri(@"pack://application:,,,/img/g1.png"));
            string maska = @"pack://application:,,,/img/g1.png";            
            string View = SelectedView.Split('.').Last();
            FilesToLoad = GetCountFiles(path);            
            foreach (var imFile in imFiles)
            {
                if (token.IsCancellationRequested)
                {
                    counter = false;                    
                    return;
                }
                counter = true;
                if (!token.IsCancellationRequested)
                {                    
                    try
                    {                        
                        finfo = new FileInfo(imFile);                        
                        Photo p = new Photo(imFile);
                        p.Size = finfo.Length;//BrokerFile.Prdouble(finfo.Length);
                        //p.RealSize = finfo.Length;
                        p.DateModified = finfo.LastWriteTime;
                        p.Icon = BlinkIcom;
                        p.maska = maska;
                        p.Image = back;
                        Photos.Add(p);
                        if (Photos.IndexOf(p) == 0)
                        {
                            p.IsSelected = true;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                counter = false;
            }
            if (GalleryView.Contains(View) && !token.IsCancellationRequested)
            {
               // Debug.WriteLine("FileListLoad Starting PhotosLoadImae...");
                FileLoaded = 0;//teraz jest to wymagane żeby liczyć od początku
                await PhotosLoadImae(token);
            }
            // tu wywala błąd przy przenoszeniu katalogów
            if (imFiles.Count() == 0) FileLoaded = 0;
        }
    }

    /// <summary>
    /// przeniesienie ładowania obrazów do osobnej metody, ponowna iteracja kolekcji
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    //private async Task PhotosLoadImae(List<Photo> PhotoSKopia, CancellationToken token)
    //{
    //    //Debug.WriteLine($"PhotosLoadImae - token: {token}")
    //    if (PhotoSKopia.Count > 0 && !token.IsCancellationRequested)
    //    {
    //        try
    //        {
    //            for (var  i = 0; i < PhotoSKopia.Count && !token.IsCancellationRequested; i++)                
    //            {
    //                var photo = PhotoSKopia[i];
    //                FileLoaded = i+1;
    //                await photo.Load(token);
    //                //_= PhotoHelper.Load(photo, token);
    //            }
    //        }
    //        catch(Exception ex)
    //        {
    //            Debug.WriteLine($"PhotosLoadImae: {ex.ToString()}");
    //        }
    //    }
    //}


    private async Task PhotosLoadImae(CancellationToken token)
    {
        if (Photos.Count > 0 && !token.IsCancellationRequested)
        {  
            try
            {                   
                string path = string.Empty;
                for (var i = 0; i < Photos.Count && !token.IsCancellationRequested; i++)
                {
                    var photo = Photos[i];
                    if (photo.Image is BitmapImage BI) 
                    {
                        path = BI.UriSource.AbsolutePath.ToString();
                    }
                    if (path == "/img/g1.png")
                    {
                        FileLoaded++; //=++licznik;//gdzieś następuje dodanie kolejnych liczb do FileLoaded, trzeba to sprawdzićS
                        //Debug.WriteLine($"PhotosLoadImae: Loading {photo.Name}, FileLoaded: {FileLoaded}, i: {i}, licznik: {licznik}");
                        await photo.Load(token);
                        //_= PhotoHelper.Load(photo,token,true);
                    }
                    path = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PhotosLoadImae: {ex.ToString()}");
            }
        }
    }

    /// <summary>
    /// do przebudowania, ma dodawać nowe główne foldery na końcu drzewa
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public bool AddRootFolderToTree(string folder)
    {
        if (TreePath == string.Empty)
        {
            TreePath = folder;
        }
        else
        {
            TreePath += ";" + folder;
        }
        if (Tree == null) Tree = [];
        Tree.Add(ScanPath(folder));
        return true;
    }


    //do przebudowania, ma wyszukać co ma usunąć i to usunąć a nie resetować całe drzewo
    public void RemoveFolder(string folder)
    {
        // Debug.WriteLine(folder);
        var x = TreePath.Split(';').ToList();
        _ = x.Remove(folder);// a tu chyba jest tworzony pusty string zamiast usuwać komurkę
        TreePath = string.Join(";", x);// to chyba dodaje nam pusty string na końcu

        foreach (var reTree in Tree)
        {
            if ((reTree.Path == folder) || string.IsNullOrEmpty(reTree.Path))
            {
                _ = Tree.Remove(reTree);
                return;
            }
        }
    }
    
    /// <summary>
    /// buduje drzewo aplikacji z zapisanych wczesniej ścieżek
    /// </summary>
    /// <param name="pathEx">opcjonalna ściezka katalogu do wyśiwtlenia, zastępuje parametr SelectedTreePath</param>
    private void BuildTree(string pathEx = "")
    {

        if ((Tree != null) && (Tree.Count > 0)) Tree.Clear();
        if (Tree == null) Tree = [];
        if ((TreePath != string.Empty) && (!string.IsNullOrWhiteSpace(TreePath)))
        {
            var listTreePath = TreePath.Split(";").ToList();
            foreach (var path in listTreePath)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (!string.IsNullOrWhiteSpace(pathEx))
                    {                        
                        Tree.Add(ScanPath(path, pathEx));
                    }
                    else
                    {
                        Tree.Add(ScanPath(path, SelectedTreePath));
                    }
                }
            }            
        }
    }


    //dodać sprawdzanie czy dodawany katalog nie jest na liście wykluczeń
    //dodać metodę scanExpand do zapisywania w pliku ini
    private TreeModel ScanPath(string path, string select = "")
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        //Debug.WriteLine("ScanPath: " + path);
        DirectoryInfo di = new(path);
        select = select.Trim();
        TreeModel tree = new() { Path = path, Name = di.Name, CountFiles = GetCountFiles(path) };
        if (!string.IsNullOrWhiteSpace(select))
        {
            //Debug.WriteLine($" Select: {select}, path: {path},SelectedTreePath: {SelectedTreePath} ");
            //wielkość liter powoduje problem, ale nie tylko tu, gdzieś jeszcze dalej
            if (select.Contains(path) || (select.ToLower().Contains(path.ToLower())))
            {
                //Debug.WriteLine($"ScanPath - Select path:{select},path: {path} ");
                if(select != path)
                tree.IsExpanded = true;
                if (select == path)
                {
                    tree.IsSelected = true;
                    SelectedItem = tree;
                    //Debug.WriteLine("ScanPath - SelectedItemPath: " + SelectedItemPath);
                }
            }
        }
        
        if (!string.IsNullOrWhiteSpace(path))
        {
            path = path.Trim();
        }

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                var imDirectories = Directory.EnumerateDirectories(path);
                foreach (var imDir in imDirectories)
                {
                    DirectoryInfo directoryInfo = new(imDir);
                    if (BrokerFile.AtrDir(imDir))
                    {
                        var t1 = ScanPath(imDir, select);
                        t1.Parent = tree;
                        tree.AddChild(t1);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        return tree;
    }

    /// <summary>
    /// to będzie wykozystywane w przyszłości do wykluczania katalogów z widoku drzewa
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool PathExcluded(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (string.IsNullOrEmpty(PathFolderExcluded)) return false;
        var arr = PathFolderExcluded.Split(";");
        foreach (var item in arr)
        {
            if (path == item) return true;
            //if (item.Contains(path)) return true; 
            //if(path.Contains(item)) return true;
        }
        return false;
    }

    #region zapis/odczyt z ini
    private string TreePath
    {
        get => BrokerIni.PathFolderTree;
        set => SetProperty(BrokerIni.PathFolderTree, value, BrokerIni, static (u, n) => u.PathFolderTree = n);
    }

    /// <summary>
    /// katalog wybrany w drzewie do przeglądania w galerii lub innej liście
    /// </summary>
    private string SelectedTreePath
    {
        get => BrokerIni.SelectedPathFolderTree;
        set => SetProperty(BrokerIni.SelectedPathFolderTree, value, BrokerIni, static (u, n) => u.SelectedPathFolderTree = n);
    }

    private string PathFolderExcluded
    {
        get => BrokerIni.PathFolderExcluded;
        set => SetProperty(BrokerIni.PathFolderExcluded, value, BrokerIni, static (u, n) => u.PathFolderExcluded = n);
    }

    private string SelectedViewWindow
    {
        get => BrokerIni.SelectedView;
        set => SetProperty(BrokerIni.SelectedView, value, BrokerIni, static (u, n) => u.SelectedView = n);
    }
    #endregion
    #endregion Tree and View

    #region Theme Window
    [RelayCommand]
    private void SwitchThemeMode()
    {
        PaletteHelper palette = new PaletteHelper();
        var theme = palette.GetTheme();
        if (SwitchTglButton)
        {
            Color primaryColor = Colors.Blue;
            theme.SetPrimaryColor(primaryColor);
            Color secondaryColor = Colors.Violet;
            theme.SetSecondaryColor(secondaryColor);
            theme.SetBaseTheme(BaseTheme.Dark);
        }
        else
        {
            Color primaryColor = Colors.Red;
            theme.SetPrimaryColor(primaryColor);
            Color secondaryColor = Colors.Yellow;
            theme.SetSecondaryColor(secondaryColor);
            theme.SetBaseTheme(BaseTheme.Light);
        }
        palette.SetTheme(theme);
        SwitchToggleButton = SwitchTglButton;
    }


    private bool SwitchToggleButton
    {
        get => BrokerIni.SwitchToggleButton;
        set => SetProperty(BrokerIni.SwitchToggleButton, value, BrokerIni, static (u, n) => u.SwitchToggleButton = n);        
    }



    private Color PrimaryColor
    {
        get => BrokerIni.PrimaryColor;
        set => SetProperty(BrokerIni.PrimaryColor, value, BrokerIni, static (u, n) => u.PrimaryColor = n);
    }

    private Color SecondaryColor
    {
        get => BrokerIni.SecondaryColor;
        set => SetProperty(BrokerIni.SecondaryColor, value, BrokerIni, static (u, n) => u.SecondaryColor = n);
    }

    #endregion Theme Window

    #region WindowState
    [RelayCommand]
    private void onCmdMin()
    {
        CurMainWindowState = WindowState.Minimized;
        // jak jest zamykany to nie należy zapisywać ustawieńzminimalizowanego bo są problemy
        // przewidziec i rozwiązać
    }

    [RelayCommand]
    private void onCmdMax()
    {   //tu można zostawić sam stan a wielkość i połozenie przeniesć do brokera
        // tam zmienią przy zmianie stanu
        if (CurMainWindowState == WindowState.Normal)
        {
            LastWidth = Width;
            LastHeihgt = Height;
            LastTop = Top;
            LastLeft = Left;
            CurMainWindowState = WindowState.Maximized;
            ContentMax = "2";
            //ButtonRefresh();

        }
        else
        {
            CurMainWindowState = WindowState.Normal;
            ContentMax = "1";
            Width = LastWidth;
            Height = LastHeihgt;
            Top = LastTop;
            Left = LastLeft;
            //ButtonRefresh();
        }
    }
    [RelayCommand]
    private void ButtonRefresh()
    {
        //Debug.WriteLine("ButtonRefresh: CurMainWindowState: " + CurMainWindowState.ToString());
        if (CurMainWindowState == WindowState.Normal)
        {
            RestoreButton = false;
            MaximizeButton = true;
        }
        else if (CurMainWindowState == WindowState.Maximized)
        {
            RestoreButton = true;
            MaximizeButton = false;

        }
    }

    [RelayCommand]
    private void Test()
    {
        Debug.WriteLine("Test");
    }

    [ObservableProperty]
    private string _ContentMax = string.Empty;

    private double LastWidth
    {
        get => BrokerIni.LastWidth;
        set => SetProperty(BrokerIni.LastWidth, value, BrokerIni, static (u, n) => u.LastWidth = n);
    }
    private double LastHeihgt
    {
        get => BrokerIni.LastHeihgt;
        set => SetProperty(BrokerIni.LastHeihgt, value, BrokerIni, static (u, n) => u.LastHeihgt = n);
    }
    private double LastTop
    {
        get => BrokerIni.LastTop;
        set => SetProperty(BrokerIni.LastTop, value, BrokerIni, static (u, n) => u.LastTop = n);
    }
    private double LastLeft
    {
        get => BrokerIni.LastLeft;
        set => SetProperty(BrokerIni.LastLeft, value, BrokerIni, static (u, n) => u.LastLeft = n);
    }



    public WindowState CurMainWindowState
    {
        get => BrokerIni.CurMainWindowState;
        set => SetProperty(BrokerIni.CurMainWindowState, value, BrokerIni, static (u, n) => u.CurMainWindowState = n);
    }

    public string CurMainWindowStateString
    {
        get => BrokerIni.CurMainWindowState.ToString();
        set => SetProperty(BrokerIni.CurMainWindowStateString, value, BrokerIni, static (u, n) => u.CurMainWindowStateString = n);
    }

    public double Width
    {
        get => BrokerIni.WindowWidth;
        set => SetProperty(BrokerIni.WindowWidth, value, BrokerIni, static (u, n) => u.WindowWidth = n);
    }

    public double Height
    {
        get => BrokerIni.WindowHeight;
        set => SetProperty(BrokerIni.WindowHeight, value, BrokerIni, static (u, n) => u.WindowHeight = n);
    }

    public double Top
    {
        get => BrokerIni.WindowTop;
        set => SetProperty(BrokerIni.WindowTop, value, BrokerIni, static (u, n) => u.WindowTop = n);
    }

    public double Left
    {
        get => BrokerIni.WindowLeft;
        set => SetProperty(BrokerIni.WindowLeft, value, BrokerIni, static (u, n) => u.WindowLeft = n);
    }
    

    #endregion WindowState
}
