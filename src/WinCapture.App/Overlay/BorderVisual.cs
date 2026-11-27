using System.Windows;
using System.Windows.Media;
using WinCapture.Core.Geometry;
using CoreRect = WinCapture.Core.Geometry.Rect;

namespace WinCapture.App.Overlay;

/// <summary>
/// Draws the border ring and the bottom-right grip triangle. Input rectangles are physical pixels
/// relative to the window origin; conversion to DIP goes through <see cref="DpiConverter"/>.
/// </summary>
public sealed class BorderVisual : FrameworkElement
{
    private CoreRect _regionLocal;
    private CoreRect _outerLocal;
    private CoreRect _gripLocal;
    private CoreRect _bandLocal;
    private double _scale = 1.0;
    private Brush _brush = Brushes.Red;

    private static readonly Brush HitBrush = CreateHitBrush();

    private static Brush CreateHitBrush()
    {
        var b = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        b.Freeze();
        return b;
    }

    public void Update(CoreRect regionLocal, CoreRect outerLocal, CoreRect gripLocal, CoreRect bandLocal, double scale, Brush brush)
    {
        _regionLocal = regionLocal;
        _outerLocal = outerLocal;
        _gripLocal = gripLocal;
        _bandLocal = bandLocal;
        _scale = scale;
        _brush = brush;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_regionLocal.IsEmpty)
        {
            return;
        }

        var outer = ToWpf(DpiConverter.ToDip(_outerLocal, _scale));
        var inner = ToWpf(DpiConverter.ToDip(_regionLocal, _scale));

        // Hit band: almost invisible (alpha 1) so the layered window receives mouse input there
        // (fully transparent pixels would be click-through). Outside the region only; capture-excluded anyway.
        var band = ToWpf(DpiConverter.ToDip(_bandLocal, _scale));
        var bandRing = new CombinedGeometry(
            GeometryCombineMode.Exclude, new RectangleGeometry(band), new RectangleGeometry(inner));
        dc.DrawGeometry(HitBrush, null, bandRing);

        // Ring = outer rectangle minus the region, so no region pixel is painted.
        var ring = new CombinedGeometry(
            GeometryCombineMode.Exclude, new RectangleGeometry(outer), new RectangleGeometry(inner));
        dc.DrawGeometry(_brush, null, ring);

        // Grip triangle inside the 12x12 grip square (outside the region).
        var grip = ToWpf(DpiConverter.ToDip(_gripLocal, _scale));
        var triangle = new StreamGeometry();
        using (var ctx = triangle.Open())
        {
            ctx.BeginFigure(new Point(grip.Right, grip.Top), isFilled: true, isClosed: true);
            ctx.LineTo(new Point(grip.Right, grip.Bottom), isStroked: false, isSmoothJoin: false);
            ctx.LineTo(new Point(grip.Left, grip.Bottom), isStroked: false, isSmoothJoin: false);
        }

        triangle.Freeze();
        dc.DrawGeometry(_brush, null, triangle);
    }

    private static System.Windows.Rect ToWpf(DipRect r) => new(r.X, r.Y, r.Width, r.Height);
}
