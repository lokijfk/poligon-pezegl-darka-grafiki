using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;



namespace poligon_pezeglądarka_grafiki.Model;

// zmienić na PathHelper
internal class BrokerFile
{
    //private static ModelPB model = null;
    public static string GetUserAppDataPath =>
        
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" +
            GetProjectName;

    public static string GetProjectName        
        => Assembly.GetExecutingAssembly().GetName().Name.ToString();

    

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

    #region kalkulacj MD5 i double to string z przeliczeniem na KB,MG i GB
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

    #endregion 

    /// <summary>
    /// przenosi usuwane pliki do kosza
    /// </summary>
    /// <param name="FileToDele"></param>
    public static bool DeleteFile(string FileToDele)
    {
        //dodać sprawdzanie czy plik można usunąć
        //jak nie może usunąć to wyrzuca wyjątek, trzeba to obsłużyć?
        //dodąć wersję z pytaniem systemowym
        if (File.Exists(FileToDele))
        {
            try
            {
                FileSystem.DeleteFile(FileToDele, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// usuwa pliki z pominięciem kosza
    /// </summary>
    /// <param name="FileToDele"></param>
    /// <returns></returns>
    public static bool DeleteFileStrong(string FileToDele)
    {
        try
        {
            File.Delete(FileToDele);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
        return true;
    }
        

    public static void DeleteDirectory(string DirectoryToDele)
    {
        //dodać sprawdzanie czy katalog można ususnąć
        if (Directory.Exists(DirectoryToDele))
        {
            try
            {
                FileSystem.DeleteDirectory(DirectoryToDele, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }
    }


    public static void CreateDirectory(string NewDirectory)
    {
        try
        {
            _ = Directory.CreateDirectory(NewDirectory);
            //Debug.WriteLine("AddFolder: " + newPath);
            //return cat;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public static bool RenameFilDirectory(string oldName, string newName)
    {
        try
        {
            Directory.Move(oldName, newName);

        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public static bool RenameFile(string oldName, string newName)
    {
        //string path = oldName.Substring(0, oldName.LastIndexOf('\\') + 1);
        //Debug.WriteLine("o: " + oldName + " ,N: " +newName);
        try
        {
            File.Move(oldName, newName);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }
        return true;
    }
}
