

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using poligon_pezeglądarka_grafiki.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace poligon_pezeglądarka_grafiki.ViewModel;

public partial class EditWindowViewModel : ObservableObject
{
    // zmiana podejścia
    // osobno obraz do edycji i jego kopia jako obraz wyświetlany
    // ten do edycji ma być zapisywany i z niego są brane dane do zmiany rozmiaru
    public string WindowTitle => $"Edytor - {System.IO.Path.GetFileName(path)}";
    //[ObservableProperty]
    //private BitmapImage myBitmapImage = new();
    /// <summary>
    /// obraz publiczny
    /// </summary>
    [ObservableProperty]
    private ImageSource _Image = null;

    /// <summary>
    /// obraz roboczy
    /// </summary>
    private ImageSource destImage = null;
    [ObservableProperty]
    double myWidth;
    [ObservableProperty]
    double myHeight;

    [ObservableProperty]
    private bool isSaved = false;
    private string path;

    #region konstruktory

    public EditWindowViewModel(string path)
    {
        if ((path != null) && (File.Exists(path)))
        {
            Image = null;
            destImage = null;
            this.path = path;
            LoadImageX(path);
            if(System.IO.Path.GetExtension(path) == ".png")
            {
                test(path);
            }
            else
            {
                Debug.WriteLine(System.IO.Path.GetExtension(path));
            }
        }
        
    }

    private void LoadImageX(string path)
    {        
        BitmapImage MyBitmapImage = new();
        MyBitmapImage.BeginInit();
        MyBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        MyBitmapImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
        MyBitmapImage.EndInit();
        //if ((MyBitmapImage.Height <= 300) || (MyBitmapImage.Width <= 300))
        //{
        //    MyHeight = MyBitmapImage.Height;
        //}
        //else
        //{
        //    MyHeight = double.NaN;
        //}
        Image = MyBitmapImage;
        destImage = MyBitmapImage;
    }
    
    #endregion konstruktory

    #region metody

    [RelayCommand]
    private void Seve()
    {
        IsSaved=BrokerFile.Save(destImage, path);
    }

    [RelayCommand]
    private void SaveAs()
    {        
        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();        
        dlg.RestoreDirectory = true;
        dlg.InitialDirectory = System.IO.Path.GetDirectoryName(path);
        dlg.FileName = System.IO.Path.GetFileNameWithoutExtension(path); 
        dlg.DefaultExt = ".jpg"; 
        dlg.Filter = "JPG(.jpg)|*.jpg|PNG(.png)|*.png|JPEG(.jpeg)|*.jpeg"; // filtr rozszeżeń, powinien być brany z pliku ini
        dlg.CreatePrompt = true;
        
        Nullable<bool> result = dlg.ShowDialog();        
        if (result == true)
        {
            string filename = dlg.FileName;
            IsSaved=BrokerFile.Save(destImage, filename);
            path = filename;
        }
    }

    [RelayCommand]
    private void RotateLeft()
    {
        destImage = RotateImage(-90);
        Image = destImage;
    }

    [RelayCommand]
    private void RotateRight()
    {       
        destImage = RotateImage(90);
        Image = destImage;
    }

    private BitmapSource RotateImage(double angle)
    {
         return TransformImage(true, angle, false);  
    }

    private BitmapSource FlipImage(bool horizontal)
    {
        return TransformImage(false, 0, horizontal); 
    }

    private BitmapSource TransformImage(bool action,double angle, bool horizontal)
    {
        try
        {
            TransformedBitmap transformBmp = new TransformedBitmap();
            transformBmp.BeginInit();
            try
            {
                transformBmp.Source = (BitmapSource)destImage;
            }
            catch (System.InvalidCastException ex)
            {
                transformBmp.Source = (TransformedBitmap)destImage;
            }
            if (action)
            {

                transformBmp.Transform = new RotateTransform(angle);
            }
            else
            {
                transformBmp.Transform = horizontal ? new ScaleTransform(-1, 1) : new ScaleTransform(1, -1);
            }
            transformBmp.EndInit();
            return (BitmapSource)transformBmp;            
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return null;
        }
    }

    [RelayCommand]
    private void FlipHorizontal()
    {
        destImage = FlipImage(true);
        Image = destImage;
    }
    [RelayCommand]
    private void FlipVertical()
    {
        destImage = FlipImage(false);
        Image = destImage;
    }
    [RelayCommand]
    private void Crop()
    {
    }

    [RelayCommand]
    private void Resize()
    {
    }


    #endregion metody

    private void test(string path)
    {
        Stream imageStreamSource = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        BitmapSource bitmapSource = decoder.Frames[0];

        string test = decoder.CodecInfo.Author.ToString();
        string t2 = decoder.CodecInfo.Version.ToString();
        //Debug.WriteLine($"test: {test} t2: {t2}");
    }
}
