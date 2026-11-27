using FluentAssertions;
using WinCapture.Core.Geometry;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public class HotkeyPlanTests
{
    [Fact]
    public void Defaults_AllSevenRegister()
    {
        var plan = HotkeyPlan.Build(new HotkeySettings());
        plan.ToRegister.Should().HaveCount(7);
        plan.Rejected.Should().BeEmpty();
    }

    [Fact]
    public void Invalid_IsRejectedOthersStillRegister()
    {
        var plan = HotkeyPlan.Build(new HotkeySettings { Screenshot = "Ctrl+Alt" });
        plan.Rejected[HotkeyAction.Screenshot].Should().Be(HotkeyStatus.Invalid);
        plan.ToRegister.Should().HaveCount(6).And.NotContainKey(HotkeyAction.Screenshot);
    }

    [Fact]
    public void Conflict_RejectsAllMembersOfTheGroup()
    {
        var plan = HotkeyPlan.Build(new HotkeySettings { RecordGif = "alt+ctrl+s" });
        plan.Rejected[HotkeyAction.Screenshot].Should().Be(HotkeyStatus.Conflict);
        plan.Rejected[HotkeyAction.RecordGif].Should().Be(HotkeyStatus.Conflict);
        plan.ToRegister.Should().HaveCount(5);
    }
}

public class AppWindowPolicyTests
{
    private static readonly Rect Region = new(100, 100, 400, 300);

    [Fact]
    public void Overlapping_AutoHideOn_Hides() =>
        AppWindowPolicy.ShouldAutoHide(true, true, new Rect(450, 350, 300, 300), Region).Should().BeTrue();

    [Fact]
    public void Overlapping_AutoHideOff_DoesNotHide() =>
        AppWindowPolicy.ShouldAutoHide(false, true, new Rect(450, 350, 300, 300), Region).Should().BeFalse();

    [Fact]
    public void NotOverlapping_DoesNotHide() =>
        AppWindowPolicy.ShouldAutoHide(true, true, new Rect(600, 100, 300, 300), Region).Should().BeFalse();

    [Fact]
    public void HiddenWindowOrNoRegion_DoesNotHide()
    {
        AppWindowPolicy.ShouldAutoHide(true, false, new Rect(450, 350, 300, 300), Region).Should().BeFalse();
        AppWindowPolicy.ShouldAutoHide(true, true, new Rect(450, 350, 300, 300), null).Should().BeFalse();
    }
}
