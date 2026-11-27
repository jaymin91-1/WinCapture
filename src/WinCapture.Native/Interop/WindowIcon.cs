using WinCapture.Core.Presentation;

namespace WinCapture.Native.Interop;

/// <summary>
/// Sets the small and big window icons with WM_SETICON from the frames of an .ico file. Owns the HICON handles
/// and destroys them on <see cref="Dispose"/>. Call <see cref="Apply"/> again after anything that may clear the icons.
/// </summary>
public sealed class WindowIcon : IDisposable
{
    private readonly byte[] _ico;
    private IntPtr _small;
    private IntPtr _big;

    public WindowIcon(byte[] ico) => _ico = ico;

    /// <summary>Returns null on success, else a short error description (for logging).</summary>
    public string? Apply(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return "window handle is zero";
        }

        var frames = IcoFile.ReadFrames(_ico);
        if (frames.Count == 0)
        {
            return "icon data has no frames";
        }

        var dpi = Math.Max(NativeMethods.GetDpiForWindow(hwnd), 96u);
        var small = Create(frames, NativeMethods.GetSystemMetricsForDpi(NativeMethods.SM_CXSMICON, dpi));
        var big = Create(frames, NativeMethods.GetSystemMetricsForDpi(NativeMethods.SM_CXICON, dpi));
        if (small == IntPtr.Zero || big == IntPtr.Zero)
        {
            if (small != IntPtr.Zero) NativeMethods.DestroyIcon(small);
            if (big != IntPtr.Zero) NativeMethods.DestroyIcon(big);
            return $"CreateIconFromResourceEx failed (Win32 error {System.Runtime.InteropServices.Marshal.GetLastWin32Error()})";
        }

        NativeMethods.SendMessage(hwnd, NativeMethods.WM_SETICON, NativeMethods.ICON_SMALL, small);
        NativeMethods.SendMessage(hwnd, NativeMethods.WM_SETICON, NativeMethods.ICON_BIG, big);

        // The window now uses the new handles; release the previous ones.
        Release();
        _small = small;
        _big = big;
        return null;
    }

    private IntPtr Create(IReadOnlyList<IcoFrame> frames, int size)
    {
        var frame = IcoFile.PickFrame(frames, size > 0 ? size : 32);
        if (frame is null)
        {
            return IntPtr.Zero;
        }

        var bytes = _ico.AsSpan(frame.Offset, frame.Length).ToArray();
        return NativeMethods.CreateIconFromResourceEx(
            bytes, (uint)bytes.Length, true, NativeMethods.ICON_RESOURCE_VERSION, size, size, NativeMethods.LR_DEFAULTCOLOR);
    }

    private void Release()
    {
        if (_small != IntPtr.Zero) NativeMethods.DestroyIcon(_small);
        if (_big != IntPtr.Zero) NativeMethods.DestroyIcon(_big);
        _small = _big = IntPtr.Zero;
    }

    public void Dispose() => Release();
}
