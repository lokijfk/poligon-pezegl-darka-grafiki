using poligon_pezeglądarka_grafiki.Model.Interface;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.Model;

/// <summary>
/// Klasa pośrednicząca pomiędzy obiektem udostępniającym dane z plików ini a resztą programu, która korzysta z tych danych,
/// głównie z ustawień interfejsu użytkownika, ale nie tylko, docelowo może być też wykorzystywana do innych plików ini, np. z ustawieniami kolorów czy schematami bazy danych
/// ma na celu ułatwienie dostępu do tych danych, zapewnie konwersję typów na te wymagane przez program, oraz dostarcza właściwości odpowiaddające pobieranym lub zapisywanym danym, 
/// a także zapewnienie, że dane są ładowane tylko raz i są dostępne dla całego programu, czyli implementacja wzorca singletona 
/// </summary>
class BrokerIni //: IBrokerIni
{
    //interfejs  jest niepotrzebny, zastosowany do testowania DI, ale na razie nie widzę zastosowanie dla DI i na razie chyba odpuszczę  to rozwiązanie, ale zostawię ten interfejs na wszelki wypadek, może kiedyś się przyda

    //albo to zmienić na dictionary, albo dodać dictionary na inne pliki ini
    // a tu zostawić taki na tylko to 
    //na pewno dojedzie ini z kolorami ustawianymi przez urzytkownika 
    // i może drugi ze schematami bazy danych, co też będzie mógł modyfikować urzytkownik

    //dodać ładowanie default.ini z zasobów programu jak nie ma pliku ini w katalogu aplikacji
    /*
    string env = BrokerFile.GetUserAppDataPath;
    string PathToExe = Path.Combine(env, "poligon pezeglądarka grafiki.exe");
    string PathToIco = Path.Combine(env, @"img\73042biohazard_109537(1).ico");
    */
    private readonly IniFile iniFile;
    private static BrokerIni brokerini;
    //private static bool FirstRunKey = false;
    #region HEAD
    //public static bool IsFirstRun() => FirstRunKey;

    public static IniFile LoadIni(string inis)
    {
        IniFile ini;
        if (File.Exists(Directory.GetCurrentDirectory() + "\\" + inis))
        {
            ini = new IniFile(Directory.GetCurrentDirectory() + "\\" + inis);
        }
        else
        {
            string katalog = BrokerFile.GetUserAppDataPath;
            string path = Path.Combine(katalog, inis);
            if (!Directory.Exists(katalog) || (Directory.Exists(katalog) && !File.Exists(path)))
            {
                //ini = new IniFile(path);
                //FirstRunKey = true;
               // Debug.WriteLine("pierwsze uruchomienie programu - ładuję ustawienia domyślne");                
                //IniFile defaultIni = new IniFile(Path.Combine(BrokerFile.GetUserAppDataPath, @"Config\\default.ini"));
                //Debug.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), "Config\\default.ini"));
                ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Config\\default.ini"));
                ini.SaveAs(path);
            }
            else
            {
                ini = new IniFile(path);
            }               
        }
        //Debug.WriteLine(Tools.GetUserAppDataPath);
        return ini;
    }

    public static IniFile LoadIniProject() => LoadIni(BrokerFile.GetProjectName + ".ini");
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
    
    /// <summary>
    /// zwraca zainicjowany obiekt BrokerIni tak żeby był jeden dla całego projektu
    /// nie jest to wałściwe rozwiązanie ale jak na razie musi wystarczyć
    /// </summary>
    /// <returns></returns>
    public static BrokerIni GetBroker()
    {
        if(brokerini == null)
        {
            brokerini = new();
            return brokerini;
        }
        else
        {
            return brokerini;
        }
    }

    public BrokerIni()
    {
        iniFile = GetIni();
    }
    #endregion HEAD


    #region Metody publiczne
    #region Global
    public string Version
    {
        get => GetStringValue(GetCurrentMethod());
        set => SetStringValue(GetCurrentMethod(), value);
    }

    #endregion Global


    #region interface
    public bool VisibleToolBar
    {
        get => GetBoolValue(GetCurrentMethod(), "Interface");
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
        get => GetStringValue(GetCurrentMethod(), "Folders");
        set => SetStringValue(GetCurrentMethod(), value, "Folders");
    }

    public string PathFolderExcluded
    {
        get => GetStringValue(GetCurrentMethod(), "Folders");
        set => SetStringValue(GetCurrentMethod(), value, "Folders");
    }

    public string SelectedPathFolderTree
    {
        get => GetStringValue(GetCurrentMethod(), "Folders");
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
            return new Color() { A = col.A, R = col.R, G = col.G, B = col.B };
        }
        set { SetStringValue(GetCurrentMethod(), value.ToString()); }
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
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 450 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }

    public double WindowWidth
    {
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 800 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
    }


    public WindowState CurMainWindowState
    {
        get => GetStringValue(GetCurrentMethod(), "Window") == string.Empty ? WindowState.Normal : Enum.Parse<WindowState>(GetStringValue(GetCurrentMethod(), "Window"), true);
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
        get => GetDoubleValue(GetCurrentMethod(), "Window") == 0 ? 800 : GetDoubleValue(GetCurrentMethod(), "Window");
        set => SetDoubleValue(GetCurrentMethod(), value, "Window");
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

    #region sortowanie

    public string Sortowaniekryterium
    {
        get => GetStringValue(GetCurrentMethod(), "Sortowanie") == string.Empty ? "Nazwa" : GetStringValue(GetCurrentMethod(), "Sortowanie");
        set => SetStringValue(GetCurrentMethod(), value, "Sortowanie");
    }

    public string Sortowaniekierunek
    {
        get => GetStringValue(GetCurrentMethod(), "Sortowanie") == string.Empty ? "Rosnąco" : GetStringValue(GetCurrentMethod(), "Sortowanie");
        set => SetStringValue(GetCurrentMethod(), value, "Sortowanie");
    }

    public Brush DropCollor
    {
        #pragma warning disable CS8603 // Możliwe zwrócenie odwołania o wartości null.
        get => GetStringValue(GetCurrentMethod(), "Color") == string.Empty ?
            (Brush)new BrushConverter().ConvertFrom("#459cfc") : 
            (Brush)new BrushConverter().ConvertFrom(GetStringValue(GetCurrentMethod(), "Color"));
        #pragma warning restore CS8603 // Możliwe zwrócenie odwołania o wartości null.
        set => SetStringValue(GetCurrentMethod(), value.ToString(), "Color");            
    }

    #endregion

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
    private int GetIntValue(string met) => iniFile.GetIniValue("General", met);
    private int GetIntValue(string met, string sec) => iniFile.GetIniValue(sec, met);
    private Double GetDoubleValue(string met, string sec) => iniFile.GetDoubleValue(sec, met);
    private void SetIntValue(string met, int val) => iniFile.SetValue("General", met, val.ToString());
    private void SetIntValue(string met, int val, string sec) => iniFile.SetValue(sec, met, val.ToString());
    private void SetDoubleValue(string met, double val, string sec) => iniFile.SetValue(sec, met, val.ToString());
    private bool GetBoolValue(string met) => iniFile.GetBoolValue("General", met);
    private bool GetBoolValue(string met, string sec) => iniFile.GetBoolValue(sec, met);
    private void SetBoolValue(string met, bool val) => iniFile.SetValue("General", met, val.ToString());
    private void SetBoolValue(string met, bool val, string sec) => iniFile.SetValue(sec, met, val.ToString());
    private string GetStringValue(string met) => iniFile.GetStringValue("General", met);
    private string GetStringValue(string met, string sec) => iniFile.GetStringValue(sec, met);
    private void SetStringValue(string met, string val) => iniFile.SetValue("General", met, val);
    private void SetStringValue(string met, string val, string sec) => iniFile.SetValue(sec, met, val);

    #endregion Metordy dostępowe prywatne
}
