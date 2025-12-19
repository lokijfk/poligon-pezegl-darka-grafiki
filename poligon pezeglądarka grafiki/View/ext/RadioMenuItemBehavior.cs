
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace poligon_pezeglądarka_grafiki.View.ext;

class RadioMenuItemBehavior : Behavior<MenuItem>
{
    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(RadioMenuItemBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(RadioMenuItemBehavior),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    private RadioButton? _radioButton;

    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        _radioButton = new RadioButton
        {
            IsHitTestVisible = false,
            Focusable = false,
            VerticalAlignment = VerticalAlignment.Center,
            GroupName = GroupName
        };

        var binding = new Binding(nameof(IsChecked))
        {
            Source = this,
            Mode = BindingMode.TwoWay
        };

        _radioButton.SetBinding(ToggleButton.IsCheckedProperty, binding);

        AssociatedObject.Icon = _radioButton;
        AssociatedObject.Click += OnMenuItemClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not null)
        {
            AssociatedObject.Click -= OnMenuItemClick;
        }

        _radioButton = null;
    }

    private void OnMenuItemClick(object sender, RoutedEventArgs e)
    {
        IsChecked = true;
    }
}
