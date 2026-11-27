using System.ComponentModel;
using System.Windows;
using Serilog;
using Wpf.Ui.Controls;
using WinCapture.Core.Geometry;
using WinCapture.Core.Presentation;
using WinCapture.Core.Settings;

namespace WinCapture.App;

/// <summary>
/// Small always-available control bar. It is a normal app window (taskbar visible, not capture-excluded,
/// not an OverlayWindowBase): it is not part of the capture overlay set, and the auto-hide rule handles
/// keeping it out of captures. Fixed size: SizeToContent + ResizeMode=CanMinimize + FixedSizeWindow (no resize, no maximize).
/// </summary>
public partial class ControlBarWindow : FluentWindow
{
    private readonly RegionState _state;
    private readonly IScreenInfo _screen;
    private bool _allowClose;
    private bool _initializing = true;
    private bool _isRecording;

    public ControlBarWindow(CaptureMode initialMode, RegionState state, IScreenInfo screen)
    {
        _state = state;
        _screen = screen;
        AppWindowIcon.Attach(this);
        InitializeComponent();
        FixedSizeWindow.Attach(this);

        Mode = initialMode;
        (initialMode switch
        {
            CaptureMode.Video => VideoModeButton,
            CaptureMode.Gif => GifModeButton,
            _ => ScreenshotModeButton,
        }).IsChecked = true;
        _initializing = false;
        RefreshCaptureButton();

        // The status card follows the region state; the subscription ends when the window is really closed
        // (the app itself only closes it on exit).
        _state.Changed += OnRegionStateChanged;
        Closed += (_, _) => _state.Changed -= OnRegionStateChanged;
        RefreshStatus();
    }

    public CaptureMode Mode { get; private set; }

    public event EventHandler? SelectRegionRequested;

    public event EventHandler? ClearRegionRequested;

    public event EventHandler<CaptureMode>? CaptureRequested;

    public event EventHandler? SettingsRequested;

    public event EventHandler? HotkeysRequested;

    public event EventHandler? ToggleWindowsRequested;

    /// <summary>Raised when the user picks another mode (persisted by the app).</summary>
    public event EventHandler<CaptureMode>? ModeChanged;

    /// <summary>Recording state for the [Capture] label (recording itself arrives in sessions 4-5).</summary>
    public void SetRecording(bool isRecording)
    {
        _isRecording = isRecording;
        RefreshCaptureButton();
    }

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

    private void OnRegionStateChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.CheckAccess())
        {
            RefreshStatus();
        }
        else
        {
            Dispatcher.BeginInvoke(RefreshStatus);
        }
    }

    // Strings come from the Core pure functions; this only assigns them.
    private void RefreshStatus()
    {
        var mode = _state.Mode;
        StatusTitleText.Text = ControlBarText.StateTitle(mode);
        BorderChipText.Text = ControlBarText.BorderChip(mode, _state.IsBorderVisible);
        StatusDot.SetResourceReference(
            System.Windows.Shapes.Shape.FillProperty,
            mode == RegionMode.Specified ? "WarningBrush" : "KeycapBorderBrush");

        if (_state.CaptureRect is not { } rect)
        {
            StatusCoordinatesText.Text = "—";
            return;
        }

        int? monitorNumber = null;
        if (mode == RegionMode.FullScreen)
        {
            try
            {
                monitorNumber = MonitorSelector.MonitorNumber(_screen.GetMonitors(), rect);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Reading the monitor list for the status card failed");
            }
        }

        StatusCoordinatesText.Text = ControlBarText.Coordinates(mode, rect, monitorNumber);
    }

    // [Capture] is enabled only for modes that CaptureAvailability reports as implemented.
    private void RefreshCaptureButton()
    {
        CaptureButton.IsEnabled = CaptureAvailability.IsImplemented(Mode);
        CaptureButton.ToolTip = CaptureAvailability.Tooltip(Mode);
        CaptureButtonText.Text = ControlBarText.CaptureButtonText(Mode, _isRecording);
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

    private void OnClearRegionClick(object sender, RoutedEventArgs e) => ClearRegionRequested?.Invoke(this, EventArgs.Empty);

    private void OnCaptureClick(object sender, RoutedEventArgs e) => CaptureRequested?.Invoke(this, Mode);

    private void OnSettingsClick(object sender, RoutedEventArgs e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

    private void OnHotkeysClick(object sender, RoutedEventArgs e) => HotkeysRequested?.Invoke(this, EventArgs.Empty);

    private void OnToggleWindowsClick(object sender, RoutedEventArgs e) => ToggleWindowsRequested?.Invoke(this, EventArgs.Empty);
}
