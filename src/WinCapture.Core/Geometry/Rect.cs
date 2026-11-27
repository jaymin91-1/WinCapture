namespace WinCapture.Core.Geometry;

/// <summary>Integer point in physical pixels (virtual screen coordinates; may be negative).</summary>
public readonly record struct PixelPoint(int X, int Y);

/// <summary>Integer size in physical pixels.</summary>
public readonly record struct PixelSize(int Width, int Height);

/// <summary>Integer rectangle in physical pixels (virtual screen coordinates; X/Y may be negative).</summary>
public readonly record struct Rect(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;

    public int Bottom => Y + Height;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Contains(PixelPoint p) => p.X >= X && p.X < Right && p.Y >= Y && p.Y < Bottom;

    /// <summary>True when the two rectangles share at least one pixel.</summary>
    public bool Intersects(Rect other) =>
        !IsEmpty && !other.IsEmpty &&
        X < other.Right && other.X < Right && Y < other.Bottom && other.Y < Bottom;
}

/// <summary>Rectangle in WPF device-independent units (1/96 inch). Only used at the DPI conversion boundary.</summary>
public readonly record struct DipRect(double X, double Y, double Width, double Height);
