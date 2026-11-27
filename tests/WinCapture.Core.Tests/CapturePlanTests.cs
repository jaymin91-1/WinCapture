using FluentAssertions;
using WinCapture.Core.Geometry;

namespace WinCapture.Core.Tests;

public class CapturePlanTests
{
    private static readonly Rect Dual = new(-1920, 0, 3840, 1080);
    private static readonly Rect Single = new(0, 0, 1920, 1080);

    [Fact]
    public void Region_inside_virtual_screen_is_unchanged()
        => CapturePlan.Clip(new Rect(100, 100, 400, 300), Dual).Should().Be(new Rect(100, 100, 400, 300));

    [Fact]
    public void Odd_sizes_are_kept()
        => CapturePlan.Clip(new Rect(11, 13, 401, 301), Single).Should().Be(new Rect(11, 13, 401, 301));

    [Fact]
    public void Region_overhanging_right_bottom_is_clipped()
        => CapturePlan.Clip(new Rect(1800, 900, 400, 300), Single).Should().Be(new Rect(1800, 900, 120, 180));

    [Fact]
    public void Negative_coordinates_are_clipped_correctly()
        => CapturePlan.Clip(new Rect(-2000, 10, 200, 100), Dual).Should().Be(new Rect(-1920, 10, 120, 100));

    [Theory]
    [InlineData(2000, 0, 100, 100)]
    [InlineData(-200, 0, 100, 100)]
    [InlineData(0, 1080, 100, 100)]
    [InlineData(10, 10, 0, 100)]
    [InlineData(10, 10, 100, 0)]
    public void Outside_or_empty_region_fails(int x, int y, int w, int h)
        => CapturePlan.Clip(new Rect(x, y, w, h), Single).Should().BeNull();
}
