using FluentAssertions;
using WinCapture.Core.Geometry;
using WinCapture.Core.Presentation;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public class ControlBarTextTests
{
    [Fact]
    public void Specified_region_shows_title_and_coordinates_without_monitor()
    {
        ControlBarText.StateTitle(RegionMode.Specified).Should().Be("지정 영역");
        ControlBarText.Coordinates(RegionMode.Specified, new Rect(100, 100, 400, 300), 1)
            .Should().Be("100, 100, 400 × 300");
    }

    [Fact]
    public void Full_screen_shows_title_and_monitor_number()
    {
        ControlBarText.StateTitle(RegionMode.FullScreen).Should().Be("전체 화면");
        ControlBarText.Coordinates(RegionMode.FullScreen, new Rect(0, 0, 1920, 1080), 1)
            .Should().Be("0, 0, 1920 × 1080 · 모니터 1");
    }

    [Fact]
    public void Full_screen_without_known_monitor_has_no_suffix()
    {
        ControlBarText.Coordinates(RegionMode.FullScreen, new Rect(0, 0, 1920, 1080), null)
            .Should().Be("0, 0, 1920 × 1080");
    }

    [Fact]
    public void Negative_coordinates_are_printed_as_is()
    {
        ControlBarText.Coordinates(RegionMode.Specified, new Rect(-1920, 0, 1920, 1080), null)
            .Should().Be("-1920, 0, 1920 × 1080");
    }

    [Theory]
    [InlineData(RegionMode.Specified, true, "테두리 표시 중")]
    [InlineData(RegionMode.Specified, false, "테두리 숨김")]
    [InlineData(RegionMode.FullScreen, true, "테두리 없음")]
    [InlineData(RegionMode.FullScreen, false, "테두리 없음")]
    public void Border_chip_text(RegionMode mode, bool visible, string expected) =>
        ControlBarText.BorderChip(mode, visible).Should().Be(expected);

    [Theory]
    [InlineData(CaptureMode.Screenshot, false, "스크린샷 찍기")]
    [InlineData(CaptureMode.Video, false, "녹화 시작")]
    [InlineData(CaptureMode.Video, true, "녹화 정지")]
    [InlineData(CaptureMode.Gif, false, "GIF 녹화 시작")]
    [InlineData(CaptureMode.Gif, true, "GIF 녹화 정지")]
    public void Capture_button_text(CaptureMode mode, bool recording, string expected) =>
        ControlBarText.CaptureButtonText(mode, recording).Should().Be(expected);

    [Fact]
    public void Monitor_number_is_one_based_by_list_order()
    {
        var monitors = new[] { new Rect(0, 0, 1920, 1080), new Rect(-1920, 0, 1920, 1080) };
        MonitorSelector.MonitorNumber(monitors, new Rect(-1920, 0, 1920, 1080)).Should().Be(2);
        MonitorSelector.MonitorNumber(monitors, new Rect(5, 5, 10, 10)).Should().BeNull();
    }
}
