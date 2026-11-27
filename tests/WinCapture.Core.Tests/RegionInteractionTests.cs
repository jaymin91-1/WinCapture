using FluentAssertions;
using WinCapture.Core.Geometry;
using Xunit;

namespace WinCapture.Core.Tests;

public class RegionInteractionTests
{
    private static readonly Rect Region = new(100, 100, 400, 300);
    private static readonly Rect Virtual = new(-1920, 0, 3840, 1080);

    [Theory]
    [InlineData(300, 250, RegionHit.Passthrough)]
    [InlineData(95, 200, RegionHit.Move)]
    [InlineData(502, 402, RegionHit.Resize)]
    [InlineData(80, 80, RegionHit.Outside)]
    public void HitTest_SpecExamples(int x, int y, RegionHit expected) =>
        RegionInteraction.HitTest(Region, new PixelPoint(x, y)).Should().Be(expected);

    [Fact]
    public void HitTest_GripWinsWhereItOverlapsBand()
    {
        // (505, 405) is inside the right/bottom band (8px) and inside the grip.
        RegionInteraction.HitTest(Region, new PixelPoint(505, 405)).Should().Be(RegionHit.Resize);
        // Just right of the band but still in the grip corner.
        RegionInteraction.HitTest(Region, new PixelPoint(511, 411)).Should().Be(RegionHit.Resize);
        RegionInteraction.HitTest(Region, new PixelPoint(512, 412)).Should().Be(RegionHit.Outside);
        // Band on the right edge away from the corner is move.
        RegionInteraction.HitTest(Region, new PixelPoint(505, 200)).Should().Be(RegionHit.Move);
    }

    [Fact]
    public void Move_ClampsToVirtualScreen()
    {
        var start = new Rect(0, 100, 400, 300);
        RegionInteraction.Move(start, 1700, 0, Virtual, false).X.Should().Be(1520);
        RegionInteraction.Move(start, 0, -150, Virtual, false).Y.Should().Be(0);
        RegionInteraction.Move(start, -5000, 0, Virtual, false).X.Should().Be(-1920);
    }

    [Fact]
    public void Resize_EnforcesMinimumSize()
    {
        var start = new Rect(100, 100, 400, 300);
        var r = RegionInteraction.Resize(start, -390, -290, Virtual, false);
        r.Should().Be(new Rect(100, 100, 32, 32));
    }

    [Fact]
    public void Resize_StopsAtVirtualScreenEdge()
    {
        var start = new Rect(100, 100, 400, 300);
        var r = RegionInteraction.Resize(start, 10000, 10000, Virtual, false);
        r.Width.Should().Be(1920 - 100);
        r.Height.Should().Be(1080 - 100);
        r.X.Should().Be(100);
        r.Y.Should().Be(100);
    }

    [Fact]
    public void Locked_ReturnsOriginalRegion()
    {
        RegionInteraction.Move(Region, 50, 50, Virtual, true).Should().Be(Region);
        RegionInteraction.Resize(Region, 50, 50, Virtual, true).Should().Be(Region);
    }

    [Theory]
    [InlineData(0, 0, 500, 500, true)]
    [InlineData(0, 0, 300, 300, false)]
    [InlineData(0, 0, 400, 400, false)]
    public void Overlaps_SpecExamples(int x, int y, int w, int h, bool expected) =>
        RegionInteraction.Overlaps(new Rect(x, y, w, h), new Rect(400, 400, 200, 200)).Should().Be(expected);

    [Fact]
    public void FormatTooltip_UsesInvariantFormat()
    {
        var old = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
            RegionInteraction.FormatTooltip(new Rect(-10, 20, 1920, 1080)).Should().Be("-10, 20, 1920 × 1080");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = old;
        }
    }
}
