namespace WinCapture.Core.Geometry;

/// <summary>Layout of the border overlay window for a region. All values are physical pixels.</summary>
public static class BorderLayout
{
    /// <summary>
    /// Window rectangle: the border outer rectangle (region grown by thickness), extended to the right and
    /// bottom so the 12x12 grip, which starts at the region's bottom-right corner, fits inside the window.
    /// Nothing in this rectangle except the grip and the band lies inside the region.
    /// </summary>
    public static Rect GetWindowRect(Rect region, int thickness)
    {
        var outer = RegionMath.GetBorderOuterRect(region, thickness);
        int right = Math.Max(outer.Right, region.Right + RegionInteraction.GripSize);
        int bottom = Math.Max(outer.Bottom, region.Bottom + RegionInteraction.GripSize);
        return new Rect(outer.X, outer.Y, right - outer.X, bottom - outer.Y);
    }

    /// <summary>Grip rectangle: 12x12 starting at the region's bottom-right corner, outside the region.</summary>
    public static Rect GetGripRect(Rect region) =>
        new(region.Right, region.Bottom, RegionInteraction.GripSize, RegionInteraction.GripSize);

    /// <summary>
    /// Window rectangle of the interactive border window: covers the whole 8px hit band around the region
    /// (and the border when it is thicker) plus the grip. Only the band, the border and the grip are painted
    /// or hit-testable; the region interior stays click-through (HTTRANSPARENT).
    /// </summary>
    public static Rect GetInputWindowRect(Rect region, int thickness)
    {
        var window = GetWindowRect(region, thickness);
        int pad = RegionInteraction.BandThickness;
        int x = Math.Min(window.X, region.X - pad);
        int y = Math.Min(window.Y, region.Y - pad);
        int right = Math.Max(window.Right, region.Right + pad);
        int bottom = Math.Max(window.Bottom, region.Bottom + pad);
        return new Rect(x, y, right - x, bottom - y);
    }

    /// <summary>The hit band's outer rectangle: the region grown by 8px on every side.</summary>
    public static Rect GetBandRect(Rect region)
    {
        int pad = RegionInteraction.BandThickness;
        return new Rect(region.X - pad, region.Y - pad, region.Width + 2 * pad, region.Height + 2 * pad);
    }
}
