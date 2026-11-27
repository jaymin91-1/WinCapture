using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinCapture.Core.Settings;

/// <summary>Capture mode selected in the control bar. Stored as a string in settings.json.</summary>
[JsonConverter(typeof(CaptureModeJsonConverter))]
public enum CaptureMode
{
    Screenshot,
    Video,
    Gif,
}

/// <summary>Writes the enum name; any unknown or non-string value is read as <see cref="CaptureMode.Screenshot"/>.</summary>
public sealed class CaptureModeJsonConverter : JsonConverter<CaptureMode>
{
    public override CaptureMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String
            && Enum.TryParse<CaptureMode>(reader.GetString(), ignoreCase: false, out var mode)
            && Enum.IsDefined(mode))
        {
            return mode;
        }

        reader.Skip(); // no-op for primitives; skips the whole value for objects/arrays
        return CaptureMode.Screenshot;
    }

    public override void Write(Utf8JsonWriter writer, CaptureMode value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
