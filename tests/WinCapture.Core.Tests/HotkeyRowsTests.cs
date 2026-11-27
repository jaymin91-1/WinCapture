using FluentAssertions;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Presentation;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public class HotkeyRowsTests
{
    [Fact]
    public void Default_settings_give_seven_rows_in_spec_order()
    {
        var rows = HotkeyRows.Build(new HotkeySettings());

        rows.Select(r => r.Action).Should().Equal(
            HotkeyAction.SelectRegion, HotkeyAction.Screenshot, HotkeyAction.RecordVideo, HotkeyAction.RecordGif,
            HotkeyAction.ToggleBorder, HotkeyAction.ClearRegion, HotkeyAction.ToggleAppWindow);
        rows[0].Keys.Should().Equal("Ctrl", "Alt", "R");
        rows[1].Keys.Should().Equal("Ctrl", "Alt", "S");
        rows.Should().OnlyContain(r => !r.IsFailed && !r.IsUnset);
    }

    [Fact]
    public void Changing_one_combo_changes_only_that_row()
    {
        var baseline = HotkeyRows.Build(new HotkeySettings());
        var changed = HotkeyRows.Build(new HotkeySettings { Screenshot = "Shift+Alt+F5" });

        changed[1].Keys.Should().Equal("Alt", "Shift", "F5");
        for (var i = 0; i < 7; i++)
        {
            if (i != 1)
            {
                changed[i].Should().BeEquivalentTo(baseline[i]);
            }
        }
    }

    [Fact]
    public void Failed_actions_are_flagged_and_others_are_not()
    {
        var failed = new HashSet<HotkeyAction> { HotkeyAction.ToggleBorder };
        var rows = HotkeyRows.Build(new HotkeySettings(), failed);

        rows.Single(r => r.IsFailed).Action.Should().Be(HotkeyAction.ToggleBorder);
        rows.Count(r => !r.IsFailed).Should().Be(6);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+Alt")]
    [InlineData("garbage")]
    public void Empty_or_invalid_combo_is_shown_as_unset(string text)
    {
        var row = HotkeyRows.Build(new HotkeySettings { RecordGif = text })[3];

        row.IsUnset.Should().BeTrue();
        row.Keys.Should().Equal("없음");
    }

    [Fact]
    public void Failed_actions_come_from_register_failed_and_conflict_statuses()
    {
        var statuses = new Dictionary<HotkeyAction, HotkeyStatus>
        {
            [HotkeyAction.SelectRegion] = HotkeyStatus.Registered,
            [HotkeyAction.Screenshot] = HotkeyStatus.RegisterFailed,
            [HotkeyAction.RecordGif] = HotkeyStatus.Conflict,
            [HotkeyAction.RecordVideo] = HotkeyStatus.Invalid,
        };

        HotkeyRows.FailedActions(statuses).Should().BeEquivalentTo(new[] { HotkeyAction.Screenshot, HotkeyAction.RecordGif });
    }
}
