using Application = System.Windows.Application;
using Rect = WinCapture.Core.Geometry.Rect;
using Serilog;
using WinCapture.Core.Geometry;
using WinCapture.Core.Presentation;
using WinCapture.Native.Interop;

namespace WinCapture.App.Toast;

/// <summary>
/// Shows <see cref="ToastWindow"/>s at the bottom-right of the cursor monitor's work area and stacks them upward.
/// All state is touched on the UI thread only.
/// </summary>
public sealed class ToastService(Win32ScreenInfo screenInfo) : IToastService
{
    private const int MarginDip = 16;
    private const int GapDip = 8;

    private readonly List<ToastWindow> _active = [];
    private Rect _workArea; // fixed while at least one toast is visible so the stack stays together

    public void Show(ToastKind kind, string title, string message, Action? onClick = null)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return;
        }

        if (dispatcher.CheckAccess())
        {
            ShowOnUiThread(kind, title, message, onClick);
        }
        else
        {
            dispatcher.BeginInvoke(() => ShowOnUiThread(kind, title, message, onClick));
        }
    }

    private void ShowOnUiThread(ToastKind kind, string title, string message, Action? onClick)
    {
        try
        {
            if (_active.Count == 0)
            {
                _workArea = screenInfo.GetWorkAreaAtCursor() ?? WindowInterop.GetVirtualScreen();
            }

            var toast = new ToastWindow(kind, title, message, onClick);
            toast.ToastClosed += (_, _) =>
            {
                _active.Remove(toast);
                Layout();
            };
            _active.Add(toast);
            toast.Show(); // ShowActivated=false and NOACTIVATE: never steals focus
            Layout();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Showing toast failed");
        }
    }

    // Index 0 is the oldest toast at the bottom; newer ones stack above it.
    private void Layout()
    {
        for (var i = 0; i < _active.Count; i++)
        {
            var hwnd = _active[i].Handle;
            var scale = DpiConverter.ScaleFromDpi(Math.Max(WindowInterop.GetDpi(hwnd), 96u));
            var size = new PixelSize(
                DpiConverter.ToPhysical(ToastWindow.WidthDip, scale),
                DpiConverter.ToPhysical(ToastWindow.HeightDip, scale));
            var pos = ToastLayout.SlotPosition(
                _workArea,
                size,
                DpiConverter.ToPhysical(MarginDip, scale),
                DpiConverter.ToPhysical(GapDip, scale),
                i);
            WindowInterop.SetBounds(hwnd, new Rect(pos.X, pos.Y, size.Width, size.Height));
        }
    }
}
