using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinCapture.Core.Geometry;
using WinCapture.Native.Interop;

namespace WinCapture.App.Overlay;

/// <summary>Small tooltip near the cursor while dragging the border: "X, Y, W × H" or the lock message.</summary>
public sealed class DragTooltipWindow : OverlayWindowBase
{
    private readonly TextBlock _text = new()
    {
        Foreground = Brushes.White,
        FontSize = 12,
        Margin = new Thickness(8, 4, 8, 4),
    };

    public DragTooltipWindow()
    {
        SizeToContent = SizeToContent.WidthAndHeight;
        IsHitTestVisible = false;
        Content = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xE6, 0x20, 0x20, 0x20)),
            CornerRadius = new CornerRadius(4),
            Child = _text,
        };
    }

    /// <summary>Shows the text with its top-left 16px right/below the given physical-pixel cursor position.</summary>
    public void ShowAt(string text, PixelPoint cursor)
    {
        _text.Text = text;
        if (!IsVisible)
        {
            Show(); // ShowActivated=false and NOACTIVATE: never steals focus
        }

        WindowInterop.MoveTo(Hwnd, cursor.X + 16, cursor.Y + 16);
    }
}
