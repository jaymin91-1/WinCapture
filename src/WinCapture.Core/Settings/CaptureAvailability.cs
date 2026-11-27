using WinCapture.Core.Hotkeys;

namespace WinCapture.Core.Settings;

/// <summary>
/// Which capture modes the [Capture] button can run. Single place to flip when a feature ships:
/// add the mode to <see cref="Implemented"/> and wire its action in the app.
/// </summary>
public static class CaptureAvailability
{
    // Screenshot (session 3), video (session 4) and GIF (session 5) are not implemented yet.
    private static readonly HashSet<CaptureMode> Implemented = new();

    public static bool IsImplemented(CaptureMode mode) => Implemented.Contains(mode);

    /// <summary>The hotkey action that runs the mode; the [Capture] button uses the same path as the hotkey/tray item.</summary>
    public static HotkeyAction ActionFor(CaptureMode mode) => mode switch
    {
        CaptureMode.Video => HotkeyAction.RecordVideo,
        CaptureMode.Gif => HotkeyAction.RecordGif,
        _ => HotkeyAction.Screenshot,
    };

    /// <summary>User-facing tooltip text for the [Capture] button.</summary>
    public static string Tooltip(CaptureMode mode) => IsImplemented(mode)
        ? "선택한 모드로 캡처합니다"
        : mode switch
        {
            CaptureMode.Video => "동영상 녹화 기능은 준비 중입니다",
            CaptureMode.Gif => "GIF 녹화 기능은 준비 중입니다",
            _ => "스크린샷 기능은 준비 중입니다",
        };
}
