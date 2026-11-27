using System.Text.Json;

namespace WinCapture.Core.Settings;

/// <summary>JSON settings store backed by a file whose path is injected.</summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _path;
    private readonly IClock _clock;
    private readonly Action<string, Exception?>? _log;

    public JsonSettingsStore(string path, IClock? clock = null, Action<string, Exception?>? log = null)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _clock = clock ?? new SystemClock();
        _log = log;
    }

    public string Path => _path;

    /// <summary>Default location: %AppData%\WinCapture\settings.json.</summary>
    public static string GetDefaultPath() =>
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinCapture",
            "settings.json");

    public AppSettings Load()
    {
        if (!File.Exists(_path))
        {
            return new AppSettings();
        }

        string text;
        try
        {
            text = File.ReadAllText(_path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _log?.Invoke($"Could not read settings file '{_path}'; using defaults.", ex);
            return new AppSettings();
        }

        try
        {
            var loaded = JsonSerializer.Deserialize<AppSettings>(text, Options);
            if (loaded is null)
            {
                throw new JsonException("Settings JSON was null.");
            }

            return Normalize(loaded);
        }
        catch (JsonException ex)
        {
            _log?.Invoke($"Settings file '{_path}' is corrupt; using defaults and keeping a backup.", ex);
            Backup(text);
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var dir = System.IO.Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Write to a temp file first so a crash cannot leave a half-written settings file.
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(settings, Options));
        File.Move(tmp, _path, overwrite: true);
    }

    // Replaces explicit JSON nulls (e.g. "border": null) with defaults.
    private static AppSettings Normalize(AppSettings s) => s with
    {
        SaveFolder = s.SaveFolder ?? new AppSettings().SaveFolder,
        FileNamePattern = s.FileNamePattern ?? new AppSettings().FileNamePattern,
        FfmpegPath = s.FfmpegPath ?? "",
        Hotkeys = s.Hotkeys ?? new HotkeySettings(),
        Border = s.Border ?? new BorderSettings(),
        Capture = s.Capture ?? new CaptureSettings(),
        Recording = s.Recording ?? new RecordingSettings(),
        Gif = s.Gif ?? new GifSettings(),
    };

    private void Backup(string originalText)
    {
        var backupPath = $"{_path}.bak-{_clock.Now:yyyyMMddHHmmss}";
        try
        {
            File.WriteAllText(backupPath, originalText);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _log?.Invoke($"Could not write settings backup '{backupPath}'.", ex);
        }
    }
}
