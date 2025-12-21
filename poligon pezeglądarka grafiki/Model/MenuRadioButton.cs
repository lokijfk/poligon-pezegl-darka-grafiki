

using CommunityToolkit.Mvvm.ComponentModel;

namespace poligon_pezeglądarka_grafiki.Model;

public partial class MenuRadioButton : ObservableObject
{
    [ObservableProperty]
    private string _Name;

    [ObservableProperty]
    private bool _IsChecked;
    public string Grupa;

    public MenuRadioButton(string Name, bool IsChecekd, string GR)
    {
        this.Name = Name;
        this.IsChecked = IsChecekd;        
        this.Grupa = GR;
    }
}
