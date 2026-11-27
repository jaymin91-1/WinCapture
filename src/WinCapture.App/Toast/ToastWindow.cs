using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WinCapture.App.Overlay;
using WinCapture.Core.Presentation;

namespace WinCapture.App.Toast;

/// <summary>
/// One toast card. Derives from <see cref="OverlayWindowBase"/> (capture exclusion, NOACTIVATE, TOOLWINDOW, Topmost)
/// without allowActivation: WM_MOUSEACTIVATE returns MA_NOACTIVATE, so clicks are still delivered to WPF
/// while the foreground window keeps the focus.
/// </summary>
public sealed class ToastWindow : OverlayWindowBase
{
    public const double WidthDip = 360;
    public const double HeightDip = 80;
    public static readonly TimeSpan DisplayTime = TimeSpan.FromSeconds(5);

    private static readonly Brush TextBrush = Frozen("#191F28");
    private static readonly Brush SubTextBrush = Frozen("#6B7684");

    private readonly Action? _onClick;
    private readonly DispatcherTimer _timer = new() { Interval = DisplayTime };
    private bool _closed;

    public ToastWindow(ToastKind kind, string title, string message, Action? onClick)
    {
        _onClick = onClick;
        Width = WidthDip;
        Height = HeightDip;
        Cursor = onClick is null ? Cursors.Arrow : Cursors.Hand;

        var accent = Frozen(kind switch
        {
            ToastKind.Success => "#2F7CF6",
            ToastKind.Warning => "#FF7A85",
            _ => "#F04452",
        });

        var texts = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        texts.Children.Add(new TextBlock
        {
            Text = ToastLayout.Truncate(title, ToastLayout.MaxTitleLength),
            Foreground = TextBrush,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });
        texts.Children.Add(new TextBlock
        {
            Text = ToastLayout.Truncate(message, ToastLayout.MaxMessageLength),
            Foreground = SubTextBrush,
            FontSize = 12,
            Margin = new Thickness(0, 2, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 34,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });

        var dot = new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            Background = accent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 6, 10, 0),
        };

        var row = new DockPanel { Margin = new Thickness(16, 10, 16, 10) };
        DockPanel.SetDock(dot, Dock.Left);
        row.Children.Add(dot);
        row.Children.Add(texts);

        Content = new Border
        {
            Background = Brushes.White,
            BorderBrush = Frozen("#E5E8EB"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Child = row,
        };

        MouseLeftButtonUp += (_, _) => Click();
        // Hovering keeps the toast open.
        MouseEnter += (_, _) => _timer.Stop();
        MouseLeave += (_, _) => RestartTimer();
        _timer.Tick += (_, _) => CloseToast();
        Loaded += (_, _) => RestartTimer();
    }

    /// <summary>Raised once when the toast has closed (timeout or click).</summary>
    public event EventHandler? ToastClosed;

    /// <summary>Native handle (creates it if needed) for positioning by the service.</summary>
    internal IntPtr Handle => Hwnd;

    private void RestartTimer()
    {
        _timer.Stop();
        _timer.Start();
    }

    // Runs the click action first, then closes the toast.
    private void Click()
    {
        if (_closed)
        {
            return;
        }

        try
        {
            _onClick?.Invoke();
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Toast click action failed");
        }

        CloseToast();
    }

    private void CloseToast()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;
        _timer.Stop();
        Close();
        ToastClosed?.Invoke(this, EventArgs.Empty);
    }

    private static Brush Frozen(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }
}
