

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View;
using poligon_pezeglądarka_grafiki.View.Control;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;



namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    #region Properties and [observableProperty]
    #region Collection
    private BrokerIni iniFile = new BrokerIni();
    private ImageSource BlinkIcom { get; set; } = Tools.CreateEmtpyBitmapSource();

    //private ObservableCollection<TreeModel> _tree = [];
    public ObservableCollection<TreeModel>? Tree { get; set; } = [];

    //to mże usuną i przepisać wszystko na Ptors
    public ObservableCollection<FilesIO> FilesList { get; set; } = [];
    public ObservableCollection<string> DirPath { get; private set; } = [];
    public ObservableCollection<Photo> Photos { get; set; } = [];
    //private readonly object _collectionOfObjectsSync = new object();  

    //public List<string> Tryb { get; set; } = ["Hello", "FDataGrid", "FList","Gallery", "SettingdFolder"];
    #endregion Collection

    [ObservableProperty]
    private string _SelectedTreeItem = string.Empty;

    private TreeModel selectedItem;
    public TreeModel SelectedItem
    {
        get => selectedItem;
        set
        {
            //selectedItem = value;
            if (SetProperty(ref selectedItem, value)) 
            {
                TreeModelLBMClick(value);
            }
            
        }
    }    

    [ObservableProperty]    
    private string _SelectedView = string.Empty;

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
        set => SetProperty(iniFile.VisibleToolBar, value, iniFile, (u, n) => u.VisibleToolBar = n);
    }

    public bool VisibleStatusBar
    {
        get => iniFile.VisibleStatusBar;
        set => SetProperty(iniFile.VisibleStatusBar, value, iniFile, (u, n) => u.VisibleStatusBar = n);
    }

    public bool VisibleFilesInTree
    {
        get => iniFile.VisibleFilesInTree;
        set => SetProperty(iniFile.VisibleFilesInTree, value, iniFile, (u, n) => u.VisibleFilesInTree = n);
    }
    /*
    public bool OnlyFoldersWithFiles
    {
        get => iniFile.OnlyFoldersWithFiles;
        set => SetProperty(iniFile.OnlyFoldersWithFiles, value, iniFile, (u, n) => u.OnlyFoldersWithFiles = n);
    }
    */
    #endregion Interface

    [ObservableProperty]
    private double _MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;        

    [ObservableProperty]
    private  string _FilesToLoad = string.Empty;

    [ObservableProperty]
    private  string _FileLoaded = string.Empty;

    #region Private

    private bool counter = false;
    private CancellationTokenSource? cts = null;

    #endregion Private
    #endregion Properties and [observableProperty]

    public MainWindowViewModel()
    {
        
        if (CurMainWindowState == WindowState.Minimized)
            CurMainWindowState = WindowState.Normal;
        //Tree.CollectionChanged += CollectionChangedMethod;
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

 
    //hmmm nie wiem czy to jest potrzebne, przecież jest TreeModelLBMClickCommand
    public void SetSelectedItem(object parameter)
    {
        
        if ((parameter is TreeModel) && (parameter != null))
        {

            ReloadFileList(parameter as TreeModel);
        }
        BuildTree();
    }

    [RelayCommand]
    private void DataGridLDoubleClick(object parameter)
    {
        Debug.WriteLine("LBM klik: "+parameter.ToString());
    }

    #region wywołania Control
    private object CallMethod(string p, object?[]? x = null)
    {
        Type thisType = GetType();
        if (thisType != null)
        {
            MethodInfo theMethod = thisType.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Instance);
            //bez parametrów
            if (theMethod != null)
            {
                var ret = theMethod?.Invoke(this, x);
                if (ret != null) return ret;
            }
            // z  parametrami
            //theMethod.Invoke(this, userParameters);
        }
        return new();
    }

    private object Hello() =>  new Welcome();
    
    private object FDataGrid() => new FileDataGrid();
    
    private object FList() => new FileList();

    private object Gallery() => new Gallery();
    

    private object SettingdFolder() =>  new Directories();

    #endregion wywołania Control


    #region Folders and Files
    public bool RenameFile(string oldName, string newName)
    {
        string path = oldName.Substring(0, oldName.LastIndexOf('\\') + 1);
        //Debug.WriteLine("o: " + oldName + " ,N: " +newName);
        try
        {
            File.Move(oldName,newName);
        }catch(Exception e) {
            Debug.WriteLine(e.Message);
            return false;
        }
        return true;
    }
    public void RenameFile(Photo photo, string newName)
    {
        newName = photo.Path.Substring(0, photo.Path.LastIndexOf('\\') + 1)+ newName;
        bool x = RenameFile(photo.Path, newName);
        if (x)
        {
            photo.rename(newName);
        }
    }



    public bool RenameFolder(string oldName, string newName)
    {
        //dodać wyskakujące okno z komunikatem o będzie
        Debug.WriteLine("o: " + oldName + " ,N: " + newName);
        try
        {
            Directory.Move(oldName, newName);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }
        return true;
    }

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
                //cts.Dispose();
                //cts = null;
               //cts = new CancellationTokenSource();
            }
            //Debug.Assert(cts.IsCancellationRequested == true, "cts.IsCancellationRequested == true, cts is not null");
            //Debug.Assert(counter == true, "counter == true, cts is not null");
            if (cts == null || ( cts.IsCancellationRequested && !counter))
            {
                var xuz = treeModel.GetSelfFromParent();//treeModel.GetSelfFromMainStream();
                //Debug.WriteLine("RenameFolder: xuz: " + xuz?.Path + " , " + xuz?.Name);
                bool x = RenameFolder(treeModel.Path, newNameX);
                if (x)
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
                Debug.WriteLine("RenameFolder: Canceled");
                /*string path = treeModel.Path;
                treeModel = null;
                treeModel = ScanPath(path, "");
                Debug.WriteLine("Model.Path: " + treeModel.Path+ " , "+treeModel.Name);
                */
                //Tree = null;
                //BuildTree();

                //Debug.WriteLine("tree model.path: " + 
                //Tree.Select((value, i) => (value, i).value.Path == path).First().ToString();
            }
            //CollectionViewSource.GetDefaultView(Tree).Refresh();
            //var parent = treeModel.Parent;
            /*if (parent != null)
            {
                treeModel = ScanPath(parent.Path, treeModel.Path);
                ReloadFileList(treeModel);

            }*/
        }
        //else Debug.WriteLine("RenameFolder: treeModel is null, cannot rename folder.");
    }

    [RelayCommand]
    private void AddFolderToTree(object param)
    {
        Debug.WriteLine("dodaj katalog active, obiect type: "+param.GetType().ToString());
    }

    #endregion Folders and Files

    #region Tree and View

    [RelayCommand]
    private void TreeModelLBMClick(TreeModel parameter)
    {
        if (parameter != null) ReloadFileList(parameter as TreeModel);
    }

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
            FileInfo finfo;
            string ext, name;
            string pattern = @"\.(jpg|bmp|png|webp)";
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
            cts.Dispose();
            cts = null;
        }
        //FilesList.Clear();
        //Debug.WriteLine("LBM klik, path:" );
        //MessageBox.Show("kliknięto: ");

        //działa tu zrobić wybór widoku dla klikniętego elementu
        //tu można dodać pole lokalne LActiveTreeModelItem
        //MessageBox.Show("kliknięto: " );
        try
        {

            if (treeModel != null)
            {
                string path = treeModel.Path;
                // Debug.WriteLine("LBM klik, path:"+path);
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
    private async void FileListLoad(string path, CancellationToken token)
    {
        FilesList.Clear();

        Photos.Clear();
        //Debug.WriteLine("skanowanie z: "+path);
        if (Directory.Exists(path))
        {
            SelectedTreeItem = path;
            var imFiles = Directory.EnumerateFiles(path);
            FileInfo finfo;
            string ext, name;
            string pattern = @"\.(jpg|bmp|png|webp)";
            Match m;
            string View = SelectedView.Split('.').Last();
            FilesToLoad = imFiles.Count().ToString();
            foreach (var imFile in imFiles.Select((value, i) => (value, i)))
            {
                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine("FileListLoad: Canceled");
                    counter = false;
                    return;
                }
                counter = true;
                FileLoaded = (imFile.i + 1).ToString();
                ext = System.IO.Path.GetExtension(imFile.value);
                m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                if (m.Success && !token.IsCancellationRequested)
                {
                    name = System.IO.Path.GetFileName(imFile.value);
                    finfo = new FileInfo(imFile.value);
                    //Debug.WriteLine("LBM klik, path:" + path+ " file: "+name);
                    //to też ładować tylko w razie potrzeby!!, dodać warónek i sprawdzanie
                    FilesList.Add(new FilesIO()
                    {
                        Name = name,
                        Extension = ext,
                        Path = path,
                        Icon = BlinkIcom,
                        Size = Tools.Prdouble(finfo.Length),
                        RealSize = finfo.Length.ToString()
                    });
                    //to jakoś zmienić, dać jakiś parametr bool zamiast uzależniać to od ładowanego widoku
                    if ((View.ToLower().Contains("gallery") || View.Contains("FileList2"))&&!token.IsCancellationRequested)
                    {
                        //Debug.WriteLine("load file: " + imFile.value);
                        //var p = new Photo(imFile.value, name, false);
                        var p = new Photo(imFile.value);
                        Photos.Add(p);
                        await p.Load(token);
                    }

                }
                counter = false;
            }


        }
    }



    //do przebudowania, ma dodawać nowe foldery na końcu drzewa
    public bool AddFolder(string folder)
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
        x.Remove(folder);// a tu chyba jest tworzony pusty string zamiast usuwać komurkę
        TreePath = string.Join(";", x);// to chyba dodaje nam pusty string na końcu
        DirPath.Clear();
        TreePath.Split(';').ToList().ForEach(path => DirPath.Add(path));
        foreach (var reTree in Tree)
        {
            if ((reTree.Path == folder)||string.IsNullOrEmpty(reTree.Path))
            {
                Tree.Remove(reTree);
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
                    selectedItem = tree;
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
                    if (Tools.AtrDir(imDir))
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
        set => SetProperty(iniFile.PathFolderTree, value, iniFile, (u, n) => u.PathFolderTree = n);
    }

    private string SelectedTreePath
    {
        get => iniFile.SelectedPathFolderTree;
        set => SetProperty(iniFile.SelectedPathFolderTree, value, iniFile, (u, n) => u.SelectedPathFolderTree = n);
    }

    private string PathFolderExcluded
    {
        get => iniFile.PathFolderExcluded;
        set => SetProperty(iniFile.PathFolderExcluded, value, iniFile, (u, n) => u.PathFolderExcluded = n);
    }

    private string SelectedViewWindow
    {
        get => iniFile.SelectedView;
        set => SetProperty(iniFile.SelectedView, value, iniFile, (u, n) => u.SelectedView = n);
    }

    [RelayCommand]
    private void SelectionChanged(object sel)
    {
        if ((sel is string) &&(sel != ""))SelectedView = (sel as string);
        //Debug.WriteLine("SelectionChangedCommand: " + SelectedView);
        SelectedViewWindow = SelectedView;//zapis do ini
        
        SelectedViewModel = CallMethod(SelectedView);
        if (SelectedViewWindow == "Gallery") ReloadFileList(selectedItem);
    }


    //to może być zbędne jeżeli zostanie obsłuzone  w CB interfejsu, w końcu nie dojdzie do zmiany danych
    // ale czy tylko o to chodzi?
    // w kazdym bądź razie nie będzie tu wywoływane inne okno
    [RelayCommand]
    private void MouseDoubleClick(object parameter)
    {
        Debug.WriteLine(" klik: " + parameter);
        int index = Convert.ToInt32(parameter);
        Debug.WriteLine("name element:" + Photos[index].Name);
        ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(Photos[index].Path) };
        viewWindow.Show();
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
        set => SetProperty(iniFile.SwitchToggleButton, value, iniFile, (u, n) => u.SwitchToggleButton = n);
            //Debug.WriteLine("SwitchToggleButton zmiana na : "+value);
    }



    private Color PrimaryColor
    {
        get => iniFile.PrimaryColor;
        set => SetProperty(iniFile.PrimaryColor, value, iniFile, (u, n) => u.PrimaryColor = n);
    }

    private Color SecondaryColor
    {
        get => iniFile.SecondaryColor;
        set => SetProperty(iniFile.SecondaryColor, value, iniFile, (u, n) => u.SecondaryColor = n);
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
        if(CurMainWindowState == WindowState.Normal)
        {
            RestoreButton = false;

            MaximizeButton = true;
        }else if(CurMainWindowState == WindowState.Maximized)
        {
            RestoreButton = true;
            MaximizeButton = false;

        }
    }
    [ObservableProperty]
    private string _ContentMax = string.Empty;

    private double LastWidth
    {
        get => iniFile.LastWidth;
        set => SetProperty(iniFile.LastWidth, value, iniFile, (u, n) => u.LastWidth = n);
    }
    private double LastHeihgt
    {
        get => iniFile.LastHeihgt;
        set => SetProperty(iniFile.LastHeihgt, value, iniFile, (u, n) => u.LastHeihgt = n);
    }
    private double LastTop
    {
        get => iniFile.LastTop;
        set => SetProperty(iniFile.LastTop, value, iniFile, (u, n) => u.LastTop = n);
    }
    private double LastLeft
    {
        get => iniFile.LastLeft;
        set => SetProperty(iniFile.LastLeft, value, iniFile, (u, n) => u.LastLeft = n);
    }



    public WindowState CurMainWindowState
    {
        get => iniFile.CurMainWindowState;
        set => SetProperty(iniFile.CurMainWindowState, value, iniFile, (u, n) => u.CurMainWindowState = n);
    }

    public string CurMainWindowStateString
    {
        get => iniFile.CurMainWindowState.ToString();
        set => SetProperty(iniFile.CurMainWindowStateString, value, iniFile, (u, n) => u.CurMainWindowStateString = n);
    }

    public double Width
    {
        get => iniFile.WindowWidth;
        set => SetProperty(iniFile.WindowWidth, value, iniFile, (u, n) => u.WindowWidth = n);
    }

    public double Height
    {
        get => iniFile.WindowHeight;
        set => SetProperty(iniFile.WindowHeight, value, iniFile, (u, n) => u.WindowHeight = n);
    }

    public double Top
    {
        get => iniFile.WindowTop;
        set => SetProperty(iniFile.WindowTop, value, iniFile, (u, n) => u.WindowTop = n);
    }

    public double Left
    {
        get => iniFile.WindowLeft;
        set => SetProperty(iniFile.WindowLeft, value, iniFile, (u, n) => u.WindowLeft = n);
    }

    #endregion WindowState
}
