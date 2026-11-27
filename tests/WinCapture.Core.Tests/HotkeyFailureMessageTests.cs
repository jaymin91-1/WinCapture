using FluentAssertions;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Presentation;

namespace WinCapture.Core.Tests;

public class HotkeyFailureMessageTests
{
    [Fact]
    public void No_failures_gives_no_message() =>
        HotkeyFailureMessage.Build([]).Should().BeNull();

    [Fact]
    public void Single_failure_names_action_and_points_to_settings_hotkey_tab()
    {
        var text = HotkeyFailureMessage.Build([HotkeyAction.Screenshot]);

        text.Should().Contain("스크린샷").And.Contain("설정 › 단축키");
    }

    [Fact]
    public void Several_failures_are_grouped_into_one_message_in_enum_order()
    {
        var text = HotkeyFailureMessage.Build([HotkeyAction.ClearRegion, HotkeyAction.SelectRegion, HotkeyAction.ClearRegion]);

        text.Should().StartWith("영역 지정, 영역 해제 단축키");
    }
}
