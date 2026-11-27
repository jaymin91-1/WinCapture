namespace WinCapture.Core.Geometry;

/// <summary>Screen information injected into Core (implemented with Win32 in the App/Native layer). Physical pixels.</summary>
public interface IScreenInfo
{
    /// <summary>Full rectangles of all monitors in virtual screen coordinates.</summary>
    IReadOnlyList<Rect> GetMonitors();

    /// <summary>Full rectangle of the primary monitor, or null when unknown.</summary>
    Rect? GetPrimaryMonitor();

    /// <summary>Mouse cursor position in virtual screen coordinates.</summary>
    PixelPoint GetCursorPosition();
}

/// <summary>Pure selection of the monitor used for the full-screen region state.</summary>
public static class MonitorSelector
{
    /// <summary>
    /// Returns the full rectangle of the monitor that contains the cursor ([X, X+W) x [Y, Y+H)).
    /// TEMPORARY fallback (not in SPEC, pending user decision): when no monitor contains the cursor or the list
    /// is empty, the primary monitor is used, else the first monitor of the list, else null.
    /// </summary>
    public static Rect? PickCursorMonitor(IReadOnlyList<Rect> monitors, PixelPoint cursor, Rect? primary = null)
    {
        foreach (var monitor in monitors)
        {
            if (monitor.Contains(cursor))
            {
                return monitor;
            }
        }

        if (primary is { } p)
        {
            return p;
        }

        return monitors.Count > 0 ? monitors[0] : null;
    }
}
