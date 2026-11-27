using WinCapture.Core.Settings;

namespace WinCapture.Core.Geometry;

/// <summary>Pure region calculations: drag normalization, border rectangle, last-region restore.</summary>
public static class RegionMath
{
    /// <summary>Minimum width and height of a valid region, in physical pixels.</summary>
    public const int MinRegionSize = 32;

    /// <summary>Normalizes a drag from start to end (any direction) into a rectangle with non-negative size.</summary>
    public static Rect Normalize(PixelPoint start, PixelPoint end)
    {
        int x = Math.Min(start.X, end.X);
        int y = Math.Min(start.Y, end.Y);
        return new Rect(x, y, Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
    }

    /// <summary>A region is valid only if both width and height are at least 32 px.</summary>
    public static bool IsValidRegion(Rect r) => r.Width >= MinRegionSize && r.Height >= MinRegionSize;

    /// <summary>
    /// Outer rectangle of the border window: the region grown by thickness on every side,
    /// so the border band lies completely outside the region.
    /// </summary>
    public static Rect GetBorderOuterRect(Rect region, int thickness)
    {
        if (thickness < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(thickness));
        }

        return new Rect(region.X - thickness, region.Y - thickness, region.Width + 2 * thickness, region.Height + 2 * thickness);
    }

    public static Rect FromSettings(RegionSettings s) => new(s.X, s.Y, s.Width, s.Height);

    public static RegionSettings ToSettings(Rect r) => new() { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height };

    /// <summary>
    /// Region to restore at startup, or null when restore is off or nothing is saved
    /// (width or height not positive; ClearRegion saves 0,0,0,0).
    /// The result is not corrected to the virtual screen; see <see cref="ClampToVirtualScreen"/>.
    /// </summary>
    public static Rect? GetRestorableRegion(AppSettings settings)
    {
        if (!settings.RestoreLastRegion)
        {
            return null;
        }

        var r = FromSettings(settings.LastRegion);
        return r.IsEmpty ? null : r;
    }

    /// <summary>
    /// Moves a saved region into the virtual screen bounding rectangle.
    /// Rules: (1) if width/height exceed the virtual screen, shrink to its size;
    /// (2) if the region is not fully inside (partly outside, or no overlap at all), shift it
    /// by the minimum amount so it lies fully inside; (3) a region already inside is unchanged.
    /// The virtual screen is treated as a single bounding rectangle (gaps between monitors are not considered).
    /// </summary>
    public static Rect ClampToVirtualScreen(Rect region, Rect virtualScreen)
    {
        if (virtualScreen.IsEmpty)
        {
            return region;
        }

        int w = Math.Min(region.Width, virtualScreen.Width);
        int h = Math.Min(region.Height, virtualScreen.Height);
        int x = Math.Clamp(region.X, virtualScreen.X, virtualScreen.Right - w);
        int y = Math.Clamp(region.Y, virtualScreen.Y, virtualScreen.Bottom - h);
        return new Rect(x, y, w, h);
    }
}
