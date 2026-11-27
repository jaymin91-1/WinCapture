using WinCapture.Core.Geometry;

namespace WinCapture.Core.Presentation;

/// <summary>Kind of a toast notification.</summary>
public enum ToastKind
{
    Success,
    Warning,
    Error,
}

/// <summary>Pure layout and text rules of toast notifications (physical pixels).</summary>
public static class ToastLayout
{
    public const int MaxTitleLength = 40;
    public const int MaxMessageLength = 90;

    /// <summary>
    /// Top-left of the toast at <paramref name="index"/> (0 = bottom-most). Toasts are anchored to the bottom-right
    /// corner of the work area (which may have negative coordinates) and stack upward.
    /// </summary>
    public static PixelPoint SlotPosition(Rect workArea, PixelSize toast, int margin, int gap, int index) =>
        new(
            workArea.Right - margin - toast.Width,
            workArea.Bottom - margin - toast.Height - index * (toast.Height + gap));

    /// <summary>Shortens text to <paramref name="maxLength"/> characters, ending with an ellipsis when cut. Null becomes empty.</summary>
    public static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (maxLength < 1 || text.Length <= maxLength)
        {
            return maxLength < 1 ? string.Empty : text;
        }

        return maxLength == 1 ? "…" : text[..(maxLength - 1)] + "…";
    }
}
