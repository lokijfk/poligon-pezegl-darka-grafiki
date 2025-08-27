using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Windows;

namespace poligon_pezeglądarka_grafiki.View.Behaviors;

public class SelectCompositeElement : TriggerAction<DependencyObject>
{
    protected override void Invoke(object parameter)
    {
        Debug.WriteLine("SelectCompositeElement: Invoke called, param: " +parameter.GetType().ToString);
        throw new NotImplementedException();
    }
}
