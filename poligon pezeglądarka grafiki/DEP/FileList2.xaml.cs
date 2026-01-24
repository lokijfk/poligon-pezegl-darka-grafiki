using System.Diagnostics;
using System.Windows.Controls;
using poligon_pezeglądarka_grafiki.ViewModel;

namespace poligon_pezeglądarka_grafiki.View.Control
{
    /// <summary>
    /// Logika interakcji dla klasy FileList2.xaml
    /// </summary>
    public partial class FileList2 : UserControl
    {
        public FileList2()
        {
            // Debug.WriteLine("-- jest: FileList");

            InitializeComponent();
        }

        private void PhotosListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var index = (e.Source as ListBox).SelectedIndex;
            var p = (this.DataContext as MainWindowViewModel)?.Photos[index].Path;
            ViewWindow viewWindow = new ViewWindow { DataContext = new ViewWindowViewModel(p) };
            viewWindow.Show();
            //Debug.WriteLine("MouseDoubleClick:"+sender.ToString()+" event:"+(e.Source as ListBox).SelectedIndex );


        }
    }
}
