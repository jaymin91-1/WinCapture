namespace WinCapture.Core.Presentation;

/// <summary>
/// Pure decisions for windows whose size the user must not change (control bar, hotkey popup).
/// The App layer feeds Win32 message values in; no Win32 or WPF types are used here.
/// </summary>
public static class FixedSizeWindowPolicy
{
    public const int HtCaption = 2;
    public const int HtSize = 4;
    public const int HtMaxButton = 9;
    public const int HtLeft = 10;
    public const int HtBottomRight = 17;

    private const int ScSize = 0xF000;
    private const int ScMaximize = 0xF030;

    /// <summary>True for hit-test codes that start a resize or maximize (HTSIZE, HTLEFT..HTBOTTOMRIGHT, HTMAXBUTTON).</summary>
    public static bool IsSizingHit(int hit) =>
        hit == HtSize || hit == HtMaxButton || (hit >= HtLeft && hit <= HtBottomRight);

    /// <summary>True for WM_SYSCOMMAND values that resize or maximize (SC_SIZE incl. edge variants, SC_MAXIMIZE).</summary>
    public static bool IsBlockedSysCommand(long wParam)
    {
        var command = (int)(wParam & 0xFFF0);
        return command == ScSize || command == ScMaximize;
    }

    /// <summary>True when a double click on this non-client area would maximize the window (caption or a sizing hit).</summary>
    public static bool IsBlockedDoubleClick(int hit) => hit == HtCaption || IsSizingHit(hit);

    /// <summary>
    /// True when the screen point lies in the outer band of the window rectangle (right/bottom exclusive).
    /// Such points are reported as client area so no resize cursor or drag starts there.
    /// </summary>
    public static bool IsInEdgeBand(int x, int y, int left, int top, int right, int bottom, int band) =>
        x >= left && x < right && y >= top && y < bottom
        && (x < left + band || x >= right - band || y < top + band || y >= bottom - band);

    /// <summary>Window size in physical pixels for a DIP size at a DPI (rounded up so content is never clipped).</summary>
    public static int ToPixelsCeiling(double dips, uint dpi) => (int)Math.Ceiling(dips * dpi / 96.0 - 1e-9);
}
