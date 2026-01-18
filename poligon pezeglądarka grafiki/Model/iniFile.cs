using System.Collections;
using System.IO;

namespace poligon_pezeglądarka_grafiki.Model;

internal class IniFile : IDisposable
{
   
    #region ToDo
    /*
     * ToDo
     * 
     * kod do poprawy  i prepisać, wprowadzić linq
     * 
     * 
     * zmiana nazewnictwa Add, Remove, count, Contains, GetValue, GetKeys, GetSections,write (do zapisu na plik), Load i read, save
     * -zrobione-ifExist - tu sekcja lub klucz
     * -zrobione-dodać findSection i findkey - ma zwracać faktyczną nazwę klucza, chodzi o pominięcie wielkości liter
     * get - tu klucz bez sekcji - przeszukuje wszystkie sekcje podaje pierwszy znaleziony
     * get - klucz + sekcja
     * set -  jak wyżej
     * osobny write, ma zapisać strukturę do pliku o ile nie była zapisana
     * odczyt i zapis komentarzy, to może każdy jako jako osobna sekcja kub klucz w sekcji
     * odczyt pliku bez sekcji
     * odczyt pliku zaczynającego się bez sekcji a dalej są sekcje
     * czy da się zrobić sekcje specjalne zaczynające się od _-lub*
     * - ? sekcje specjalne będzie trzeba usuwać w metodzie zwracającej sekcje
     * - ? dorobić metodę prywatną zwracającą wszystkie sekcje
     * odczyt i zapis pustych wierszy - EMPTY
     * czy da sę zrobić konwertowanie stringów do np.: liczb od ręki 
     * tak żeby tego nie robić po pobraniu danych ?
     * dodać przechowywanie w kluczu tablicy lub listy, docelowo ma być zapisana jako stringi oddzielone przecinkami
     * lub jako wcześniej zdefiniowany obiekt, ale do tego potrzebna jest jakaś funkcja ładująca lub obiekt narzędziowy
     * to pozostaje kwestja jak to zrobić i podłączać pod klasę 
     */
    #endregion

    //public bool Transaction { get; set; }
    public bool ReadOnly { get; set; }
    public bool Restriction { get; set; }
    public string IniPath { get { return iniPath; } }

    #region ptivate
    private string iniPath;
    //ma określać czy klucze i sekcje będą sprawdzane dokładnie czy też wpisywane tak jak je pamięta wprowadzający
    //private bool restriction = false;
    // ta konstrukcja w przybliżeniu odzwierciedla wygląd pliku ini
    private readonly Dictionary<string, Dictionary<string, string>> ListAdw = [];

    /// <summary>
    /// załadowanie danych z pliku do struktury
    /// </summary>
    private bool LoadIni()
    {
        string header = "", key, value, temp;
        int found;
        if (File.Exists(this.iniPath))
        {
            foreach (string line in File.ReadLines(this.iniPath))
            {
                temp = line.Trim();
                if (temp.StartsWith(';') || temp.StartsWith('#') || (temp.Length == 0)) continue;
                if (temp.StartsWith('['))
                {
                    found = temp.IndexOf("[");
                    header = temp[(found + 1)..].Remove(temp.IndexOf("]", found) - 1);
                    ListAdw[header] = [];
                }
                else
                {
                    found = temp.IndexOf("=");
                    key = temp.Remove(found);
                    value = temp[(found + 1)..];//temp.Substring(found + 1);                        
                    if (!header.Equals(""))
                        ListAdw[header][key] = value;
                }
            }
            return true;
        }
        else
        {
            string dir = iniPath.Substring(0, iniPath.LastIndexOf('\\'));
            //MessageBox.Show(dir);
            //tu zrobić inną obsługę katalogu, objekt powinien sięgać do pliku i nie przejmować sie katalogiem
            // co najwyżej utworzyć nowy plik w podanym katalogu o ile istnieje jak nie ma katalogu to 
            // powinien wywalić odpowiedni błąd
            _ = Directory.CreateDirectory(dir);
        }
        return false;
    }

    /// <summary>
    /// usuwa wszystkie dane i podklucze ze struktury przed ponownym załadowaniem pliku ini
    /// </summary>
    private void ClearAll()
    {
        foreach (KeyValuePair<string, Dictionary<string, string>> item in ListAdw)
        {
            ListAdw[item.Key].Clear();
        }
        ListAdw.Clear();

        foreach (KeyValuePair<string, Dictionary<string, string>> item in ListAdw)
        {
            foreach (KeyValuePair<string, string> kvp in ListAdw[item.Key])
            {
                _ = ListAdw[item.Key].Remove(kvp.Key);
            }
            _ = ListAdw.Remove(item.Key);
        }
    }

    /// <summary>       
    /// przepisanie danych ze struktury do tablicy jednowymiarowej        
    /// uzupełnia nawiasy w nagłówku sekcji, łączy klucz z danymi        
    /// </summary>        
    /// <returns></returns>
    private string[] IniToArray()
    {
        if (ListAdw.Count > 0)
        {
            List<string> lista = [];
            foreach (KeyValuePair<string, Dictionary<string, string>> item in ListAdw)
            {
                lista.Add("[" + item.Key + "]");
                foreach (KeyValuePair<string, string> kvp in ListAdw[item.Key])
                {
                    lista.Add(kvp.Key + "=" + kvp.Value);
                }
            }
            return [.. lista];
        }
        return Array.Empty<string>();
    }

    #region Find private
    /// <summary>
    /// zwraca poprawną nazwę sekcji patrząc pod względem wielkości liter
    /// search - to nazwa poszukiwanej sekcji
    /// selector - to przełącznik, true - jak nie znaleziono pasującej sekcji to zwraca szukaną
    /// - pozwala to kontrolować wprowadzane sekcje
    /// </summary>
    /// <param name="search"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private string FindSection(string search, bool selector = false)
    {
        if (ExistsSection(search))
        {
            return search;
        }
        else
        {
            search = search.ToLower();
            var list = GetSectionList();
            foreach (var item in list)
            {
                if (item.ToLower().Equals(search))
                {
                    return item;
                }
            }

        }
        if (selector) return search;
        return string.Empty;
    }

    private string FindKey(string search, string section, bool selector = false)
    {
        if (ExistsKey(search))
        {
            return search;
        }
        else
        {
            search = search.ToLower();
            //tu przydała by się lista wszystkch kluczy a nie tylko tych z sekcji!!
            var keys = GetKeysList(section);
            foreach (var key in keys)
            {
                if (key.ToLower().Equals(search))
                {
                    return key;
                }
            }
        }
        if (selector) return search;
        return string.Empty;
    }


    private string FindKey(string search, bool selector = false)
    {
        if (ExistsKey(search))
        {
            return search;
        }
        else
        {
            search = search.ToLower();
            var keys = GetKeysList();
            foreach (var key in keys)
            {
                if (key.ToLower().Equals(search))
                {
                    return key;
                }
            }
        }
        if (selector) return search;
        return string.Empty;
    }

    #endregion find private


    #endregion private
    /// <summary>
    /// Sprawdza czy istnieje taka sekcja lub klucz
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public bool Exists(string search)
    {
        if (ExistsSection(search))
        {
            return true;
        }
        else if (ExistsKey(search))
        {
            return true;
        }
      
        return false;
    }
    /// <summary>
    /// sprawdza czy istnieje taki klucz w jakiej kolwiek sekcji
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public bool ExistsSection(string search)
    {
        if (ListAdw.ContainsKey(search))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// sprawdza czy istnieje taka sekcja
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public bool ExistsKey(string search)
    {
        var sekcje = GetSectionList();
        foreach (var sekcja in sekcje)
        {
            if (ListAdw[sekcja].ContainsKey(search))
            {
                return true;
            }
        }
        return false;
    }

    #region Get

    public string GetSectionKey(string search)
    {
        var sekcje = GetSectionList();
        foreach (var sekcja in sekcje)
        {
            if (ListAdw[sekcja].ContainsKey(search))
            {
                return sekcja;
            }
        }
        return string.Empty;
    }

    public string GetValue(string section, string key)
    {
        if (ListAdw.ContainsKey(section) && ListAdw[section].ContainsKey(key))
        {
            return ListAdw[section][key];
        }

        return string.Empty;
    }

    public string GetStringValue(string section, string key)
    {
        return GetValue(section, key);
    }

    public int GetIniValue(string section, string key)
    {
        string x = GetValue(section, key);
        if ((x == null) || (x == string.Empty)) return -1;//throw new ArgumentNullException("wartość jest pusta lub nie istnieje", nameof(key));

        return Convert.ToInt32(x);
    }

    public double GetDoubleValue(string section, string key)
    {
        string x = GetValue(section, key);
        if ((x == null) || (x == string.Empty)) return 0;//throw new ArgumentNullException("wartość jest pusta lub nie istnieje", nameof(key));

        //return Convert.ToInt32(x);
        //return Double.Parse(x);
        return Convert.ToDouble(x);
    }

    public string GetValue(string key)
    {
        string section;
        if (ExistsKey(key))
        {
            section = GetSectionKey(key);
            return GetValue(section, key);
        }
        return "";
    }

    public bool GetBoolValue(string section, string key)
    {
        string x = GetValue(section, key);
        //if(x == "true") return true;
        if ((x == "") || (x == string.Empty)) return false;
        return Convert.ToBoolean(x);
        //return Convert.ToInt32(x);
        //return true
    }


    /// <summary>
    /// zwraca zawartość całej sekcji w postaci obiektu Dictionary<string,string>
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public Dictionary<string, string>? GetSection(string section)
    {
        if (ListAdw.ContainsKey(section))
        {
            return ListAdw[section];
        }
        return null;
    }


    /// <summary>
    /// Zwraca listę kluczy sekcji
    /// </summary>
    /// <returns></returns>
    public string[] GetSectionList()
    {
        return [.. ListAdw.Keys];
        // return Array.Empty<string>();
    }

    /// <summary>
    /// Zwraca listę kluczy w wybranej sekcji
    /// </summary>
    /// <param name="Section"></param>
    /// <returns></returns>
    public string[] GetKeysList(string Section)
    {
        return [.. ListAdw[Section].Keys];
    }

    /// <summary>
    /// zwraca listę wszystkich kluczy se struktury
    /// !!! przetestować !!!
    /// </summary>
    /// <returns></returns>
    public string[] GetKeysList()
    {
        var section = GetSectionList();
        var keys = new ArrayList();
        foreach (var sec in section)
        {
            _ = keys.Add(ListAdw[sec].Keys.ToArray());
        }
        if (keys.Count > 0) return (string[])keys.ToArray();
        return Array.Empty<string>();
    }

    #endregion Get



    //czy tu dodać parametr wywołania?? pominiemy wtedy saveAs
    public void SaveIni()
    {
        if (!ReadOnly)
        {
            //tu zabezpieczenie żeby całkowicie nie usunąć danych ale czy potrzebne ??
            string[] tab = IniToArray();
            //if(tab.Length > 0)
            File.WriteAllLines(this.iniPath, tab);
        }
    }

    /// <summary>
    /// zmiana pliku docelowego na nowy
    /// </summary>
    /// <param name="path"></param>
    public void SaveAs(string path = "")
    {
        if ((this.iniPath != "") && (this.iniPath != String.Empty))
        {
            //badanie czy ścieszka istnieje
            //if()
            iniPath = path;
            SaveIni();
        }
    }

    //---- initialize, open the file and load the ini if it exists
    public IniFile(string path = "")
    {
        //Transaction = false;
        ReadOnly = false;
        iniPath = path;
        if ((this.iniPath != "") && (this.iniPath != String.Empty))
        {
            _ = LoadIni();
        }
    }

    ~IniFile() => Dispose();

    /// <summary>
    /// usuwa istniejącą strukturę i wczytuje nową z podanego pliku ini
    /// </summary>
    /// <param name="path"></param>
    public void LoadIni(String path)
    {
        if ((this.iniPath != "") && (this.iniPath != String.Empty))
        {
            ClearAll();
        }
        this.iniPath = path;
        //ListAdw.Clear();
        _ = LoadIni();
    }

    //---- reading data from ini keys

    #region Set do Zmiany

    //dodać get i set jednoparametrowe

    public void SetValue(string section, string key, string value)
    {
        if (ListAdw.ContainsKey(section))
        {
            if (ListAdw[section].ContainsKey(key))
            {
                ListAdw[section][key] = value;
            }
            else
            {
                ListAdw[section].Add(key, value);
            }
        }
        else
        {
            ListAdw[section] = new Dictionary<string, string>
            {
                { key, value }

            };
        }
        //to do poprawy a na pewno do zmiany
        if (!ReadOnly) SaveIni();
    }
    /* ToDo - ma zmieniać wartość istniejącego klucza, jak klucza nie ma to rzuca wyjątek
    public void SetValue(string key, string value)
    {

    //tu zobaczyć jak ma wyglądać rzucanie wyjątków


    }
    */
    #endregion Set do zmiany


    /// <summary>
    /// zapis danych do struktury, nadpisuje istniejące dane
    /// </summary>
    /// <param name="section"></param>
    /// <param name="KeyVal"></param>
    public void WriteAppendAll(string section, Dictionary<string, string> KeyVal)
    {
        if (ListAdw.ContainsKey(section))
        {
            ListAdw[section].Clear();
        }
        ListAdw[section] = KeyVal;
        //to do poprawy a na pewno do zmiany
        SaveIni();
    }

    /// <summary>
    /// zapis danych do struktury, dopisuje do istniejącej struktury
    /// </summary>
    /// <param name="section"></param>
    /// <param name="KeyVal"></param>
    public void Write(string section, Dictionary<string, string> KeyVal)
    {
        if (!ListAdw.ContainsKey(section))
        {
            ListAdw[section] = [];
        }
        foreach (KeyValuePair<string, string> kvp in KeyVal)
        {
            //lista.Add(kvp.Key + "=" + kvp.Value);
            ListAdw[section][kvp.Key] = kvp.Value;
        }
        //to do poprawy a na pewno do zmiany
        SaveIni();
    }

    //---- Remove (delete) keys and section

    /// <summary>
    /// usuwa całą wskazaną sekcję
    /// </summary>
    /// <param name="section"></param>
    public void Remove(string section)
    {
        if (ListAdw.ContainsKey(section))
        {
            _ = ListAdw.Remove(section);
            SaveIni();
        }

    }

    /// <summary>
    /// usuwa klucz 'key' z wartością z sekcji 'section"
    /// </summary>
    /// <param name="section"></param>
    /// <param name="key"></param>
    public void Remove(string section, string key)
    {
        if (ListAdw.ContainsKey(section) && ListAdw[section].ContainsKey(key))
        {
            _ = ListAdw[section].Remove(key);
            SaveIni();
        }
    }

    //---- tools

    public void Dispose()
    {
        SaveIni();

        ((IDisposable)ListAdw).Dispose();

        // throw new NotImplementedException();
    }
}
