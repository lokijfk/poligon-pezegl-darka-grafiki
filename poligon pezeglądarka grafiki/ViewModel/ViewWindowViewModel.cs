using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using poligon_pezeglądarka_grafiki.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class ViewWindowViewModel: ObservableObject
{
    [ObservableProperty]
    private BitmapImage myBitmapImage = new();
    [ObservableProperty]
    private Color _Bacground_C;
    [ObservableProperty]
    private double _Opacity_C;
    //czy to jest potrzebne??
    private List<string> ImagePaths = new();

    public ObservableCollection<Photo> Photos;
    private int currentImageIndex = 0;
    public ViewWindowViewModel()
    {
        // Create a new instance of the model
       // Model = new Model.Model();
    }
    public ViewWindowViewModel( string path)
    {
        if ((path != null)&& (File.Exists(path)))
        {
            Debug.WriteLine("Load image and load files");
            LoadImage(path);
            LoadFiles(path);
        }
        Init();
    }

    public ViewWindowViewModel(int index, ref readonly ObservableCollection<Photo> Photos_X)
    {
        this.Photos = Photos_X;
       // Debug.WriteLine("load only image");
        LoadImage(Photos[index].Path);
        Init();
    }

    private void Init()
    {
        var vx = Application.Current.Resources["MaterialDesign.Brush.Background"];
        Bacground_C = (Color)ColorConverter.ConvertFromString(vx.ToString());
        Opacity_C = 0.9;
    }

    /*
    public ViewWindowViewModel(string path, ref readonly BrokerIni iniFile)
    {
        if ((path != null) && (File.Exists(path)))
        {
            LoadImage(path);
            LoadFiles(path);
        }
    }
    */

    private void LoadImage(string path)
    {
        //Debug.WriteLine("load image:" + path);
        MyBitmapImage = new();
        MyBitmapImage.BeginInit();
        MyBitmapImage.UriSource = new Uri(path);
        MyBitmapImage.EndInit();
        //OnPropertyChanged(nameof(MyBitmapImage));
    }


    //czy to nie jst duplikat z MWVM? 
    //dodaje wszystkie obsługiwane pliki(ze ścieżkami) z katalogu do listy
    // i wyszukuje index podanego pliku 
    // czy dało by się wykożystać Photos do tego ?? zamiast od początku skanować i pobierać dane bez autoryzacji
    // inaczej mają się sprawy jak to okno jest otworzone jko pierwsze a więc treba badać jak to jest...
    private void LoadFiles(string path)
    {
        
        //if ((path == null) || (!Directory.Exists(path))) return;
        var p = Path.GetDirectoryName(path);
        if ((path != null) && (Directory.Exists(p)))
        {
            //Debug.WriteLine("katalog istnieje");
            string pattern = @"\.(jpg|bmp|png)";
            var imFiles = Directory.EnumerateFiles(p);
            Match m;
            foreach (var item in imFiles.Select((value, i) => (value, i)))
            {
                string currentFile = item.value;
                string ext = System.IO.Path.GetExtension(currentFile);
                m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                if (m.Success)//jeżeli rozszeżenia jest jednym z obsługiwanych to dodaje plik do listy  plików
                {
                    ImagePaths.Add(currentFile);
                    if(currentFile == path)// jeżeli dodany plik ma taką samąścieżkęjak ten na wejściu to zapisujemy jego insex z ImagePaths
                    {
                        //currentImageIndex = item.i;
                        currentImageIndex = ImagePaths.FindIndex(x => x == path);
                        //Debug.WriteLine("index z listy:"+ currentImageIndex+" , index kolejny z for: "+ item.i);
                        // tego nie jestem pewien, zepisuje kolejn¹ liczbê a nie indeks z listy
                    }
                }
            }
        }
    }


    private Window GetWindow()
    {
        foreach (Window item in Application.Current.Windows)
        {
            if (item.DataContext == this)
            {
                return item; // to nie zamyka aplikacji z powodu że jest inaczej odpalona
            }
        }
        throw new NotImplementedException("nie odnaleziono aktualnego okna");
        //return null;// może tu przed nulem wywalić wyjątek ?? 
    }

    private bool MainIsShow()
    {
        foreach (Window item in Application.Current.Windows)
        {
            if(item.GetType() == typeof(MainWindow)) { return true; }
        }
        return false;
    }

    [RelayCommand]
    private void BTClick(object? parameter)
    {
        //Debug.WriteLine("jest ewent");
        //myBitmapImage = Model.GetImage();
        if (parameter is KeyEventArgs e)
        {
            //Debug.WriteLine("jest ewent - ewent KEY");
            if (e.Key is Key.Escape)
            {
                if (!MainIsShow())
                {
                    System.Windows.Application.Current.Shutdown();
                }else GetWindow().Close();// tu to samo jak wyjdzie null to będzie błąd
            }
            else if (e.Key is Key.Left)
            {
                if (ImagePaths != null && ImagePaths.Count > 0)
                {
                    if ((currentImageIndex <= ImagePaths.Count) && (currentImageIndex > 0))
                    {
                        currentImageIndex--;
                        LoadImage(ImagePaths[currentImageIndex]);
                        //Debug.WriteLine("left - index z listy:" + currentImageIndex + " : "+ ImagePaths[currentImageIndex]);
                    }
                }else if (Photos != null && Photos.Count > 0)
                {
                    if ((currentImageIndex <= Photos.Count) && (currentImageIndex > 0))
                    {
                        currentImageIndex--;
                        LoadImage(Photos[currentImageIndex].Path);
                        //Debug.WriteLine("left - index z listy:" + currentImageIndex + " : "+ ImagePaths[currentImageIndex]);
                    }
                }
            }
            else if (e.Key is Key.Right)
            {
                if (ImagePaths != null && ImagePaths.Count > 0)
                {
                    if (currentImageIndex + 1 < ImagePaths.Count)
                    {
                        currentImageIndex++;
                        LoadImage(ImagePaths[currentImageIndex]);
                        //Debug.WriteLine("right - index z listy:" + currentImageIndex + " : " + ImagePaths[currentImageIndex]);
                    }
                }else if(Photos != null && Photos.Count > 0)
                {
                    if (currentImageIndex + 1 < Photos.Count)
                    {
                        currentImageIndex++;
                        LoadImage(Photos[currentImageIndex].Path);
                        //Debug.WriteLine("right - index z listy:" + currentImageIndex + " : " + ImagePaths[currentImageIndex]);
                    }
                }
            }
            else if (e.Key is Key.Enter)
            {
                // otwieranie okna powinno działać o ile nie ma go otwartego
                //tu dodać DAtaContext i będzie ok
                // tu trzeba dodać jakoś przekazywanie parametrów i jak przekazać info na jakim obrazku kliknięto enter??
                // bo na ten obrazek powinno wskazywać okno
                // może uda się jako parametr do MWVM
                Window view = GetWindow();
                if (!MainIsShow())
                {
                    MainWindow main = new MainWindow { DataContext = new MainWindowViewModel() };
                    // tu będzie błąd jak wywalu null i trzeba to jakoś przechwycić
                    main.Owner = view.Owner;
                    main.Show();
                    view.Owner = main;
                }
                view.Close();

                /*
                foreach (Window item in Application.Current.Windows)
                {
                    if (item.DataContext == this)
                    {                        
                        mnw.Owner = item.Owner;
                        
                        mnw.Show();
                        item.Owner = mnw;
                        item.Close(); // to nie zamyka aplikacji z powodu że jest inaczej odpalona
                    }
                }
                */
  
            }
        }
    }

}
