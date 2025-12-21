

using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace poligon_pezeglądarka_grafiki.Model;

public partial class Photo : ObservableObject//,IDisposable
{
    //private readonly Uri _source;
    //private readonly Stream _streamSource;
    public string Path { get; private set; }

    [ObservableProperty]
    private string _Name;

    [ObservableProperty]
    private ImageSource image;

    [ObservableProperty]
    private bool isSelected = false;

    //public override string ToString() => _source.ToString();
    public override string ToString() => Path;
    private CancellationToken ctoken;
    //private bool isLoaded = false;
    //public ExifMetadata Metadata { get; } tu trzeba zbudować właśną klasę do odczytu metadanych
    // test jest po to żeby załadować w odpowiedniej kolejności pliki ale bez grafiki
    // żeby nie było opóźnień a samą grafikę przerobić w osobnym wątku jak się da
    // i do tego służy metoda publiczna

    // exif mogą też przechowywać orientację zdjęcia/obrazu, warto ją odczytać
    // tak samo jak i komentarz z PNG

    public string maska = string.Empty;

    public Photo(string path)
    {
        Path = path;
        //_streamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
        //_source = new Uri(path);       
        Name = System.IO.Path.GetFileName(path);
        Image = CreateEmtpyBitmapSource();
    }

    public static BitmapSource CreateEmtpyBitmapSource()
    {
        return BitmapImage.Create(16, 16, 96, 96, PixelFormats.Indexed1,
                    new BitmapPalette([Colors.Transparent]), new byte[32], 2);
    }

    /// <summary>
    /// zwraca przeroczystą bitmapę o wymarach 16x16
    /// przerobić tak żeby wymary trzeba było podać
    /// </summary>
    /// <returns></returns>
    public static BitmapImage GetBitmapImage()
    {
        // before encoding/decoding, check if bitmapSource is already a BitmapImage
        BitmapSource bitmapSource = CreateEmtpyBitmapSource();
        if (!(bitmapSource is BitmapImage bitmapImage))
        {
            bitmapImage = new BitmapImage();

            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
            }
        }

        return bitmapImage;
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
        _ = Load(new(), false);
    }

    public void AddToken(CancellationToken Ctoken)
    {
        ctoken = Ctoken;
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
    public async Task Load(CancellationToken token = default(CancellationToken), bool x = true)
    {
        bool errored = false;
        if (token.IsCancellationRequested && x)
        {
            //Debug.WriteLine("Load cancelled");
            token.ThrowIfCancellationRequested();
            return;
        }

        if (token == default(CancellationToken) && ctoken != default(CancellationToken))
        {
            token = this.ctoken;
        }
        //this.token = token;
        try
        {
            Image = await Task.Run(() =>
            {
                using (var fileStream = new FileStream(
                    Path, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        return Task.FromResult(BitmapFrame.Create(
                            fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad));
                    }
                    catch (FileFormatException)
                    {
                        errored = true;
                        return Task.FromResult(BitmapFrame.Create(
                            new Uri(maska), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None));

                    }
                    catch (ArgumentException)
                    {
                        errored = true;
                        return Task.FromResult(BitmapFrame.Create(
                           new Uri(maska), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        return null;
                    }
                    
                }
            });

            if (errored)
            {                
                var UriX = new Uri(Path, UriKind.RelativeOrAbsolute);
                BitmapImage MyBitmapImage = new();
                MyBitmapImage.BeginInit();
                MyBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                MyBitmapImage.UriSource = UriX;
                MyBitmapImage.DecodePixelHeight = 200;                
                MyBitmapImage.EndInit();  
                //MyBitmapImage.getQuery()
                Image = MyBitmapImage;
            }
            //BitmapMetadata MyMeta = (BitmapMetadata)Image.Frames[0].Metadata;
            Image = CreateResizedImage(Image, (int)((200 / Image.Height) * Image.Width), (int)200, 0);
            //Debug.WriteLine($"komentarz: {getComment(Path)}");
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine($"zadanie wstrzymane (Task): {ex.Message}");
            //return null; // Return null if loading fails
        }
    }

    private String getComment(string inFullPath)
    {
        //DateTime returnDateTime = DateTime.MinValue;
        string Comment = string.Empty;
        try
        {
            FileStream picStream = new FileStream(inFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            BitmapSource bitSource = BitmapFrame.Create(picStream);
            picStream.Close();
            BitmapMetadata metaData = (BitmapMetadata)bitSource.Metadata;
            //returnDateTime = DateTime.Parse(metaData.DateTaken);
            Comment = metaData.Comment;
        }
        catch
        {
            //do nothing  
        }
        return Comment;
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
