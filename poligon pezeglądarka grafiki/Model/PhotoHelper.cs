using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace poligon_pezeglądarka_grafiki.Model;

static internal class PhotoHelper
{
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

}
