using FluentAssertions;
using WinCapture.Core.Settings;

namespace WinCapture.Core.Tests;

public sealed class CaptureModeTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "WinCaptureTests-" + Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_dir, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    private void WriteRaw(string text)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(SettingsPath, text);
    }

    [Fact]
    public void Default_is_Screenshot() => new AppSettings().CaptureMode.Should().Be(CaptureMode.Screenshot);

    [Theory]
    [InlineData(CaptureMode.Screenshot)]
    [InlineData(CaptureMode.Video)]
    [InlineData(CaptureMode.Gif)]
    public void Saved_mode_round_trips_and_is_written_as_a_string(CaptureMode mode)
    {
        var store = new JsonSettingsStore(SettingsPath);
        store.Save(new AppSettings { CaptureMode = mode });

        File.ReadAllText(SettingsPath).Should().Contain($"\"captureMode\": \"{mode}\"");
        store.Load().CaptureMode.Should().Be(mode);
    }

    [Theory]
    [InlineData("{\"captureMode\":\"Banana\"}")]
    [InlineData("{\"captureMode\":\"\"}")]
    [InlineData("{\"captureMode\":42}")]
    [InlineData("{\"captureMode\":null}")]
    [InlineData("{\"captureMode\":{\"a\":1}}")]
    [InlineData("{\"autoHideAppWindow\":false,\"reshowAppWindowAfterRecording\":true}")]
    public void Unknown_value_or_missing_key_restores_Screenshot(string json)
    {
        WriteRaw(json);
        var loaded = new JsonSettingsStore(SettingsPath).Load();
        loaded.CaptureMode.Should().Be(CaptureMode.Screenshot);
        Directory.GetFiles(_dir).Should().ContainSingle(); // not treated as a corrupt file (no backup)
    }

    [Fact]
    public void Other_settings_survive_an_unknown_captureMode()
    {
        WriteRaw("{\"captureMode\":\"Banana\",\"saveFolder\":\"D:\\\\Shots\"}");
        new JsonSettingsStore(SettingsPath).Load().SaveFolder.Should().Be("D:\\Shots");
    }

    [Fact]
    public void Removed_auto_hide_keys_in_an_old_file_are_ignored_without_warnings()
    {
        WriteRaw("{\"captureMode\":\"Gif\",\"saveFolder\":\"D:\\\\Shots\",\"autoHideAppWindow\":true,\"reshowAppWindowAfterRecording\":true}");
        var logs = new List<string>();
        var loaded = new JsonSettingsStore(SettingsPath, log: (m, _) => logs.Add(m)).Load();

        loaded.CaptureMode.Should().Be(CaptureMode.Gif);
        loaded.SaveFolder.Should().Be("D:\\Shots");
        logs.Should().BeEmpty();
        Directory.GetFiles(_dir).Should().ContainSingle(); // not treated as corrupt
    }
}
