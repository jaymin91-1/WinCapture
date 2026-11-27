using FluentAssertions;
using WinCapture.Core.Storage;

namespace WinCapture.Core.Tests;

public sealed class CaptureFileNamerTests
{
    private sealed class FakeFs : ISaveFileSystem
    {
        public HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool FileExists(string path) => Files.Contains(path);

        public IReadOnlyList<string> GetFileNames(string folder) =>
            Files.Where(f => f.StartsWith(folder + "\\", StringComparison.OrdinalIgnoreCase))
                 .Select(f => f[(folder.Length + 1)..]).ToList();
    }

    private static readonly DateTime Now = new(2026, 11, 27, 9, 30, 5);

    private static string Build(string pattern, FakeFs fs, int w = 400, int h = 300, string type = "png",
        string folder = "C:\\Shots", bool sub = false, Func<string, string>? env = null) =>
        CaptureFileNamer.BuildPath(pattern, Now, w, h, type, folder, sub, env ?? (s => s), fs);

    [Fact]
    public void DateTimePattern_ProducesSpecName()
    {
        Build("WinCapture_{date}_{time}", new FakeFs()).Should().Be("C:\\Shots\\WinCapture_20261127_093005.png");
    }

    [Fact]
    public void ExistingName_GetsSuffixBeforeExtension()
    {
        var fs = new FakeFs();
        fs.Files.Add("C:\\Shots\\WinCapture_20261127_093005.png");
        Build("WinCapture_{date}_{time}", fs).Should().Be("C:\\Shots\\WinCapture_20261127_093005_1.png");
        fs.Files.Add("C:\\Shots\\WinCapture_20261127_093005_1.png");
        Build("WinCapture_{date}_{time}", fs).Should().Be("C:\\Shots\\WinCapture_20261127_093005_2.png");
    }

    [Fact]
    public void WidthHeightTypeTokens_AreReplaced()
    {
        Build("shot_{w}x{h}_{type}", new FakeFs()).Should().Be("C:\\Shots\\shot_400x300_png.png");
    }

    [Fact]
    public void InvalidChars_BecomeUnderscore()
    {
        Build("a:b*c", new FakeFs()).Should().Be("C:\\Shots\\a_b_c.png");
        CaptureFileNamer.Sanitize("\\/:*?\"<>|").Should().Be("_________");
    }

    [Fact]
    public void TypeSubfolders_ControlsPath()
    {
        Build("x", new FakeFs(), sub: true).Should().Be("C:\\Shots\\Screenshots\\x.png");
        Build("x", new FakeFs(), sub: false).Should().Be("C:\\Shots\\x.png");
    }

    [Fact]
    public void SaveFolder_UsesInjectedEnvironmentExpansion()
    {
        Build("x", new FakeFs(), folder: "%USERPROFILE%\\Pictures\\WinCapture\\",
            env: s => s.Replace("%USERPROFILE%", "D:\\u"))
            .Should().Be("D:\\u\\Pictures\\WinCapture\\x.png");
    }

    [Fact]
    public void Counter_NoFiles_StartsAt0001()
    {
        Build("shot_{counter}", new FakeFs()).Should().Be("C:\\Shots\\shot_0001.png");
    }

    [Fact]
    public void Counter_UsesMaxPlusOne_IgnoringOtherPatternsAndGaps()
    {
        var fs = new FakeFs();
        fs.Files.Add("C:\\Shots\\shot_0001.png");
        fs.Files.Add("C:\\Shots\\shot_0007.png");
        fs.Files.Add("C:\\Shots\\other_0099.png");
        fs.Files.Add("C:\\Shots\\shot_0100.gif");
        Build("shot_{counter}", fs).Should().Be("C:\\Shots\\shot_0008.png");
    }

    [Fact]
    public void Counter_ScopedToTypeSubfolder()
    {
        var fs = new FakeFs();
        fs.Files.Add("C:\\Shots\\shot_0005.png");
        fs.Files.Add("C:\\Shots\\Screenshots\\shot_0002.png");
        Build("shot_{counter}", fs, sub: true).Should().Be("C:\\Shots\\Screenshots\\shot_0003.png");
    }

    [Fact]
    public void Counter_PastFourDigits_KeepsGrowing()
    {
        var fs = new FakeFs();
        fs.Files.Add("C:\\Shots\\shot_9999.png");
        Build("shot_{counter}", fs).Should().Be("C:\\Shots\\shot_10000.png");
    }

    [Fact]
    public void Counter_WithOtherTokens_ResolvesThemFirst()
    {
        var fs = new FakeFs();
        fs.Files.Add("C:\\Shots\\20261127_0004_400x300.png");
        fs.Files.Add("C:\\Shots\\20261126_0009_400x300.png");
        Build("{date}_{counter}_{w}x{h}", fs).Should().Be("C:\\Shots\\20261127_0005_400x300.png");
    }
}
