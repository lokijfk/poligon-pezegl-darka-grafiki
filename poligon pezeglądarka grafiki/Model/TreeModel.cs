using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace poligon_pezeglądarka_grafiki.Model;

public partial class TreeModel :ObservableObject
{
    /*
    public TreeModel()
    {
        this.IsSelected = false;
        Children = [];
        Name = string.Empty;
    }
    */
    #region properties

    //dodać pole "ukryty" i zaimplementować w drzewie, dodać przycisk pokazyjący okno z ukrytymi i możliwością odkrycia
    //usunąć zbędna, obiekt przystosowany do bazy danych a nie do systemu katalogów !!
  /*  [ObservableProperty]
    private int _Id = -1;
    [ObservableProperty]
    public int _ParentID = -1;*/
    
    public TreeModel? Parent { get; set; } = null;// zostawiam może ułatwi nawigowanie
    
    public ObservableCollection<TreeModel> Children { get; set; } = [];
    //[ObservableProperty]
    //private T1? _selectedValue;
    [ObservableProperty]
    private string _name  = string.Empty;
    /*
    public string Name
    {
        get => _name;
        //set => _name = value;
        set
        {
            SetProperty(ref _name, value);
            //OnPropertyChanged(Name); // Notify that Name has changed
        }
    }*/
    public bool IsSelected { get; set; } = false;
    
    public bool IsExpanded { get; set; } = false;
    
    public bool IsRightSelected { get; set; } = false;
    
    public string View { get; set; } = string.Empty;
    [ObservableProperty]
    private string _path = string.Empty;
    /*
    public string Path
    { 
        get => _path; 
        //set => _path = value;
        
        set
        {
            SetProperty(ref _path, value);
            //OnPropertyChanged(Path); // Notify that Path has changed
        }
    }//*/
    
    [ObservableProperty]
    private int _CountFiles  = 0;

    #endregion properties

    #region methods

    public override string ToString()
    {
        return this.Name + "," + this.Path;
    }

    public void AddChild(TreeModel child)
    {
        child.Parent = this;
        this.Children.Add(child);
    }

    public void Addchild(ObservableCollection<TreeModel> treeModels)
    {
        foreach (var child in treeModels)
        {
            child.Parent = this;
            this.Children.Add(child);
        }
    }
    // public TreeModel GetSelectedItem => Children.FirstOrDefault(i => i.IsSelected);
    #endregion methods
    // to jest jakieś rozwiązanie nie najlepsze ale innego chwilowo nie mam
    public TreeModel? GetSelectedItem(TreeModel nodes = null)
    {
        //nie jest tu wykożystywane
        if (nodes == null) nodes = this;
        //var nodes = this.Children;
        foreach (var node in nodes.Children)
        {
            if (node.IsSelected)
            {
                Debug.WriteLine(GetPathSelecetedNode());
                return node;

            }

            var selectedChild = GetSelectedItem(node);
            if (selectedChild != null)
                return selectedChild;
        }

        return null;
    }


    public string GetPathSelecetedNode()
    {
        // to też jest nie potrzebne, to jest brane z path elementu
        TreeModel tree = GetSelectedItem();
        if (tree != null)
        return tree.Path;// przetestować co zwróci
        return String.Empty;
    }

    public TreeModel? GetParent() => this.Parent;
  
    public TreeModel? GetRootNode(TreeModel item)
    {
        TreeModel root = null;
        while(item.Parent != null)
        {
            root = item.Parent;
            item = root;
        }
        return root;
    }

    public TreeModel? FindChild(string name)
    {
        TreeModel root = this;
        foreach (var child in root.Children)
        {
            if (child.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            var foundChild = child.FindChild(name);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        return null;
    }
     

    public TreeModel? FindChildByPath(string path)
    {
        TreeModel root = this;
        foreach (var child in root.Children)
        {
            if (child.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            var foundChild = child.FindChildByPath(path);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        return null;
    }

    public TreeModel? FindChild(TreeModel item)
    {
        TreeModel root = this;
        foreach (var child in root.Children)
        {
            if (child.Equals(item))
            {
                return child;
            }
            var foundChild = child.FindChild(item);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        return null;
    }
    /// <summary>
    /// zwraca element z głównego drzewa, nie z rodzica, 
    /// wyszukuje root i z tamtąd szuka identycznego elementu
    /// </summary>
    /// <returns></returns>
    public TreeModel? GetSelfFromMainStream()
    {
        //TreeModel item = this;
        TreeModel? root = GetRootNode(this);
        if (root != null)
        {
            return root.FindChild(this.Name);
        }
        return null;
    }
    /// <summary>
    /// to któtsza alternatywa do GetSelfFromMainStream, zwraca element z rodzica
    /// </summary>
    /// <returns></returns>
    public TreeModel? GetSelfFromParent()
    {
        //TreeModel item = this;
        TreeModel? parent = this.Parent;
        if (parent != null)
        {
            return parent.FindChild(this.Name);
        }
        return null;
    }

    public TreeModel? GetElementByPath(string path)
    {
        // to jest do poprawy, nie działa jak powinno
        // nie szuka w drzewie tylko w ścieżce
        TreeModel? root = this;
        if(root.Parent != null)
        {
            root = GetRootNode(this);
        }
        //TreeModel? root = GetRootNode(this);
        //Debug.WriteLine("GetElementByPath,root: " + root.Name+" , "+root.Path+" , path: "+ path);
        if (root != null)
        {
            return root.FindChildByPath(path);
        }
        return null;
    }

    /**
     * metoda testowa, zwraca pełną ścieżkę do katalogu który jest reprezentowany przez ten obiekt
     * 
     */
    public string GetFullPath()
    {
        TreeModel? current = this;
        //string fullPath = current.Name;
        if (current.Parent != null)
        {
            return current.Parent.GetFullPath() + "\\" + current.Name;
        }
        else
        {
            return current.Path;
        }
    }

    #region static methods
    // metody statyczne można przenieść do innego obiektu, tu raczej nie mają sensu 
    // operują na nim ale się do niego nie odwołują
    // część przestaje być urzeteczna z powodu wykozystania zachowań "behaviors" co jest bardziej praktyczne
    // dodać odnajdywanie pierszego elementu- będzie on nazwą bazy w drzewie
    /*
    public static TreeModel<T1>? GetNodeById(T1 id, IEnumerable<TreeModel<T1>> nodes)
    {
        foreach (var node in nodes)
        {
            if ((node.SelectedValue != null) &&(node.SelectedValue.Equals(id)))
                return node;

            var foundChild = GetNodeById(id, node.Children);
            if (foundChild != null)
                return foundChild;
        }
        return null;
    }

    public static TreeModel<T1>? GetSelectedNode(IEnumerable<TreeModel<T1>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.IsSelected)
                return node;

            var selectedChild = GetSelectedNode(node.Children);
            if (selectedChild != null)
                return selectedChild;
        }

        return null;
    }

    public static void ExpandParentNodes(TreeModel<T1> node)
    {
        if (node.Parent != null)
        {
            node.Parent.IsExpanded = true;
            ExpandParentNodes(node.Parent);
        }
    }

    public static void ToggleExpanded(IEnumerable<TreeModel<T1>> nodes, bool isExpanded)
    {
        foreach (var node in nodes)
        {
            node.IsExpanded = isExpanded;
            ToggleExpanded(node.Children, isExpanded);
        }
    }
    */
#endregion static methods

}
/*
public class TreeModel : TreeModel<Guid>
{
}
*/