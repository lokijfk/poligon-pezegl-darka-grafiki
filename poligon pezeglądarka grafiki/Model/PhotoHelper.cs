using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace poligon_pezeglądarka_grafiki.Model;



static internal class PhotoHelper
{

    /// <summary>
    /// próba odczytania z exif komentarza
    /// przenieść do brokera?
    /// </summary>
    /// <param name="inFullPath"></param>
    /// <returns></returns>
    public static String getComment(string inFullPath)
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

    public static BitmapSource CreateEmtpyBitmapSource()
    {
        try
        {
            return BitmapImage.Create(16, 16, 96, 96, PixelFormats.Indexed1,
                        new BitmapPalette([Colors.Transparent]), new byte[32], 2);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        return null;
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

    public static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
    {
        try
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
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        return null;
    }

    
    //trzeba pomysleć co zrobić z Path skoro zrobiłem ją właściwość tylko do odczytu
    public static void Rename(Photo myImage,string newName)
    {
        myImage.Path = newName;
        myImage.Name = newName.Substring(newName.LastIndexOf("\\") + 1);
        _ = myImage.Load(new(), false);
    }
    
    /// <summary>
    /// to sie tu nie sprawdza, będzie do usinięcia z tego miejsca
    /// </summary>
    /// <param name="myImage"></param>
    /// <param name="token"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    /*
    public static async Task Load(Photo myImage,CancellationToken token = default(CancellationToken), bool x = true)
    {
        bool errored = false;
        if (token.IsCancellationRequested && x)
        {
            //Debug.WriteLine("Load cancelled");
            token.ThrowIfCancellationRequested();
            return;
        }

        if (token == default(CancellationToken) && myImage.Ctoken != default(CancellationToken))
        {
            token = myImage.Ctoken;
        }
        //this.token = token;
        try
        {
            myImage.Image = await Task.Run(() =>
            {
                using (var fileStream = new FileStream(
                    myImage.Path, FileMode.Open, FileAccess.Read))
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
                            new Uri(myImage.maska), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None));

                    }
                    catch (ArgumentException)
                    {
                        errored = true;
                        return Task.FromResult(BitmapFrame.Create(
                           new Uri(myImage.maska), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None));
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
                var UriX = new Uri(myImage.Path, UriKind.RelativeOrAbsolute);
                BitmapImage MyBitmapImage = new();
                MyBitmapImage.BeginInit();
                MyBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                MyBitmapImage.UriSource = UriX;
                MyBitmapImage.DecodePixelHeight = 200;
                MyBitmapImage.EndInit();
                //MyBitmapImage.getQuery()
                myImage.Image = MyBitmapImage;
            }
            //BitmapMetadata MyMeta = (BitmapMetadata)Image.Frames[0].Metadata;
            myImage.Image = PhotoHelper.CreateResizedImage(myImage.Image, (int)((200 / myImage.Image.Height) * myImage.Image.Width), (int)200, 0);
            //Debug.WriteLine($"komentarz: {getComment(Path)}");
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine($"zadanie wstrzymane (Task): {ex.Message}");
            //return null; // Return null if loading fails
        }
    }
    */
}
