
using System.Windows.Media;

namespace poligon_pezeglądarka_grafiki.Model;

/// <summary>
/// ta klasa jest tylko pojenikiem na dane o plikach
/// </summary>
public class FilesIO
{
    //public bool Select { get; set; } = false;
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public ImageSource Icon { get; set; }
    public string Size { get; set; } = string.Empty;
    public string RealSize { get; set; } = string.Empty;

    public string File { get; set; } = string.Empty;

    public void Clear()
    {
        //Select  = false;
        Path = string.Empty;
        Name = string.Empty;
        Extension = string.Empty;
        Icon = null;
        Size = string.Empty;
        RealSize = string.Empty;
        File = string.Empty;
    }
}
