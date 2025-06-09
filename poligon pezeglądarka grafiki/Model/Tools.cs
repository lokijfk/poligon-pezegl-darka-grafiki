using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;


namespace poligon_pezeglądarka_grafiki.Model;

// zmienić na PathHelper
static internal class Tools
{
    //private static ModelPB model = null;
    public static string GetUserAppDataPath =>
        
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" +
            GetProjectName;

    public static string GetProjectName        
        => Assembly.GetExecutingAssembly().GetName().Name.ToString();

    /// <summary>
    /// zwraca przeroczystą bitmapę o wymarach 16x16
    /// przerobić tak żeby wymarytrzeba było podać
    /// </summary>
    /// <returns></returns>
    public static BitmapSource CreateEmtpyBitmapSource()
    {
        /*
        int width = 16;
        int height = width;
        int stride = width / 8;
        byte[] pixels = new byte[height * stride];

        // Try creating a new image with a custom palette.
        //List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
        //colors.Add(System.Windows.Media.Colors.Red);
        //colors.Add(System.Windows.Media.Colors.Blue);
        //colors.Add(System.Windows.Media.Colors.Green);
        //BitmapPalette myPalette = new BitmapPalette(colors);
        BitmapPalette myPalette = new BitmapPalette(new List<Color> { Colors.Transparent });
        // Creates a new empty image with the pre-defined palette
        BitmapSource image = BitmapSource.Create(width, height,96, 96,PixelFormats.Indexed1,
                                                 myPalette,pixels,stride);
        return image;*/
        return BitmapImage.Create(16, 16, 96, 96, PixelFormats.Indexed1,
                    new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[32], 2);
    }

    /// <summary>
    /// zwraca przeroczystą bitmapę o wymarach 16x16
    /// przerobić tak żeby wymarytrzeba było podać
    /// </summary>
    /// <returns></returns>
    public static BitmapImage GetBitmapImage( )
    {
        // before encoding/decoding, check if bitmapSource is already a BitmapImage
        BitmapSource bitmapSource = Tools.CreateEmtpyBitmapSource();
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
    /// <summary>
    /// do wywalenia - przenieść do BrokeraINI - przeniesione
    /// Zwraca plik imi o podanej nazwie umieszczony w {user}\AppData\Local\{AppName}
    /// jeżeli nie ma pliko to tworzynowy
    /// Może zwrócić plik znajdujący się w aktualnym katalogu aplikacji o ile wcześniej został tam utworzony
    /// </summary>
    /// <param name="inis"></param>
    /// <returns>zwraca obiekt IniFile lub null</returns>
    
    static public IniFile LoadIni(string inis)
    {
        IniFile ini;
        if (File.Exists(Directory.GetCurrentDirectory() + "\\" + inis))
        {
            ini = new IniFile(Directory.GetCurrentDirectory() + "\\" + inis);            
        }
        else
        {           
            ini = new IniFile(GetUserAppDataPath + "\\" + inis);
        }
        return ini;
    }

    static public IniFile LoadIniProject() => LoadIni(GetProjectName+ ".ini");
    */

    /// <summary>
    /// sprawdza czy podany katalog nie ma atrybutu "ukryty"
    /// lub nie jest koszem albo volume inf..
    /// </summary>
    /// <param name="pathx">Scieżka do sprawdzenia</param>
    /// <returns>nie jest ukryty?</returns>
    public static bool AtrDir(string pathx)
    {
        if (string.IsNullOrEmpty(pathx) || !Directory.Exists(pathx)) return false;
        bool ret = true;
        try
        {
            DirectoryInfo di = new(pathx);
            FileAttributes attributes = File.GetAttributes(pathx);
            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                ret = false;
            }
            if ((di.Name.Equals("RECYCLE")) || (di.Name.Equals("System Volume Information")))
            {
                ret = false;
            }
        }
        catch { return false; }
        return ret;
    }

    /// <summary>
    /// sprawdza czy urzytkownik może odczytać podany katalog
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool AccessDirectory(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;
        try
        {

            DirectoryInfo di = new(path);
            DirectorySecurity security = di.GetAccessControl(AccessControlSections.Access);
            SecurityIdentifier users = new(WellKnownSidType.BuiltinUsersSid, null);
            /*Debug.WriteLine("-- jest: " + path
                + " security "+ security + " users "+ users

                );*/
            foreach (AuthorizationRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
            {
                /*Debug.WriteLine("-- jest w foreach - rule.IdentityReference: "+ rule.IdentityReference
                    +" users: "+ users
                    );*/
                if (rule.IdentityReference == users)
                {
                    Debug.WriteLine("-- jestw if");
                    FileSystemAccessRule rights = ((FileSystemAccessRule)rule);
                    if (rights.AccessControlType == AccessControlType.Allow)
                    {
                        Debug.WriteLine("-- jestw if Allow");
                        if ((FileSystemRights.Modify == (rights.FileSystemRights & FileSystemRights.Modify))
                            || (FileSystemRights.ReadAndExecute == (rights.FileSystemRights & FileSystemRights.ReadAndExecute))
                            )
                            return true;
                    }
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    #region Not Use
    static public string CalculateMD5Sting(string input)
    {

        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes); // .NET 5 +
        }
    }
    static public string CalculateMD5(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))// tu sprawdzanie zrobić sprawdzanie czy do pliku jest dostęp jak nie ma to wyjątek
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                //return Encoding.Default.GetString(md5.ComputeHash(stream));// tu wychodzą jakieś haszcze
                //return md5.ComputeHash(stream).ToString();
            }
        }
    }

    static public string Prdouble(double size)
    {
        double kb = 0.0;
        string og = string.Empty;
        if (size > 1000)
        {
            kb = size / 1024;
            og = " KB";
        }
        if (kb > 1000)
        {
            kb = kb / 1024;
            og = " MB";
        }
        if (kb > 1000)
        {
            kb = kb / 1024;
            og = " GB";
        }
        return kb.ToString("F2") + og;
        //return string.Empty;
    }

    #endregion Not Use
}
