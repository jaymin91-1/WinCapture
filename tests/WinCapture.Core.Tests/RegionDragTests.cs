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
    public void UpdateRegion_Refused_While_Locked_And_Persists_Only_When_Asked()
    {
        var store = new FakeStore();
        var state = new RegionState();
        var c = new RegionCoordinator(state, store);

        state.SetLocked(true);
        c.UpdateRegion(Start, persist: true).Should().BeFalse();
        state.Region.Should().BeNull();

        state.SetLocked(false);
        c.UpdateRegion(Start, persist: false).Should().BeTrue();
        store.Saves.Should().Be(0);
        c.UpdateRegion(new Rect(10, 10, 200, 200), persist: true).Should().BeTrue();
        store.Saves.Should().Be(1);
        store.Current.LastRegion.Should().Be(new RegionSettings { X = 10, Y = 10, Width = 200, Height = 200 });
    }

    [Fact]
    public void UpdateRegion_Does_Not_Persist_When_Restore_Off()
    {
        var store = new FakeStore { Current = new AppSettings { RestoreLastRegion = false } };
        var c = new RegionCoordinator(new RegionState(), store);
        c.UpdateRegion(Start, persist: true).Should().BeTrue();
        store.Saves.Should().Be(0);
    }

    private sealed class FakeStore : ISettingsStore
    {
        public AppSettings Current { get; set; } = new();
        public int Saves { get; private set; }
        public AppSettings Load() => Current;
        public void Save(AppSettings settings) { Current = settings; Saves++; }
    }
}
