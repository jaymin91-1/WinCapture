using FluentAssertions;
using WinCapture.Core.Geometry;

namespace WinCapture.Core.Tests;

public class BorderLayoutTests
{
    [Fact]
    public void GetWindowRect_Covers_Border_And_Grip()
    {
        var region = new Rect(100, 100, 400, 300);
        var w = BorderLayout.GetWindowRect(region, 2);

        w.Should().Be(new Rect(98, 98, 414, 314));
        w.Contains(new PixelPoint(98, 98)).Should().BeTrue();
        w.Contains(new PixelPoint(511, 411)).Should().BeTrue();
    }

    [Fact]
    public void GetGripRect_Is_12x12_Outside_Region()
    {
        var region = new Rect(100, 100, 400, 300);
        var g = BorderLayout.GetGripRect(region);

        g.Should().Be(new Rect(500, 400, 12, 12));
        g.Intersects(region).Should().BeFalse();
    }

    [Fact]
    public void RegionState_Set_Toggle_Clear()
    {
        var s = new RegionState();
        int changes = 0;
        s.Changed += (_, _) => changes++;

        s.ToggleBorder();
        s.IsBorderVisible.Should().BeTrue();
        changes.Should().Be(0);

        s.SetRegion(new Rect(10, 10, 100, 100));
        s.ShouldShowBorder.Should().BeTrue();

        s.ToggleBorder();
        s.ShouldShowBorder.Should().BeFalse();
        s.HasRegion.Should().BeTrue();

        s.ToggleBorder();
        s.ShouldShowBorder.Should().BeTrue();

        s.ClearRegion();
        s.HasRegion.Should().BeFalse();
        s.ShouldShowBorder.Should().BeFalse();
        changes.Should().Be(4);
    }
}
