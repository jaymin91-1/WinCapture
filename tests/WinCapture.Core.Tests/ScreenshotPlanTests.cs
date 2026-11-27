using FluentAssertions;
using WinCapture.Core.Capture;
using WinCapture.Core.Geometry;

namespace WinCapture.Core.Tests;

public class ScreenshotPlanTests
{
    [Fact]
    public void Null_capture_rect_fails_with_message()
    {
        var plan = ScreenshotPlan.Decide(null, true);
        plan.CanCapture.Should().BeFalse();
        plan.Error.Should().Be(ScreenshotPlan.NoRegionError);
        plan.CopyToClipboard.Should().BeFalse();
    }

    [Fact]
    public void Empty_capture_rect_fails() =>
        ScreenshotPlan.Decide(new Rect(10, 10, 0, 50), false).CanCapture.Should().BeFalse();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Clipboard_step_follows_the_option(bool copy)
    {
        var plan = ScreenshotPlan.Decide(new Rect(-100, 20, 400, 300), copy);
        plan.CanCapture.Should().BeTrue();
        plan.Area.Should().Be(new Rect(-100, 20, 400, 300));
        plan.CopyToClipboard.Should().Be(copy);
    }

    [Fact]
    public void Success_message_shows_file_name_and_saved_size() =>
        ScreenshotMessages.SuccessMessage("a.png", 400, 300).Should().Contain("a.png").And.Contain("400×300");

    [Fact]
    public void Error_message_appends_detail_only_when_present()
    {
        ScreenshotMessages.ErrorMessage("").Should().Be("스크린샷을 저장하지 못했습니다.");
        ScreenshotMessages.ErrorMessage("access denied").Should().EndWith("access denied");
    }

    [Fact]
    public void Explorer_arguments_quote_paths_with_spaces_and_specials() =>
        ScreenshotMessages.ExplorerSelectArguments("C:\\My Shots\\a&b (1).png")
            .Should().Be("/select,\"C:\\My Shots\\a&b (1).png\"");

    [Theory]
    [InlineData("C:\\Shots\\a.png", "a.png")]
    [InlineData("C:/Shots/b.png", "b.png")]
    [InlineData("c.png", "c.png")]
    public void FileNameOf_returns_last_segment(string path, string expected) =>
        ScreenshotMessages.FileNameOf(path).Should().Be(expected);

    [Fact]
    public void Gate_refuses_overlapping_runs_until_exit()
    {
        var gate = new SingleFlightGate();
        gate.TryEnter().Should().BeTrue();
        gate.IsBusy.Should().BeTrue();
        gate.TryEnter().Should().BeFalse();
        gate.Exit();
        gate.TryEnter().Should().BeTrue();
    }
}
