using FluentAssertions;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public class CaptureAvailabilityTests
{
    [Fact]
    public void ActionFor_maps_each_mode_to_its_hotkey_action()
    {
        CaptureAvailability.ActionFor(CaptureMode.Screenshot).Should().Be(HotkeyAction.Screenshot);
        CaptureAvailability.ActionFor(CaptureMode.Video).Should().Be(HotkeyAction.RecordVideo);
        CaptureAvailability.ActionFor(CaptureMode.Gif).Should().Be(HotkeyAction.RecordGif);
    }

    [Theory]
    [InlineData(CaptureMode.Screenshot)]
    [InlineData(CaptureMode.Video)]
    [InlineData(CaptureMode.Gif)]
    public void Every_mode_has_a_non_empty_tooltip_without_dev_wording(CaptureMode mode)
    {
        var tip = CaptureAvailability.Tooltip(mode);
        tip.Should().NotBeNullOrWhiteSpace();
        tip.Should().NotContain("세션");
    }
}
