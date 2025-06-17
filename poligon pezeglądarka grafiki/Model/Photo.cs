

using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace poligon_pezeglądarka_grafiki.Model;

public partial class Photo : ObservableObject
{
    private readonly Uri _source;
    public string Path { get; private set; }

    [ObservableProperty]
    private string _Name;
    
    [ObservableProperty]
    private ImageSource image;
    
    public override string ToString() => _source.ToString();

    //public ExifMetadata Metadata { get; } tu trzeba zbudować właśną klasę do odczytu metadanych
    // test jest po to żeby załadować w odpowiedniej kolejności pliki ale bez grafiki
    // żeby nie było opóźnień a samą grafikę przerobić w osobnym wątku jak się da
    // i do tego służy metoda publiczna
    public Photo(string path,string name,bool test = true)
    {
        //Debug.WriteLine("create Photo");
        Path = path;
        //Debug.WriteLine("create Photo-> uri");
        _source = new Uri(path);
        Name = name;
        //Debug.WriteLine("create Photo-> image");
        if (test) Image = ShortPiec(path, 200, _source);
        else Image = Tools.GetBitmapImage();
        //Image = BitmapFrame.Create(_source);
        //Metadata = new ExifMetadata(_source);
        // Debug.WriteLine(ToString());
    }

    public Photo(string path)
    {        
        Path = path;        
        _source = new Uri(path);       
        Name = System.IO.Path.GetFileName(path);
        
    }
    
    
    public void rename(string newName)
    {
        Path = newName;
        Name = newName.Substring(newName.LastIndexOf("\\")+1);
        _ = Load();
    }

    private BitmapImage ShortPiec(string path, int height, Uri uri = null)
    {
        //Image xim = new();
        BitmapImage myBitmapImage = new();
        //myBitmapImage = new();
        myBitmapImage.BeginInit();
        if (uri == null)
        {
            myBitmapImage.UriSource = new Uri(path);
        }
        else
        {
            myBitmapImage.UriSource = uri;
        }
            myBitmapImage.DecodePixelHeight = (int)height;
        myBitmapImage.EndInit();
        //xim.Source = myBitmapImage;
        return myBitmapImage;
    }

    public void CreateShortPic()
    {
        Image = ShortPiec(Path, 200, _source);
    }

    public async Task Load()
    {
        Image = await Task.Run(() =>
        {
            using (var fileStream = new FileStream(
                Path, FileMode.Open, FileAccess.Read))
            {
                return BitmapFrame.Create(
                    fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        });        
    }

    public async Task Load(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            //Debug.WriteLine("Load cancelled");
            return;
        }
        Image = await Task.Run(() =>
        {
            using (var fileStream = new FileStream(
                Path, FileMode.Open, FileAccess.Read))
            {
                return BitmapFrame.Create(
                    fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        });
    }
}
