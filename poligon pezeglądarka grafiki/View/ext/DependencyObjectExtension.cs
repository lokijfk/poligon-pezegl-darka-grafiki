
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.View.ext;

public static class DependencyObjectExtension
{
   /*
    public static Grid GetParentAsGrid(this DependencyObject item)
    {
        //DependencyObject item = block;
        while (item != null && !(item is Grid))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as Grid;
    }
   */
    /// <summary>
    /// Zwraca pierwszy znaleziony TreeViewItem z parent DependencyObject.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static TreeViewItem GetTreeViewItem(this DependencyObject item)
    {
        //DependencyObject item = block;
        while (item != null && !(item is TreeViewItem))
        {
            item = VisualTreeHelper.GetParent(item);
        }
        return item as TreeViewItem;
    }

    /*
    public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        if (obj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                {
                    return (T)child;
                }

                T childItem = FindVisualChild<T>(child);
                if (childItem != null) return childItem;
            }
        }
        return null;
    }*/

    /*
    public static TextBox GetSisTexBox(this DependencyObject item)
    {
        //DependencyObject item = block;

        var parent = VisualTreeHelper.GetParent(item);
        foreach (var child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is TextBox textBox)
            {
                return textBox;
            }
        }
        return null;
    }
    */

    public static TextBlock GetSisTextBlock(this DependencyObject item)
    {
        //DependencyObject item = block;
        /*
        var parent = VisualTreeHelper.GetParent(item);
        foreach (var child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is TextBlock textBlock)
            {
                return textBlock;
            }
        }
        return null;
        */
        var parent = VisualTreeHelper.GetParent(item);
        return FindChildren<TextBlock>(parent).FirstOrDefault();
    }
    
    /*
    public static ListBoxItem GetParentAsListBoxItem(this DependencyObject item)
    {

        return TryFindParent<ListBoxItem>(item);
    }

    public static ListBox GetParentAsListBox(this DependencyObject item)
    {

        return TryFindParent<ListBox>(item);
    }
    */
    //dodać listbox i listboxitem

    /// <summary> 
    /// Znajduje element nadrzędny danego elementu w drzewie wizualnym. 
    /// </summary> 
    /// <typeparam name="T">Typ elementu zapytania.</typeparam> 
    /// <param name="child">Bezpośredni lub pośredni element podrzędny 
    /// elementu zapytania.</param> 
    /// <returns>Pierwszy element nadrzędny, który pasuje do przesłanego 
    /// parametru typu. Jeśli zostanie znaleziony element niezgodny, 
    ///zwracane jest odwołanie null.</returns> 
    /// source:  http://www.hardcodet.net/2009/06/finding-elements-in-wpf-tree-both-ways
    public static T TryFindParent<T>(this DependencyObject child)
        where T : DependencyObject
    {
        //pobierz element nadrzędny
        DependencyObject parentObject = GetParentObject(child);

        //dotarliśmy do końca drzewa 
        if (parentObject == null) return null;

        //sprawdza, czy obiekt nadrzędny pasuje do typu, którego szukamy 
        T parent = parentObject as T;
        if (parent != null)
        {
            return parent;
        }
        else
        {
            //użyj rekurencji, aby przejść do następnego poziomu
            return TryFindParent<T>(parentObject);
        }
    }

    /// <summary> 
    /// Ta metoda jest alternatywą dla metody WPF 
    /// <see cref="VisualTreeHelper.GetParent"/>, która również 
    /// obsługuje elementy treści. Należy pamiętać, że w przypadku elementu treści 
    /// ta metoda odwołuje się do drzewa logicznego elementu! 
    /// </summary> 
    /// <param name="child">Element do przetworzenia.</param> 
    /// <returns>Element nadrzędny przesłanego elementu, jeśli jest dostępny. W przeciwnym razie 
    /// null.</returns> 
    /// source:  http://www.hardcodet.net/2009/06/finding-elements-in-wpf-tree-both-ways
    public static DependencyObject GetParentObject(this DependencyObject child)
    {
        if (child == null) return null;

        //obsługa elementów zawartości oddzielnie 
        ContentElement contentElement = child as ContentElement;
        if (contentElement != null)
        {
            DependencyObject parent = ContentOperations.GetParent(contentElement);
            if (parent != null) return parent;

            FrameworkContentElement fce = contentElement as FrameworkContentElement;
            return fce != null ? fce.Parent : null;
        }

        //spróbuj również wyszukać rodzica w elementach frameworka (takich jak DockPanel itp.)
        FrameworkElement frameworkElement = child as FrameworkElement;
        if (frameworkElement != null)
        {
            DependencyObject parent = frameworkElement.Parent;
            if (parent != null) return parent;
        }

        //jeśli to nie jest ContentElement/FrameworkElement, polegaj na VisualTreeHelper
        return VisualTreeHelper.GetParent(child);
    }

    /// <summary> 
    /// Analizuje zarówno drzewo wizualne, jak i logiczne, aby znaleźć wszystkie elementy danego 
    /// typu, które są potomkami elementu <paramref name="source"/> . 
    /// </summary> 
    /// <typeparam name="T">Typ elementów zapytania.</typeparam> 
    /// <param name="source">Element główny oznaczający źródło wyszukiwania. Jeśli 
    /// źródło jest już żądanego typu, nie zostanie uwzględnione w wyniku.</param> 
    /// <returns>Wszyscy potomkowie <paramref name="source"/>, którzy pasują do żądanego typu.</returns> 
    /// source:  http://www.hardcodet.net/2009/06/finding-elements-in-wpf-tree-both-ways
    public static IEnumerable<T> FindChildren<T>(this DependencyObject source) where T : DependencyObject
    {
        if (source != null)
        {
            var childs = GetChildObjects(source);
            foreach (DependencyObject child in childs)
            {
                //Analizuj, czy elementy potomne pasują do żądanego typu
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                //rekursywne drzewo 
                foreach (T descendant in FindChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }
    }


    /// <summary> 
    /// Ta metoda jest alternatywą dla metody WPF 
    /// <see cref="VisualTreeHelper.GetChild"/>, która również 
    /// obsługuje elementy treści. Należy pamiętać, że w przypadku elementów treści 
    /// ta metoda odwołuje się do drzewa logicznego elementu. 
    /// </summary> 
    /// <param name="parent">Element do przetworzenia.</param> 
    /// <returns>Elementy podrzędne przesłanego elementu, jeśli są dostępne.</returns>
    /// /// source:  http://www.hardcodet.net/2009/06/finding-elements-in-wpf-tree-both-ways
    public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent)
    {
        if (parent == null) yield break;

        if (parent is ContentElement || parent is FrameworkElement)
        {
            //użyj drzewa logicznego dla elementów zawartości/frameworka
            foreach (object obj in LogicalTreeHelper.GetChildren(parent))
            {
                var depObj = obj as DependencyObject;
                if (depObj != null) yield return (DependencyObject)obj;
            }
        }
        else
        {
            //domyślnie użyj drzewa wizualnego
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                yield return VisualTreeHelper.GetChild(parent, i);
            }
        }
    }

    /// <summary> 
    /// Próbuje zlokalizować dany element w drzewie wizualnym, 
    /// zaczynając od obiektu zależności w danej pozycji. 
    /// </summary> 
    /// <typeparam name="T">Typ elementu, który ma zostać znaleziony 
    /// w drzewie wizualnym elementu w danej lokalizacji.</typeparam> 
    /// <param name="reference">Główny element, który jest używany do przeprowadzania 
    /// testów trafień.</param> 
    /// <param name="point">Pozycja, która ma zostać oceniona w źródle.</param> 
    /// source:  http://www.hardcodet.net/2009/06/finding-elements-in-wpf-tree-both-ways
    public static T TryFindFromPoint<T>(UIElement reference, Point point)
        where T : DependencyObject
    {
        DependencyObject element = reference.InputHitTest(point) as DependencyObject;

        if (element == null) return null;
        else if (element is T) return (T)element;
        else return TryFindParent<T>(element);
    }
}
