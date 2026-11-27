using System.Globalization;

namespace WinCapture.Core.Hotkeys;

/// <summary>Modifier flags. Values match the Win32 RegisterHotKey fsModifiers bits.</summary>
[Flags]
public enum Modifiers
{
    None = 0,
    Alt = 0x1,
    Ctrl = 0x2,
    Shift = 0x4,
    Win = 0x8,
}

/// <summary>A hotkey: one or more modifiers plus exactly one key (A-Z, 0-9, F1-F24).</summary>
public readonly record struct KeyCombo
{
    private KeyCombo(Modifiers modifiers, string key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    public Modifiers Modifiers { get; }

    /// <summary>Canonical upper-case key name, e.g. "R", "5", "F13".</summary>
    public string Key { get; }

    /// <summary>Parses text like "Ctrl+Alt+R". Case and whitespace tolerant; modifier order is free.</summary>
    public static bool TryParse(string? text, out KeyCombo combo)
    {
        combo = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var modifiers = Modifiers.None;
        string? key = null;
        foreach (var rawPart in text.Split('+'))
        {
            var part = rawPart.Trim();
            if (part.Length == 0)
            {
                return false;
            }

            var modifier = ParseModifier(part);
            if (modifier != Modifiers.None)
            {
                if ((modifiers & modifier) != 0)
                {
                    return false; // duplicate modifier
                }

                modifiers |= modifier;
                continue;
            }

            if (key is not null || !IsValidKey(part))
            {
                return false;
            }

            key = part.ToUpperInvariant();
        }

        if (modifiers == Modifiers.None || key is null)
        {
            return false;
        }

        combo = new KeyCombo(modifiers, key);
        return true;
    }

    public static bool IsValid(string? text) => TryParse(text, out _);

    /// <summary>Win32 virtual-key code for the key (A-Z, 0-9: ASCII; F1-F24: 0x70 + n - 1).</summary>
    public int VirtualKey
    {
        get
        {
            if (Key.Length == 1)
            {
                return Key[0];
            }

            return 0x70 + int.Parse(Key.AsSpan(1), CultureInfo.InvariantCulture) - 1;
        }
    }

    /// <summary>Canonical text with modifier order Ctrl, Alt, Shift, Win.</summary>
    public override string ToString()
    {
        if (Key is null)
        {
            return string.Empty;
        }

        var parts = new List<string>(5);
        if ((Modifiers & Modifiers.Ctrl) != 0) parts.Add("Ctrl");
        if ((Modifiers & Modifiers.Alt) != 0) parts.Add("Alt");
        if ((Modifiers & Modifiers.Shift) != 0) parts.Add("Shift");
        if ((Modifiers & Modifiers.Win) != 0) parts.Add("Win");
        parts.Add(Key);
        return string.Join('+', parts);
    }

    private static Modifiers ParseModifier(string part) => part.ToUpperInvariant() switch
    {
        "CTRL" or "CONTROL" => Modifiers.Ctrl,
        "ALT" => Modifiers.Alt,
        "SHIFT" => Modifiers.Shift,
        "WIN" or "WINDOWS" => Modifiers.Win,
        _ => Modifiers.None,
    };

    private static bool IsValidKey(string part)
    {
        if (part.Length == 1)
        {
            var c = char.ToUpperInvariant(part[0]);
            return (c is >= 'A' and <= 'Z') || (c is >= '0' and <= '9');
        }

        if ((part[0] is 'F' or 'f') && part.Length <= 3
            && int.TryParse(part.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var n))
        {
            return n is >= 1 and <= 24 && part.Length == (n < 10 ? 2 : 3);
        }

        return false;
    }
}
