using WinCapture.Core.Geometry;
using WinCapture.Core.Settings;

namespace WinCapture.App.Overlay;

/// <summary>Shows/hides/moves the border window according to <see cref="RegionState"/>. UI thread only.</summary>
public sealed class BorderController : IDisposable
{
    private readonly RegionState _state;
    private readonly ISettingsStore _settings;
    private readonly RegionCoordinator _coordinator;
    private BorderWindow? _window;

    public BorderController(RegionState state, ISettingsStore settings, RegionCoordinator coordinator)
    {
        _state = state;
        _settings = settings;
        _coordinator = coordinator;
        _state.Changed += OnChanged;
    }

    public void Dispose()
    {
        _state.Changed -= OnChanged;
        _window?.Close();
        _window = null;
    }

    private void OnChanged(object? sender, EventArgs e)
    {
        if (!_state.ShouldShowBorder || _state.Region is not { } region)
        {
            _window?.Hide();
            return;
        }

        _window ??= new BorderWindow(_coordinator);
        _window.SetRegion(region, _settings.Load().Border);
        if (!_window.IsVisible)
        {
            _window.Show(); // ShowActivated=false: never steals focus
        }
    }
}
