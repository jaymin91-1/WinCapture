namespace WinCapture.Core.Geometry;

/// <summary>Normalizes a requested capture rectangle against the virtual screen (physical pixels).</summary>
public static class CapturePlan
{
    /// <summary>
    /// Returns the part of <paramref name="region"/> that lies inside <paramref name="virtualScreen"/>,
    /// or null when the region is empty or completely outside. Odd sizes are kept as they are.
    /// </summary>
    public static Rect? Clip(Rect region, Rect virtualScreen)
    {
        if (region.IsEmpty || virtualScreen.IsEmpty)
        {
            return null;
        }

        int left = Math.Max(region.X, virtualScreen.X);
        int top = Math.Max(region.Y, virtualScreen.Y);
        int right = Math.Min(region.Right, virtualScreen.Right);
        int bottom = Math.Min(region.Bottom, virtualScreen.Bottom);
        if (right <= left || bottom <= top)
        {
            return null;
        }

        return new Rect(left, top, right - left, bottom - top);
    }
}
