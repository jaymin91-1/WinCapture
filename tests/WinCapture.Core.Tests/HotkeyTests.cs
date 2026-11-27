using FluentAssertions;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public class HotkeyTests
{
    [Fact]
    public void Parse_CtrlAltR_RoundTrips()
    {
        KeyCombo.TryParse("Ctrl+Alt+R", out var c).Should().BeTrue();
        c.Modifiers.Should().Be(Modifiers.Ctrl | Modifiers.Alt);
        c.Key.Should().Be("R");
        c.ToString().Should().Be("Ctrl+Alt+R");
    }

    [Fact]
    public void Parse_ModifierOrderIsNormalized()
    {
        KeyCombo.TryParse("Alt+Ctrl+R", out var c).Should().BeTrue();
        c.ToString().Should().Be("Ctrl+Alt+R");
        KeyCombo.TryParse("Win+Shift+Alt+Ctrl+a", out var d).Should().BeTrue();
        d.ToString().Should().Be("Ctrl+Alt+Shift+Win+A");
    }

    [Fact]
    public void Parse_IsCaseAndWhitespaceTolerant()
    {
        KeyCombo.TryParse("  ctrl + ALT +  f5 ", out var c).Should().BeTrue();
        c.ToString().Should().Be("Ctrl+Alt+F5");
    }

    [Theory]
    [InlineData("R")]
    [InlineData("Ctrl+Alt")]
    [InlineData("Ctrl+A+B")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+Ctrl+A")]
    [InlineData("Ctrl+")]
    [InlineData("Ctrl+F0")]
    [InlineData("Ctrl+F25")]
    [InlineData("Ctrl+F01")]
    [InlineData("Ctrl+Space")]
    public void Invalid_Rejected(string text)
    {
        KeyCombo.IsValid(text).Should().BeFalse();
    }

    [Theory]
    [InlineData("Ctrl+F13")]
    [InlineData("Alt+F24")]
    [InlineData("Shift+F1")]
    [InlineData("Win+0")]
    [InlineData("Ctrl+Alt+Z")]
    public void Valid_Accepted(string text)
    {
        KeyCombo.IsValid(text).Should().BeTrue();
    }

    [Fact]
    public void Win32Values_AreComputed()
    {
        KeyCombo.TryParse("Ctrl+Alt+Shift+Win+F13", out var c).Should().BeTrue();
        ((int)c.Modifiers).Should().Be(0x2 | 0x1 | 0x4 | 0x8);
        c.VirtualKey.Should().Be(0x7C);
        KeyCombo.TryParse("Ctrl+R", out var r).Should().BeTrue();
        r.VirtualKey.Should().Be(0x52);
    }

    [Fact]
    public void Conflict_SameComboReturnsBothActions()
    {
        KeyCombo.TryParse("Ctrl+Alt+R", out var combo);
        KeyCombo.TryParse("Ctrl+Alt+S", out var other);
        var map = new Dictionary<HotkeyAction, KeyCombo>
        {
            [HotkeyAction.SelectRegion] = combo,
            [HotkeyAction.Screenshot] = combo,
            [HotkeyAction.RecordVideo] = other,
        };
        var groups = HotkeyConflicts.Find(map);
        groups.Should().ContainSingle();
        groups[0].Should().BeEquivalentTo(new[] { HotkeyAction.SelectRegion, HotkeyAction.Screenshot });
    }

    [Fact]
    public void Defaults_ConvertAndHaveNoConflict()
    {
        HotkeyConflicts.TryFromSettings(new HotkeySettings(), out var map).Should().BeTrue();
        map.Should().HaveCount(7);
        map[HotkeyAction.ToggleAppWindow].ToString().Should().Be("Ctrl+Alt+W");
        HotkeyConflicts.Find(map).Should().BeEmpty();
    }
}
