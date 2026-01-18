
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace poligon_pezeglądarka_grafiki.View;

/// <summary>
/// 
/// </summary>
public partial class ViewWindow : Window //czemu musi być partial ??
{
    /*
     * dodać ogranicnie miejszania bo wywala błędy
     * 
     */
    private Point mousePositionAfterCapture;

    public double CanvasLeft { get; set; } = 30;
    public double CanvasTop { get; set; } = 30;
    public double MyImageHeight { get; set; } = double.NaN;
    public ViewWindow()
    {
        InitializeComponent();
        //CanvasLeft = (WindowView.ActualWidth / 2) + (ImageS.ActualWidth / 2);
        //CanvasTop = (WindowView.ActualHeight / 2) + (ImageS.ActualHeight / 2);
        //MyImageHeight = WindowView.ActualHeight - 20;
        //Debug.WriteLine("CanvasLeft 1: " + CanvasLeft + ", CanvasTop 1: " + CanvasTop);
    }

    private void Image_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        // Debug.WriteLine("Mouse wheel event detected."+ e.Delta+", "+ImageS.Source.ToString());        
        ImageS.Height = ImageS.ActualHeight + e.Delta / 10;
        CanvasLeft = (WindowView.ActualWidth / 2) - (ImageS.ActualWidth / 2);
        CanvasTop = (WindowView.ActualHeight / 2) - (ImageS.ActualHeight / 2);
        //Canvas.SetLeft(ImageS, CanvasLeft);
        //Canvas.SetTop(ImageS, CanvasTop);

        //Debug.WriteLine("CanvasLeft 2: " + CanvasLeft + ", CanvasTop 2: " + CanvasTop);
        //Canvas.SetLeft(ImageS, (WindowView.ActualHeight / 2) - (ImageS.ActualHeight / 2));
        //ImageS.Width = ImageS.ActualWidth + e.Delta / 10;
        //var mv = (this.DataContext as ViewWindowViewModel);
        //mv.SetImage(WindowView.ActualHeight, WindowView.ActualHeight, ImageS.ActualHeight, ImageS.ActualWidth);
        if ((ImageS.ActualHeight > WindowView.ActualHeight) || (ImageS.ActualWidth > WindowView.ActualWidth))
        {
            WindowView.Cursor = Cursors.Hand;
        }
        else
        {
            WindowView.Cursor = Cursors.None;
        }

    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        //ImageS.HorizontalAlignment = HorizontalAlignment.Stretch;
        // ImageS.VerticalAlignment = VerticalAlignment.Stretch;
        if (((ImageS.ActualHeight > WindowView.ActualHeight) || (ImageS.ActualWidth > WindowView.ActualWidth)) && (WindowView.Cursor == Cursors.Hand))
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPossition = e.GetPosition(WindowView);

                if (currentPossition != mousePositionAfterCapture)
                {
                    if (currentPossition.X != mousePositionAfterCapture.X)
                    {
                        Point actual = ImageS.TransformToAncestor(WindowView).Transform(new Point(0, 0));//to dać przy zmianie rozmiaru i zobaczyć co wyjdzie
                        //Debug.WriteLine("przesunięto w x: " + (currentPossition.X - mousePositionAfterCapture.X)+", actual: "+actual.Y+" , "+actual.Y);
                        //ImageS.VerticalAlignment
                        //ImageS.TransformToAncestor(WindowView).Transform(new Point(actual.X+ (currentPossition.X - mousePositionAfterCapture.X), actual.Y+ (currentPossition.Y - mousePositionAfterCapture.Y)));
                        CanvasLeft = (WindowView.ActualWidth / 2) - (ImageS.ActualWidth / 2);

                        Canvas.SetLeft(ImageS, CanvasLeft + (currentPossition.X - mousePositionAfterCapture.X));


                    }
                    if (currentPossition.Y != mousePositionAfterCapture.Y)
                    {
                        CanvasTop = (WindowView.ActualHeight / 2) - (ImageS.ActualHeight / 2);
                        Canvas.SetTop(ImageS, CanvasTop + (currentPossition.Y - mousePositionAfterCapture.Y));
                        //Debug.WriteLine("przesunięto w y: " + (currentPossition.Y - mousePositionAfterCapture.Y));
                    }
                }

            }
        }
    }

    private void Image_LostMouseCapture(object sender, MouseEventArgs e)
    {
        mousePositionAfterCapture = e.GetPosition(WindowView);
        Debug.WriteLine("mousePositionAfterCapture X: " + mousePositionAfterCapture.X + ", Y:" + mousePositionAfterCapture.Y);
    }

    private void Image_Loaded(object sender, RoutedEventArgs e)
    {
        ImageS.Height = WindowView.ActualHeight;
        /*
        if (ImageS.ActualWidth > WindowView.ActualWidth)
        {
            ImageS.Width = WindowView.ActualWidth;
        }
        else if(ImageS.ActualHeight > 300) 
        {
            ImageS.Height = WindowView.ActualHeight;
        }*/

        //CanvasLeft = (WindowView.ActualWidth/2)-(ImageS.ActualWidth/2);
        //CanvasTop = (WindowView.ActualHeight/2)-(ImageS.ActualHeight/2);

        //Canvas.SetLeft(ImageS, CanvasLeft);
        //Canvas.SetTop(ImageS, CanvasTop);
        //Debug.WriteLine("WindowView.ActualWidth: " + WindowView.ActualWidth + ", WindowView.ActualHeight: " 
        //    + WindowView.ActualHeight + ", ImageS.ActualWidth: " + ImageS.ActualWidth + ", ImageS.ActualHeight: " + ImageS.ActualHeight);

        //Debug.WriteLine("CanvasLeft: " + CanvasLeft + ", CanvasTop: " + CanvasTop + ", canvaImage left: "+Canvas.GetLeft(ImageS) + ", canvaImage top: " + Canvas.GetTop(ImageS));

    }

    private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        CanvasLeft = (WindowView.ActualWidth / 2) - (ImageS.ActualWidth / 2);
        CanvasTop = (WindowView.ActualHeight / 2) - (ImageS.ActualHeight / 2);
        Canvas.SetLeft(ImageS, CanvasLeft);
        Canvas.SetTop(ImageS, CanvasTop);
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // tu jako punkt odniesienia może dać okno a nie image to może być inaczej
        // nadal są problemy ale już nie takie
        // nie działa to idealnie ale takie działanie moze eliminować kilka błędów,
        // między innymi przeniesienie całego obrazu poza ekran
        mousePositionAfterCapture = e.GetPosition(WindowView);
    }

    //protected override void OnSourceInitialized(EventArgs e)
    //{
    //    base.OnSourceInitialized(e);
    //}
}
