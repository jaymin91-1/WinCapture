using WinCapture.Core.Settings;

namespace WinCapture.Core.Hotkeys;

/// <summary>Per-action hotkey state, exposed for the settings hotkey tab (session 6).</summary>
public enum HotkeyStatus
{
    /// <summary>Not registered (yet).</summary>
    None,
    Registered,
    /// <summary>The setting text is not a valid combo.</summary>
    Invalid,
    /// <summary>Another action uses the same combo.</summary>
    Conflict,
    /// <summary>RegisterHotKey failed (usually taken by another app).</summary>
    RegisterFailed,
}

/// <summary>Result of filtering settings: combos that may be registered and actions that must not be.</summary>
public sealed record HotkeyPlan(
    IReadOnlyDictionary<HotkeyAction, KeyCombo> ToRegister,
    IReadOnlyDictionary<HotkeyAction, HotkeyStatus> Rejected)
{
    /// <summary>Parses each action separately; invalid and conflicting (all members of a group) actions are rejected.</summary>
    public static HotkeyPlan Build(HotkeySettings settings)
    {
        var texts = new Dictionary<HotkeyAction, string>
        {
            [HotkeyAction.SelectRegion] = settings.SelectRegion,
            [HotkeyAction.Screenshot] = settings.Screenshot,
            [HotkeyAction.RecordVideo] = settings.RecordVideo,
            [HotkeyAction.RecordGif] = settings.RecordGif,
            [HotkeyAction.ToggleBorder] = settings.ToggleBorder,
            [HotkeyAction.ClearRegion] = settings.ClearRegion,
            [HotkeyAction.ToggleAppWindow] = settings.ToggleAppWindow,
        };

        var valid = new Dictionary<HotkeyAction, KeyCombo>();
        var rejected = new Dictionary<HotkeyAction, HotkeyStatus>();
        foreach (var (action, text) in texts)
        {
            if (KeyCombo.TryParse(text, out var combo))
            {
                valid[action] = combo;
            }
            else
            {
                rejected[action] = HotkeyStatus.Invalid;
            }
        }

        foreach (var group in HotkeyConflicts.Find(valid))
        {
            foreach (var action in group)
            {
                valid.Remove(action);
                rejected[action] = HotkeyStatus.Conflict;
            }
        }

        return new HotkeyPlan(valid, rejected);
    }
}
