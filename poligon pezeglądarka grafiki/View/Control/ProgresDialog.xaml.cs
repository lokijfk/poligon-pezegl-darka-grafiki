
using System.Windows.Controls;


namespace poligon_pezeglądarka_grafiki.View.Control;

/// <summary>
/// Logika interakcji dla klasy ProgresDialog.xaml
/// </summary>
public partial class ProgresDialog : UserControl
{
    public ProgresDialog()
    {
        InitializeComponent();
    }

    public string WindowName { get; set; } = "Copy or Move";
    public string Hint { get; set; }

    public string FilesToCopy { get; set; } = "0";
    public string FilesCopy { get; set;} = "0";

    public string ProgressText { get; set; } = string.Empty;
}
