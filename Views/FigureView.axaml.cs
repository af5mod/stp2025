using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GraphicEditor.ViewModels;

namespace GraphicEditor.Views;

public partial class FigureView : UserControl
{
    private bool _isPressed;
    private Point _pointerOffset;
    private TranslateTransform _transform = null!;
    public static readonly DirectProperty<FigureView, string> GeometryProperty =
        AvaloniaProperty.RegisterDirect<FigureView, string>(
            nameof(Geometry),
            o => o.Geometry,
            (o, v) => o.Geometry = v);

    private string _geometry = "";

    public FigureView()
    {
        InitializeComponent();
    }

    public string Geometry
    {
        get => _geometry;
        set => SetAndRaise(GeometryProperty, ref _geometry, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isPressed = true;

        var FigureVM = DataContext as FigureViewModel;
        var FigureCenter = FigureVM!.Figure.Center;
        var pointerPosition = e.GetPosition((Visual?)Parent);

        _pointerOffset = new(pointerPosition.X - FigureCenter.X, pointerPosition.Y - FigureCenter.Y);


        base.OnPointerPressed(e);
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isPressed = false;
        base.OnPointerReleased(e);
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isPressed)
            return;

        if (Parent == null)
            return;

        var currentPosition = e.GetPosition((Visual?)Parent);
        var FigureVM = DataContext as FigureViewModel;

        var newCenter = new System.Drawing.PointF((float)(currentPosition.X - _pointerOffset.X), (float)(currentPosition.Y - _pointerOffset.Y));
        FigureVM!.Figure.SetPosition(new System.Drawing.PointF((float)newCenter.X, (float)newCenter.Y));

        // FigureVM.CenterX = (float)FigureVM!.Figure.Center.X;
        // FigureVM.CenterY = (float)FigureVM!.Figure.Center.Y;

        base.OnPointerMoved(e);
    }
}
