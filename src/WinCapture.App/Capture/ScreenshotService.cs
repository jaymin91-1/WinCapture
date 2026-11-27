using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Serilog;
using WinCapture.Core.Geometry;
using WinCapture.Native.Interop;

namespace WinCapture.App.Capture;

/// <summary>Result of a screenshot attempt: PNG bytes and the captured pixel rectangle, or an error message.</summary>
public sealed record ScreenshotResult(byte[]? Png, Rect Area, string? Error)
{
    public bool Success => Png is not null;
}

/// <summary>Captures a physical-pixel rectangle with BitBlt and encodes it as PNG. Never blocks the calling (UI) thread.</summary>
public static class ScreenshotService
{
    /// <summary>
    /// Captures <paramref name="region"/> (clipped to the virtual screen) on a background thread and returns PNG bytes.
    /// Errors are logged and returned in the result; they are not thrown.
    /// </summary>
    public static Task<ScreenshotResult> CaptureAsync(Rect region, bool includeCursor) =>
        Task.Run(() => Capture(region, includeCursor));

    private static ScreenshotResult Capture(Rect region, bool includeCursor)
    {
        var area = CapturePlan.Clip(region, ScreenCapturer.GetVirtualScreen());
        if (area is not { } clipped)
        {
            const string message = "Capture region is empty or outside the virtual screen.";
            Log.Warning("{Message} Region={Region}", message, region);
            return new ScreenshotResult(null, region, message);
        }

        try
        {
            var captured = ScreenCapturer.Capture(clipped, includeCursor);
            if (captured.Bitmap is not { } bitmap)
            {
                Log.Error("Screen capture failed: {Error}", captured.Error);
                return new ScreenshotResult(null, clipped, captured.Error);
            }

            return new ScreenshotResult(EncodePng(bitmap), clipped, null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Screenshot capture threw for {Area}", clipped);
            return new ScreenshotResult(null, clipped, ex.Message);
        }
    }

    private static byte[] EncodePng(CapturedBitmap bitmap)
    {
        // DPI is fixed at 96 so the PNG's pixel size always equals the captured rectangle size.
        var source = BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Bgr32, null, bitmap.Bgra, bitmap.Width * 4);
        source.Freeze();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
