using System.Runtime.InteropServices;
using WinCapture.Core.Geometry;

namespace WinCapture.Native.Interop;

/// <summary>
/// <see cref="IScreenInfo"/> backed by Win32 (EnumDisplayMonitors, GetCursorPos). The app is Per-Monitor V2 DPI aware,
/// so all values are physical pixels in virtual screen coordinates.
/// </summary>
public sealed unsafe class Win32ScreenInfo : IScreenInfo
{
    public IReadOnlyList<Rect> GetMonitors()
    {
        var list = new List<Rect>();
        var handle = GCHandle.Alloc(list);
        try
        {
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, &OnMonitor, GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
        }

        return list;
    }

    public Rect? GetPrimaryMonitor()
    {
        var monitor = NativeMethods.MonitorFromPoint(new NativeMethods.POINT { X = 0, Y = 0 }, NativeMethods.MONITOR_DEFAULTTOPRIMARY);
        if (monitor == IntPtr.Zero)
        {
            return null;
        }

        var info = new NativeMethods.MONITORINFO { cbSize = (uint)sizeof(NativeMethods.MONITORINFO) };
        if (!NativeMethods.GetMonitorInfo(monitor, ref info))
        {
            return null;
        }

        return ToRect(info.rcMonitor);
    }

    /// <summary>Work area (excluding the taskbar) of the monitor containing the cursor, nearest monitor when outside; null on failure.</summary>
    public Rect? GetWorkAreaAtCursor()
    {
        var cursor = WindowInterop.GetCursorPosition();
        var monitor = NativeMethods.MonitorFromPoint(new NativeMethods.POINT { X = cursor.X, Y = cursor.Y }, NativeMethods.MONITOR_DEFAULTTONEAREST);
        if (monitor == IntPtr.Zero)
        {
            return null;
        }

        var info = new NativeMethods.MONITORINFO { cbSize = (uint)sizeof(NativeMethods.MONITORINFO) };
        return NativeMethods.GetMonitorInfo(monitor, ref info) ? ToRect(info.rcWork) : null;
    }

    public PixelPoint GetCursorPosition() => WindowInterop.GetCursorPosition();

    [UnmanagedCallersOnly]
    private static int OnMonitor(IntPtr monitor, IntPtr hdc, NativeMethods.RECT* rect, IntPtr data)
    {
        if (GCHandle.FromIntPtr(data).Target is List<Rect> list)
        {
            list.Add(ToRect(*rect));
        }

        return 1; // continue enumeration
    }

    private static Rect ToRect(NativeMethods.RECT r) => new(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
}
