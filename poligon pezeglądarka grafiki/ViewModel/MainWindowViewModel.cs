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
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;




namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    #region Properties and [observableProperty]
    #region Collection
    private BrokerIni iniFile = new BrokerIni();
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
    public ObservableCollection<string> DirPath { get; private set; } = [];
    
    /// <summary>
    /// kolekcja przechowująca kolekcją obrazów w katalogu do urzytku w widoku galerii
    /// </summary>
    public ObservableCollection<Photo> Photos { get; set; } = [];

    #endregion Collection

    [ObservableProperty]
    private SelectionMode _CurSelectionMode = SelectionMode.Single;

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

    [ObservableProperty]
    private object _selectedViewModel = new();

    [ObservableProperty]
    private string _WindowTitle = "Poligon - Przeglądarka";

    [ObservableProperty]
    private bool _SwitchTglButton ;

    [ObservableProperty]
    private bool restoreButton = false;
       
    [ObservableProperty]
    private bool maximizeButton = false;

[ObservableProperty]
    private double _MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;        

    [ObservableProperty]
    private  string _FilesToLoad = string.Empty;

    [ObservableProperty]
    private  string _FileLoaded = string.Empty;

    #region Interface
    public bool FirstAdd
    {
        get => !(TreePath.Count() >0);
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

    public string version { get; set; } = "251114 Alfa";
    public string pattern { get; set; }  = @"\.(jpg|jpeg|bmp|png|webp)";

    //public readonly string HGelpFile = @"pack://application:,,,/Help/help.rtf";
    #region Private


    private bool counter = false;
    private CancellationTokenSource? cts = null;
    string[] GalleryView = ["Gallery", "Gallery2"];
    string[] FileView =[ "FDataGrid", "FList" ];
    #endregion Private
    #endregion Properties and [observableProperty]

    //public string ViewPath => SelectedTreePath;

    #region RelayCommand
    [RelayCommand(CanExecute = nameof(ClipboardListenerResoult))]
    private void MenuPaste()
    {
        Match m;
        if (Clipboard.ContainsFileDropList())
        {
            var fileList = Clipboard.GetFileDropList();//zwraca StringCollection i taką kolekcję trzeba tam podawać
            foreach (var file in fileList)
            {
                if (System.IO.Path.GetExtension(file) is string ext)
                {
                    m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                    if (m.Success) { 
                        MoveFileToFolder(file, SelectedTreePath,!_cut);                        
                    }
                }
            }
        }
    }

    [RelayCommand]
    private void MenuCopy(object parameter)
    {
        CopyX();
    }

    [RelayCommand]
    private void MenuCut(object parameter)
    {
        if(CopyX())
        {
            _cut = true;
        }
    }


    private bool CopyX()
    {
        string[] pathsFx = [.. Photos.Where(static p => p.IsSelected).Select(static p => p.Path)];        
        if (pathsFx != null && pathsFx.Length > 0)
        {            
            StringCollection paths = [.. pathsFx];
            Clipboard.SetFileDropList(paths);
            _cut = false;
            return true;
        }
        return false;
    }

    [RelayCommand]
    private void MenuRefresh()
    {
        RefreshFileList();
    }

    [ObservableProperty]    
    [NotifyCanExecuteChangedFor(nameof(MenuPasteCommand))]
    private bool _ClipboardListenerResoult = false;

    public bool RefreshClipboardListenerResoult()
    {
        Debug.WriteLine("RefreshClipboardListenerResoult called.");
        Match m;
        if (Clipboard.ContainsFileDropList())
        {
            Debug.WriteLine("Clipboard contains FileDropList data.");//to jest
            var fileList = Clipboard.GetFileDropList();//zwraca StringCollection i taką kolekcję trzeba tam podawać
            foreach (var file in fileList)
            {
                if(System.IO.Path.GetExtension(file) is string ext)
                { 
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

    [RelayCommand]
    private void DataGridLDoubleClick(object parameter)
    {
        Debug.WriteLine("LBM klik: " + parameter.ToString());
    }

    [RelayCommand]
    private void DeleteThumbnails(object parameter)
    {
        if(parameter is System.Collections.IList ph)
        {
            DeleteFile([.. ph.Cast<Photo>()]);
        }
        else
        {
            Photo[] photos = [.. Photos.Where(static p => p.IsSelected)];
            DeleteFile(photos);
        }
        
    }

    

    [RelayCommand]
    private void SelectAll(object parameter)
    {
        //to będzie zaznaczanie wszystkich elementów w widoku galerii lub liście
       // Debug.WriteLine("SelectAll");
        if (Photos != null && Photos.Count > 0)
        {
            bool sel = true;
            if ((parameter is System.Collections.IList ph)&&(ph != null))
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

    private int GetCountSelectedItem()
    {
        int counter = 0;

        //Photo[] photos = [.. Photos.Where(static p => p.IsSelected)];
        //string[] pathsFx = [.. Photos.Where(static p => p.IsSelected).Select(static p => p.Path)];

         Photo[] photos = [.. Photos.Where(static p => p.IsSelected)];
        return photos.Length;
        /*
        if (Photos != null && Photos.Count > 0)
        {            
            foreach (var photo in Photos)
            {
                if (photo.IsSelected) counter++;
            }
        }
        return counter;*/
    }

    [RelayCommand]
    private void TestMethod(object parameter)
    {
        Debug.WriteLine("TestMethod klik: " + parameter.ToString());

        foreach(var item in parameter as System.Collections.IList)
        {
            if(item is Photo p)
            {
                Debug.WriteLine("path: " + p.Path);
            }
        }
    }

    #endregion

    public MainWindowViewModel()
    {
        
        if (CurMainWindowState == WindowState.Minimized)
            CurMainWindowState = WindowState.Normal;

        BuildDirParh();
        
        SwitchTglButton = SwitchToggleButton;
        SwitchThemeMode();
        ButtonRefresh();
        BuildTree();     
        
        SelectedView = SelectedViewWindow;
        if (SelectedView == String.Empty)
        {
            SelectedView = "Hello";           
        }
        SelectedViewModel = CallMethod(SelectedView);
       
        cts = new CancellationTokenSource();
        FileListLoad(SelectedTreePath, cts.Token);
    }


    #region wywołania Control

    [RelayCommand]
    private void SelectionChanged(object sel)
    {
        if((sel is string sx) &&(sx != ""))SelectedView = sx;
        //if ((sel is string) && (sel != "")) SelectedView = (sel as string);
        //Debug.WriteLine("SelectionChangedCommand: " + SelectedView);
        if (SelectedView != "Settings")
        {
            SelectedViewWindow = SelectedView;//zapis do ini

            SelectedViewModel = CallMethod(SelectedView);
        }
        else
        {
            //if (SelectedViewModel.GetType().ToString().Contains("Directories"))
            //{
            //    Debug.WriteLine("Settings already loaded: "+ SelectedViewWindow);
            //    SelectedViewModel = CallMethod(SelectedViewWindow);// tu jest jakiś bubel bo nic się nie dzieje
            //}
            //else
            SelectedViewModel = CallMethod(SelectedView);
        }
        //Debug.WriteLine("SelectionChangedCommand: " + SelectedViewModel.ToString() + ", sel: " + sel.ToString() + ", SelectedView: " + SelectedView);
        if (SelectedViewWindow == "Gallery") ReloadFileList(SelectedItem);
    }

    private object CallMethod(string p, object?[]? x = null)
    {
        Type thisType = GetType();
        if (thisType != null)
        {
            if((thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance) is MethodInfo theMethod)
                && (theMethod != null))
            //MethodInfo theMethod = thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance);
            //bez parametrów
            //if (theMethod != null)
            {
                var ret = theMethod?.Invoke(this, x);

                if (ret != null)
                {
                    return ret;
                }
            }
            // z  parametrami
            //theMethod.Invoke(this, userParameters);
        }
        return new();
    }

    private object Welcome() =>  new Welcome();
    
    private object FDataGrid() => new FileDataGrid();
    
    private object FList() => new FileList();

    private object Gallery() => new Gallery();    

    private object Settings() =>  new Directories();

    private object Gallery2() => new FileList2();
    //to też jest wyświetlanie miniatur ale nie ma ich łądowania asynchronicznie tak jak w gallery


    #endregion wywołania Control


    #region Folders and Files
    //public bool RenameFile(string oldName, string newName)
    //{
    //    string path = oldName.Substring(0, oldName.LastIndexOf('\\') + 1);
    //    //Debug.WriteLine("o: " + oldName + " ,N: " +newName);
    //    try
    //    {
    //        File.Move(oldName,newName);
    //    }catch(Exception e) {
    //        Debug.WriteLine(e.Message);
    //        return false;
    //    }
    //    return true;
    //}
    public void RenameFile(Photo photo, string newName)
    {
        newName = photo.Path.Substring(0, photo.Path.LastIndexOf('\\') + 1)+ newName;
        //bool x = RenameFile(photo.Path, newName);
        
        bool x = BrokerFile.RenameFile(photo.Path, newName);
        if (x)
        {
            photo.rename(newName);
        }
    }

    //public bool RenameFolder(string oldName, string newName)
    //{
    //    //dodać wyskakujące okno z komunikatem o będzie
    //    Debug.WriteLine("o: " + oldName + " ,N: " + newName);
    //    //try
    //    //{
    //    //    Directory.Move(oldName, newName);
    //    //}
    //    //catch (Exception e)
    //    //{
    //    //    Debug.WriteLine(e.Message);
    //    //    return false;
    //    //}
    //    return BrokerFile.RenameFilDirectory(oldName, newName);
    //}

    public void RenameFolder( TreeModel treeModel, string newName)
    {
        Debug.WriteLine("RenameFolder: " + treeModel.Path + " newName: " + newName);
        if (treeModel != null)
        {
            string newNameX = treeModel.Path.Substring(0, treeModel.Path.LastIndexOf('\\') + 1) + newName;
            Debug.WriteLine("RenameFolder new path: newNameX: " + newNameX);
            if (cts != null)
            {
                cts.Cancel();
                while (!cts.IsCancellationRequested && counter)
                {
                    //Debug.WriteLine("RenameFolder: still waiting...");
                    Thread.SpinWait(50000);
                }
            }
            if (cts == null || ( cts.IsCancellationRequested && !counter))
            {
                var xuz = treeModel.GetSelfFromParent();//treeModel.GetSelfFromMainStream();
                //Debug.WriteLine("RenameFolder: xuz: " + xuz?.Path + " , " + xuz?.Name);
                //bool x = RenameFolder(treeModel.Path, newNameX);
                bool x = BrokerFile.RenameFilDirectory(treeModel.Path, newNameX);
                if (x && xuz != null)
                {
                    xuz.Name = newName;
                    xuz.Path = newNameX;
                    xuz.Children.Clear();
                    xuz.Addchild(ScanPath(newNameX, "").Children);
                    //Debug.WriteLine("RenameFolder: campare treeModel.xyz.Path: " + xuz.Path + " , " + xuz.Name);
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
        if (File.Exists(file)){
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
    public bool DirectroyExists( TreeModel treeModel, string newName)
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
        
        string cat  = "Nowy katalog";
        var newPath = System.IO.Path.Combine(path, cat);
        int i = 1;
        while (Directory.Exists(newPath))
        {
            cat = "Nowy katalog (" + i.ToString() + ")";
            newPath = System.IO.Path.Combine(path,cat);
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
            string x = AddFolder(treeModel.Path);
            if (x != string.Empty)
            {
                //odświeżenie drzewa
                if((treeModel.GetSelfFromParent() is TreeModel xuz)&& (xuz != null))
                //TreeModel xuz = treeModel.GetSelfFromParent();//treeModel.GetSelfFromMainStream();
                //if (xuz != null)
                {
                    xuz.Children.Clear();
                    xuz.Addchild(ScanPath(xuz.Path, "").Children);
                    xuz.IsExpanded = true;
                    if((xuz.FindChild(x) is TreeModel newItem)&& (newItem != null))
                    //TreeModel newItem = xuz.FindChild(x);
                    //if (newItem != null)
                    {
                        //newItem.IsSelected = true;
                        newItem.IsExpanded = true;
                        //SelectedItem = newItem;
                        return newItem;
                    }
                    //xuz.IsSelected = true;
                }
            }

            //string y = System.IO.Path.GetDirectoryName(x);
            //Debug.WriteLine("AddFolder to tree: " + y);
            return null;
        }
        return null;
    }

    public void DeleteFolder(TreeModel treeModel)
    {
        if (treeModel != null)
        {
            bool x = DeleteFolder(treeModel.Path);
            if (x)
            {
                var xuz = treeModel.GetSelfFromParent();//treeModel.GetSelfFromMainStream();
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
            //Directory.Delete(folder, true);
            //FileSystem.DeleteDirectory(folder, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            BrokerFile.DeleteDirectory(folder);
            Debug.WriteLine("DeleteFolder: " + folder);
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
            //File.Delete(file);
            //FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            if (BrokerFile.DeleteFile(file))
            {
                if(Photos.FirstOrDefault(p => p.Path == file) is Photo x)
                {
                    _ = Photos.Remove(x);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        // i tu jest problem, powinno wywalać ten jeden plik a nie odświeżać całość
        //ReloadFileList(SelectedItem);
    }

    public bool DeleteFileStrong(string file)
    {

        return BrokerFile.DeleteFileStrong(file);
        //return false;
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

    private async Task<string> ShowDialogAddFolder( TreeModel treeModel, string newName)
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
        string newDestinyPath = Path.Combine(DestinyPath, folder.Substring(folder.LastIndexOf('\\')+1));
        //Debug.WriteLine($"{newDestinyPath}");
        if ((!File.Exists(folder)) && (Directory.Exists(folder)) && (Directory.Exists(DestinyPath))
            && (!Directory.Exists(newDestinyPath)))
        {

            try
            {
                Debug.WriteLine($"deftiny: {newDestinyPath}"+$" Source: {folder}");
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

                if ((TreeFoldr != null)&&(TreeDestinyPath != null)&& (TreeFoldr.Path != string.Empty) 
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
        MoveFileToFolder(String.Empty, String.Empty);
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
        MoveFileToFolder(file, ViewPath,copy);

    }

    /// <summary>
    /// przenoszenie pliku do innego katalogu
    /// </summary>
    /// <param name="path">katalog docelowy</param>
    /// <param name="file">plik ze ścieżką</param>
    public void MoveFileToFolder(string file,string path,bool copy = false)
    {
        if ((file == String.Empty) && (path == String.Empty) && !copy)
        {
            //czyszczenie kolekcji po przeniesieniu pliku do np explorera plików
            RefreshFileList();
            return;
        }
        //tu brakuje testowania czy plik nie jest empty itd...
        // a co jeżeli kopiujemy katalog a nie plik ??
        //zrobić na to osobną metodę ....

        if (string.IsNullOrEmpty(file) || !File.Exists(file) || !Directory.Exists(path) || !Path.HasExtension(file))
        {
            //Debug.WriteLine("brak pliku lub zł ścieżka");
            //if(string.IsNullOrEmpty(file)) Debug.WriteLine("plik jest pusty");
            //if(!File.Exists(file)) Debug.WriteLine("plik nie istnieje");
            if (!File.Exists(file) && Directory.Exists(file))
            {
                //Debug.WriteLine("'plik' może być katalogiem");//działa, zbadane ... tu można testować jak przenieść katalog..
                MoveFoderToFolder(file, path);
            }
            //if (!Directory.Exists(path)) Debug.WriteLine("katalog docelowy nie istniej");
            return;
        }
        string ext = Path.GetExtension(file).ToLower();
        //to jakoś trzeba zamienić na rozszeżenia brane z ustawień

        //string pattern = @"\.(jpg|jpeg|bmp|png|webp)";
        Match m;
        //ext = System.IO.Path.GetExtension(imFile);
        m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);


        //if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".tiff" || ext == ".webp")
        if (m.Success)
        {
             string newFilePath = FileMove(file, path, copy);

            string pathFile = System.IO.Path.GetDirectoryName(file);
            //to dlatego że Tree jest tablicą kilku drzew
            //wyszukiwanie właściwego drzewa
            foreach (var treeItem in Tree)
            {
                TreeModel? item = treeItem.GetElementByPath(path);
                if (item != null) item.CountFiles = GetCountFiles(item.Path);//dodaje liczbę plików w katalogu
                item = treeItem.GetElementByPath(pathFile);
                if (item != null) item.CountFiles = GetCountFiles(item.Path);//odejmuje liczbę plików w katalogu
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
                if(!newPhoto(newFilePath)) ReloadFileList(SelectedItem);//to jako ostateczność
            }
        }else Debug.WriteLine("MoveFileToFolder: nieobsługiwany format pliku: " + ext);
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
                if(pos == -1)
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
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <param name="copy"></param>
    /// <returns>string: zwraca nowąścieżkę pliku z nazwą lub String.Empty jak pojawią się błędy</returns>
    private string FileMove(string file, string path,bool copy = false)
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
                fileName = filenameX + "(" + i.ToString() + ")"+ext;
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
        if (parameter != null)
        {
            SelectedItem = parameter;
            //Debug.WriteLine("SelectedItemPath: " + SelectedItemPath);
            //string ModelType = SelectedViewModel.ToString();// = CallMethod(SelectedView);
            //Debug.WriteLine("LBM klik - TreeModelLBMClick, path:" + parameter.Path + ", ViewModel: " + ModelType);

            ReloadFileList(parameter);
        }
    }

    [RelayCommand]
    private void ThumbnailHeightMinus(){ ThumbnailHeight--; }

    [RelayCommand]
    private void ThumbnailHeightPlus() { ThumbnailHeight++; }

    /// <summary>
    /// ma obliczać ilość plików możliwych do wyświetlenia ale tu jest jakiś bubel, bo źle liczy
    /// </summary>
    /// <param name="path"></param>
    /// <returns>ilość plików możliwych do wyświetlenia</returns>
    private int GetCountFiles(string path)
    {
        /*files = System.IO.Directory.GetFiles(yourFolder).OrderBy(
          Function(f) DateTime.ParseExact(System.Text.RegularExpressions.Regex.Match(System.IO.Path.GetFileNameWithoutExtension(f),"\d{8}$").
          Value,"ddMMyyyy",System.Globalization.CultureInfo.InvariantCulture)).ToArray
        
        
        */
        if (Directory.Exists(path))
        {
            SelectedTreeItem = path;
            var imFiles = Directory.EnumerateFiles(path);
            //FileInfo finfo;
            string ext;//, name;
            //string pattern = @"\.(jpg|jpeg|bmp|png|webp)";
            Match m;
            int i = 0;
            foreach (var imFile in imFiles)
            {
                ext = System.IO.Path.GetExtension(imFile);
                m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                if (m.Success) i++;
            }
            return i;

        }//if
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
    /// </summary>
    /// <param name="path"></param>
    /// <param name="token"></param>
    private async void FileListLoad(string path, CancellationToken token)
    {

        FilesList.Clear();
        
        Photos.Clear();
        GC.Collect();
        //Debug.WriteLine("skanowanie z: "+path);
        if (Directory.Exists(path))
        {
            SelectedTreeItem = path;
            var imFiles = Directory.EnumerateFiles(path);
            FileInfo finfo;
            string ext, name;
            var back = new BitmapImage(new Uri(@"pack://application:,,,/img/g1.png"));
            //string pattern = @"\.(jpg|jgeg|bmp|png|webp)";
            Match m;
            string View = SelectedView.Split('.').Last();
            //FilesToLoad = imFiles.Count().ToString();
            FilesToLoad = GetCountFiles(path).ToString();
            int licznik = 0;
            foreach (var imFile in imFiles.Select(static (value, i) => (value, i)))
            {
                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("FileListLoad: Canceled");
                    counter = false;
                    return;
                    /*
                    while (!token.IsCancellationRequested)
                    {
                        Thread.SpinWait(50000);
                    }
                    */
                }
                counter = true;

                
                ext = System.IO.Path.GetExtension(imFile.value);
                //pattern to zmienne globalna, będzie ustawiana przy starcie z ini, na razie jest to string na stałe
                m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                if (m.Success && !token.IsCancellationRequested)
                {
                    //licznik++;
                    FileLoaded = (++licznik).ToString();
                    try
                    {                        
                        
                        name = System.IO.Path.GetFileName(imFile.value);
                        finfo = new FileInfo(imFile.value);
                        //Debug.WriteLine("LBM klik, path:" + path+ " file: "+name);
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
                            p.Image = back;
                            Photos.Add(p);
                            //p.AddToken(token);
                            //jak zrobić żeby to było odpalane przez interfejs? a nie tutaj
                            await p.Load(token);
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
            if(imFiles.Count() ==0) FileLoaded = "0";
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
        if (DirPath != null)
        {
            //sprawdza czy taka ścieżka nie jest już wpisana
            foreach (string p in DirPath)
            {
                if (p.Equals(folder)) return false;
            }
            
        }
        //DirPath.IndexOf(folder);
        //if (DirPath != null) DirPath.Clear();
        //if (DirPath == null) DirPath = [];
        if (TreePath == string.Empty)
        {
            TreePath = folder;            
            //DirPath.Add(folder);
        }
        else
        {
            TreePath += ";" + folder;
            //TreePath.Split(';').ToList().ForEach(path => DirPath.Add(path));
            
        }
        BuildDirParh();
        if (Tree == null) Tree = [];
        //if (_tree == null) _tree = [];
        Tree.Add(ScanPath(folder));
        return true;
    }

    private void BuildDirParh()
    {
        //odczyt z ini ścieżek do głównych katalogów
        if (TreePath != string.Empty)
        {            
            if (DirPath == null) DirPath = [];
            if (DirPath.Count > 0) DirPath.Clear();
            var ListTreePath = TreePath.Split(';').ToList();
            //.ForEach(path => DirPath.Add(path));
            foreach (var p in ListTreePath)
            {
                if (!string.IsNullOrEmpty(p))
                {
                    DirPath.Add(p);
                }
            }
            
        }
    }

    //do przebudowania, ma wyszukać co ma usunąć i to usunąć a nie resetować całe drzewo
    public void RemoveFolder(string folder)
    {
       // Debug.WriteLine(folder);
        var x = TreePath.Split(';').ToList();
        _ = x.Remove(folder);// a tu chyba jest tworzony pusty string zamiast usuwać komurkę
        TreePath = string.Join(";", x);// to chyba dodaje nam pusty string na końcu
        DirPath.Clear();
        TreePath.Split(';').ToList().ForEach(path => DirPath.Add(path));
        foreach (var reTree in Tree)
        {
            if ((reTree.Path == folder)||string.IsNullOrEmpty(reTree.Path))
            {
                _ = Tree.Remove(reTree);
                return;
            }
        }
    }
    //public void ReBuildTree() => BuildTree(); //chwilowo nie potrzebne
    private void BuildTree()
    {

        if ((Tree != null)&&(Tree.Count > 0)) Tree.Clear();
        if(Tree == null)Tree = [];
        if ((TreePath != string.Empty)&&(!string.IsNullOrWhiteSpace(TreePath)))
        {
            var listTreePath = TreePath.Split(";").ToList();
            foreach (var path in listTreePath)
            {
                if(path != string.Empty)
                {
                    Tree.Add(ScanPath(path, SelectedTreePath));
                }

            }
            //.ForEach(path => Tree.Add(ScanPath(path, SelectedTreePath)));
        }
    }


    //dodać sprawdzanie czy dodawany katalog nie jest na liście wykluczeń
    //dodać metodę scanExpand do zapisywania w pliku ini
    private TreeModel ScanPath(string path,string select = "")
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        //Debug.WriteLine("ScanPath: " + path);
        DirectoryInfo di = new(path);

        TreeModel tree = new() {Path = path,Name = di.Name,CountFiles = GetCountFiles(path) };
        if((select != null)&&(select != ""))
        {
            if (select.Contains(path))
            {
                tree.IsExpanded = true;
                if (select == path)
                {
                    tree.IsSelected = true;
                    SelectedItem = tree;
                    //Debug.WriteLine("ScanPath - SelectedItemPath: " + SelectedItemPath);
                }
            }
        }

        var files = Directory.EnumerateFiles(path);


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
        if(string.IsNullOrEmpty(PathFolderExcluded)) return false;
        var arr = PathFolderExcluded.Split(";");
        foreach (var item in arr)
        {
            if(path == item) return true;
            //if (item.Contains(path)) return true; 
            //if(path.Contains(item)) return true;
        }
        return false;
    } 
    
    private string TreePath
    {
        get => iniFile.PathFolderTree;
        set => SetProperty(iniFile.PathFolderTree, value, iniFile, static (u, n) => u.PathFolderTree = n);
    }

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
        Debug.WriteLine("ButtonRefresh: CurMainWindowState: " + CurMainWindowState.ToString());
        if (CurMainWindowState == WindowState.Normal)
        {
            RestoreButton = false;

            MaximizeButton = true;
        }else if(CurMainWindowState == WindowState.Maximized)
        {
            RestoreButton = true;
            MaximizeButton = false;

        }
    }

    [RelayCommand]
    private void Test() { 
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
