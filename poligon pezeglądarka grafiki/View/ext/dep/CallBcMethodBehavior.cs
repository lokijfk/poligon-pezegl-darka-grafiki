using Microsoft.Xaml.Behaviors;
using System.Windows;
using System;


namespace poligon_pezeglądarka_grafiki.View.ext;
//klasa pobrana z google podpowiadana przez AI
/// <summary>
/// nie urzywane, ale zostawione na wszelki wypadek, bo może się przydać
/// </summary>
public class CallBcMethodBehavior : Behavior<FrameworkElement>
{
    // Definicja DependencyProperty przechowującej Action (metodę z code-behind)
    public static readonly DependencyProperty MethodActionProperty =
        DependencyProperty.Register("MethodAction", typeof(Action), typeof(CallBcMethodBehavior), new PropertyMetadata(null));

    public Action MethodAction
    {
        get => (Action)GetValue(MethodActionProperty);
        set => SetValue(MethodActionProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= OnLoaded;
        base.OnDetaching();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Wywołanie metody z code-behind, jeśli została przypisana
        MethodAction?.Invoke();
    }
}
