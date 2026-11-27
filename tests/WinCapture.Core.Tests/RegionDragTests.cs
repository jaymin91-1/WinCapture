using FluentAssertions;
using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public class RegionDragTests
{
    private static readonly Rect Screen = new(-1920, 0, 3840, 1080);
    private static readonly Rect Start = new(100, 100, 400, 300);

    [Fact]
    public void Move_Uses_Delta_From_Drag_Start()
    {
        var drag = new RegionDrag(RegionHit.Move, Start, new PixelPoint(95, 200));
        drag.Compute(new PixelPoint(125, 190), Screen, locked: false).Should().Be(new Rect(130, 90, 400, 300));
        // Same cursor again gives the same result (no accumulation).
        drag.Compute(new PixelPoint(125, 190), Screen, locked: false).Should().Be(new Rect(130, 90, 400, 300));
    }

    [Fact]
    public void Resize_Uses_Delta_From_Drag_Start_And_Min_Size()
    {
        var drag = new RegionDrag(RegionHit.Resize, Start, new PixelPoint(502, 402));
        drag.Compute(new PixelPoint(552, 392), Screen, false).Should().Be(new Rect(100, 100, 450, 290));
        drag.Compute(new PixelPoint(0, 0), Screen, false).Should().Be(new Rect(100, 100, 32, 32));
    }

    [Fact]
    public void Locked_Returns_Start_Region()
    {
        new RegionDrag(RegionHit.Move, Start, new PixelPoint(0, 0)).Compute(new PixelPoint(50, 50), Screen, true).Should().Be(Start);
        new RegionDrag(RegionHit.Resize, Start, new PixelPoint(0, 0)).Compute(new PixelPoint(50, 50), Screen, true).Should().Be(Start);
    }

    [Fact]
    public void InputWindowRect_Covers_8px_Band_And_Grip()
    {
        var w = BorderLayout.GetInputWindowRect(Start, 2);
        w.Should().Be(new Rect(92, 92, 420, 320)); // right/bottom = max(region+8, grip end 512/412)
        BorderLayout.GetBandRect(Start).Should().Be(new Rect(92, 92, 416, 316));
    }

    [Fact]
    public void LockedMessage_Text()
    {
        RegionInteraction.LockedMessage.Should().Be("녹화 중에는 영역을 변경할 수 없습니다");
    }

    [Fact]
    public void UpdateRegion_Refused_While_Locked_And_Applied_When_Unlocked()
    {
        var state = new RegionState();
        var c = new RegionCoordinator(state, new RegionCoordinatorTests.FakeScreen());

        state.SetLocked(true);
        c.UpdateRegion(Start).Should().BeFalse();
        state.Region.Should().BeNull();

        state.SetLocked(false);
        c.UpdateRegion(Start).Should().BeTrue();
        c.UpdateRegion(new Rect(10, 10, 200, 200)).Should().BeTrue();
        state.Region.Should().Be(new Rect(10, 10, 200, 200));
    }
}
