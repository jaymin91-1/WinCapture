using System.Text.Json.Serialization;

namespace WinCapture.Core.Settings;

/// <summary>Application settings. Shape and defaults follow SPEC section 7.</summary>
public sealed record AppSettings
{
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    // Stored as-is; environment variables are expanded at use time, never written back.
    [JsonPropertyName("saveFolder")]
    public string SaveFolder { get; init; } = "%USERPROFILE%\\Pictures\\WinCapture";

    [JsonPropertyName("useTypeSubfolders")]
    public bool UseTypeSubfolders { get; init; }

    [JsonPropertyName("fileNamePattern")]
    public string FileNamePattern { get; init; } = "WinCapture_{date}_{time}";

    [JsonPropertyName("hotkeys")]
    public HotkeySettings Hotkeys { get; init; } = new();

    [JsonPropertyName("border")]
    public BorderSettings Border { get; init; } = new();

    [JsonPropertyName("capture")]
    public CaptureSettings Capture { get; init; } = new();

    [JsonPropertyName("captureMode")]
    public CaptureMode CaptureMode { get; init; } = CaptureMode.Screenshot;

    [JsonPropertyName("recording")]
    public RecordingSettings Recording { get; init; } = new();

    [JsonPropertyName("gif")]
    public GifSettings Gif { get; init; } = new();

    [JsonPropertyName("ffmpegPath")]
    public string FfmpegPath { get; init; } = "";

    [JsonPropertyName("startWithWindows")]
    public bool StartWithWindows { get; init; }

    [JsonPropertyName("onboardingDone")]
    public bool OnboardingDone { get; init; }
}

/// <summary>Hotkey strings only; parsing is done in a later session.</summary>
public sealed record HotkeySettings
{
    [JsonPropertyName("SelectRegion")]
    public string SelectRegion { get; init; } = "Ctrl+Alt+R";

    [JsonPropertyName("Screenshot")]
    public string Screenshot { get; init; } = "Ctrl+Alt+S";

    [JsonPropertyName("RecordVideo")]
    public string RecordVideo { get; init; } = "Ctrl+Alt+V";

    [JsonPropertyName("RecordGif")]
    public string RecordGif { get; init; } = "Ctrl+Alt+G";

    [JsonPropertyName("ToggleBorder")]
    public string ToggleBorder { get; init; } = "Ctrl+Alt+H";

    [JsonPropertyName("ClearRegion")]
    public string ClearRegion { get; init; } = "Ctrl+Alt+X";

    [JsonPropertyName("ToggleAppWindow")]
    public string ToggleAppWindow { get; init; } = "Ctrl+Alt+W";
}

public sealed record BorderSettings
{
    [JsonPropertyName("color")]
    public string Color { get; init; } = "#FF3B30";

    [JsonPropertyName("thickness")]
    public int Thickness { get; init; } = 2;
}

public sealed record CaptureSettings
{
    [JsonPropertyName("includeCursor")]
    public bool IncludeCursor { get; init; } = true;

    [JsonPropertyName("copyToClipboard")]
    public bool CopyToClipboard { get; init; }
}

public sealed record RecordingSettings
{
    [JsonPropertyName("fps")]
    public int Fps { get; init; } = 30;

    [JsonPropertyName("preset")]
    public string Preset { get; init; } = "Standard";

    [JsonPropertyName("maxDurationSeconds")]
    public int MaxDurationSeconds { get; init; }
}

public sealed record GifSettings
{
    [JsonPropertyName("fps")]
    public int Fps { get; init; } = 12;

    [JsonPropertyName("maxWidth")]
    public int MaxWidth { get; init; } = 800;
}
