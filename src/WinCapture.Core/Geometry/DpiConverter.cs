namespace WinCapture.Core.Geometry;

/// <summary>
/// The single place where WPF DIP and physical pixels are converted (TECH-RULES 4).
/// Each of X, Y, Width, Height is scaled and rounded independently with MidpointRounding.AwayFromZero.
/// </summary>
public static class DpiConverter
{
    /// <summary>Scale for a DPI value, e.g. 144 dpi -> 1.5.</summary>
    public static double ScaleFromDpi(double dpi) => dpi / 96.0;

    public static int ToPhysical(double dip, double scale) =>
        (int)Math.Round(dip * scale, MidpointRounding.AwayFromZero);

    public static double ToDip(int physical, double scale) => physical / scale;

    public static Rect ToPhysical(DipRect r, double scale)
    {
        ValidateScale(scale);
        return new Rect(
            ToPhysical(r.X, scale),
            ToPhysical(r.Y, scale),
            ToPhysical(r.Width, scale),
            ToPhysical(r.Height, scale));
    }

    public static DipRect ToDip(Rect r, double scale)
    {
        ValidateScale(scale);
        return new DipRect(ToDip(r.X, scale), ToDip(r.Y, scale), ToDip(r.Width, scale), ToDip(r.Height, scale));
    }

    private static void ValidateScale(double scale)
    {
        if (!(scale > 0))
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be positive.");
        }
    }
}
