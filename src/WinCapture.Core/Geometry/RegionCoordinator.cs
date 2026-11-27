using System.Globalization;

namespace WinCapture.Core.Geometry;

/// <summary>
/// Region state rules: the state starts as full screen on the cursor monitor, a valid drag selection makes it a
/// specified region, and Clear returns to full screen on the monitor under the cursor at that moment.
/// Regions are never saved or restored. No UI dependencies; screen information is injected.
/// </summary>
public sealed class RegionCoordinator
{
    private readonly RegionState _state;
    private readonly IScreenInfo _screen;
    private readonly Action<string, Exception?>? _log;

    public RegionCoordinator(RegionState state, IScreenInfo screen, Action<string, Exception?>? log = null)
    {
        _state = state;
        _screen = screen;
        _log = log;
        ResetToFullScreen(); // startup state: full screen on the cursor monitor
    }

    public RegionState State => _state;

    /// <summary>Size label of the selection overlay, e.g. "800 × 600".</summary>
    public static string FormatSizeLabel(Rect r) =>
        string.Create(CultureInfo.InvariantCulture, $"{r.Width} × {r.Height}");

    /// <summary>
    /// Applies a selection result. A null result (Esc) or an invalid one (smaller than 32 px) keeps the current
    /// state (specified region or full screen). A valid one makes it a specified region. Returns true when applied.
    /// </summary>
    public bool CommitSelection(Rect? selected)
    {
        if (selected is not { } region || !RegionMath.IsValidRegion(region))
        {
            return false;
        }

        _state.SetRegion(region);
        return true;
    }

    /// <summary>Applies a region changed by dragging (move/resize). Refused while locked or when invalid.</summary>
    public bool UpdateRegion(Rect region)
    {
        if (_state.IsLocked || !RegionMath.IsValidRegion(region))
        {
            return false;
        }

        _state.SetRegion(region);
        return true;
    }

    /// <summary>ClearRegion: discards the specified region and re-picks the monitor under the cursor (full screen).</summary>
    public void Clear() => ResetToFullScreen();

    private void ResetToFullScreen()
    {
        try
        {
            var monitor = MonitorSelector.PickCursorMonitor(
                _screen.GetMonitors(), _screen.GetCursorPosition(), _screen.GetPrimaryMonitor());
            if (monitor is { } m)
            {
                _state.SetFullScreen(m);
            }
            else
            {
                _state.ClearRegion(); // no monitor known: still leave the specified state
            }
        }
        catch (Exception ex)
        {
            _log?.Invoke("Failed to pick the full-screen monitor", ex);
            _state.ClearRegion();
        }
    }
}
