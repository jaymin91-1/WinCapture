using FluentAssertions;
using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public sealed class RegionCoordinatorTests
{
    private sealed class MemoryStore : ISettingsStore
    {
        public AppSettings Current { get; set; } = new();
        public int Saves { get; private set; }
        public bool ThrowOnSave { get; set; }

        public AppSettings Load() => Current;

        public void Save(AppSettings settings)
        {
            if (ThrowOnSave)
            {
                throw new IOException("disk full");
            }

            Saves++;
            Current = settings;
        }
    }

    private static readonly Rect Virtual = new(-1920, 0, 3840, 1080);

    private static (RegionCoordinator c, MemoryStore s) Make(bool restore = true)
    {
        var store = new MemoryStore { Current = new AppSettings { RestoreLastRegion = restore } };
        return (new RegionCoordinator(new RegionState(), store), store);
    }

    [Fact]
    public void Commit_Valid_ReplacesAndSaves()
    {
        var (c, s) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300)).Should().BeTrue();
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
        s.Current.LastRegion.Should().BeEquivalentTo(new RegionSettings { X = 100, Y = 100, Width = 400, Height = 300 });
    }

    [Fact]
    public void Commit_Null_Esc_KeepsPreviousRegionAndSettings()
    {
        var (c, s) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300));
        int saves = s.Saves;
        c.CommitSelection(null).Should().BeFalse();
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
        s.Saves.Should().Be(saves);
    }

    [Theory]
    [InlineData(31, 100)]
    [InlineData(100, 31)]
    [InlineData(0, 0)]
    public void Commit_TooSmall_KeepsPreviousRegion(int w, int h)
    {
        var (c, s) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300));
        c.CommitSelection(new Rect(5, 5, w, h)).Should().BeFalse();
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
        s.Current.LastRegion.Width.Should().Be(400);
    }

    [Fact]
    public void Commit_TooSmall_WithNoPreviousRegion_StaysEmpty()
    {
        var (c, _) = Make();
        c.CommitSelection(new Rect(5, 5, 10, 10));
        c.State.HasRegion.Should().BeFalse();
    }

    [Fact]
    public void Commit_RestoreOff_DoesNotSave()
    {
        var (c, s) = Make(restore: false);
        c.CommitSelection(new Rect(100, 100, 400, 300)).Should().BeTrue();
        c.State.HasRegion.Should().BeTrue();
        s.Saves.Should().Be(0);
    }

    [Fact]
    public void Commit_SaveFailure_DoesNotThrow_AndIsLogged()
    {
        var store = new MemoryStore { ThrowOnSave = true };
        Exception? logged = null;
        var c = new RegionCoordinator(new RegionState(), store, (_, ex) => logged = ex);
        c.CommitSelection(new Rect(0, 0, 100, 100)).Should().BeTrue();
        logged.Should().BeOfType<IOException>();
    }

    [Fact]
    public void Restore_SavedRegion_IsApplied()
    {
        var (c, s) = Make();
        s.Current = s.Current with { LastRegion = new RegionSettings { X = 100, Y = 100, Width = 400, Height = 300 } };
        c.Restore(Virtual).Should().BeTrue();
        c.State.Region.Should().Be(new Rect(100, 100, 400, 300));
    }

    [Fact]
    public void Restore_OffscreenRegion_IsClampedIntoVirtualScreen()
    {
        var (c, s) = Make();
        s.Current = s.Current with { LastRegion = new RegionSettings { X = 5000, Y = 100, Width = 400, Height = 300 } };
        c.Restore(Virtual).Should().BeTrue();
        c.State.Region.Should().Be(new Rect(1520, 100, 400, 300));
    }

    [Fact]
    public void Restore_OptionOff_DoesNothing()
    {
        var (c, s) = Make(restore: false);
        s.Current = s.Current with { LastRegion = new RegionSettings { X = 100, Y = 100, Width = 400, Height = 300 } };
        c.Restore(Virtual).Should().BeFalse();
        c.State.HasRegion.Should().BeFalse();
    }

    [Fact]
    public void Restore_ClearedOrTooSmallSavedRegion_IsNotRestored()
    {
        var (c, s) = Make();
        c.Restore(Virtual).Should().BeFalse(); // default 0,0,0,0
        s.Current = s.Current with { LastRegion = new RegionSettings { X = 10, Y = 10, Width = 20, Height = 20 } };
        c.Restore(Virtual).Should().BeFalse();
        c.State.HasRegion.Should().BeFalse();
    }

    [Fact]
    public void Restore_ShrunkBelowMinimumByVirtualScreen_IsNotRestored()
    {
        var (c, s) = Make();
        s.Current = s.Current with { LastRegion = new RegionSettings { X = 0, Y = 0, Width = 400, Height = 300 } };
        c.Restore(new Rect(0, 0, 400, 20)).Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesRegionAndEmptiesSavedLastRegion_ThenNoRestore()
    {
        var (c, s) = Make();
        c.CommitSelection(new Rect(100, 100, 400, 300));
        c.Clear();
        c.State.HasRegion.Should().BeFalse();
        s.Current.LastRegion.Should().BeEquivalentTo(new RegionSettings { X = 0, Y = 0, Width = 0, Height = 0 });
        new RegionCoordinator(new RegionState(), s).Restore(Virtual).Should().BeFalse();
    }

    [Fact]
    public void FormatSizeLabel_UsesTimesSign() =>
        RegionCoordinator.FormatSizeLabel(new Rect(0, 0, 800, 600)).Should().Be("800 × 600");
}
