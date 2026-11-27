namespace WinCapture.Core.Geometry;

/// <summary>
/// One drag gesture on the border band (move) or grip (resize). Every delta is measured from the cursor
/// position at the drag start and applied to the region at the drag start, so results do not drift
/// even though the border window itself moves while dragging. Physical pixels.
/// </summary>
public sealed class RegionDrag
{
    private readonly RegionHit _kind;
    private readonly Rect _startRegion;
    private readonly PixelPoint _startCursor;

    public RegionDrag(RegionHit kind, Rect startRegion, PixelPoint startCursor)
    {
        _kind = kind;
        _startRegion = startRegion;
        _startCursor = startCursor;
    }

    public Rect StartRegion => _startRegion;

    /// <summary>Region for the current cursor position. Equals the start region when locked or not a drag target.</summary>
    public Rect Compute(PixelPoint cursor, Rect virtualScreen, bool locked)
    {
        int dx = cursor.X - _startCursor.X;
        int dy = cursor.Y - _startCursor.Y;
        return _kind switch
        {
            RegionHit.Move => RegionInteraction.Move(_startRegion, dx, dy, virtualScreen, locked),
            RegionHit.Resize => RegionInteraction.Resize(_startRegion, dx, dy, virtualScreen, locked),
            _ => _startRegion,
        };
    }
}
