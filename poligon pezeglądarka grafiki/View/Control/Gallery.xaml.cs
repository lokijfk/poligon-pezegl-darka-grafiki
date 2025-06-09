using GongSolutions.Wpf.DragDrop.Utilities;
using poligon_pezeglądarka_grafiki.Model;
using poligon_pezeglądarka_grafiki.View.ext;
using poligon_pezeglądarka_grafiki.ViewModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;




namespace poligon_pezeglądarka_grafiki.View.Control;

/// <summary>
/// Logika interakcji dla klasy Gallery.xaml
/// </summary>
public partial class Gallery : UserControl
{

    private decimal znacznik = 0;
    private bool edit = false;
    //private TextBox TBed = null;
    private ListBoxItem? selectedItem = null;

    public Gallery()
    {
        InitializeComponent();
    }

    /*
    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ListBoxItem listBoxItem = sender as ListBoxItem;
        var ph = listBoxItem.DataContext as Photo;        
        var index = (this.DataContext as MainWindowViewModel).Photos.IndexOf(ph);
        var p = (this.DataContext as MainWindowViewModel)?.Photos[index].Path;
        ViewWindow viewWindow = new()
        {
            DataContext = new ViewWindowViewModel(index,
            (this.DataContext as MainWindowViewModel).Photos)
        };
        viewWindow.Show();
    }*/

    private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        //znacznik = 0; //znacznik do zmiany nazwy powoduje problemy
        if (!edit)
        {
            ListBox List = sender as ListBox;
            Photo ph = List.SelectedItem as Photo;

            var y = List.GetListBoxItem(e.GetPosition(List));
            //Debug.WriteLine(x.GetType().ToString()+" : "+y.GetType().ToString());
            if ((ph != null) && (y != null) && (ph == y.DataContext))
            {
                //Photo ph = x as Photo;
                var index = (this.DataContext as MainWindowViewModel).Photos.IndexOf(ph);
                //ViewWindow viewWindow = new() { DataContext = new ViewWindowViewModel((y.DataContext as Photo).Path) };
                ViewWindow viewWindow = new()
                {
                    DataContext = new ViewWindowViewModel(index,
                    (this.DataContext as MainWindowViewModel).Photos)
                };
                viewWindow.Show();
            }
        }
    }

    


    /*
    private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        //dzia\\la ale jest wywoływane za każdym razem przed double klick
        // więc nie może być wykozystane
        Debug.WriteLine("mouse down z");
        ListBoxItem listBoxItem = sender as ListBoxItem;
        Photo photo = listBoxItem.DataContext as Photo;
        string path = photo.Path;
        ListBox listBox = listBoxItem.GetListBox(listBoxItem);
        Photo select = listBox.SelectedItem as Photo;
        if ((select != null) && (photo != null) && (photo == select))
            Debug.WriteLine("jest zgodność !!");
        Debug.WriteLine($"{path}");
    }//*/

    /*
    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //var timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmssffff");
        Debug.WriteLine(" ok - text blok mouse down 1");
        decimal milliseconds = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
        if ((znacznik > 0) && (milliseconds - znacznik >= 1000) && (milliseconds - znacznik <= 3500))
        {   
            edit = true;
            Debug.WriteLine(" ok - text blok mouse down 2");
            TextBlock label = sender as TextBlock;
            ListBoxItem lbi = label.GetParentAsListBoxItem();
            ListBox LB = lbi.GetParentAsListBox();
            if ((lbi != null) && (LB != null))
            {
                Photo photo = lbi.DataContext as Photo;
                Photo select = LB.SelectedItem as Photo;
                if (photo == select)
                {
                    label.Visibility = System.Windows.Visibility.Collapsed;
                    Grid p = label.GetParentAsGrid();//to jest z DependencyObject                
                    if (p != null)
                    {
                        lastIndex = LB.SelectedIndex;
                        LB.SelectedIndex = -1; // to nic nie zmienia
                        //LB.Items[lastIndex].

                        TextBox tb = GetChildrenTB(p);
                        //LB.ClearSelectedItems();
                        tb.Visibility = System.Windows.Visibility.Visible;
                        // tu prawdopodobnie focus jest "kradziony" przez listboxa
                        // jeszcze nie wiem jak to rozwiązać,
                        // może zamiast zmiany na textbox tywoływać coś w rodzaju małego okna z tezboxem i je obcłużyć ??
                        bool test = tb.Focus();
                        Debug.WriteLine("foccus: "+test.ToString());
                        //tb.Select(tb.Text.Length, 0);
                        
                        //tb.CaretIndex = tb.Text.Length;
                        //tb.ScrollToEnd();
                        //tb.Focusable = true;
                        //tb.Focus();
                        //tb.Select(tb.Text.Length,0);
                        //tb.Select(0,tb.Text.Length);
                        
                        tb.SelectAll();

                    }

                }
            }
        }
        else
        {
            znacznik = milliseconds;
            Debug.WriteLine(" ok m: "+milliseconds+" m-z: "+(milliseconds-znacznik).ToString());
        }
    }//*/

    private TextBox GetChildrenTB(DependencyObject obj)
    {
        Grid p = obj as Grid;
        foreach (var x in p.Children)
        {
            if (x is TextBox)
            {
                return x as TextBox;
            }
        }
        return null;
    }


    private TextBlock GetChildrenTBO(DependencyObject obj)
    {
        Grid p = obj as Grid;
        foreach (var x in p.Children)
        {
            if (x is TextBlock)
            {
                return x as TextBlock;
            }
        }
        return null;
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        Debug.WriteLine(" ok - key down text box");
        

        if (e.Key == Key.Enter)
        {
            TextBox textBox = sender as TextBox;
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = GetChildrenTBO(textBox.GetParentAsGrid());
            lb.Visibility = System.Windows.Visibility.Visible;
            edit = false;

            //string oldName = (textBox.DataContext as Photo).Path;
            //Debug.WriteLine("path: " + oldName.Substring(0, oldName.LastIndexOf('\\')+1) );
            //To już powinno być w NM a nie tu !!
            //Photo photo = textBox.DataContext as Photo;
            //Debug.WriteLine("T : DC = " +photo.Path+textBox.Text + " : " +photo.Path+photo.Name);
            if((textBox.DataContext as Photo).Name != textBox.Text)
            (DataContext as MainWindowViewModel).RenameFile(textBox.DataContext as Photo, textBox.Text);
        }else if (e.Key == Key.Escape)
        {
            TextBox textBox = sender as TextBox;
            textBox.Visibility = System.Windows.Visibility.Collapsed;
            TextBlock lb = GetChildrenTBO(textBox.GetParentAsGrid());
            lb.Visibility = System.Windows.Visibility.Visible;
            edit = false;
        }
        //*/
        //TextBox tb = sender as TextBox;
        //tb.IsEnabled = false;
        //edit = false;

    }

 
    private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {        
        Debug.WriteLine(" ok - text blok mouse up 1");
        decimal milliseconds = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
        if ((znacznik > 0) && (milliseconds - znacznik >= 1000) && (milliseconds - znacznik <= 3500))
        {
            edit = true;
            Debug.WriteLine(" ok - text blok mouse up 2");
            TextBlock label = sender as TextBlock;
            ListBoxItem lbi = label.GetParentAsListBoxItem();
            ListBox LB = lbi.GetParentAsListBox();
            if ((lbi != null) && (LB != null))
            {
                Photo photo = lbi.DataContext as Photo;
                Photo select = LB.SelectedItem as Photo;
                if (photo == select)
                {
                    label.Visibility = System.Windows.Visibility.Collapsed;
                    Grid p = label.GetParentAsGrid();//to jest z DependencyObject                
                    if (p != null) ShowTextBox(GetChildrenTB(p));
                }
            }
        }
        
        else
        {
            
            Debug.WriteLine(" ok m: " + milliseconds + " m-z: " + (milliseconds - znacznik).ToString());
            znacznik = milliseconds;
        }//*/
    }

    private void ShowTextBox(TextBox tb)
    {
        //znacznik = 0;
        tb.Visibility = System.Windows.Visibility.Visible;
        //Debug.WriteLine(" ok m: " + milliseconds + " m-z: " + (milliseconds - znacznik).ToString());
        bool test = tb.Focus();
        // tu działa ale albo jest całe zaznaczone albo jest karetka
        //tego nie zmienimy taka jest specyfika tego pola i tak chyba jest w windows
        //tb.CaretIndex = tb.Text.Length;
        //tb.SelectAll();
        tb.Select(tb.Text.Length, 0);//ustawia karetk na końcu tekstu
    }
    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {

        Debug.WriteLine("klik menu: "+ (selectedItem.DataContext as Photo).Name);

    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {

    }

    private void ListBoxItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        //to chyba nie będzie potrzebne lista automatycznie zaznacza element
        // i mo zna go uzyskać jako selecteditem?
        selectedItem = sender as ListBoxItem;
    }

}
