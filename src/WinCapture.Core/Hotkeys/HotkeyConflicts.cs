using WinCapture.Core.Settings;

namespace WinCapture.Core.Hotkeys;

public static class HotkeyConflicts
{
    /// <summary>Returns groups of action IDs (2 or more) that share the same combo.</summary>
    public static IReadOnlyList<IReadOnlyList<HotkeyAction>> Find(IReadOnlyDictionary<HotkeyAction, KeyCombo> map) =>
        map.GroupBy(p => p.Value)
            .Where(g => g.Count() > 1)
            .Select(g => (IReadOnlyList<HotkeyAction>)g.Select(p => p.Key).OrderBy(a => a).ToList())
            .ToList();

    /// <summary>Converts settings hotkey strings to a model map. Returns false if any string is invalid.</summary>
    public static bool TryFromSettings(HotkeySettings settings, out Dictionary<HotkeyAction, KeyCombo> map)
    {
        var strings = new Dictionary<HotkeyAction, string>
        {
            [HotkeyAction.SelectRegion] = settings.SelectRegion,
            [HotkeyAction.Screenshot] = settings.Screenshot,
            [HotkeyAction.RecordVideo] = settings.RecordVideo,
            [HotkeyAction.RecordGif] = settings.RecordGif,
            [HotkeyAction.ToggleBorder] = settings.ToggleBorder,
            [HotkeyAction.ClearRegion] = settings.ClearRegion,
            [HotkeyAction.ToggleAppWindow] = settings.ToggleAppWindow,
        };

        map = new Dictionary<HotkeyAction, KeyCombo>();
        var ok = true;
        foreach (var (action, text) in strings)
        {
            if (KeyCombo.TryParse(text, out var combo))
            {
                map[action] = combo;
            }
            else
            {
                ok = false;
            }
        }

        return ok;
    }
}
