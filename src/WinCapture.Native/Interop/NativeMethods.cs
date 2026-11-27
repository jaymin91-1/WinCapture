using System.Runtime.InteropServices;

namespace WinCapture.Native.Interop;

/// <summary>Raw Win32 declarations (LibraryImport). All P/Invoke in the solution lives in this project.</summary>
public static partial class NativeMethods
{
    public const uint WDA_NONE = 0x0;
    public const uint WDA_EXCLUDEFROMCAPTURE = 0x11;

    public const int GWL_EXSTYLE = -20;
    public const long WS_EX_TRANSPARENT = 0x20;
    public const long WS_EX_TOOLWINDOW = 0x80;
    public const long WS_EX_LAYERED = 0x80000;
    public const long WS_EX_NOACTIVATE = 0x08000000;

    public const int WM_NCHITTEST = 0x0084;
    public const int WM_MOUSEACTIVATE = 0x0021;
    public const int HTCLIENT = 1;
    public const int HTTRANSPARENT = -1;
    public const int MA_NOACTIVATE = 3;

    public const int SM_XVIRTUALSCREEN = 76;
    public const int SM_YVIRTUALSCREEN = 77;
    public const int SM_CXVIRTUALSCREEN = 78;
    public const int SM_CYVIRTUALSCREEN = 79;

    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_NOOWNERZORDER = 0x0200;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static partial nint GetWindowLongPtr(IntPtr hwnd, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static partial nint SetWindowLongPtr(IntPtr hwnd, int index, nint newLong);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out POINT point);

    [LibraryImport("user32.dll")]
    public static partial int GetSystemMetrics(int index);

    [LibraryImport("user32.dll")]
    public static partial uint GetDpiForWindow(IntPtr hwnd);

    public const int WM_HOTKEY = 0x0312;
    public const uint MOD_NOREPEAT = 0x4000;
    public static readonly IntPtr HWND_MESSAGE = new(-3);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint virtualKey);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterHotKey(IntPtr hwnd, int id);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(IntPtr hwnd, out RECT rect);

    public const uint MONITOR_DEFAULTTOPRIMARY = 1;
    public const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static unsafe partial bool EnumDisplayMonitors(
        IntPtr hdc, IntPtr clip, delegate* unmanaged<IntPtr, IntPtr, RECT*, IntPtr, int> callback, IntPtr data);

    [LibraryImport("user32.dll")]
    public static partial IntPtr MonitorFromPoint(POINT point, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

    public const uint WM_SETICON = 0x0080;
    public const int ICON_SMALL = 0;
    public const int ICON_BIG = 1;
    public const int SM_CXICON = 11;
    public const int SM_CXSMICON = 49;
    public const uint LR_DEFAULTCOLOR = 0;
    public const uint ICON_RESOURCE_VERSION = 0x00030000;

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial IntPtr CreateIconFromResourceEx(
        byte[] bits, uint size, [MarshalAs(UnmanagedType.Bool)] bool isIcon, uint version, int cx, int cy, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(IntPtr icon);

    [LibraryImport("user32.dll")]
    public static partial int GetSystemMetricsForDpi(int index, uint dpi);

    // Fixed-size window support (control bar, hotkey popup).
    public const int GWL_STYLE = -16;
    public const long WS_THICKFRAME = 0x00040000;
    public const long WS_MAXIMIZEBOX = 0x00010000;
    public const int WM_GETMINMAXINFO = 0x0024;
    public const int WM_SYSCOMMAND = 0x0112;
    public const int WM_NCLBUTTONDBLCLK = 0x00A3;
    public const uint SC_SIZE = 0xF000;
    public const uint SC_MAXIMIZE = 0xF030;
    public const uint MF_BYCOMMAND = 0x0;

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetSystemMenu(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool revert);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteMenu(IntPtr menu, uint position, uint flags);

    // Outermost window-procedure subclass (sees the final WM_NCHITTEST result of every WPF / WPF-UI hook).
    public const int WM_SETCURSOR = 0x0020;
    public const int WM_NCDESTROY = 0x0082;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_FRAMECHANGED = 0x0020;
    public const int IDC_ARROW = 32512;
    public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr SubclassProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, UIntPtr subclassId, UIntPtr refData);

    [LibraryImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowSubclass(IntPtr hwnd, IntPtr subclassProc, UIntPtr subclassId, UIntPtr refData);

    [LibraryImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RemoveWindowSubclass(IntPtr hwnd, IntPtr subclassProc, UIntPtr subclassId);

    [LibraryImport("comctl32.dll")]
    public static partial IntPtr DefSubclassProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "LoadCursorW", SetLastError = true)]
    public static partial IntPtr LoadCursor(IntPtr instance, IntPtr cursorName);

    [LibraryImport("user32.dll")]
    public static partial IntPtr SetCursor(IntPtr cursor);

    [LibraryImport("dwmapi.dll")]
    public static partial int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out RECT value, int size);

    // ---- GDI screen capture ----
    public const uint SRCCOPY = 0x00CC0020;
    public const uint CAPTUREBLT = 0x40000000;
    public const uint DIB_RGB_COLORS = 0;
    public const uint DI_NORMAL = 0x0003;
    public const uint CURSOR_SHOWING = 0x00000001;

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public uint cbSize;
        public uint flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public int fIcon;
        public uint xHotspot;
        public uint yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial IntPtr GetDC(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    public static partial int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    public static partial IntPtr CreateCompatibleDC(IntPtr hdc);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteDC(IntPtr hdc);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    public static partial IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFOHEADER bmi, uint usage, out IntPtr bits, IntPtr section, uint offset);

    [LibraryImport("gdi32.dll")]
    public static partial IntPtr SelectObject(IntPtr hdc, IntPtr obj);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteObject(IntPtr obj);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorInfo(ref CURSORINFO info);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetIconInfo(IntPtr icon, out ICONINFO info);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DrawIconEx(IntPtr hdc, int x, int y, IntPtr icon, int width, int height, uint step, IntPtr brush, uint flags);
}
