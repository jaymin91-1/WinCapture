using WinCapture.Core.Geometry;

namespace WinCapture.Core.Capture;

/// <summary>What a screenshot attempt should do, decided before any capture happens (SPEC F-09).</summary>
public sealed record ScreenshotPlan(Rect? Area, bool CopyToClipboard, string? Error)
{
    public const string NoRegionError = "캡처할 영역을 알 수 없습니다. 영역을 지정하거나 전체 화면 상태를 확인해 주세요.";

    public bool CanCapture => Error is null && Area is not null;

    /// <summary>Fails when the capture rectangle is unknown or empty; otherwise captures it, copying to the clipboard only when enabled.</summary>
    public static ScreenshotPlan Decide(Rect? captureRect, bool copyToClipboard) =>
        captureRect is { IsEmpty: false } area
            ? new ScreenshotPlan(area, copyToClipboard, null)
            : new ScreenshotPlan(null, false, NoRegionError);
}

/// <summary>User-facing texts and the explorer command line of the screenshot flow.</summary>
public static class ScreenshotMessages
{
    public const string SuccessTitle = "스크린샷 저장됨";
    public const string ErrorTitle = "스크린샷 실패";
    public const string ClipboardWarningTitle = "클립보드 복사 실패";
    public const string ClipboardWarningMessage = "파일은 저장되었지만 클립보드에 복사하지 못했습니다.";

    /// <summary>"file.png (W×H) · 클릭하면 폴더에서 보기". Size is the saved pixel size, which can be smaller than the region when clipped.</summary>
    public static string SuccessMessage(string fileName, int width, int height) =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{fileName} ({width}×{height}) · 클릭하면 폴더에서 보기");

    public static string ErrorMessage(string detail) =>
        string.IsNullOrWhiteSpace(detail) ? "스크린샷을 저장하지 못했습니다." : "스크린샷을 저장하지 못했습니다. " + detail;

    /// <summary>Arguments for explorer.exe that open the folder with the file selected: /select,"path" (path quoted).</summary>
    public static string ExplorerSelectArguments(string path) => "/select,\"" + path + "\"";

    /// <summary>File name part of a Windows path (Core runs on any OS, so both separators are handled here).</summary>
    public static string FileNameOf(string path)
    {
        var i = path.LastIndexOfAny(new[] { '\\', '/' });
        return i < 0 ? path : path[(i + 1)..];
    }
}

/// <summary>Allows one run at a time; overlapping calls are refused instead of queued (rapid presses are ignored).</summary>
public sealed class SingleFlightGate
{
    private int _busy;

    public bool IsBusy => Volatile.Read(ref _busy) != 0;

    /// <summary>True when the caller acquired the gate and must call <see cref="Exit"/>.</summary>
    public bool TryEnter() => Interlocked.CompareExchange(ref _busy, 1, 0) == 0;

    public void Exit() => Volatile.Write(ref _busy, 0);
}
