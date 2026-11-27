using WinCapture.Core.Geometry;

namespace WinCapture.Native.Interop;

/// <summary>Raw captured pixels: top-down, 32 bits per pixel BGRX (alpha forced to 255), stride = Width * 4.</summary>
public sealed record CapturedBitmap(int Width, int Height, byte[] Bgra);

/// <summary>Outcome of a capture attempt. Failures are reported here instead of being thrown.</summary>
public sealed record CaptureResult(CapturedBitmap? Bitmap, string? Error)
{
    public bool Success => Bitmap is not null;
}

/// <summary>GDI BitBlt based screen capture in virtual screen coordinates (physical pixels, may be negative).</summary>
public static unsafe class ScreenCapturer
{
    /// <summary>Virtual screen bounds from GetSystemMetrics.</summary>
    public static Rect GetVirtualScreen() => new(
        NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN),
        NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN),
        NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN),
        NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN));

    /// <summary>
    /// Captures <paramref name="area"/> (already clipped to the virtual screen). Synchronous; callers run it off the UI thread.
    /// When <paramref name="includeCursor"/> is true the current cursor image is drawn if it is inside the area.
    /// </summary>
    public static CaptureResult Capture(Rect area, bool includeCursor)
    {
        if (area.IsEmpty)
        {
            return new CaptureResult(null, "Capture area is empty.");
        }

        IntPtr screenDc = IntPtr.Zero;
        IntPtr memDc = IntPtr.Zero;
        IntPtr dib = IntPtr.Zero;
        IntPtr oldObject = IntPtr.Zero;
        try
        {
            screenDc = NativeMethods.GetDC(IntPtr.Zero);
            if (screenDc == IntPtr.Zero)
            {
                return Fail("GetDC failed");
            }

            memDc = NativeMethods.CreateCompatibleDC(screenDc);
            if (memDc == IntPtr.Zero)
            {
                return Fail("CreateCompatibleDC failed");
            }

            var header = new NativeMethods.BITMAPINFOHEADER
            {
                biSize = (uint)sizeof(NativeMethods.BITMAPINFOHEADER),
                biWidth = area.Width,
                biHeight = -area.Height, // negative = top-down
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0, // BI_RGB
            };
            dib = NativeMethods.CreateDIBSection(screenDc, ref header, NativeMethods.DIB_RGB_COLORS, out IntPtr bits, IntPtr.Zero, 0);
            if (dib == IntPtr.Zero || bits == IntPtr.Zero)
            {
                return Fail("CreateDIBSection failed");
            }

            oldObject = NativeMethods.SelectObject(memDc, dib);
            if (!NativeMethods.BitBlt(memDc, 0, 0, area.Width, area.Height, screenDc, area.X, area.Y,
                    NativeMethods.SRCCOPY | NativeMethods.CAPTUREBLT))
            {
                return Fail("BitBlt failed");
            }

            if (includeCursor)
            {
                DrawCursor(memDc, area);
            }

            // BitBlt/DrawIconEx on a DIB section are complete when they return; copy the pixels out.
            var pixels = new byte[checked(area.Width * area.Height * 4)];
            new ReadOnlySpan<byte>((void*)bits, pixels.Length).CopyTo(pixels);
            for (int i = 3; i < pixels.Length; i += 4)
            {
                pixels[i] = 255; // BitBlt leaves alpha undefined
            }

            return new CaptureResult(new CapturedBitmap(area.Width, area.Height, pixels), null);
        }
        finally
        {
            // Release in reverse order of acquisition on every path (success, failure, exception).
            if (memDc != IntPtr.Zero && oldObject != IntPtr.Zero)
            {
                NativeMethods.SelectObject(memDc, oldObject);
            }

            if (dib != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(dib);
            }

            if (memDc != IntPtr.Zero)
            {
                NativeMethods.DeleteDC(memDc);
            }

            if (screenDc != IntPtr.Zero)
            {
                NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
            }
        }
    }

    private static CaptureResult Fail(string what) =>
        new(null, $"{what} (Win32 error {System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");

    /// <summary>Draws the current cursor (default image from GetCursorInfo) at its hotspot position relative to the area.</summary>
    private static void DrawCursor(IntPtr memDc, Rect area)
    {
        var info = new NativeMethods.CURSORINFO { cbSize = (uint)sizeof(NativeMethods.CURSORINFO) };
        if (!NativeMethods.GetCursorInfo(ref info) || (info.flags & NativeMethods.CURSOR_SHOWING) == 0 || info.hCursor == IntPtr.Zero)
        {
            return;
        }

        if (!area.Contains(new PixelPoint(info.ptScreenPos.X, info.ptScreenPos.Y)))
        {
            return;
        }

        int hotX = 0, hotY = 0;
        if (NativeMethods.GetIconInfo(info.hCursor, out var icon))
        {
            try
            {
                hotX = (int)icon.xHotspot;
                hotY = (int)icon.yHotspot;
            }
            finally
            {
                if (icon.hbmMask != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(icon.hbmMask);
                }

                if (icon.hbmColor != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(icon.hbmColor);
                }
            }
        }

        NativeMethods.DrawIconEx(memDc, info.ptScreenPos.X - area.X - hotX, info.ptScreenPos.Y - area.Y - hotY,
            info.hCursor, 0, 0, 0, IntPtr.Zero, NativeMethods.DI_NORMAL);
    }
}
