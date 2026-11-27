using System.Globalization;

namespace WinCapture.Core.Geometry;

/// <summary>Result of hit-testing a point against the border overlay.</summary>
public enum RegionHit
{
    /// <summary>Inside the region: clicks pass through.</summary>
    Passthrough,

    /// <summary>On the border band: drag moves the region.</summary>
    Move,

    /// <summary>On the bottom-right grip: drag resizes the region.</summary>
    Resize,

    /// <summary>Not part of the overlay.</summary>
    Outside,
}

/// <summary>Pure hit-test, move/resize, lock and overlap calculations. All values are physical pixels.</summary>
public static class RegionInteraction
{
    public const int BandThickness = 8;
    public const int GripSize = 12;
    public const int MinSize = 32;

    /// <summary>Tooltip shown instead of changing the region while it is locked (recording).</summary>
    public const string LockedMessage = "녹화 중에는 영역을 변경할 수 없습니다";

    /// <summary>Classifies a point. The grip wins over the band where they overlap.</summary>
    public static RegionHit HitTest(Rect region, PixelPoint p)
    {
        // Grip: from the bottom-right corner, 12px to the right and down.
        if (p.X >= region.Right && p.X < region.Right + GripSize &&
            p.Y >= region.Bottom && p.Y < region.Bottom + GripSize)
        {
            return RegionHit.Resize;
        }

        if (region.Contains(p))
        {
            return RegionHit.Passthrough;
        }

        bool inBand =
            p.X >= region.X - BandThickness && p.X < region.Right + BandThickness &&
            p.Y >= region.Y - BandThickness && p.Y < region.Bottom + BandThickness;
        return inBand ? RegionHit.Move : RegionHit.Outside;
    }

    /// <summary>Moves the region by a drag delta, keeping it inside the virtual screen. Returns start when locked.</summary>
    public static Rect Move(Rect start, int dx, int dy, Rect virtualScreen, bool locked)
    {
        if (locked)
        {
            return start;
        }

        return start with
        {
            X = ClampPosition(start.X + dx, start.Width, virtualScreen.X, virtualScreen.Right),
            Y = ClampPosition(start.Y + dy, start.Height, virtualScreen.Y, virtualScreen.Bottom),
        };
    }

    /// <summary>Resizes by a drag delta with the top-left fixed; min 32x32, stops at the virtual screen edge. Returns start when locked.</summary>
    public static Rect Resize(Rect start, int dw, int dh, Rect virtualScreen, bool locked)
    {
        if (locked)
        {
            return start;
        }

        return start with
        {
            Width = ClampSize(start.Width + dw, start.X, virtualScreen.Right),
            Height = ClampSize(start.Height + dh, start.Y, virtualScreen.Bottom),
        };
    }

    /// <summary>Formats a drag tooltip as "X, Y, W × H" (culture independent).</summary>
    public static string FormatTooltip(Rect r) =>
        string.Create(CultureInfo.InvariantCulture, $"{r.X}, {r.Y}, {r.Width} × {r.Height}");

    private static int ClampPosition(int pos, int size, int min, int max)
    {
        int upper = max - size;
        if (upper < min)
        {
            return min;
        }

        return Math.Clamp(pos, min, upper);
    }

    private static int ClampSize(int size, int pos, int max)
    {
        int upper = Math.Max(MinSize, max - pos);
        return Math.Clamp(size, MinSize, upper);
    }
}
