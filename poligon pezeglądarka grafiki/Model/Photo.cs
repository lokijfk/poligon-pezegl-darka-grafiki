

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace poligon_pezeglądarka_grafiki.Model;

public partial class Photo : ObservableObject
{
    /*jak by to działało jak bym zrobił do tego helpera?
     * - przeniesć tam wszystkie metody
     * zintegrować Ohoto Z FilesIO
     * - uzupełnić o brakujące pola
     * -uzupełnić helper o brakujące metody
     */
    public string Path { get; set; }

    [ObservableProperty]
    private string _Name;// powinno być bez rozszeżenia skoro osobno jest Extension ??

    [ObservableProperty]
    private ImageSource image;

    [ObservableProperty]
    private bool isSelected = false;
    //from filesIO
    public string Extension { get; set; } = string.Empty;
    public ImageSource Icon { get; set; }
    public long Size { get; set; }//tu dać normalną wartość
    //public long RealSize { get; set; }// a tu zostawiamy long do obliczeń
    public CancellationToken Ctoken { get; set; }

    public DateTime DateModified { get; set; }
    public override string ToString() => Path;
        
    //public ExifMetadata Metadata { get; } tu trzeba zbudować właśną klasę do odczytu metadanych
    // test jest po to żeby załadować w odpowiedniej kolejności pliki ale bez grafiki
    // żeby nie było opóźnień a samą grafikę przerobić w osobnym wątku jak się da
    // i do tego służy metoda publiczna

    // exif mogą też przechowywać orientację zdjęcia/obrazu, warto ją odczytać
    // tak samo jak i komentarz z PNG (png trzeba samemu rozkodować)

    public string maska = string.Empty;

    public Photo(string path)
    {
        Path = path;
        //_streamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
        //_source = new Uri(path);       
        Name = System.IO.Path.GetFileName(path);
        Image = PhotoHelper.CreateEmtpyBitmapSource();
        Extension = System.IO.Path.GetExtension(path);
    }
    
    /// <summary>
    /// asynchroniczne łądowanie obrazów do postaci miniatury
    /// w formacie BitmapFrame do zmiennej Image
    /// jak na razie tylko tutaj działa poprawnie
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

        if (token == default(CancellationToken) && Ctoken != default(CancellationToken))
        {
            token = this.Ctoken;
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
            Image = PhotoHelper.CreateResizedImage(Image, (int)((200 / Image.Height) * Image.Width), (int)200, 0);
            //Debug.WriteLine($"komentarz: {getComment(Path)}");
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine($"zadanie wstrzymane (Task): {ex.Message}");
            //return null; // Return null if loading fails
        }
    }   
}
