using System.Windows;
using System.Windows.Media;
using Serilog;
using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;
using WinCapture.Native.Interop;
using CoreRect = WinCapture.Core.Geometry.Rect;

namespace WinCapture.App.Overlay;

/// <summary>
/// Red border drawn OUTSIDE the capture region. Placement uses Core calculations (physical pixels).
/// Region interior is click-through (HTTRANSPARENT); band and grip are the only input-receiving parts
/// (their drag handling is added in a later task).
/// </summary>
public sealed class BorderWindow : OverlayWindowBase
{
    private const string FallbackColor = "#FF3B30";

    private readonly BorderVisual _visual = new();
    private CoreRect _region;
    private CoreRect _windowRect;
    private int _thickness = 2;
    private Brush _brush = Brushes.Red;

    public BorderWindow()
    {
        Content = _visual;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
    }

    /// <summary>Sets the region (physical pixels) and border look, then positions the window.</summary>
    public void SetRegion(CoreRect region, BorderSettings border)
    {
        _region = region;
        _thickness = Math.Max(0, border.Thickness);
        _brush = ParseBrush(border.Color);
        Apply();
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);

        // WPF resizes the window for the new DPI; restore the exact physical bounds.
        Apply();
    }

    private void Apply()
    {
        if (_region.IsEmpty)
        {
            return;
        }

        _windowRect = BorderLayout.GetWindowRect(_region, _thickness);
        WindowInterop.SetBounds(Hwnd, _windowRect);

        var outer = RegionMath.GetBorderOuterRect(_region, _thickness);
        _visual.Update(
            ToLocal(_region),
            ToLocal(outer),
            ToLocal(BorderLayout.GetGripRect(_region)),
            VisualTreeHelper.GetDpi(this).DpiScaleX,
            _brush);
    }

    private CoreRect ToLocal(CoreRect r) => new(r.X - _windowRect.X, r.Y - _windowRect.Y, r.Width, r.Height);

    protected override bool TryHitTest(PixelPoint screenPoint, out int hitResult)
    {
        // Interior and anything not on the band/grip pass clicks to the window below.
        hitResult = RegionInteraction.HitTest(_region, screenPoint) switch
        {
            RegionHit.Move or RegionHit.Resize => NativeMethods.HTCLIENT,
            _ => NativeMethods.HTTRANSPARENT,
        };
        return true;
    }

    private static Brush ParseBrush(string color)
    {
        Color parsed;
        try
        {
            parsed = (Color)ColorConverter.ConvertFromString(color);
        }
        catch (Exception ex) when (ex is FormatException or NotSupportedException)
        {
            Log.Warning(ex, "Invalid border color '{Color}', using {Fallback}", color, FallbackColor);
            parsed = (Color)ColorConverter.ConvertFromString(FallbackColor);
        }

        var brush = new SolidColorBrush(parsed);
        brush.Freeze();
        return brush;
    }
}
