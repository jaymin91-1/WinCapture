using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WinCapture.Core.Geometry;
using WinCapture.Native.Interop;
using CoreRect = WinCapture.Core.Geometry.Rect;

namespace WinCapture.App.Overlay;

/// <summary>
/// Full virtual-screen dim overlay for drag-selecting a region. All geometry math is in Core
/// (<see cref="RegionMath"/>, <see cref="DpiConverter"/>); this class only forwards pointer positions.
/// Needs keyboard input (Esc), so it uses the base class activation exception; capture exclusion stays.
/// </summary>
public sealed class SelectionOverlayWindow : OverlayWindowBase
{
    private readonly CoreRect _virtualScreen;
    private readonly Path _dim = new() { Fill = new SolidColorBrush(Color.FromArgb(0x66, 0, 0, 0)), IsHitTestVisible = false };
    private readonly Path _outline = new() { Stroke = Brushes.White, StrokeThickness = 1, IsHitTestVisible = false };
    private readonly Border _labelBox = new()
    {
        Background = new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0)),
        CornerRadius = new CornerRadius(4),
        Padding = new Thickness(6, 2, 6, 2),
        Visibility = Visibility.Collapsed,
        IsHitTestVisible = false,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
    };
    private readonly TextBlock _label = new() { Foreground = Brushes.White, FontSize = 13 };
    private PixelPoint? _start;
    private bool _completed;

    /// <summary>Raised once: the dragged rectangle (physical pixels), or null when cancelled by Esc.</summary>
    public event EventHandler<CoreRect?>? Completed;

    public SelectionOverlayWindow(CoreRect virtualScreen)
        : base(allowActivation: true)
    {
        _virtualScreen = virtualScreen;
        _labelBox.Child = _label;

        // Near-transparent background so the layered window still receives mouse input everywhere.
        var root = new Grid { Background = new SolidColorBrush(Color.FromArgb(0x01, 0, 0, 0)), Cursor = Cursors.Cross };
        root.Children.Add(_dim);
        root.Children.Add(_outline);
        root.Children.Add(_labelBox);
        Content = root;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        Loaded += (_, _) =>
        {
            ApplyBounds();
            Activate();
            Focus();
            UpdateVisuals(null);
        };
        DpiChanged += (_, _) =>
        {
            ApplyBounds();
            UpdateVisuals(null);
        };
        MouseLeftButtonDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseUp;
        KeyDown += OnKeyDown;
    }

    private double Scale => DpiConverter.ScaleFromDpi(VisualTreeHelper.GetDpi(this).PixelsPerInchX);

    private void ApplyBounds() => WindowInterop.SetBounds(Hwnd, _virtualScreen);

    private PixelPoint ToScreen(Point dipPosition) => new(
        _virtualScreen.X + DpiConverter.ToPhysical(dipPosition.X, Scale),
        _virtualScreen.Y + DpiConverter.ToPhysical(dipPosition.Y, Scale));

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _start = ToScreen(e.GetPosition(this));
        CaptureMouse();
        UpdateVisuals(_start.Value);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_start is not null)
        {
            UpdateVisuals(ToScreen(e.GetPosition(this)));
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_start is not { } start)
        {
            return;
        }

        var rect = RegionMath.Normalize(start, ToScreen(e.GetPosition(this)));
        Finish(rect);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Finish(null);
        }
    }

    private void Finish(CoreRect? result)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        ReleaseMouseCapture();
        Completed?.Invoke(this, result);
    }

    private void UpdateVisuals(PixelPoint? current)
    {
        var scale = Scale;
        var full = new CoreRect(0, 0, _virtualScreen.Width, _virtualScreen.Height);
        var group = new GeometryGroup { FillRule = FillRule.EvenOdd };
        group.Children.Add(new RectangleGeometry(ToDipRect(full, scale)));

        if (_start is { } start && current is { } cur)
        {
            var sel = RegionMath.Normalize(start, cur);
            var local = new CoreRect(sel.X - _virtualScreen.X, sel.Y - _virtualScreen.Y, sel.Width, sel.Height);
            var dip = ToDipRect(local, scale);
            group.Children.Add(new RectangleGeometry(dip)); // hole: original brightness
            _outline.Data = new RectangleGeometry(dip);

            _label.Text = RegionCoordinator.FormatSizeLabel(sel);
            _labelBox.Margin = new Thickness(dip.X, Math.Max(0, dip.Y - 26), 0, 0);
            _labelBox.Visibility = Visibility.Visible;
        }
        else
        {
            _outline.Data = null;
            _labelBox.Visibility = Visibility.Collapsed;
        }

        _dim.Data = group;
    }

    private static System.Windows.Rect ToDipRect(CoreRect r, double scale)
    {
        var d = DpiConverter.ToDip(r, scale);
        return new System.Windows.Rect(d.X, d.Y, d.Width, d.Height);
    }
}
