using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Core;
using poligon_pezeglądarka_grafiki.View.ext;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace poligon_pezeglądarka_grafiki.View.Behaviors;

public class TreeViewItemButtonUp : Behavior<DependencyObject>
{
    public static readonly DependencyProperty EditModeProperty =
        DependencyProperty.Register(nameof(EditMode), typeof(bool), typeof(TreeViewItemButtonUp), new PropertyMetadata(true));
    public bool EditMode
    {
        get => (bool)GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TreeViewItemButtonUp), new PropertyMetadata(null));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    // bardziej potrzeba source, czyli TreeViewItem. prześledzić jak dodać i jakie będą zależności
    // bo może lepiej pobawić siętriggerem zamiast behaviorem

    /*public static readonly DependencyProperty TargetOProperty = DependencyProperty.Register("TargetO", typeof(object), typeof(TreeViewItemButtonUp), new PropertyMetadata(OnTargetOChanged));

    private static void OnTargetOChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //throw new NotImplementedException();
        //CallMethodAction callMethodAction = (CallMethodAction)sender;
        //callMethodAction.UpdateMethodInfo()
        //MethodInfo.Invoke(d, null);
        Debug.WriteLine("TreeViewItemButtonUp: OnTargetObjectChanged "+d.GetType().Name+" , "+e.NewValue.GetType().Name);
    }
    
    public object TargetO
    {
        get { return (object)this.GetValue(TargetOProperty); }
        set { this.SetValue(TargetOProperty, value); }
    }
    //*/
    public object SelectedItem//z datacontext
    {
        get { return (object)GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    //public static MouseButtonEventHandler OnPreviewMouseLeftButtonUp { get; private set; }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewItemButtonUp), new UIPropertyMetadata(null, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var item = e.NewValue as TreeViewItem;
        if (item != null)
        {
            item.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        }
    }



    protected override void OnAttached()
    {
        //Debug.WriteLine("TreeViewItemButtonUp: OnAttached");
        base.OnAttached();
        if(AssociatedObject is TreeView treeView)
        {
            //Debug.WriteLine("TreeViewItemButtonUp: " + AssociatedObject.GetType().Name);
            treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }
        //this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        if ((AssociatedObject != null)&&(AssociatedObject is TreeView))
        {
            
            treeView = AssociatedObject as TreeView;
            treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            //var treeViewItem = treeView.SelectedItem as TreeViewItem;
            //to jest żle, bo tu jest TreeModel a nie treeViewItem

            //Debug.WriteLine("TreeViewItemButtonUp: " + AssociatedObject.GetType().Name);//treeview
            //var treeViewItem = AssociatedObject as TreeViewItem;
            //treeViewItem.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;

        }
    }

    protected override void OnDetaching()
    {
            base.OnDetaching();
           // Debug.WriteLine("TreeViewItemButtonUp: OnDetaching");
            /*
            if ((AssociatedObject != null) && (AssociatedObject is TreeViewItem treeViewItem))
            {
                Debug.WriteLine("TreeViewItemButtonUp: " + AssociatedObject.GetType().Name);
                treeViewItem.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            }
            */
    }
    private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //Debug.WriteLine("TreeViewItemButtonUp: OnPreviewMouseLeftButtonUp");
        //throw new NotImplementedException();
        //hmm tu jest już zaznaczone i to jest w pewnym sensie problem
        if (sender is TreeViewItem treeViewItem)
        {

            var textBox = treeViewItem.GetCHildTextBox();
            if (textBox == null) return; // nie ma TextBoxa, więc nic nie robimy
            
            var textBlock = textBox.GetSisTextBlock();
            if (textBlock == null) return; // nie ma TextBlocka, więc nic nie robimy
            //textBlock.MouseLeftButtonUp.Invoke(textBlock, e);// to nie działa
            //chyba będzie trzeba dodać cały kod z TextBlock_MouseLeftButtonUp
            //Debug.WriteLine("TreeViewItemButtonUp: " + AssociatedObject.GetType().Name);
            /*
            if (EditMode && Command != null && Command.CanExecute(null))
            {
                Command.Execute(null);
            }
            */
            //treeViewItem.IsSelected = true; // to jest problem, bo to już jest zaznaczone
        }
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        //Debug.WriteLine("TreeViewItemButtonUp: OnTreeViewSelectedItemChanged");
        //Debug.WriteLine("e.source: " + e.Source.GetType().Name.ToString() + " orginalSource: " + e.OriginalSource.GetType().Name.ToString());
        if (e.NewValue is TreeViewItem newItem)// to nigdy nie nastąpi
        {
            SelectedItem = newItem;
            //var tvi = SelectedItem as TreeViewItem;
            if (newItem != null)
            {
                //Debug.WriteLine("TVI:"+ newItem.Name);
                newItem.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            }
            
            //e.OriginalSource.GetType().Name.ToString();
        }
        else
        {
            //Debug.WriteLine("new Walue:"+e.NewValue.GetType().Name+" Sender: "+sender.GetType().Name);//treemodel, czyli  to co jest z datacontext
            SelectedItem = null;
        }
    }



}

