using FluentAssertions;
using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public sealed class RegionTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "WinCaptureRegionTests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTime Now { get; } = new(2026, 1, 1);
    }

    [Theory]
    [InlineData(500, 400, 100, 100)]
    [InlineData(100, 100, 500, 400)]
    [InlineData(100, 400, 500, 100)]
    [InlineData(500, 100, 100, 400)]
    public void Normalize_AllFourDirections(int sx, int sy, int ex, int ey)
    {
        RegionMath.Normalize(new PixelPoint(sx, sy), new PixelPoint(ex, ey))
            .Should().Be(new Rect(100, 100, 400, 300));
    }

    [Theory]
    [InlineData(31, 100, false)]
    [InlineData(100, 31, false)]
    [InlineData(31, 31, false)]
    [InlineData(0, 0, false)]
    [InlineData(32, 32, true)]
    [InlineData(32, 100, true)]
    public void IsValidRegion_MinimumIs32(int w, int h, bool expected)
    {
        RegionMath.IsValidRegion(new Rect(10, 10, w, h)).Should().Be(expected);
    }

    [Fact]
    public void Dpi_150Percent_ConvertsAndRoundTrips()
    {
        var dip = new DipRect(100, 100, 400, 300);
        var phys = DpiConverter.ToPhysical(dip, 1.5);
        phys.Should().Be(new Rect(150, 150, 600, 450));
        DpiConverter.ToDip(phys, 1.5).Should().Be(dip);
    }

    [Fact]
    public void Dpi_125Percent_RoundsEachComponent()
    {
        DpiConverter.ToPhysical(new DipRect(101, 101, 401, 301), 1.25)
            .Should().Be(new Rect(126, 126, 501, 376));
    }

    [Fact]
    public void Dpi_100Percent_NegativeCoordinates()
    {
        DpiConverter.ToPhysical(new DipRect(-1920, 0, 800, 600), 1.0)
            .Should().Be(new Rect(-1920, 0, 800, 600));
    }

    [Fact]
    public void Dpi_RoundsHalfAwayFromZero()
    {
        DpiConverter.ToPhysical(2.5, 1.0).Should().Be(3);
        DpiConverter.ToPhysical(-2.5, 1.0).Should().Be(-3);
    }

    [Fact]
    public void Dpi_ScaleFromDpi()
    {
        DpiConverter.ScaleFromDpi(144).Should().Be(1.5);
    }

    [Fact]
    public void BorderOuterRect_GrowsByThickness_AndDoesNotCoverRegion()
    {
        var region = new Rect(100, 100, 400, 300);
        var outer = RegionMath.GetBorderOuterRect(region, 2);
        outer.Should().Be(new Rect(98, 98, 404, 304));

        // Band = outer minus region. Inner pixels (region) are all strictly inside outer's inset by thickness.
        var inner = new Rect(outer.X + 2, outer.Y + 2, outer.Width - 4, outer.Height - 4);
        inner.Should().Be(region);
    }

    private static readonly Rect Virtual = new(-1920, 0, 4480, 1440);

    [Fact]
    public void Clamp_InsideRegion_Unchanged()
    {
        var r = new Rect(100, 100, 400, 300);
        RegionMath.ClampToVirtualScreen(r, Virtual).Should().Be(r);
    }

    [Fact]
    public void Clamp_NoOverlap_ShiftsInside()
    {
        // Saved on a monitor that is no longer connected (far right).
        var r = RegionMath.ClampToVirtualScreen(new Rect(9000, 5000, 400, 300), Virtual);
        r.Should().Be(new Rect(2160, 1140, 400, 300));
        Virtual.Intersects(r).Should().BeTrue();
    }

    [Fact]
    public void Clamp_PartlyOutside_ShiftsInside()
    {
        RegionMath.ClampToVirtualScreen(new Rect(-2000, -50, 400, 300), Virtual)
            .Should().Be(new Rect(-1920, 0, 400, 300));
    }

    [Fact]
    public void Clamp_LargerThanVirtualScreen_ShrinksToIt()
    {
        RegionMath.ClampToVirtualScreen(new Rect(0, 0, 9000, 9000), Virtual).Should().Be(Virtual);
    }

    [Fact]
    public void Rect_Intersects()
    {
        new Rect(0, 0, 10, 10).Intersects(new Rect(10, 0, 10, 10)).Should().BeFalse();
        new Rect(0, 0, 10, 10).Intersects(new Rect(9, 9, 10, 10)).Should().BeTrue();
    }
}
