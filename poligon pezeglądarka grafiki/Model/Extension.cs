

using CommunityToolkit.Mvvm.ComponentModel;

namespace poligon_pezeglądarka_grafiki.Model;

public partial class Extension : ObservableObject
{
    [ObservableProperty]
    private string name;
    public string Description;
    public string IconPath;
    [ObservableProperty]
    private bool isLinked;
    public Extension(string name, string description, string iconPath)
    {
        Name = name;
        Description = description;
        IconPath = iconPath;
    }

}
