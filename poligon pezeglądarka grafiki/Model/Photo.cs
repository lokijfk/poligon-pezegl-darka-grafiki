

using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
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
    
    public Photo(string path)
    {        
        Path = path;        
        _source = new Uri(path);       
        Name = System.IO.Path.GetFileName(path);
        
    }

    /*
    public Photo(string path,string name,bool test = true)
    {
        Path = path;
        _source = new Uri(path);
        Name = name;
        if (test) Image = ShortPiec(path, 200, _source);
        else Image = Tools.GetBitmapImage();
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

    private BitmapImage ShortPiec(string path, int height)
    {
        //Image xim = new();
        BitmapImage myBitmapImage = new();
        //myBitmapImage = new();
        myBitmapImage.BeginInit();

        myBitmapImage = Image.Clone() as BitmapImage;
        
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
     */

    /// <summary>
    /// to będzie chyba do usunięcia, urzyteczność znikoma
    /// </summary>
    /// <param name="newName"></param>
    public void rename(string newName)
    {
        Path = newName;
        Name = newName.Substring(newName.LastIndexOf("\\") + 1);
        _ = Load(new(),false);
    }



    /// <summary>
    /// asynchroniczne łądowanie obrazów do postaci miniatury
    /// w formacie BitmapFrame do zmiennej Image
    /// </summary>
    /// <param name="token">tokem do anulowania ładowania 
    /// urzywany przy łądowaniu większej ilości obrazów</param>
    /// <param name="x">umożliwia pominięcie sprawdzania tokena, 
    /// jednak nalezy go utworzyć</param>
    /// <returns></returns>
    public async Task Load(CancellationToken token, bool x = true)
    {
        if (token.IsCancellationRequested && x)
        {
            //Debug.WriteLine("Load cancelled");
            return;
        }


            Image = await Task.Run(() =>
            {
                using (var fileStream = new FileStream(
                    Path, FileMode.Open, FileAccess.Read))
                {
                    //Ładuje obrazy w normalnej wielkości ??
                    // jak to zmienić na miniatury ?? żeby nie zawalało to pamięci
                    try
                    {
                        return Task.FromResult(BitmapFrame.Create(
                            fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error creating BitmapFrame: {ex.Message}");
                        return null; // Return null if loading fails
                    }
                }
            });
            //NewWidth= (NewHwight/OldHeight) * OldWidth;
            Image = CreateResizedImage(Image, (int)((200 / Image.Height) * Image.Width), (int)200, 0);
            //ImageWidth = Image.Width;

    }


    private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
    {
        var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

        var group = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
        group.Children.Add(new ImageDrawing(source, rect));

        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
            drawingContext.DrawDrawing(group);

        var resizedImage = new RenderTargetBitmap(
            width, height,         // Resized dimensions
            96, 96,                // Default DPI values
            PixelFormats.Default); // Default pixel format
        resizedImage.Render(drawingVisual);

        return BitmapFrame.Create(resizedImage);
    }


}
