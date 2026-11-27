using System.Globalization;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Geometry;

/// <summary>
/// Region state rules that involve settings: applying a selection result, restoring the last region
/// at startup, and clearing. No UI dependencies; settings access goes through <see cref="ISettingsStore"/>.
/// </summary>
public sealed class RegionCoordinator
{
    private readonly RegionState _state;
    private readonly ISettingsStore _settings;
    private readonly Action<string, Exception?>? _log;

    public RegionCoordinator(RegionState state, ISettingsStore settings, Action<string, Exception?>? log = null)
    {
        _state = state;
        _settings = settings;
        _log = log;
    }

    public RegionState State => _state;

    /// <summary>Size label of the selection overlay, e.g. "800 × 600".</summary>
    public static string FormatSizeLabel(Rect r) =>
        string.Create(CultureInfo.InvariantCulture, $"{r.Width} × {r.Height}");

    /// <summary>
    /// Applies a selection result. A null result (Esc) or an invalid one (smaller than 32 px) keeps the
    /// previous region untouched. A valid one replaces the region and, when restoreLastRegion is on,
    /// is saved to settings. Returns true when the region was replaced.
    /// </summary>
    public bool CommitSelection(Rect? selected)
    {
        if (selected is not { } region || !RegionMath.IsValidRegion(region))
        {
            return false;
        }

        _state.SetRegion(region);
        SaveLastRegion(region, onlyIfRestoreEnabled: true);
        return true;
    }

    /// <summary>
    /// Startup restore: loads the saved region (when enabled), corrects it into the virtual screen,
    /// and applies it only if it is still a valid region (at least 32 px). Returns true when restored.
    /// </summary>
    public bool Restore(Rect virtualScreen)
    {
        var saved = RegionMath.GetRestorableRegion(_settings.Load());
        if (saved is not { } region)
        {
            return false;
        }

        var clamped = RegionMath.ClampToVirtualScreen(region, virtualScreen);
        if (!RegionMath.IsValidRegion(clamped))
        {
            return false;
        }

        _state.SetRegion(clamped);
        return true;
    }

    /// <summary>
    /// Applies a region changed by dragging (move/resize). Refused while locked. When <paramref name="persist"/>
    /// is true (drag finished) lastRegion is saved if restoreLastRegion is on. Returns true when the state changed or was saved.
    /// </summary>
    public bool UpdateRegion(Rect region, bool persist)
    {
        if (_state.IsLocked || !RegionMath.IsValidRegion(region))
        {
            return false;
        }

        _state.SetRegion(region);
        if (persist)
        {
            SaveLastRegion(region, onlyIfRestoreEnabled: true);
        }

        return true;
    }

    /// <summary>ClearRegion: removes the region and empties the saved lastRegion (0,0,0,0), regardless of the restore option.</summary>
    public void Clear()
    {
        _state.ClearRegion();
        SaveLastRegion(new Rect(0, 0, 0, 0), onlyIfRestoreEnabled: false);
    }

    private void SaveLastRegion(Rect region, bool onlyIfRestoreEnabled)
    {
        try
        {
            var current = _settings.Load();
            if (onlyIfRestoreEnabled && !current.RestoreLastRegion)
            {
                return;
            }

            _settings.Save(current with { LastRegion = RegionMath.ToSettings(region) });
        }
        catch (Exception ex)
        {
            _log?.Invoke("Failed to save lastRegion", ex);
        }
    }
}
