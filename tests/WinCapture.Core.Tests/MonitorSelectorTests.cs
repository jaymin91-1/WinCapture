using FluentAssertions;
using WinCapture.Core.Geometry;

namespace WinCapture.Core.Tests;

public sealed class MonitorSelectorTests
{
    private static readonly Rect[] Right = [new(0, 0, 1920, 1080), new(1920, 0, 2560, 1440)];
    private static readonly Rect[] Left = [new(-1920, 0, 1920, 1080), new(0, 0, 2560, 1440)];

    [Theory]
    [InlineData(100, 100, 0)]
    [InlineData(3000, 500, 1)]
    [InlineData(1920, 0, 1)]
    [InlineData(1919, 1079, 0)]
    public void PicksMonitorContainingCursor(int x, int y, int expected) =>
        MonitorSelector.PickCursorMonitor(Right, new PixelPoint(x, y)).Should().Be(Right[expected]);

    [Theory]
    [InlineData(-1, 10, 0)]
    [InlineData(0, 10, 1)]
    public void SupportsNegativeCoordinates(int x, int y, int expected) =>
        MonitorSelector.PickCursorMonitor(Left, new PixelPoint(x, y)).Should().Be(Left[expected]);

    // Temporary fallback pending user decision: primary, else first monitor, else null.
    [Fact]
    public void Fallback_Primary_ThenFirst_ThenNull()
    {
        var outside = new PixelPoint(9999, 9999);
        MonitorSelector.PickCursorMonitor(Right, outside, Right[1]).Should().Be(Right[1]);
        MonitorSelector.PickCursorMonitor(Right, outside).Should().Be(Right[0]);
        MonitorSelector.PickCursorMonitor([], outside, Right[1]).Should().Be(Right[1]);
        MonitorSelector.PickCursorMonitor([], outside).Should().BeNull();
    }
}
