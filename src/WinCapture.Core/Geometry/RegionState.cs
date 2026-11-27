namespace WinCapture.Core.Geometry;

/// <summary>Lock switch for region changes. Session 4 (recording) turns it on while recording.</summary>
public interface IRegionLock
{
    /// <summary>True while move/resize must be refused (recording in progress).</summary>
    bool IsLocked { get; }

    void SetLocked(bool locked);
}

/// <summary>The two region states (SPEC "영역 상태").</summary>
public enum RegionMode
{
    /// <summary>The whole monitor under the cursor (default). No border.</summary>
    FullScreen,

    /// <summary>A rectangle chosen by dragging.</summary>
    Specified,
}

/// <summary>Region/border state: specified region or full screen, and whether the border is shown. No UI dependencies.</summary>
public sealed class RegionState : IRegionLock
{
    public bool IsLocked { get; private set; }

    public void SetLocked(bool locked) => IsLocked = locked;

    /// <summary>The user-specified region, or null in the full-screen state.</summary>
    public Rect? Region { get; private set; }

    /// <summary>Full rectangle of the chosen monitor (used in the full-screen state; empty until first set).</summary>
    public Rect FullScreenRect { get; private set; }

    public RegionMode Mode => Region is null ? RegionMode.FullScreen : RegionMode.Specified;

    /// <summary>Rectangle a capture should use: the specified region, or the full-screen monitor rectangle. Null when unknown.</summary>
    public Rect? CaptureRect => Region ?? (FullScreenRect.IsEmpty ? null : FullScreenRect);

    public bool IsBorderVisible { get; private set; } = true;

    public bool HasRegion => Region is not null;

    /// <summary>True when the border window should be on screen.</summary>
    public bool ShouldShowBorder => HasRegion && IsBorderVisible;

    public event EventHandler? Changed;

    public void SetRegion(Rect region)
    {
        if (Region == region)
        {
            return;
        }

        Region = region;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Drops the specified region (full-screen state keeps the current monitor rectangle).</summary>
    public void ClearRegion()
    {
        if (Region is null)
        {
            return;
        }

        Region = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Switches to the full-screen state on the given monitor rectangle; raises Changed if anything changed.</summary>
    public void SetFullScreen(Rect monitor)
    {
        if (Region is null && FullScreenRect == monitor)
        {
            return;
        }

        Region = null;
        FullScreenRect = monitor;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Toggles border visibility (ToggleBorder). Does nothing without a region.</summary>
    public void ToggleBorder()
    {
        if (!HasRegion)
        {
            return;
        }

        IsBorderVisible = !IsBorderVisible;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
