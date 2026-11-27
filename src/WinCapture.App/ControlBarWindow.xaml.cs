using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using WinCapture.Core.Settings;

namespace WinCapture.App;

/// <summary>
/// Small always-available control bar. It is a normal app window (taskbar visible, not capture-excluded,
/// not an OverlayWindowBase): it is not part of the capture overlay set, and the auto-hide rule handles
/// keeping it out of captures.
/// </summary>
public partial class ControlBarWindow : FluentWindow
{
    private bool _allowClose;
    private bool _initializing = true;

    public ControlBarWindow(CaptureMode initialMode)
    {
        SystemThemeWatcher.Watch(this, WindowBackdropType.None, updateAccents: false);
        InitializeComponent();

        Mode = initialMode;
        (initialMode switch
        {
            CaptureMode.Video => VideoModeButton,
            CaptureMode.Gif => GifModeButton,
            _ => ScreenshotModeButton,
        }).IsChecked = true;
        _initializing = false;
        RefreshCaptureButton();
    }

    public CaptureMode Mode { get; private set; }

    public event EventHandler? SelectRegionRequested;

    public event EventHandler<CaptureMode>? CaptureRequested;

    public event EventHandler? SettingsRequested;

    public event EventHandler? ToggleWindowsRequested;

    /// <summary>Raised when the user picks another mode (persisted by the app).</summary>
    public event EventHandler<CaptureMode>? ModeChanged;

    /// <summary>Lets the window really close (called only when the app is exiting).</summary>
    public void AllowClose() => _allowClose = true;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_allowClose)
        {
            // X hides the bar; the app keeps running in the tray.
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    // [Capture] is enabled only for modes that CaptureAvailability reports as implemented.
    private void RefreshCaptureButton()
    {
        CaptureButton.IsEnabled = CaptureAvailability.IsImplemented(Mode);
        CaptureButton.ToolTip = CaptureAvailability.Tooltip(Mode);
    }

    private void OnModeChecked(object sender, RoutedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        Mode = sender == VideoModeButton ? CaptureMode.Video
            : sender == GifModeButton ? CaptureMode.Gif
            : CaptureMode.Screenshot;
        RefreshCaptureButton();
        ModeChanged?.Invoke(this, Mode);
    }

    private void OnSelectRegionClick(object sender, RoutedEventArgs e) => SelectRegionRequested?.Invoke(this, EventArgs.Empty);

    private void OnCaptureClick(object sender, RoutedEventArgs e) => CaptureRequested?.Invoke(this, Mode);

    private void OnSettingsClick(object sender, RoutedEventArgs e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

    private void OnToggleWindowsClick(object sender, RoutedEventArgs e) => ToggleWindowsRequested?.Invoke(this, EventArgs.Empty);
}
