using WinCapture.Core.Geometry;

namespace WinCapture.Native.Interop;

/// <summary>Thin helpers over <see cref="NativeMethods"/> returning Core types (physical pixels).</summary>
public static class WindowInterop
{
    /// <summary>Returns false on failure; the Win32 error is available via Marshal.GetLastPInvokeError.</summary>
    public static bool ExcludeFromCapture(IntPtr hwnd) =>
        NativeMethods.SetWindowDisplayAffinity(hwnd, NativeMethods.WDA_EXCLUDEFROMCAPTURE);

    public static void AddExtendedStyles(IntPtr hwnd, long styles)
    {
        long current = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, (nint)(current | styles));
    }

    /// <summary>Moves/resizes in physical pixels, topmost, without activation.</summary>
    public static bool SetBounds(IntPtr hwnd, Rect bounds) =>
        NativeMethods.SetWindowPos(
            hwnd, NativeMethods.HWND_TOPMOST, bounds.X, bounds.Y, bounds.Width, bounds.Height,
            NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOOWNERZORDER);

    /// <summary>Moves only (keeps size), physical pixels, topmost, without activation.</summary>
    public static bool MoveTo(IntPtr hwnd, int x, int y) =>
        NativeMethods.SetWindowPos(
            hwnd, NativeMethods.HWND_TOPMOST, x, y, 0, 0,
            NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOOWNERZORDER);

    /// <summary>Cursor position in virtual screen physical pixels.</summary>
    public static PixelPoint GetCursorPosition() =>
        NativeMethods.GetCursorPos(out var p) ? new PixelPoint(p.X, p.Y) : new PixelPoint(0, 0);

    /// <summary>Bounding rectangle of all monitors in virtual screen coordinates.</summary>
    public static Rect GetVirtualScreen() => new(
        NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN),
        NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN),
        NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN),
        NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN));

    public static uint GetDpi(IntPtr hwnd) => NativeMethods.GetDpiForWindow(hwnd);

    /// <summary>Registers a global hotkey (RegisterHotKey only, no keyboard hook). Returns the Win32 error code, 0 on success.</summary>
    public static int RegisterHotkey(IntPtr hwnd, int id, uint modifiers, uint virtualKey) =>
        NativeMethods.RegisterHotKey(hwnd, id, modifiers | NativeMethods.MOD_NOREPEAT, virtualKey)
            ? 0
            : System.Runtime.InteropServices.Marshal.GetLastPInvokeError();

    public static void UnregisterHotkey(IntPtr hwnd, int id) => NativeMethods.UnregisterHotKey(hwnd, id);

    /// <summary>Window bounds in virtual screen physical pixels, or null on failure.</summary>
    public static Rect? GetWindowBounds(IntPtr hwnd) =>
        NativeMethods.GetWindowRect(hwnd, out var r) ? new Rect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top) : null;
}
