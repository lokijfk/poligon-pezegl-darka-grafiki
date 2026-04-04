using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using poligon_pezeglądarka_grafiki.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class ViewWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private BitmapImage myBitmapImage = new();
    [ObservableProperty]
    double myWidth;
    [ObservableProperty]
    double myHeight;
    //private ImageSource myBitmapImage;// = new();
    [ObservableProperty]
    private Color _Bacground_C;
    [ObservableProperty]
    private double _Opacity_C;
    //czy to jest potrzebne??
    private List<string> ImagePaths = [];

    public ObservableCollection<Photo> Photos;

    public int currentImageIndex = 0;
    public ViewWindowViewModel()
    {
        // Create a new instance of the model
        // Model = new Model.Model();
    }
    public ViewWindowViewModel(string path)
    {
        if ((path != null) && (File.Exists(path)))
        {
            //Debug.WriteLine("Load image and load files");
            LoadImage(path);
            LoadFiles(path);
        }
        Init();        
    }



    public ViewWindowViewModel(int index, ref readonly ObservableCollection<Photo> Photos_X)
    {
        this.Photos = Photos_X;
        // Debug.WriteLine("load only image");
        currentImageIndex = index;
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
        /*
        MyBitmapImage = new();
        MyBitmapImage.BeginInit();
        MyBitmapImage.UriSource = new Uri(path);
        MyBitmapImage.EndInit();
        */

        //Uri src = new Uri(path, UriKind.RelativeOrAbsolute);
        //dodać wysokość i szerokosć obrazka o ile jest to zaznaczone w ini

        
        MyBitmapImage = new();
        MyBitmapImage.BeginInit();
        MyBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        MyBitmapImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute); //src;

        MyBitmapImage.EndInit();
        //myHeight = MyBitmapImage.Height.ToString();
        //myWidth = MyBitmapImage.Width.ToString();
        if ((MyBitmapImage.Height <= 300) || (MyBitmapImage.Width <= 300))
        {
            MyHeight = MyBitmapImage.Height;
            //MyWidth = MyBitmapImage.Width;
        }
        else
        {
            MyHeight = double.NaN;
            //MyWidth = double.NaN;
        }


        // Debug.WriteLine("rozmiar obrazka: " + myWidth + " x " + myHeight +", orginalny: "+MyBitmapImage.Width +" x "+MyBitmapImage.Height);
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
            //string pattern = @"\.(jpg|bmp|png)";
            string[] patternArray = [".jpg", ".jpeg",".bmp",".png",".webp"];
            //tu wprowadzić poprawki zgodnie ze zmianami z VM
            //List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(new FileInfo(f).Extension.ToLower()))];
            List<string> files = [.. Directory.GetFiles(p).Where(f => patternArray.Contains(System.IO.Path.GetExtension(f).ToLower()))];
            //var imFiles = Directory.EnumerateFiles(p);
            //Match m;
            //foreach (var item in imFiles.Select((value, i) => (value, i)))
            ImagePaths.AddRange(files);
            currentImageIndex = ImagePaths.FindIndex(x => x == path);
            /*
            foreach (var item in files)
            {
                string currentFile = item;
               // string ext = System.IO.Path.GetExtension(currentFile);
                //m = Regex.Match(ext, pattern, RegexOptions.IgnoreCase);
                //if (m.Success)//jeżeli rozszeżenia jest jednym z obsługiwanych to dodaje plik do listy  plików
                //{
                    ImagePaths.Add(currentFile);
                    if (currentFile == path)// jeżeli dodany plik ma taką samąścieżkęjak ten na wejściu to zapisujemy jego insex z ImagePaths
                    {
                        //currentImageIndex = item.i;
                        currentImageIndex = ImagePaths.FindIndex(x => x == path);
                        //Debug.WriteLine("index z listy:"+ currentImageIndex+" , index kolejny z for: "+ item.i);
                        // tego nie jestem pewien, zepisuje kolejn¹ liczbê a nie indeks z listy
                    }
                //}
            }
            */
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
            if (item.GetType() == typeof(MainWindow)) { return true; }
        }
        return false;
    }

    [RelayCommand]
    private void BTClick(object? parameter)
    {
        /*NOTATKI
         * dodać wyświetlanie w orginalnym rozmiarze
         * dodać zoom in/out    
         * dodać przyciski do nawigacji - prawo, lewo i zamykanie
         * 
         * 
         */
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
                }
                else GetWindow().Close();// tu to samo jak wyjdzie null to będzie błąd
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
                }
                else if (Photos != null && Photos.Count > 0)
                {
                    if ((currentImageIndex <= Photos.Count) && (currentImageIndex > 0))
                    {
                        currentImageIndex--;
                        LoadImage(Photos[currentImageIndex].Path);
                        //Debug.WriteLine("left - index z listy:" + currentImageIndex + " : "+ ImagePaths[currentImageIndex]);
                    }
                }
                //Debug.WriteLine("curentImageIndex: " + currentImageIndex);
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
                }
                else if (Photos != null && Photos.Count > 0)
                {
                    if (currentImageIndex + 1 < Photos.Count)
                    {
                        currentImageIndex++;
                        LoadImage(Photos[currentImageIndex].Path);
                        //Debug.WriteLine("right - index z listy:" + currentImageIndex + " : " + ImagePaths[currentImageIndex]);
                    }
                }
                //Debug.WriteLine("curentImageIndex: " + currentImageIndex);
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
                    try
                    {
                        MainWindow main = new MainWindow { DataContext = new MainWindowViewModel(ImagePaths[currentImageIndex]) };
                        // tu będzie błąd jak wywalu null i trzeba to jakoś przechwycić
                        main.Owner = view.Owner;
                        main.Show();
                        view.Owner = main;
                    }
                    catch
                    {
                        view.Close();
                    }
                }
                view.Close();// jak przekazać index obrazu który jest wyświetlany w tym oknie?

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
            else if (e.Key is Key.Delete)
            {
                //Debug.WriteLine("usuwanie pliku");
                // dodać obsługę przenoszeniao do kosza
                string FileToDele = Photos[currentImageIndex].Path;
                // to trzeba dodać do VM, lub zrobić klasę typu helper? która to przechowa
                if (File.Exists(FileToDele))
                {
                    try
                    {
                        //to blokuje wątek przeglądania i blokuje program !!!
                        //wyskoczenia dodatkowego okna powoduje że to okno przestaje być aktywne 
                        _ = BrokerFile.DeleteFile(FileToDele);
                        //FileSystem.DeleteFile(FileToDele, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        //Debug.WriteLine("plik usunięty: " + FileToDele);
                        Photos.RemoveAt(currentImageIndex);
                        if (Photos.Count == 0)
                        {
                            GetWindow().Close();//?
                        }
                        else if (currentImageIndex < Photos.Count)
                        {
                            //currentImageIndex++;//Photos.Count - 1;
                            LoadImage(Photos[currentImageIndex].Path);
                        }
                        /*
                        else
                        {
                            LoadImage(Photos[currentImageIndex].Path);
                        }//*/
                    }
                    catch (Exception ex)
                    {
                        //Debug.WriteLine("Błąd przy usuwaniu pliku: " + ex.Message);
                        _ = GetWindow().Activate();// aktywuje okno, żeby można było zamknąć komunikat
                    }
                }

            }
        }
    }

}
