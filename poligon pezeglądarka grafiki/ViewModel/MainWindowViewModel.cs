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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;





namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    /***** NOTATKI****
     * scalić FilesIO i Photo w jeden ojekt FileIO dodać dane typu movie
     * 
     * 
     */

    #region Properties and [observableProperty]
    #region Collection
    private BrokerIni iniFile = BrokerIni.GetBroker();  //new BrokerIni();
    private ImageSource BlinkIcom { get; set; } = PhotoHelper.CreateEmtpyBitmapSource();

    //private ObservableCollection<TreeModel> _tree = [];
    /// <summary>
    /// kolekcja zawierająca tablicę z osobnymi drzewami katalogów do przeglądu
    /// </summary>
    public ObservableCollection<TreeModel>? Tree { get; set; } = [];

    //to mże usuną i przepisać wszystko na Ptors
    /// <summary>
    /// kolekcja przechowująca tablicę obrazów w katalogu do widoków tabelarycznych
    /// </summary>
    public ObservableCollection<FilesIO> FilesList { get; set; } = [];

    /// <summary>
    /// lista wyliczeniowa możliwości sortowania
    /// </summary>  
    public ObservableCollection<string> TypSotowania { get; set; } = ["Nazwa", "Data", "Wielkość"];

    //public List<string> TypSortowania = ["Nazwa","Data", "Wielkość"];

    /// <summary>
    /// kolekcja przechowująca kolekcją obrazów w katalogu do urzytku w widoku galerii
    /// </summary>
    public ObservableCollection<Photo> Photos { get; set; } = [];

    public ObservableCollection<MenuRadioButton> MenuSort { get; set; } = [];
    //public ObservableCollection<MenuRadioButton> MenuSortAD { get; set; } = [];

    #endregion Collection

    [ObservableProperty]
    private SelectionMode _CurSelectionMode = SelectionMode.Single;

    public readonly DateTime CreationTime = File.GetCreationTime(Assembly.GetExecutingAssembly().Location);

    [ObservableProperty]
    private int _ThumbnailHeight = 200;

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
    private string _FilesToLoad = string.Empty;

    [ObservableProperty]
    private string _FileLoaded = string.Empty;


    /// <summary>
    /// odnośniki do katalogów do wyswietlenia w ustawieniach
    /// to jest błędne, wskazuje miejsce z którego został uruchomiony program a nie jego katalog
    /// w krócie na pulpicie wystarczy dodać katalog startowy i działa
    /// </summary>
    public string ActualPath { get => Directory.GetCurrentDirectory(); }
    public string InstallPath { get => BrokerFile.GetUserAppDataPath; }

    //public readonly string[] TypSotowania = ["Nazwa", "Data", "Rozmiar"];

    #region Interface
    public bool FirstAdd
    {
        get => !(TreePath.Count() > 0);
    }

    public bool VisibleSeparator
    {
        get => !iniFile.VisibleToolBar;
    }

    public bool VisibleToolBar
    {
        get => iniFile.VisibleToolBar;
        set => SetProperty(iniFile.VisibleToolBar, value, iniFile, static (u, n) => u.VisibleToolBar = n);
    }

    public bool VisibleStatusBar
    {
        get => iniFile.VisibleStatusBar;
        set => SetProperty(iniFile.VisibleStatusBar, value, iniFile, static (u, n) => u.VisibleStatusBar = n);
    }

    public bool VisibleFilesInTree
    {
        get => iniFile.VisibleFilesInTree;
        set => SetProperty(iniFile.VisibleFilesInTree, value, iniFile, static (u, n) => u.VisibleFilesInTree = n);
    }
    /*
    public bool OnlyFoldersWithFiles
    {
        get => iniFile.OnlyFoldersWithFiles;
        set => SetProperty(iniFile.OnlyFoldersWithFiles, value, iniFile, (u, n) => u.OnlyFoldersWithFiles = n);
    }
    */
    #endregion Interface

    private bool _cut = false;
    public string pattern { get; set; } = @"\.(jpg|jpeg|bmp|png|webp)";
    private string[] patternArray = [".jpg", ".jpeg",".bmp",".png",".webp"];

    #region Private

    /// <summary>
    /// zmienna wspomagająca wklejanie plików do katalogu wybranego w menu kontekstowym drzewa
    /// </summary>
    private TreeModel? MenuSelectedTreeItem = null;
    private bool counter = false;
    private CancellationTokenSource? cts = null;
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
        //Debug.WriteLine("install to: " + BrokerFile.GetUserAppDataPath + " from: " + Directory.GetCurrentDirectory());        
        string sourcedir = Directory.GetCurrentDirectory();
        string destinyDir = BrokerFile.GetUserAppDataPath;
        string[] dir = Directory.GetDirectories(sourcedir);
        string destDirImg = string.Empty, dirName, subDirectory = string.Empty, pathExe = string.Empty;
        /*
        foreach (string directory in dir)
        {
            dirName = Path.GetFileName(directory);            
            if (dirName == "img")
            {
                destDirImg = Path.Combine(destinyDir, dirName);
                _ = Directory.CreateDirectory(destDirImg);
                subDirectory = directory;                
                break;
            }
        }*/
        destDirImg = Path.Combine(destinyDir, "img");
        subDirectory = Path.Combine(sourcedir, "img");
        _ = Directory.CreateDirectory(destDirImg);
        string[] files = Directory.GetFiles(sourcedir);
        foreach (string file in files)
        {
            if (File.Exists(file))
            {
                string ext = Path.GetExtension(file);
                //string fileName = Path.GetFileName(file);
                if (ext == ".exe" || ext == ".dll" || ext == ".json")
                {
                    _ = FileMove(file, destinyDir, true);
                    if (ext == ".exe")
                    {
                        pathExe = Path.Combine(destinyDir, Path.GetFileName(file));
                    }
                }
            }
        }
        if (!string.IsNullOrEmpty(subDirectory) && !string.IsNullOrEmpty(destDirImg))
        {
            files = Directory.GetFiles(subDirectory);
            foreach (string file in files)
            {
                _ = FileMove(file, destDirImg, true);
            }
            if (pathExe != string.Empty) StartExe(pathExe);
            Application.Current.Shutdown();
        }//tu dodać info o błędzie w kopiowaniu


    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    private bool _InstallCanExecute = true;

    [RelayCommand]
    private void InsatallCanExecuteTest()
    {
        string directoryApp = BrokerFile.GetUserAppDataPath;
        string[] files = Directory.GetFiles(directoryApp);
        string sourceDir = Directory.GetCurrentDirectory();
        if (files.Length > 1)
        {
            InstallCanExecute = false;
        }
        else
        {
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
        }
        if (directoryApp != sourceDir)
        {
            UpdateCanExecute = !InstallCanExecute;
        }
        else
        {
            UpdateCanExecute = false;
        }

    }

    [RelayCommand(CanExecute = nameof(UpdateCanExecute))]
    private void Update()
    {
        //Debug.WriteLine("update");
        //tu jakoś muszę jeszcze zrobić sprawdzanie wersji
        string DestFolder = BrokerFile.GetUserAppDataPath;
        string sourcedir = Directory.GetCurrentDirectory();
        if (Directory.Exists(DestFolder) && (DestFolder != sourcedir))
        {
            var files = Directory.GetFiles(DestFolder);
            var sourceFiles = Directory.GetFiles(sourcedir);
            if (files.Length > 1)
            {
                foreach (var file in sourceFiles)
                {
                    var filename = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(DestFolder, filename), true);
                }
            }
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    private bool _UpdateCanExecute = false;

    private void StartExe(string path, string param = "")
    {
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
                        //ale tu to naprawiam
                        _ = Process.Start(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wystąpił błąd: {ex.Message}");
            }
        }
    }

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeinstallCommand))]
    private bool _DeinstallCanExecute = false;

    /// <summary>
    /// otwiera katalog programu w explorerze
    /// katalog programu to katalog dzie jest plik ini i gdzie zostanie zainstalowany program
    /// </summary>
    [RelayCommand]
    private void OpenExplorer()
    {
        string destinyDir = BrokerFile.GetUserAppDataPath;
        StartExe("explorer", destinyDir);
    }

    /// <summary>
    /// tworzy skrót na pulpicie o ile program jest zainstalowany
    /// </summary>
    [RelayCommand(CanExecute = nameof(ShortcutCanExecute))]
    private void ShortcutCall()
    {
        string env = BrokerFile.GetUserAppDataPath;
        string PathToExe = Path.Combine(env, "poligon pezeglądarka grafiki.exe");
        string PathToIco = Path.Combine(env, @"img\73042biohazard_109537(1).ico");
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
    private void ReloadView()
    {
        //Debug.WriteLine($"reload view - selectView: {SelectedViewWindow}");
        //ChangeView(SelectedViewWindow);
        SelectedView = SelectedViewWindow;//to na wszelki wypadek
        SelectionChanged(SelectedView);
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
            // Debug.WriteLine($"Selection Changed: {sx} ");
            //SelectedView = sx;
            if (sx != "Settings")
            {
                SelectedViewWindow = sx;//zapis do ini
                if (SelectedViewModel != null) SelectedViewModel = null;
                SelectedViewModel = CallMethod(sx);
                SelectedView = sx;//to na wszelki wypadek
            }
            else
            {
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
            }
            if (SelectedViewWindow == "Gallery") ReloadFileList(SelectedItem);
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
            MenuSelectedItem(null);
        }
    }

    #region Menu snd InputBindings
    [RelayCommand(CanExecute = nameof(ClipboardListenerResoult))]
    private void MenuPaste()
    {
        //Debug.WriteLine("MenuPaste called.");
        Match m;
        if (Clipboard.ContainsFileDropList())
        {
            var fileList = Clipboard.GetFileDropList();//zwraca StringCollection i taką kolekcję trzeba tam podawać
            foreach (var file in fileList)
            {
                if (System.IO.Path.GetExtension(file) is string ext)
                {
                    //Debug.WriteLine("Pasting file: " + file);
                    //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
                    m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        if (MenuSelectedTreeItem != null)
                        {
                            //to do wklejania do katalogu wskazanego poprzez context menu w drzewie
                            MoveFileToFolder(file, MenuSelectedTreeItem, !_cut);
                            MenuSelectedTreeItem = null;
                        }
                        else
                            //to do wklejania do aktualnie wybranego katalogu w drzewie
                            MoveFileToFolder(file, SelectedTreePath, !_cut);
                    }
                }
            }
            Clipboard.Clear();
            _ = RefreshClipboardListenerResoult();
        }
    }

    public void MenuSelectedItem(object parameter)
    {
        if (parameter is TreeModel ti)
        {
            MenuSelectedTreeItem = ti;
            //Debug.WriteLine("MenuSelectedItem: " + ti.Path);
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
        //Debug.WriteLine("MenuCopy called." +parameter.GetType().ToString() );
        //ok mamy prametr jako IList z zaznaczonymi elementami :)
        if (parameter is System.Collections.IList ph)
        {
            //string[] photos = [.. ph.Cast<Photo>().Select(static p => p.Path)];
            StringCollection paths = [.. ph.Cast<Photo>().Select(static p => p.Path)];
            _ = CopyX(paths);
        }
        else _ = CopyX();
    }

    /// <summary>
    /// Wycinanie do schowka systemowego zaznaczonych elementów w galerii (CTRL+X)
    /// </summary>
    /// <param name="parameter">lista zaznaczonych elementów podan z XAML</param>
    [RelayCommand]
    private void MenuCut(object parameter)
    {
        //Debug.WriteLine("MenuCut called.");
        if (parameter is System.Collections.IList ph)
        {
            StringCollection paths = [.. ph.Cast<Photo>().Select(static p => p.Path)];
            _cut = CopyX(paths);
        }
        else
        {
            _cut = CopyX();
        }
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
        Match m;
        if (Clipboard.ContainsFileDropList())
        {
            //Debug.WriteLine("Clipboard contains FileDropList data.");//to jest
            var fileList = Clipboard.GetFileDropList();//zwraca StringCollection i taką kolekcję trzeba tam podawać
            foreach (var file in fileList)
            {
                if (System.IO.Path.GetExtension(file) is string ext)
                {
                    //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
                    //pattern to zmienne globalna, będzie ustawiana przy starcie z ini, na razie jest to string na stałe
                    m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                    if (m.Success) { ClipboardListenerResoult = true; return true; }
                }
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
                        selectedItem.CountFiles = x;
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


    public void SortAD(object DC)
    {
        if (DC is MenuRadioButton par)
        {
            string grupa = par.Grupa;           
                MenuSort.Where(q => q.Grupa == grupa).ToList().ForEach(x => x.IsChecked = false); 
            par.IsChecked = true;

            string kryterium = MenuSort.Where(q => q.IsChecked && q.Grupa == "kryterium").Select(q => q.Name).ToArray()[0].ToString();
            string kierunek = MenuSort.Where(predicate: q => q.IsChecked && q.Grupa == "kierunek").Select(q => q.Name).ToArray()[0].ToString();
            //tu dodać jeszcze zapisywanie do ini
            // Debug.WriteLine($"kryterium: {kryterium} + kierunek: {kierunek}");
            Sortowanie(kryterium, kierunek);

        }
    }

    #endregion

    [RelayCommand]
    private void DataGridLDoubleClick(object parameter)
    {
        Debug.WriteLine("LBM klik: " + parameter.ToString());
    }



    /// <summary>
    /// zwraca ilość zaznaczonych elementów w Photos
    /// </summary>
    /// <returns></returns>
    private int GetCountSelectedItem()
    {
        Photo[] photos = [.. Photos.Where(static p => p.IsSelected)];
        return photos.Length;
    }



    /// <summary>
    /// odśwież ilość plików w pasku statusu (po przenoszeniu, kopiowaniu, wycinaniu)
    /// </summary>
    private void RefreshStatusBarFileCount()
    {
        if (selectedItem != null)
        {
            string x = GetCountFiles(selectedItem.Path).ToString();
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
    }

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
        MenuSort.Add(new("Nazwa", (kryterium == "Nazwa"), 2, "kryterium"));
        MenuSort.Add(new("Data", (kryterium == "Data"), 3, "kryterium"));
        MenuSort.Add(new("Wielkość", (kryterium == "Wielkość"), 4, "kryterium"));
        MenuSort.Add(new("Rosnąco", (kierunek == "Rosnąco"), 0, "kierunek"));
        MenuSort.Add(new("Malejąco", (kierunek == "Malejąco"), 1, "kierunek"));
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
        BuildTree(Path.GetDirectoryName(path));//to tu wskazuje katalog ostatnio urzyty i tu jest problem

        SelectedView = SelectedViewWindow;
        if (SelectedView == String.Empty)
        {
            SelectedView = "Hello";
        }
        SelectedViewModel = CallMethod(SelectedView);

        try
        {
            cts = new CancellationTokenSource();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string pathFile = Path.GetDirectoryName(path);
                //Debug.WriteLine($"ściezka do katalogu: {pathFile}");
                FileListLoad(pathFile, cts.Token);
            }
            else
            {
                //Debug.WriteLine("nie znaleziono pliku: " + path);
                FileListLoad(SelectedTreePath, cts.Token);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    #endregion

    #region wywołania Control



    private object CallMethod(string p, object?[]? x = null)
    {
        Debug.WriteLine("CallMethod: " + p);
        Type thisType = GetType();
        if (thisType != null)
        {
            if ((thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo theMethod)
                && (theMethod != null))
            //MethodInfo theMethod = thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance);
            //bez parametrów
            //if (theMethod != null)
            {
                // Debug.WriteLine($"CallMethod - wywołanie poprawne thisType : {thisType} && theMethod: {theMethod}");
                var ret = theMethod?.Invoke(this, x);

                if (ret != null)
                {
                    //Debug.WriteLine($"{p} {ret.GetType}");
                    return ret;
                }
                //else
                //{
                //    Debug.WriteLine("CallMethod - tu jest problem");
                //}
            }
            //else
            //{
            //    Debug.WriteLine($"CallMethod - tu jest problem thisType.GetMethod... == null?: {thisType} && theMethod: {thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance)}");
            //}
            // z  parametrami
            //theMethod.Invoke(this, userParameters);
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
        get => iniFile.Sortowaniekryterium;
        set => SetProperty(iniFile.Sortowaniekryterium, value, iniFile, static (u,n) => u.Sortowaniekryterium = n);
    }

    private string Sortowaniekierunek
    {
        get =>iniFile.Sortowaniekierunek;
        set => SetProperty(iniFile.Sortowaniekierunek, value, iniFile, static (u,n) => u.Sortowaniekierunek = n);
    }
    private void Sortowanie(string kryterium, string kierunek)
    {
        //Debug.WriteLine($"sortowanie= ktyterium: {kryterium} + kierunek: {kierunek}");
        //dodać przerwanie ładowania obrazów lub sprawdzanie czy są załadowane
        // dodać pobieranie plików zgodne z aktualnym wybranym sortowaniem
        List<Photo> PhotoSKopia = Photos.ToList();
        if (PhotoSKopia.Count == Photos.Count)
        {
            //Debug.WriteLine("sortowanie, ok liczba się zgadza");
            string ViewPath = SelectedTreePath;
            //Debug.WriteLine($" aktualny folder: {ViewPath}");
            //string[] files;
            //string[] files = GetFiles(ViewPath, kryterium, kierunek);
            string[] files =  BrokerFile.GetFiles(ViewPath, kryterium, kierunek);
            //Debug.WriteLine($" files type: {files.GetType().ToString}");
            Photos.Clear();
            foreach (var file in files)
            {
                Photos.Add(PhotoSKopia.First(x => x.Path == file));
            }
            PhotoSKopia.Clear();
            Sortowaniekierunek = kierunek;
            Sortowaniekryterium = kryterium;
        }
        /*
         * sortowanie po nazwie można zrobić bez wczytywanie plików
         * po dacie czy rozmiarze to już chyba trzeba wczytać
         * ciekawe ile zajmuje struktura z FileInfo? może ją dołączyć do obiektu File?
         * FileInfo to objekt i raczej nie będę go dołączał, ale wezmęz niego informacje
         * zmienię kod pod niego
         * 
        List<string> words = new List<string> { "falcon", "order", "war", "sky" };
        var sortedWords = words.OrderBy(w => w);
        // sortedWords is an IEnumerable<string> (or use .ToList() to make a new List<string>)
        // Result: { "falcon", "order", "sky", "war" }
        var descendingWords = words.OrderByDescending(w => w);
        // Result: { "war", "sky", "order", "falcon" }
        //Możesz użyć ThenBy() lub ThenByDescending() w celu określenia drugorzędnych kryteriów sortowania
        //var sortedPeople = people.OrderBy(p => p.Name.Length).ThenBy(p => p.Age).ToList();
        */

    }
    #endregion

    /*
    private string[] GetFiles(string folder, string kryterium, string kierunek)
    {
        string[] files = [];
        if (kierunek == "Rosnąco")
        {  
            if (kryterium == "Nazwa")
            {                
               return files = Directory.GetFiles(folder).Where(f => patternArray.Contains(  new FileInfo(f).Extension.ToLower()) ).OrderBy(f => new FileInfo(f).Name).ToArray();
            }
            else if (kryterium == "Data")
            {
                return files = Directory.GetFiles(folder).Where(f => patternArray.Contains(new FileInfo(f).Extension.ToLower()))
                    .OrderBy(f => new FileInfo(f).CreationTime).ToArray();//Select(static fn => new FileInfo(fn)).OrderBy(f => f.CreationTime);
            }
            else if (kryterium == "Wielkość")
            {
                return files = Directory.GetFiles(folder).Where(f => patternArray.Contains(new FileInfo(f).Extension.ToLower()))
                    .OrderBy(f => new FileInfo(f).Length).ToArray();
            }
        }
        else
        {
            if (kryterium == "Nazwa")
            {
                return files = Directory.GetFiles(folder).Where(f => patternArray.Contains(new FileInfo(f).Extension.ToLower()))
                    .OrderByDescending(f => new FileInfo(f).Name).ToArray();
            }
            else if (kryterium == "Data")
            {
                return files = Directory.GetFiles(folder).Where(f => patternArray.Contains(new FileInfo(f).Extension.ToLower()))
                    .OrderByDescending(f => new FileInfo(f).CreationTime).ToArray();//Select(static fn => new FileInfo(fn)).OrderBy(f => f.CreationTime);
            }
            else if (kryterium == "Wielkość")
            {
                return files = Directory.GetFiles(folder).Where(f => patternArray.Contains(new FileInfo(f).Extension.ToLower()))
                    .OrderByDescending(f => new FileInfo(f).Length).ToArray();
            }
            //return files = Directory.GetFiles(folder).OrderByDescending(f => new FileInfo(f).Name).ToArray();//OrderByDescending(f => f.Name);
        }
        return files;
    }
    */
    public void RenameFile(Photo photo, string newName)
    {
        newName = photo.Path.Substring(0, photo.Path.LastIndexOf('\\') + 1) + newName;
        bool x = BrokerFile.RenameFile(photo.Path, newName);
        if (x)
        {
            photo.rename(newName);
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
                    Thread.SpinWait(50000);
                }
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
            //Directory.CreateDirectory(newPath);
            BrokerFile.CreateDirectory(newPath);
            //Debug.WriteLine("AddFolder: " + newPath);
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
                /*
                if ((treeModel.GetSelfFromParent() is TreeModel xuz) && (xuz != null))
                {
                    xuz.Children.Clear();
                    xuz.Addchild(ScanPath(xuz.Path).Children);
                    xuz.IsExpanded = true;
                    if ((xuz.FindChild(x) is TreeModel newItem) && (newItem != null))
                    {
                        newItem.IsExpanded = true;
                        return newItem;
                    }
                }
                else if (treeModel.Parent == null)
                {*/
                //jest reochę problemów z aktualizacją drzewa ;(
                //Debug.WriteLine($"tree path:{treeModel.Path} ");
                treeModel.Children.Clear();
                treeModel.Addchild(ScanPath(treeModel.Path).Children);
                treeModel.IsExpanded = true;

                //}
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// inicjuje usuwanie folderu z dysku do kosza, aktualizuje drzewo
    /// </summary>
    /// <param name="treeModel"></param>
    public void DeleteFolder(TreeModel treeModel)
    {
        if (treeModel != null)
        {
            bool x = DeleteFolder(treeModel.Path);
            if (x)
            {
                var xuz = treeModel.GetSelfFromParent();
                if (xuz != null)
                {
                    _ = (xuz.Parent?.Children.Remove(xuz));
                }
            }
        }
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
            //Debug.WriteLine("DeleteFolder: " + folder);
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



    #endregion Folders and Files

    #region okna
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



    private void MoveFoderToFolder(string folder, string DestinyPath)
    {
        //Debug.WriteLine($"MoveFoderToFolder: Source: {folder}, Destiny: {DestinyPath}");
        //tu brakuje testowania czy plik nie jest empty itd...
        // tu powinny być wywołane wyjatki jeżeli coś jest nie tak i obsłużone wyżej
        if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(DestinyPath))
        {
            //Debug.WriteLine("MoveFoderToFolder: brak ścieżki źródłowej lub docelowej");
            throw new FileNotFoundException(@"[Katalog żródłowy lub docelowy nie istnieje]");
            //return;
        }
        if (folder == DestinyPath)
        {
            Debug.WriteLine("MoveFoderToFolder: ścieżka źródłowa i docelowa są takie same");
            return;
        }
        string newDestinyPath = Path.Combine(DestinyPath, folder.Substring(folder.LastIndexOf('\\') + 1));
        //Debug.WriteLine($"{newDestinyPath}");
        if ((!File.Exists(folder)) && (Directory.Exists(folder)) && (Directory.Exists(DestinyPath))
            && (!Directory.Exists(newDestinyPath)))
        {

            try
            {
                Debug.WriteLine($"deftiny: {newDestinyPath}" + $" Source: {folder}");
                //tu pewnie trzeba zatrzymać wątek ładujący pliki z katalogu źródłowego
                if (cts != null)
                {
                    cts.Cancel();
                    //Debug.WriteLine("MoveFoderToFolder: Waiting for cancellation");
                    while (!cts.IsCancellationRequested)
                    {
                        //Debug.WriteLine("MoveFoderToFolder: still waiting...");
                        Thread.SpinWait(50000);
                    }
                    //Debug.WriteLine("MoveFoderToFolder: Canceled");
                }

                Directory.Move(folder, newDestinyPath);
                TreeModel? TreeFoldr = null, TreeDestinyPath = null;
                foreach (var tree in Tree)
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
                        //to na wypadek gdyby coś zostało znalezione ale ścieżki się nie zgadzały
                        //co nie powinno się zdażyć, tu powinien być wywołany wyjątek!!
                        TreeFoldr = null;
                        TreeDestinyPath = null;
                    }
                }
                //Debug.WriteLine("TreeFoldr.Path: " + TreeFoldr.Path + ", TreeDrstinyPath.Path: "
                // + TreeDestinyPath.Path+ ", DestinyPath: "+ DestinyPath);

                //dobra to się nie wykonuje tu coś namieszałem

                if ((TreeFoldr != null) && (TreeDestinyPath != null) && (TreeFoldr.Path != string.Empty)
                    && (TreeDestinyPath.Path != string.Empty))
                {
                    //Debug.WriteLine("MoveFoderToFolder: Refreshing trees");
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
                Debug.WriteLine(ex.ToString());
            }
            //return selectedItem;
        }
        else if ((Directory.Exists(newDestinyPath))) Debug.WriteLine("katalog już istnieje");
    }


    /// <summary>
    /// to w zasadzie zakończenie przenoszenia, ma za zadanie odświezyć aktualny katalog
    /// </summary>
    /// <remarks></remarks>
    public void MoveFileToFolder()
    {
        //MoveFileToFolder(String.Empty, String.Empty);
        //MoveFileToFolder(String.Empty, null);// niejednoznaczne odwołanie
        RefreshFileList();
    }
    /// <summary>
    /// to jest do wrzucania plików do aktualnmie wyświetlanego folderu w listbox
    /// </summary>
    /// <param name="file"></param>
    /// <param name="copy"></param>
    public void MoveFileToFolder(string file, bool copy = false)
    {
        if (string.IsNullOrEmpty(file)) return;
        if (!File.Exists(file)) return;
        string ViewPath = SelectedTreePath;
        MoveFileToFolder(file, ViewPath, copy);

    }

    public void MoveFileToFolder(string file, TreeModel path, bool copy = false)
    {
        //Debug.WriteLine($"MoveFileToFolder: File: {file}, DestinyPath: {path.Path}, Copy: {copy}");
        if ((file == String.Empty) && (path == null) && !copy)
        {
            //czyszczenie kolekcji po przeniesieniu pliku do np explorera plików
            RefreshFileList();
            return;
        }

        if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Directory.Exists(path.Path) || !Path.HasExtension(file))
        {
            if (!File.Exists(file) && Directory.Exists(file))
            {
                MoveFoderToFolder(file, path.Path);
            }
            return;
        }

        //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
        string ext = Path.GetExtension(file).ToLower();
        Match m;
        m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
        if (m.Success)
        {
            string newFilePath = FileMove(file, path.Path, copy);
            string pathFile = System.IO.Path.GetDirectoryName(file);

            path.CountFiles = GetCountFiles(path.Path);//dodaje liczbę plików w katalogu docelowym
            selectedItem.CountFiles = GetCountFiles(selectedItem.Path);
            RefreshStatusBarFileCount();

            if (!copy)
                _ = Photos.Remove(Photos.FirstOrDefault(i => i.Path == file));
            //usuwamy go z FilesList o ile tam istnieje
            if ((FilesList != null) && (FilesList.Count > 0))
            {
                if (FilesList.Any(i => i.Path == file))
                    _ = FilesList.Remove(FilesList.FirstOrDefault(i => i.Path == file));
            }

            if (SelectedTreePath == path.Path)
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
                if (!newPhoto(newFilePath)) ReloadFileList(SelectedItem);//to jako ostateczność
            }

        }
        else Debug.WriteLine("MoveFileToFolder: nieobsługiwany format pliku: " + ext);
    }



    /// <summary>
    /// przenoszenie pliku do innego katalogu
    /// </summary>
    /// <param name="path">katalog docelowy</param>
    /// <param name="file">plik ze ścieżką</param>
    public void MoveFileToFolder(string file, string path, bool copy = false)
    {
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
                MoveFoderToFolder(file, path);
            }
            return;
        }
        //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
        string ext = Path.GetExtension(file).ToLower();
        Match m;
        m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
        if (m.Success)
        {
            string newFilePath = FileMove(file, path, copy);

            string pathFile = System.IO.Path.GetDirectoryName(file);
            //to dlatego że Tree jest tablicą kilku drzew
            //wyszukiwanie właściwego drzewa
            foreach (var treeItem in Tree)
            {
                TreeModel? item = treeItem.GetElementByPath(path);
                if (item != null) item.CountFiles = GetCountFiles(item.Path);//dodaje liczbę plików w katalogu docelowym
                item = treeItem.GetElementByPath(pathFile);
                if (item != null) item.CountFiles = GetCountFiles(item.Path);//odejmuje liczbę plików w katalogu źródłowym
                RefreshStatusBarFileCount();
            }
            //jeżeli nie kopiujemy to usuwamy go z Photos o ile tam istnieje
            if (!copy)
                _ = Photos.Remove(Photos.FirstOrDefault(i => i.Path == file));
            //usuwamy go z FilesList o ile tam istnieje
            if ((FilesList != null) && (FilesList.Count > 0))
            {
                if (FilesList.Any(i => i.Path == file))
                    _ = FilesList.Remove(FilesList.FirstOrDefault(i => i.Path == file));
            }

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
                if (!newPhoto(newFilePath)) ReloadFileList(SelectedItem);//to jako ostateczność
            }
        }
        else Debug.WriteLine("MoveFileToFolder: nieobsługiwany format pliku: " + ext);
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
    private string FileMove(string file, string path, bool copy = false)
    {
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
            Debug.WriteLine("MoveFileToFolder: path does not exist: " + path);
            return String.Empty;
        }
        //Debug.WriteLine("MoveFileToFolder: path: " + path + " , file: " + file);
        string fileName = System.IO.Path.GetFileName(file);
        string newPath = System.IO.Path.Combine(path, fileName);
        if (File.Exists(newPath))
        {
            string ext = Path.GetExtension(file);
            string filenameX = Path.GetFileNameWithoutExtension(file);
            //for (int i = 0; true; i++)
            //{
            //    fileName = filenameX + "(i)" + ext;
            //    newPath = System.IO.Path.Combine(path, fileName);
            //}
            int i = 1;
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
            if (copy) File.Copy(file, newPath);
            else File.Move(file, newPath);
            return newPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
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
        //Debug.WriteLine("TreeModelLBMClick");
        if (parameter != null)
        {
            if (selectedItem == parameter)
            {
                //tu zrobić tak żey odpalać okno zapisane (gallery itd) zamiast na przykład settings czy welcome
            }
            SelectedItem = parameter;
            ReloadFileList(parameter);
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
            SelectedTreeItem = path;
            var imFiles = Directory.EnumerateFiles(path);
            string ext;
            Match m;
            int i = 0;
            //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
            foreach (var imFile in imFiles)
            {
                ext = System.IO.Path.GetExtension(imFile);
                m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                if (m.Success) i++;
            }
            return i;
        }
        return 0;
    }

    private void ReloadFileList(TreeModel treeModel)
    {
        if (cts != null)
        {
            //cts.Token.ThrowIfCancellationRequested();
            cts.Cancel();
            while (!cts.IsCancellationRequested && counter)
            {
                //Debug.WriteLine("ReloadFileList: still waiting...");
                Thread.SpinWait(50000);
            }
            cts.Dispose();
            cts = null;
        }
        try
        {

            if (treeModel != null)
            {
                string path = treeModel.Path;
                //Debug.WriteLine("LBM klik - ReloadFileList, path:" + path);
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
        //Debug.WriteLine("FileListLoad");
        //SelectedTreePath
        FilesList.Clear();

        Photos.Clear();
        GC.Collect();
        //Debug.WriteLine("skanowanie z: "+path);
        if (Directory.Exists(path))
        {
            SelectedTreeItem = path;
            //var imFiles = Directory.EnumerateFiles(path);//to może powodować problemy, tu zmienić na dostep z BrokerFile
            var imFiles = BrokerFile.GetFiles(path, Sortowaniekryterium, Sortowaniekierunek);
            FileInfo finfo;
            string ext, name;
            var back = new BitmapImage(new Uri(@"pack://application:,,,/img/g1.png"));
            string maska = @"pack://application:,,,/img/g1.png";
            Match m;
            string View = SelectedView.Split('.').Last();
            FilesToLoad = GetCountFiles(path).ToString();
            int licznik = 0;
            foreach (var imFile in imFiles.Select(static (value, i) => (value, i)))
            {
                if (token.IsCancellationRequested)
                {
                    counter = false;
                    return;
                }
                counter = true;
                ext = System.IO.Path.GetExtension(imFile.value);
                //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
                //pattern to zmienne globalna, będzie ustawiana przy starcie z ini, na razie jest to string na stałe
                m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                if (m.Success && !token.IsCancellationRequested)
                {
                    //FileLoaded = (++licznik).ToString();
                    try
                    {
                        name = System.IO.Path.GetFileName(imFile.value);
                        finfo = new FileInfo(imFile.value);

                        //to też ładować tylko w razie potrzeby!!, dodać warónek i sprawdzanie
                        //if (FileView.Contains(View))
                        FilesList.Add(new FilesIO()
                        {
                            Name = name,
                            Extension = ext,
                            Path = path,
                            Icon = BlinkIcom,
                            Size = BrokerFile.Prdouble(finfo.Length),
                            RealSize = finfo.Length.ToString()
                        });
                        //to jakoś zmienić, dać jakiś parametr bool zamiast uzależniać to od ładowanego widoku
                        //if ((View.ToLower().Contains("gallery") || View.Contains("Gallery2")) && !token.IsCancellationRequested)
                        if (GalleryView.Contains(View) && !token.IsCancellationRequested)
                        {
                            //Debug.WriteLine("load file: " + imFile.value);

                            Photo p = new Photo(imFile.value);
                            p.maska = maska;
                            p.Image = back;
                            Photos.Add(p);
                            if (Photos.IndexOf(p) == 0)
                            {
                                p.IsSelected = true;
                            }
                            //p.AddToken(token);
                            //jak zrobić żeby to było odpalane przez interfejs? a nie tutaj
                            //await p.Load(token);
                            //_ = p.Load(token);
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                counter = false;
            }
            await PhotosLoadImae(token);
            // wywala przy zmianie katalogu jak łąduje jeszcze obrazy, ale działi to chyba szybciej i bez problemu
            // brak licznika obrazów, liczy pliki które ładuje a nie obrazy
            //if (SelectedView == "Gallery") PhotosLoadImae(token);

            if (imFiles.Count() == 0) FileLoaded = "0";
        }
    }

    /// <summary>
    /// przeniesienie ładowania obrazów do osobnej metody, ponowna iteracja kolekcji
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task PhotosLoadImae(CancellationToken token)
    {
        if (Photos.Count > 0 && !token.IsCancellationRequested)
        {
            //może jak tu dam for  to to by przeszło?
            try
            {
                int licznik = 0;
                foreach (var photo in Photos)
                {
                    if (token.IsCancellationRequested) 
                    {
                        //to wogóle nie jest wywoływane !! jakby token nie był ustawiany!!
                        Debug.WriteLine($"Could not load {photo}");
                        return; 
                    }
                    FileLoaded = (++licznik).ToString();
                    await photo.Load(token);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    /// <summary>
    /// dodanie folderu do aktywnego drzewa według ścieżki
    /// </summary>
    /// <param name="folder"></param>
    public void AddFolderToTree(String folder)
    {
        //   AddRootFolderToTree(folder);
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
        //DirPath.Clear();
        //TreePath.Split(';').ToList().ForEach(path => DirPath.Add(path));
        foreach (var reTree in Tree)
        {
            if ((reTree.Path == folder) || string.IsNullOrEmpty(reTree.Path))
            {
                _ = Tree.Remove(reTree);
                return;
            }
        }
    }
    //public void ReBuildTree() => BuildTree(); //chwilowo nie potrzebne
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
                        //Debug.WriteLine($"BuildTree - pathEX: {pathEx}");
                        Tree.Add(ScanPath(path, pathEx));
                    }
                    else
                    {
                        Tree.Add(ScanPath(path, SelectedTreePath));
                    }
                }
            }
            //.ForEach(path => Tree.Add(ScanPath(path, SelectedTreePath)));
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
                tree.IsExpanded = true;
                if (select == path)
                {
                    tree.IsSelected = true;
                    SelectedItem = tree;
                    //Debug.WriteLine("ScanPath - SelectedItemPath: " + SelectedItemPath);
                }
            }
        }

        //var files = Directory.EnumerateFiles(path);
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
        get => iniFile.PathFolderTree;
        set => SetProperty(iniFile.PathFolderTree, value, iniFile, static (u, n) => u.PathFolderTree = n);
    }

    /// <summary>
    /// katalog wybrany w drzewie do przeglądania w galerii lub innej liście
    /// </summary>
    private string SelectedTreePath
    {
        get => iniFile.SelectedPathFolderTree;
        set => SetProperty(iniFile.SelectedPathFolderTree, value, iniFile, static (u, n) => u.SelectedPathFolderTree = n);
    }

    private string PathFolderExcluded
    {
        get => iniFile.PathFolderExcluded;
        set => SetProperty(iniFile.PathFolderExcluded, value, iniFile, static (u, n) => u.PathFolderExcluded = n);
    }

    private string SelectedViewWindow
    {
        get => iniFile.SelectedView;
        set => SetProperty(iniFile.SelectedView, value, iniFile, static (u, n) => u.SelectedView = n);
    }
    #endregion
    #endregion Tree and View

    #region Theme Window
    [RelayCommand]
    private void SwitchThemeMode()
    {
        PaletteHelper palette = new PaletteHelper();
        //var A = Colors.Red.A;

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
        get => iniFile.SwitchToggleButton;
        set => SetProperty(iniFile.SwitchToggleButton, value, iniFile, static (u, n) => u.SwitchToggleButton = n);
        //Debug.WriteLine("SwitchToggleButton zmiana na : "+value);
    }



    private Color PrimaryColor
    {
        get => iniFile.PrimaryColor;
        set => SetProperty(iniFile.PrimaryColor, value, iniFile, static (u, n) => u.PrimaryColor = n);
    }

    private Color SecondaryColor
    {
        get => iniFile.SecondaryColor;
        set => SetProperty(iniFile.SecondaryColor, value, iniFile, static (u, n) => u.SecondaryColor = n);
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
        get => iniFile.LastWidth;
        set => SetProperty(iniFile.LastWidth, value, iniFile, static (u, n) => u.LastWidth = n);
    }
    private double LastHeihgt
    {
        get => iniFile.LastHeihgt;
        set => SetProperty(iniFile.LastHeihgt, value, iniFile, static (u, n) => u.LastHeihgt = n);
    }
    private double LastTop
    {
        get => iniFile.LastTop;
        set => SetProperty(iniFile.LastTop, value, iniFile, static (u, n) => u.LastTop = n);
    }
    private double LastLeft
    {
        get => iniFile.LastLeft;
        set => SetProperty(iniFile.LastLeft, value, iniFile, static (u, n) => u.LastLeft = n);
    }



    public WindowState CurMainWindowState
    {
        get => iniFile.CurMainWindowState;
        set => SetProperty(iniFile.CurMainWindowState, value, iniFile, static (u, n) => u.CurMainWindowState = n);
    }

    public string CurMainWindowStateString
    {
        get => iniFile.CurMainWindowState.ToString();
        set => SetProperty(iniFile.CurMainWindowStateString, value, iniFile, static (u, n) => u.CurMainWindowStateString = n);
    }

    public double Width
    {
        get => iniFile.WindowWidth;
        set => SetProperty(iniFile.WindowWidth, value, iniFile, static (u, n) => u.WindowWidth = n);
    }

    public double Height
    {
        get => iniFile.WindowHeight;
        set => SetProperty(iniFile.WindowHeight, value, iniFile, static (u, n) => u.WindowHeight = n);
    }

    public double Top
    {
        get => iniFile.WindowTop;
        set => SetProperty(iniFile.WindowTop, value, iniFile, static (u, n) => u.WindowTop = n);
    }

    public double Left
    {
        get => iniFile.WindowLeft;
        set => SetProperty(iniFile.WindowLeft, value, iniFile, static (u, n) => u.WindowLeft = n);
    }

    #endregion WindowState
}
