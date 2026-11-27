using FluentAssertions;
using WinCapture.Core.Geometry;

namespace WinCapture.Core.Tests;

public sealed class RegionCoordinatorTests
{
    internal sealed class FakeScreen : IScreenInfo
    {
        public List<Rect> Monitors { get; set; } = [new Rect(0, 0, 1920, 1080), new Rect(1920, 0, 2560, 1440)];
        public Rect? Primary { get; set; } = new Rect(0, 0, 1920, 1080);
        public PixelPoint Cursor { get; set; } = new(100, 100);
        public IReadOnlyList<Rect> GetMonitors() => Monitors;
        public Rect? GetPrimaryMonitor() => Primary;
        public PixelPoint GetCursorPosition() => Cursor;
    }

    private static readonly Rect First = new(0, 0, 1920, 1080);
    private static readonly Rect Second = new(1920, 0, 2560, 1440);

    private static (RegionCoordinator c, FakeScreen s) Make(PixelPoint? cursor = null)
    {
        var screen = new FakeScreen { Cursor = cursor ?? new PixelPoint(100, 100) };
        return (new RegionCoordinator(new RegionState(), screen), screen);
    }

    [Fact]
    public void NewState_IsFullScreen_OnInjectedCursorMonitor()
    {
        var (c, _) = Make(new PixelPoint(3000, 500));
        c.State.Mode.Should().Be(RegionMode.FullScreen);
        c.State.HasRegion.Should().BeFalse();
        c.State.CaptureRect.Should().Be(Second);
        c.State.ShouldShowBorder.Should().BeFalse();
    }

    [Fact]
    public void Clear_FromSpecified_ReturnsToFullScreen_OnCurrentCursorMonitor_AndRaisesChanged()
    {
        var (c, s) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300));
        c.State.Mode.Should().Be(RegionMode.Specified);
        int changes = 0;
        c.State.Changed += (_, _) => changes++;

        s.Cursor = new PixelPoint(3000, 500);
        c.Clear();

        c.State.Mode.Should().Be(RegionMode.FullScreen);
        c.State.CaptureRect.Should().Be(Second);
        changes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clear_WhenAlreadyFullScreen_RePicksCursorMonitor()
    {
        var (c, s) = Make();
        c.State.CaptureRect.Should().Be(First);
        s.Cursor = new PixelPoint(2000, 10);
        c.Clear();
        c.State.CaptureRect.Should().Be(Second);
    }

    [Fact]
    public void Commit_Valid_BecomesSpecified()
    {
        var (c, _) = Make();
        c.CommitSelection(RegionMath.Normalize(new PixelPoint(500, 400), new PixelPoint(100, 100))).Should().BeTrue();
        c.State.Mode.Should().Be(RegionMode.Specified);
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
        c.State.CaptureRect.Should().Be(new Rect(100, 100, 400, 300));
    }

    [Fact]
    public void Commit_Null_Esc_KeepsSpecifiedRegion()
    {
        var (c, _) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300));
        c.CommitSelection(null).Should().BeFalse();
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
    }

    [Theory]
    [InlineData(31, 100)]
    [InlineData(100, 31)]
    [InlineData(0, 0)]
    public void Commit_TooSmall_KeepsSpecifiedRegion(int w, int h)
    {
        var (c, _) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300));
        c.CommitSelection(new Rect(5, 5, w, h)).Should().BeFalse();
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
    }

    [Fact]
    public void Commit_TooSmallOrCancelled_KeepsFullScreen()
    {
        var (c, _) = Make();
        c.CommitSelection(new Rect(5, 5, 10, 10)).Should().BeFalse();
        c.CommitSelection(null).Should().BeFalse();
        c.State.Mode.Should().Be(RegionMode.FullScreen);
        c.State.CaptureRect.Should().Be(First);
    }

    [Fact]
    public void FullScreen_BorderHidden_EvenWhenToggleIsOn_SpecifiedShowsOnlyWhenToggleOn()
    {
        var (c, _) = Make();
        c.State.IsBorderVisible.Should().BeTrue();
        c.State.ShouldShowBorder.Should().BeFalse();
        c.State.ToggleBorder(); // no effect without a region
        c.State.IsBorderVisible.Should().BeTrue();
        c.State.ShouldShowBorder.Should().BeFalse();

        c.CommitSelection(new Rect(100, 100, 400, 300));
        c.State.ShouldShowBorder.Should().BeTrue();
        c.State.ToggleBorder();
        c.State.ShouldShowBorder.Should().BeFalse();
        c.State.ToggleBorder();
        c.Clear();
        c.State.ShouldShowBorder.Should().BeFalse();
    }

    [Fact]
    public void Startup_PicksPrimary_WhenCursorOutsideEveryMonitor()
    {
        var screen = new FakeScreen { Cursor = new PixelPoint(-5000, -5000), Primary = Second };
        new RegionCoordinator(new RegionState(), screen).State.CaptureRect.Should().Be(Second);
    }

    [Fact]
    public void FormatSizeLabel_UsesTimesSign() =>
        RegionCoordinator.FormatSizeLabel(new Rect(0, 0, 800, 600)).Should().Be("800 × 600");
}
