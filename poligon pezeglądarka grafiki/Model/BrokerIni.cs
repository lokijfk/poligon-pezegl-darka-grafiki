using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
//using System.Drawing;

namespace poligon_pezeglądarka_grafiki.Model;

class BrokerIni
{
    //albo to zmienić na dictionary, albo dodać dictionary na inne pliki ini
    // a tu zostawić taki na tylko to 
    //na pewno dojedzie ini z kolorami ustawianymi przez urzytkownika 
    // i może drugi ze schematami bazy danych, co też będzie mógł modyfikować urzytkownik
    private readonly IniFile iniFile;


    #region HEAD
    public static IniFile LoadIni(string inis)
    {
        IniFile ini;
        if (File.Exists(Directory.GetCurrentDirectory() + "\\" + inis))
        {
            ini = new IniFile(Directory.GetCurrentDirectory() + "\\" + inis);
        }
        else
        {
            ini = new IniFile(Tools.GetUserAppDataPath + "\\" + inis);
        }
        //Debug.WriteLine(Tools.GetUserAppDataPath);
        return ini;
    }

    public static IniFile LoadIniProject() => LoadIni(Tools.GetProjectName + ".ini");
    private IniFile GetIni()
    {
        if (iniFile == null)
        {
            //to przerobić, powinno być już lokalnie a nie w tools
            // tools należy rozłozyć na części składowe i usunąć
            return LoadIniProject();
        }
        return iniFile;
    }

    public BrokerIni()
    { 
        iniFile = GetIni();
    }
    #endregion HEAD


    #region Metody publiczne
    #region interface
    public bool VisibleToolBar
    {
        get => GetBoolValue(GetCurrentMethod(),"Interface");
        set => SetBoolValue(GetCurrentMethod(), value, "Interface");
    }

    public bool VisibleStatusBar
    {
        get => GetBoolValue(GetCurrentMethod(), "Interface");
        set => SetBoolValue(GetCurrentMethod(), value, "Interface");
    }

    public bool VisibleFilesInTree
    {
        get => GetBoolValue(GetCurrentMethod(), "Interface");
        set => SetBoolValue(GetCurrentMethod(), value, "Interface");
    }

    /*
    public bool OnlyFoldersWithFiles
    {
        get => GetBoolValue(GetCurrentMethod(), "Interface");
        set => SetBoolValue(GetCurrentMethod(), value, "Interface");
    }*/

    #endregion interface

    #region Tree 
    public string PathFolderTree
    {
        get => GetStringValue(GetCurrentMethod(),"Folders");
        set => SetStringValue(GetCurrentMethod(), value,"Folders");
    }

    public string PathFolderExcluded
    {
        get => GetStringValue(GetCurrentMethod(), "Folders");
        set => SetStringValue(GetCurrentMethod(), value, "Folders");
    }

    public string SelectedPathFolderTree
    {
        get => GetStringValue(GetCurrentMethod(),"Folders");
        set => SetStringValue(GetCurrentMethod(), value, "Folders");
    }

    public string SelectedView
    {
        get => GetStringValue(GetCurrentMethod(), "Interface");
        set => SetStringValue(GetCurrentMethod(), value, "Interface");
    }

    #endregion Tree

    #region Collor and switch
    public Color PrimaryColor
    {
        get
        {
            string name = GetStringValue(GetCurrentMethod());
            if (name == null) return new Color();
            System.Drawing.Color col = System.Drawing.Color.FromName(GetStringValue(GetCurrentMethod()));
            return   new Color() { A = col.A, R = col.R, G = col.G, B = col.B };
        }
        set  { SetStringValue(GetCurrentMethod(), value.ToString());  }
    }

    public Color SecondaryColor
    {
        get
        {
            string name = GetStringValue(GetCurrentMethod());
            if (name == null) return new Color();
            System.Drawing.Color col = System.Drawing.Color.FromName(GetStringValue(GetCurrentMethod()));
            return new Color() { A = col.A, R = col.R, G = col.G, B = col.B };
        }
        set { SetStringValue(GetCurrentMethod(), value.ToString()); }
    }


    public bool SwitchToggleButton
    {
        get => GetBoolValue(GetCurrentMethod(), "Interface");
        set => SetBoolValue(GetCurrentMethod(), value, "Interface");
    }

    #endregion Collor and switch

    #region Window

    public double WindowTop 
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 5 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }


    public double WindowLeft 
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 5 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }



    public double WindowHeight
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ?  450: GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }

    public double WindowWidth
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window")==0? 800 : GetDoubleValue(GetCurrentMethod(), "Window") ;
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }
            

    public WindowState CurMainWindowState
    {
        get => GetStringValue(GetCurrentMethod(), "Window") == string.Empty ? WindowState.Normal : (WindowState)Enum.Parse(typeof(WindowState), GetStringValue(GetCurrentMethod(), "Window"), true);
        /*get {
            if (GetStringValue(GetCurrentMethod(), "Window") != string.Empty)
           return (WindowState)Enum.Parse(typeof(WindowState), GetStringValue(GetCurrentMethod(), "Window"), true);
            else return WindowState.Normal;

        }//*/
        set => SetStringValue(GetCurrentMethod(), value.ToString(), "Window");
    }

    public string CurMainWindowStateString
    {
        get => GetStringValue(GetCurrentMethod(), "Window");
        set => SetStringValue(GetCurrentMethod(), value, "Window");
    }

    public double LastWidth
    {
        get => GetDoubleValue(GetCurrentMethod(),"Window") == 0 ? 800 : GetDoubleValue(GetCurrentMethod(),"Window");
        set => SetDoubleValue(GetCurrentMethod(), value,"Window");
    }

    public double LastHeihgt
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 450 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }

    public double LastLeft
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 5 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }

    public double LastTop
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 5 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }
    #endregion Window

    #endregion Metody publiczne

    #region Metordy dostępowe prywatne

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetCurrentMethod()
    {
        var st = new StackTrace();
        var sf = st.GetFrame(1);
        if (IsNotNull(sf))
        {
            var sx = sf.GetMethod();
            if (IsNotNull(sx)) return sx.Name.Substring(4);
        }
        return string.Empty;
    }

    //do tych metod dodać mozliwość zmiany sekcji a więc jeszcze jeden paramwetr bo na potrzeby produkcyjne może być wszystko w "G"
    // ale później lepiej żeby to można było rozdzielić
    private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;
    private int GetIntValue(string mert) => iniFile.GetIniValue("General", mert);
    private int GetIntValue(string mert,string sec) => iniFile.GetIniValue(sec, mert);
    private Double GetDoubleValue(string mert, string sec) => iniFile.GetDoubleValue(sec, mert);
    private void SetIntValue(string met, int val) => iniFile.SetValue("General", met, val.ToString());
    private void SetIntValue(string met, int val,string sec) => iniFile.SetValue(sec, met, val.ToString());
    private void SetDoubleValue(string met, double val, string sec) => iniFile.SetValue(sec, met, val.ToString());
    private bool GetBoolValue(string mert) => iniFile.GetBoolValue("General", mert);
    private bool GetBoolValue(string mert,string sec) => iniFile.GetBoolValue(sec, mert);
    private void SetBoolValue(string met, bool val) => iniFile.SetValue("General", met, val.ToString());
    private void SetBoolValue(string met, bool val,string sec) => iniFile.SetValue(sec, met, val.ToString());
    private string GetStringValue(string mert) => iniFile.GetStringValue("General", mert);
    private string GetStringValue(string mert,string sec) => iniFile.GetStringValue(sec, mert);
    private void SetStringValue(string met, string val) => iniFile.SetValue("General", met, val);
    private void SetStringValue(string met, string val,string sec) => iniFile.SetValue(sec, met, val);

    #endregion Metordy dostępowe prywatne
}
