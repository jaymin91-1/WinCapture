using FluentAssertions;
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
