using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using Serilog;
using WinCapture.Core.Presentation;
using WinCapture.Native.Interop;

namespace WinCapture.App;

/// <summary>
/// Makes a FluentWindow impossible to resize or maximize (control bar, hotkey popup). Several independent layers,
/// because WPF-UI's TitleBar returns HTLEFT/HTBOTTOMRIGHT/... from WM_NCHITTEST from the WindowChrome
/// ResizeBorderThickness regardless of Window.ResizeMode:
///  1. WindowChrome.ResizeBorderThickness = 0 (no resize band is computed at all).
///  2. WS_THICKFRAME and WS_MAXIMIZEBOX removed (no sizing border, no Aero Snap, no Win+Up).
///  3. WM_NCHITTEST: points in the window's outer band are answered HTCLIENT.
///  4. WM_SYSCOMMAND SC_SIZE / SC_MAXIMIZE swallowed; Size and Maximize deleted from the system menu (Alt+Space).
///  5. WM_NCLBUTTONDBLCLK on the caption swallowed (no double-click maximize).
///  6. WM_GETMINMAXINFO pins min = max = current size.
///  7. WindowState is forced back to Normal if it ever becomes Maximized.
/// Minimize, hide and moving by the title bar are untouched.
/// </summary>
internal static class FixedSizeWindow
{
    private const int EdgeBandDips = 6;

    /// <summary>Call from the window constructor, after InitializeComponent.</summary>
    public static void Attach(Window window)
    {
        HwndSourceHook? hook = null;
        HwndSource? source = null;

        void Apply()
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero)
                {
                    return;
                }

                // Layer 1: no resize band in the chrome (FluentWindow's ResizeBorderThickness is non-zero by default).
                if (WindowChrome.GetWindowChrome(window) is { } chrome)
                {
                    chrome.ResizeBorderThickness = new Thickness(0);
                }

                // Layers 2 and 4.
                WindowInterop.RemoveSizingStyles(hwnd);
                WindowInterop.RemoveSizeMenuItems(hwnd);

                // The hook must run before WPF-UI's TitleBar hook (hooks added later run first), so it is
                // (re)registered after the window has loaded as well.
                var newSource = HwndSource.FromHwnd(hwnd);
                if (newSource is null)
                {
                    return;
                }

                hook ??= (IntPtr h, int msg, IntPtr w, IntPtr l, ref bool handled) => WndProc(window, h, msg, w, l, ref handled);
                source?.RemoveHook(hook);
                newSource.AddHook(hook);
                source = newSource;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Applying the fixed-size lock failed for {Window}", window.GetType().Name);
            }
        }

        window.SourceInitialized += (_, _) => Apply();
        window.ContentRendered += (_, _) => Apply();

        // Layer 7.
        window.StateChanged += (_, _) =>
        {
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
        };
    }

    private static IntPtr WndProc(Window window, IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case NativeMethods.WM_NCHITTEST:
            {
                var l = lParam.ToInt64();
                var x = (short)(l & 0xFFFF);
                var y = (short)((l >> 16) & 0xFFFF);
                if (WindowInterop.GetWindowBounds(hwnd) is { } r)
                {
                    var band = FixedSizeWindowPolicy.ToPixelsCeiling(EdgeBandDips, WindowInterop.GetDpi(hwnd));
                    if (FixedSizeWindowPolicy.IsInEdgeBand(x, y, r.X, r.Y, r.X + r.Width, r.Y + r.Height, band))
                    {
                        handled = true;
                        return NativeMethods.HTCLIENT;
                    }
                }

                break;
            }

            case NativeMethods.WM_SYSCOMMAND:
                if (FixedSizeWindowPolicy.IsBlockedSysCommand(wParam.ToInt64()))
                {
                    handled = true;
                    return IntPtr.Zero;
                }

                break;

            case NativeMethods.WM_NCLBUTTONDBLCLK:
                if (FixedSizeWindowPolicy.IsBlockedDoubleClick((int)wParam.ToInt64()))
                {
                    handled = true;
                    return IntPtr.Zero;
                }

                break;

            case NativeMethods.WM_GETMINMAXINFO:
                // ActualWidth/Height are DIPs of the content-sized window; size in pixels at the window's current DPI.
                if (window.ActualWidth > 0 && window.ActualHeight > 0)
                {
                    var dpi = WindowInterop.GetDpi(hwnd);
                    WindowInterop.PinMinMaxInfo(
                        lParam,
                        FixedSizeWindowPolicy.ToPixelsCeiling(window.ActualWidth, dpi),
                        FixedSizeWindowPolicy.ToPixelsCeiling(window.ActualHeight, dpi));
                    // Handled: Window's own WM_GETMINMAXINFO processing (MinWidth/MaxWidth) must not overwrite these values.
                    handled = true;
                    return IntPtr.Zero;
                }

                break;
        }

        return IntPtr.Zero;
    }
}
