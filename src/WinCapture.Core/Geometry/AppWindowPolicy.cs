namespace WinCapture.Core.Geometry;

/// <summary>Decides whether the app window must be hidden before a capture (F-08).</summary>
public static class AppWindowPolicy
{
    /// <summary>True when autoHideAppWindow is on, the window is visible, a region exists and they overlap.</summary>
    public static bool ShouldAutoHide(bool autoHideEnabled, bool windowVisible, Rect? windowBounds, Rect? region) =>
        autoHideEnabled && windowVisible
        && windowBounds is { } window && region is { } r
        && RegionInteraction.Overlaps(window, r);
}
