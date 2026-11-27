using WinCapture.Core.Hotkeys;

namespace WinCapture.Core.Presentation;

/// <summary>Builds the single toast text shown when one or more hotkeys could not be registered (SPEC F-03-5).</summary>
public static class HotkeyFailureMessage
{
    public const string Title = "단축키 등록 실패";

    /// <summary>Returns null when nothing failed; otherwise one message that lists all failed actions in enum order.</summary>
    public static string? Build(IEnumerable<HotkeyAction> failed)
    {
        var labels = failed.Distinct().OrderBy(a => a).Select(Label).ToList();
        if (labels.Count == 0)
        {
            return null;
        }

        return $"{string.Join(", ", labels)} 단축키를 등록하지 못했습니다. 다른 앱이 사용 중일 수 있습니다. 설정 › 단축키 탭에서 변경해 주세요.";
    }

    private static string Label(HotkeyAction action) => action switch
    {
        HotkeyAction.SelectRegion => "영역 지정",
        HotkeyAction.Screenshot => "스크린샷",
        HotkeyAction.RecordVideo => "동영상 녹화",
        HotkeyAction.RecordGif => "GIF 녹화",
        HotkeyAction.ToggleBorder => "테두리 표시/숨김",
        HotkeyAction.ClearRegion => "영역 해제",
        HotkeyAction.ToggleAppWindow => "앱 창 표시/숨김",
        _ => action.ToString(),
    };
}
