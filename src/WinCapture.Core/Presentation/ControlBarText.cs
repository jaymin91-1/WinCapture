using System.Globalization;
using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Presentation;

/// <summary>Pure text builders for the control bar (SPEC section 4 "컨트롤 바").</summary>
public static class ControlBarText
{
    public const string SpecifiedRegionTitle = "지정 영역";
    public const string FullScreenTitle = "전체 화면";

    /// <summary>"지정 영역" / "전체 화면".</summary>
    public static string StateTitle(RegionMode mode) =>
        mode == RegionMode.Specified ? SpecifiedRegionTitle : FullScreenTitle;

    /// <summary>
    /// "X, Y, W × H" in physical pixels (negative X/Y allowed). The full-screen state appends " · 모니터 N" when the
    /// monitor number is known; a specified region never carries the monitor suffix.
    /// </summary>
    public static string Coordinates(RegionMode mode, Rect rect, int? monitorNumber)
    {
        var text = string.Create(CultureInfo.InvariantCulture, $"{rect.X}, {rect.Y}, {rect.Width} × {rect.Height}");
        if (mode == RegionMode.FullScreen && monitorNumber is { } n)
        {
            text += string.Create(CultureInfo.InvariantCulture, $" · 모니터 {n}");
        }

        return text;
    }

    /// <summary>Border chip: specified+visible "테두리 표시 중", specified+hidden "테두리 숨김", full screen "테두리 없음".</summary>
    public static string BorderChip(RegionMode mode, bool borderVisible) => mode switch
    {
        RegionMode.FullScreen => "테두리 없음",
        _ => borderVisible ? "테두리 표시 중" : "테두리 숨김",
    };

    /// <summary>Label of the big [Capture] button for the selected mode and recording state.</summary>
    public static string CaptureButtonText(CaptureMode mode, bool isRecording) => mode switch
    {
        CaptureMode.Video => isRecording ? "녹화 정지" : "녹화 시작",
        CaptureMode.Gif => isRecording ? "GIF 녹화 정지" : "GIF 녹화 시작",
        _ => "스크린샷 찍기",
    };
}
