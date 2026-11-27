namespace WinCapture.Core.Geometry;

/// <summary>Region/border state: whether a region exists and whether its border is shown. No UI dependencies.</summary>
/// <summary>Lock switch for region changes. Session 4 (recording) turns it on while recording.</summary>
public interface IRegionLock
{
    /// <summary>True while move/resize must be refused (recording in progress).</summary>
    bool IsLocked { get; }

    void SetLocked(bool locked);
}

public sealed class RegionState : IRegionLock
{
    public bool IsLocked { get; private set; }

    public void SetLocked(bool locked) => IsLocked = locked;

    public Rect? Region { get; private set; }

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

    /// <summary>Removes the region (ClearRegion). Persisting the cleared lastRegion is the caller's job.</summary>
    public void ClearRegion()
    {
        if (Region is null)
        {
            return;
        }

        Region = null;
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
