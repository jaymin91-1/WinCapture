using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Serilog;
using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;
using WinCapture.Native.Interop;
using CoreRect = WinCapture.Core.Geometry.Rect;

namespace WinCapture.App.Overlay;

/// <summary>
/// Red border drawn OUTSIDE the capture region. Placement uses Core calculations (physical pixels).
/// Region interior is click-through (HTTRANSPARENT); band and grip are the only input-receiving parts.
/// The window covers the whole 8px hit band (painted at alpha 1 so the layered window receives input there).
/// Dragging the band moves the region, dragging the grip resizes it; all calculations are Core calls.
/// </summary>
public sealed class BorderWindow : OverlayWindowBase
{
    private const string FallbackColor = "#FF3B30";

    private readonly BorderVisual _visual = new();
    private CoreRect _region;
    private CoreRect _windowRect;
    private int _thickness = 2;
    private Brush _brush = Brushes.Red;
    private readonly RegionCoordinator? _coordinator;
    private readonly DragTooltipWindow _tooltip = new();
    private RegionDrag? _drag;

    public BorderWindow(RegionCoordinator? coordinator = null)
    {
        _coordinator = coordinator;
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

        _windowRect = BorderLayout.GetInputWindowRect(_region, _thickness);
        WindowInterop.SetBounds(Hwnd, _windowRect);

        var outer = RegionMath.GetBorderOuterRect(_region, _thickness);
        _visual.Update(
            ToLocal(_region),
            ToLocal(outer),
            ToLocal(BorderLayout.GetGripRect(_region)),
            ToLocal(BorderLayout.GetBandRect(_region)),
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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var cursor = WindowInterop.GetCursorPosition();

        if (_drag is { } drag && _coordinator is { } coordinator)
        {
            var state = coordinator.State;
            var next = drag.Compute(cursor, WindowInterop.GetVirtualScreen(), state.IsLocked);
            if (state.IsLocked)
            {
                // Locked (recording): never change the region, only explain why.
                _tooltip.ShowAt(RegionInteraction.LockedMessage, cursor);
                return;
            }

            coordinator.UpdateRegion(next, persist: false);
            _tooltip.ShowAt(RegionInteraction.FormatTooltip(next), cursor);
            return;
        }

        Cursor = RegionInteraction.HitTest(_region, cursor) switch
        {
            RegionHit.Resize => Cursors.SizeNWSE,
            RegionHit.Move => Cursors.SizeAll,
            _ => Cursors.Arrow,
        };
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var cursor = WindowInterop.GetCursorPosition();
        var hit = RegionInteraction.HitTest(_region, cursor);
        if (hit is not (RegionHit.Move or RegionHit.Resize) || _coordinator is null)
        {
            return;
        }

        _drag = new RegionDrag(hit, _region, cursor);
        CaptureMouse();
        if (_coordinator.State.IsLocked)
        {
            _tooltip.ShowAt(RegionInteraction.LockedMessage, cursor);
        }
        else
        {
            _tooltip.ShowAt(RegionInteraction.FormatTooltip(_region), cursor);
        }

        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        EndDrag(persist: true);
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        EndDrag(persist: true);
    }

    private void EndDrag(bool persist)
    {
        if (_drag is null)
        {
            return;
        }

        _drag = null;
        ReleaseMouseCapture();
        _tooltip.Hide();
        if (persist && _coordinator?.State.Region is { } final)
        {
            _coordinator.UpdateRegion(final, persist: true); // save lastRegion once, at the end
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _tooltip.Close();
        base.OnClosed(e);
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
