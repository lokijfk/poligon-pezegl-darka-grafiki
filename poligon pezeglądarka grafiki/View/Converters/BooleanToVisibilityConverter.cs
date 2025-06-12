using System.Windows;

namespace poligon_pezeglądarka_grafiki.View.Converters;

public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
{
    public BooleanToVisibilityConverter() :
        base(Visibility.Visible, Visibility.Collapsed)
    { }
}
