

using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace poligon_pezeglądarka_grafiki.Model;

/// <summary>
/// to objekt teoretyczny, w trakcie ... jeszcze nie wykożystywany
/// stanowi połączenie Photo i FilesIO
/// </summary>
public partial class FileCom : ObservableObject
{

    public string Path { get; set; } = string.Empty;
    //public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public ImageSource Icon { get; set; }    
    public string Size { get; set; } = string.Empty;
    public string RealSize { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    //dodać rozmiar i czas tworzenia
    //zobaczyć co z FileInfo można zaimportować
    //jak by się dało z fileinfo wyeksportować do listy lub tablicy a tu zaimportować to
    //lista by była chyba lepsza
    
    [ObservableProperty]
    private string _Name;

    /// <summary>
    /// ściezka do pliku zastępczego
    /// </summary>
    public string maska = string.Empty;
    [ObservableProperty]
    private ImageSource image;
    //dodać typ odpowiadający za plik filmu o ile taki jest

    [ObservableProperty]
    private bool isSelected = false;
        
    public override string ToString() => Path;
    private CancellationToken ctoken;

    public void Clear()
    {
        //Select  = false;
        Path = string.Empty;
        Name = string.Empty;
        Extension = string.Empty;
        Icon = null;
        Size = string.Empty;
        RealSize = string.Empty;
        File = string.Empty;
        //dodać resztę właściwości do wyczyszczenia
    }


    public FileCom(string path)
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
                    catch (FileFormatException ex)
                    {
                        errored = true;
                        return Task.FromResult(BitmapFrame.Create(
                            new Uri(maska), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None));

                    }
                    catch (Exception ex)
                    {
                        errored = true;
                        return Task.FromResult(BitmapFrame.Create(
                           new Uri(maska), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None));
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
                Image = MyBitmapImage;
            }
            Image = CreateResizedImage(Image, (int)((200 / Image.Height) * Image.Width), (int)200, 0);

        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine($"zadanie wstrzymane (Task): {ex.Message}");
            //return null; // Return null if loading fails
        }
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
