using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;


namespace poligon_pezeglądarka_grafiki.Model;

public partial class TreeModel : ObservableObject
{

    #region properties

    //dodać pole "ukryty" i zaimplementować w drzewie, dodać przycisk pokazyjący okno z ukrytymi i możliwością odkrycia
    //usunąć zbędna, obiekt przystosowany do bazy danych a nie do systemu katalogów !!

    public TreeModel? Parent { get; set; } = null;// zostawiam może ułatwi nawigowanie

    public ObservableCollection<TreeModel> Children { get; set; } = [];

    [ObservableProperty]
    private string _name = string.Empty;
    public bool IsSelected { get; set; } = false;

    [ObservableProperty]
    private bool _IsExpanded  = false;

    public string IsRightSelected { get; set; } = string.Empty;

    public string View { get; set; } = string.Empty;
    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private int _CountFiles = 0;

    #endregion properties

    #region methods

    public void RightSelect()
    {
        if (Parent != null)
        {
            GetRootNode(this).IsRightSelected = _path;
        }
        else
        {
            IsRightSelected = _path;
        }
    }

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
                //Debug.WriteLine(GetPathSelecetedNode());
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

    /// <summary>
    /// zwraca korzeń drzewa dla podanego elementu
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public TreeModel? GetRootNode(TreeModel item)
    {
        TreeModel root = null;
        while (item.Parent != null)
        {
            root = item.Parent;
            item = root;
        }
        return root;
    }

    /// <summary>
    /// znajduje dziecko o podanej nazwie w drzewie
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
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

    /// <summary>
    /// znajduje dziecko o podanej ścieżce w drzewie
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public TreeModel? FindChildByPath(string path)
    {
        //Debug.WriteLine("FindChildByPath, searching for: " + path);
        TreeModel root = this;
        if (root.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) return root;
        foreach (var child in root.Children)
        {
            if (child.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                //Debug.WriteLine("FindChildByPath 1, found direct: " + path);
                return child;
            }
            var foundChild = child.FindChildByPath(path);
            if (foundChild != null)
            {
                //Debug.WriteLine("FindChildByPath 2, found recursive: " + path);
                return foundChild;
            }
        }
        //Debug.WriteLine("FindChildByPath, not found: " + path);
        return null;
    }

    /// <summary>
    /// znajduje dziecko o podanym obiekcie w drzewie
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
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
    /// wymagane do edycji danych w drzewie
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

    /// <summary>
    /// zwraca element o podanej ścieżce z głównego drzewa
    /// alternatywa do FindChildByPath
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public TreeModel? GetElementByPath(string path)
    {
        // to jest do poprawy, nie działa jak powinno
        // nie szuka w drzewie tylko w ścieżce
        TreeModel? root = this;
        if (root.Parent != null)
        {
            root = GetRootNode(this);
        }
        //TreeModel? root = GetRootNode(this);
        //Debug.WriteLine("GetElementByPath,root: " + root.Name+" , "+root.Path+" , path: "+ path);
        TreeModel? result = null;
        if (root != null)
        {
            result = root.FindChildByPath(path);
            if (result != null)
            {
                //Debug.WriteLine("GetElementByPath, found: " + result.Name + ", path: " + result.Path);
                return result;  //root.FindChildByPath(path);
            }
        }
        return null;
    }

    /// <summary>
    /// zwraca pełną ścieżkę do elementu
    /// metoda testowa do budowy ścieżki w oprciu o drzewo
    /// </summary>
    /// <returns></returns>
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