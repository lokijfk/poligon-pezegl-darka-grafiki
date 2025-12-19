using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace poligon_pezeglądarka_grafiki.DEP;


//tu nie ma potrazeby wstawiania CT observable który generuje nadmiarowy kod
// to w zasadzie pojemnik to wystarczy sam public
public partial class TreeModel : ObservableObject
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
    [ObservableProperty]
    private TreeModel? _Parent = null;// zostawiam może ułatwi nawigowanie
    [ObservableProperty]
    private ObservableCollection<TreeModel> _children = [];
    //[ObservableProperty]
    //private T1? _selectedValue;
    [ObservableProperty]
    private string _Name = string.Empty;
    [ObservableProperty]
    private bool _isSelected = false;
    [ObservableProperty]
    private bool _isExpanded = false;
    [ObservableProperty]
    private bool _isRightSelected = false;
    [ObservableProperty]
    private string _view = string.Empty;
    [ObservableProperty]
    private string _Path = string.Empty;
    [ObservableProperty]
    private string count = string.Empty;

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

    public TreeModel? GetParent(TreeModel item) => item.Parent;


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