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
}
