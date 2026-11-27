using FluentAssertions;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "WinCaptureTests-" + Guid.NewGuid().ToString("N"));
    private readonly FixedClock _clock = new(new DateTime(2026, 3, 4, 5, 6, 7));

    private sealed class FixedClock(DateTime now) : IClock
    {
        public DateTime Now { get; } = now;
    }

    private string SettingsPath => Path.Combine(_dir, "settings.json");

    private JsonSettingsStore NewStore() => new(SettingsPath, _clock);

    private void WriteRaw(string text)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(SettingsPath, text);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void Defaults_MatchSpecSection7()
    {
        var s = new AppSettings();
        s.Version.Should().Be(1);
        s.SaveFolder.Should().Be("%USERPROFILE%\\Pictures\\WinCapture");
        s.UseTypeSubfolders.Should().BeFalse();
        s.FileNamePattern.Should().Be("WinCapture_{date}_{time}");
        s.RestoreLastRegion.Should().BeTrue();
        s.LastRegion.Should().Be(new RegionSettings { X = 0, Y = 0, Width = 0, Height = 0 });
        s.Hotkeys.SelectRegion.Should().Be("Ctrl+Alt+R");
        s.Hotkeys.Screenshot.Should().Be("Ctrl+Alt+S");
        s.Hotkeys.RecordVideo.Should().Be("Ctrl+Alt+V");
        s.Hotkeys.RecordGif.Should().Be("Ctrl+Alt+G");
        s.Hotkeys.ToggleBorder.Should().Be("Ctrl+Alt+H");
        s.Hotkeys.ClearRegion.Should().Be("Ctrl+Alt+X");
        s.Hotkeys.ToggleAppWindow.Should().Be("Ctrl+Alt+W");
        s.Border.Color.Should().Be("#FF3B30");
        s.Border.Thickness.Should().Be(2);
        s.Capture.IncludeCursor.Should().BeTrue();
        s.Capture.CopyToClipboard.Should().BeFalse();
        s.AutoHideAppWindow.Should().BeTrue();
        s.ReshowAppWindowAfterRecording.Should().BeFalse();
        s.Recording.Fps.Should().Be(30);
        s.Recording.Preset.Should().Be("Standard");
        s.Recording.MaxDurationSeconds.Should().Be(0);
        s.Gif.Fps.Should().Be(12);
        s.Gif.MaxWidth.Should().Be(800);
        s.FfmpegPath.Should().Be("");
        s.StartWithWindows.Should().BeFalse();
        s.OnboardingDone.Should().BeFalse();
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        NewStore().Load().Should().Be(new AppSettings());
    }

    [Fact]
    public void RoundTrip_Defaults_AreIdentical()
    {
        var store = NewStore();
        store.Save(new AppSettings());
        store.Load().Should().Be(new AppSettings());
    }

    [Fact]
    public void RoundTrip_ChangedValues_AreIdentical()
    {
        var changed = new AppSettings
        {
            SaveFolder = "D:\\Shots",
            UseTypeSubfolders = true,
            FileNamePattern = "Cap_{date}",
            RestoreLastRegion = false,
            LastRegion = new RegionSettings { X = -1920, Y = 10, Width = 640, Height = 480 },
            Hotkeys = new HotkeySettings { SelectRegion = "Ctrl+Shift+1", ToggleAppWindow = "Alt+F9" },
            Border = new BorderSettings { Color = "#00FF00", Thickness = 4 },
            Capture = new CaptureSettings { IncludeCursor = false, CopyToClipboard = true },
            AutoHideAppWindow = false,
            ReshowAppWindowAfterRecording = true,
            Recording = new RecordingSettings { Fps = 60, Preset = "HighQuality", MaxDurationSeconds = 120 },
            Gif = new GifSettings { Fps = 20, MaxWidth = 400 },
            FfmpegPath = "C:\\tools\\ffmpeg.exe",
            StartWithWindows = true,
            OnboardingDone = true,
        };
        var store = NewStore();
        store.Save(changed);
        store.Load().Should().Be(changed);
    }

    [Fact]
    public void Save_WritesCamelCaseKeysAndUnexpandedEnvVar()
    {
        NewStore().Save(new AppSettings());
        var text = File.ReadAllText(SettingsPath);
        text.Should().Contain("\"saveFolder\"").And.Contain("%USERPROFILE%").And.Contain("\"SelectRegion\"");
    }

    [Fact]
    public void Load_CorruptJson_ReturnsDefaultsAndKeepsBackup()
    {
        const string corrupt = "{\"saveFolder\":";
        WriteRaw(corrupt);

        var loaded = NewStore().Load();

        loaded.Should().Be(new AppSettings());
        var backup = Path.Combine(_dir, "settings.json.bak-20260304050607");
        File.Exists(backup).Should().BeTrue();
        File.ReadAllText(backup).Should().Be(corrupt);
    }

    [Fact]
    public void Load_UnknownKeys_AreIgnoredAndKnownKeysRead()
    {
        WriteRaw("{\"version\":1,\"mystery\":{\"a\":1},\"saveFolder\":\"E:\\\\X\",\"border\":{\"thickness\":5,\"glow\":true}}");

        var loaded = NewStore().Load();

        loaded.SaveFolder.Should().Be("E:\\X");
        loaded.Border.Thickness.Should().Be(5);
        loaded.Border.Color.Should().Be("#FF3B30");
    }

    [Fact]
    public void Load_PartialFile_FillsRestWithDefaults()
    {
        WriteRaw("{\"gif\":{\"fps\":5},\"onboardingDone\":true}");

        var loaded = NewStore().Load();

        loaded.Should().Be(new AppSettings
        {
            Gif = new GifSettings { Fps = 5, MaxWidth = 800 },
            OnboardingDone = true,
        });
    }

    [Fact]
    public void Load_NullSection_FallsBackToDefaults()
    {
        WriteRaw("{\"border\":null}");
        NewStore().Load().Border.Should().Be(new BorderSettings());
    }

    [Fact]
    public void Path_IsInjected_AndSaveCreatesMissingFolder()
    {
        var nested = Path.Combine(_dir, "a", "b", "s.json");
        new JsonSettingsStore(nested).Save(new AppSettings());
        File.Exists(nested).Should().BeTrue();
    }
}
