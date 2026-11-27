using WinCapture.Core.Hotkeys;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Presentation;

/// <summary>One row of the hotkey popup: action name, keycap tokens and failure flag.</summary>
public sealed record HotkeyRow(HotkeyAction Action, string Name, IReadOnlyList<string> Keys, bool IsUnset, bool IsFailed);

/// <summary>Builds the read-only rows of the hotkey popup (SPEC F-03 table order).</summary>
public static class HotkeyRows
{
    /// <summary>Text shown instead of keycaps when an action has no (valid) combo.</summary>
    public const string UnsetText = "없음";

    /// <summary>
    /// Seven rows in F-03 order. Keys: the canonical combo split into keycap tokens (e.g. Ctrl, Alt, R).
    /// Rule: an empty, whitespace-only or unparsable setting is "unset": Keys = ["없음"], IsUnset = true.
    /// IsFailed is true for actions in <paramref name="failedActions"/> (registration failed / combo in use).
    /// </summary>
    public static IReadOnlyList<HotkeyRow> Build(HotkeySettings settings, IReadOnlySet<HotkeyAction>? failedActions = null)
    {
        failedActions ??= new HashSet<HotkeyAction>();
        return new[]
        {
            Row(HotkeyAction.SelectRegion, "영역 지정", settings.SelectRegion),
            Row(HotkeyAction.Screenshot, "스크린샷", settings.Screenshot),
            Row(HotkeyAction.RecordVideo, "동영상 녹화 시작·정지", settings.RecordVideo),
            Row(HotkeyAction.RecordGif, "GIF 녹화 시작·정지", settings.RecordGif),
            Row(HotkeyAction.ToggleBorder, "테두리 표시·숨김", settings.ToggleBorder),
            Row(HotkeyAction.ClearRegion, "영역 해제", settings.ClearRegion),
            Row(HotkeyAction.ToggleAppWindow, "창 표시·숨김", settings.ToggleAppWindow),
        }.Select(r => r with { IsFailed = failedActions.Contains(r.Action) }).ToList();
    }

    /// <summary>Actions whose combo could not be used: another app owns it, or another action uses the same combo.</summary>
    public static IReadOnlySet<HotkeyAction> FailedActions(IReadOnlyDictionary<HotkeyAction, HotkeyStatus> statuses) =>
        statuses.Where(p => p.Value is HotkeyStatus.RegisterFailed or HotkeyStatus.Conflict)
            .Select(p => p.Key)
            .ToHashSet();

    private static HotkeyRow Row(HotkeyAction action, string name, string? text)
    {
        if (KeyCombo.TryParse(text, out var combo))
        {
            return new HotkeyRow(action, name, combo.ToString().Split('+'), false, false);
        }

        return new HotkeyRow(action, name, new[] { UnsetText }, true, false);
    }
}
