using FluentAssertions;
using WinCapture.Core.Presentation;

namespace WinCapture.Core.Tests;

/// <summary>Checks the real app icon file: valid multi-size ICO that includes the taskbar sizes.</summary>
public class IcoFileTests
{
    private static byte[] LoadAppIcon()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "WinCapture.sln")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        dir.Should().NotBeNull();
        return File.ReadAllBytes(Path.Combine(dir!, "src", "WinCapture.App", "Assets", "AppIcon.ico"));
    }

    [Fact]
    public void App_icon_contains_all_required_sizes()
    {
        var frames = IcoFile.ReadFrames(LoadAppIcon());

        frames.Select(f => f.Size).Should().Contain(new[] { 16, 24, 32, 48, 256 });
        frames.Should().OnlyContain(f => f.BitCount == 32);
    }

    [Fact]
    public void Small_frames_are_classic_dib_and_large_frame_is_png()
    {
        var frames = IcoFile.ReadFrames(LoadAppIcon());

        frames.Where(f => f.Size < 256).Should().OnlyContain(f => !f.IsPng);
        frames.Single(f => f.Size == 256).IsPng.Should().BeTrue();
    }

    [Fact]
    public void Pick_frame_returns_smallest_frame_not_smaller_than_requested()
    {
        var frames = IcoFile.ReadFrames(LoadAppIcon());

        IcoFile.PickFrame(frames, 16)!.Size.Should().Be(16);
        IcoFile.PickFrame(frames, 20)!.Size.Should().Be(24);
        IcoFile.PickFrame(frames, 1000)!.Size.Should().Be(256);
    }

    [Fact]
    public void Invalid_data_gives_no_frames()
    {
        IcoFile.ReadFrames(new byte[] { 1, 2, 3 }).Should().BeEmpty();
        IcoFile.PickFrame(Array.Empty<IcoFrame>(), 16).Should().BeNull();
    }
}
