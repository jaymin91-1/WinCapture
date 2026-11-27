using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Serilog;
using WinCapture.Core.Geometry;
using WinCapture.Native.Interop;

namespace WinCapture.App.Overlay;

/// <summary>
/// Base class of every overlay window (border, selection overlay, timer pill, drag tooltip).
/// This is the ONLY place that applies capture exclusion (WDA_EXCLUDEFROMCAPTURE) and the overlay
/// window styles (LAYERED | TOOLWINDOW | NOACTIVATE, Topmost). Subclasses must not set them again.
/// </summary>
public abstract class OverlayWindowBase : Window
{
    private readonly bool _allowActivation;

    /// <param name="allowActivation">
    /// Exception for windows that must receive keyboard input (the selection overlay needs Esc).
    /// When true, WS_EX_NOACTIVATE is not applied and the window can be activated. Capture exclusion,
    /// LAYERED, TOOLWINDOW and Topmost are always applied. Default false (never takes focus).
    /// </param>
    protected OverlayWindowBase(bool allowActivation = false)
    {
        _allowActivation = allowActivation;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        ShowActivated = allowActivation;
        Topmost = true;
        Focusable = allowActivation;

        SourceInitialized += OnSourceInitialized;
    }

    /// <summary>Native window handle; creates it if needed (fires SourceInitialized).</summary>
    protected IntPtr Hwnd => new WindowInteropHelper(this).EnsureHandle();

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;

        // Capture exclusion, right after the handle exists and before the window is ever shown.
        if (!WindowInterop.ExcludeFromCapture(hwnd))
        {
            Log.Error(
                "SetWindowDisplayAffinity(WDA_EXCLUDEFROMCAPTURE) failed for {Window} (Win32 error {Error})",
                GetType().Name,
                Marshal.GetLastPInvokeError());
        }

        WindowInterop.AddExtendedStyles(
            hwnd,
            NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TOOLWINDOW |
            (_allowActivation ? 0 : NativeMethods.WS_EX_NOACTIVATE));

        HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
    }

    /// <summary>
    /// Subclass hook for WM_NCHITTEST. Screen point is in physical pixels (virtual screen coordinates).
    /// Return true and set <paramref name="hitResult"/> (HTCLIENT / HTTRANSPARENT ...) to handle it.
    /// </summary>
    protected virtual bool TryHitTest(PixelPoint screenPoint, out int hitResult)
    {
        hitResult = 0;
        return false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case NativeMethods.WM_MOUSEACTIVATE when !_allowActivation:
                // Never take focus when clicked.
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);

            case NativeMethods.WM_NCHITTEST:
                long l = lParam.ToInt64();
                var point = new PixelPoint((short)(l & 0xFFFF), (short)((l >> 16) & 0xFFFF));
                if (TryHitTest(point, out int result))
                {
                    handled = true;
                    return new IntPtr(result);
                }

                break;
        }

        return IntPtr.Zero;
    }
}
