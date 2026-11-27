namespace WinCapture.Core.Settings;

public interface ISettingsStore
{
    /// <summary>Loads settings. Never throws for missing or corrupt files; returns defaults instead.</summary>
    AppSettings Load();

    void Save(AppSettings settings);
}

/// <summary>Clock abstraction so backup file names are testable.</summary>
public interface IClock
{
    DateTime Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}
