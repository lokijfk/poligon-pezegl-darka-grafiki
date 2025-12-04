
using System.Windows.Controls;


namespace poligon_pezeglądarka_grafiki.View.Control;


public partial class SimpleDialog : UserControl
{
    public SimpleDialog()
    {
        InitializeComponent();
    }

    public string WindowName { get; private set; }
    public string Hint { get; private set; }
    public string ReturnName { get; set; }


}
