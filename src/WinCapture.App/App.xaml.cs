using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinCapture.App.Overlay;
using WinCapture.Core.Geometry;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Settings;

namespace WinCapture.App;

public partial class App : Application
{
    private SingleInstanceGuard? _guard;
    private IHost? _host;
    private TaskbarIcon? _trayIcon;
    private SettingsWindow? _settingsWindow;
    private ControlBarWindow? _controlBar;
    // Windows that were visible when "hide windows" ran; restored by the next toggle.
    private bool _restoreSettingsWindow;
    private bool _restoreControlBar;
    private HotkeyService? _hotkeys;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _guard = SingleInstanceGuard.Acquire();
        if (!_guard.IsFirstInstance)
        {
            // The running instance has been signaled; exit immediately.
            _guard.Dispose();
            _guard = null;
            Shutdown();
            return;
        }

        ConfigureLogging();
        DispatcherUnhandledException += (_, args) =>
        {
            Log.Error(args.Exception, "Unhandled dispatcher exception");
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled exception (terminating={Terminating})", args.IsTerminating);
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Error(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ISettingsStore>(_ => new JsonSettingsStore(
                    JsonSettingsStore.GetDefaultPath(),
                    log: (message, ex) =>
                    {
                        if (ex is null)
                        {
                            Log.Warning("{Message}", message);
                        }
                        else
                        {
                            Log.Warning(ex, "{Message}", message);
                        }
                    }));
                services.AddSingleton<RegionState>();
                services.AddSingleton(sp => new RegionCoordinator(
                    sp.GetRequiredService<RegionState>(),
                    sp.GetRequiredService<ISettingsStore>(),
                    (message, ex) => Log.Warning(ex, "{Message}", message)));
                services.AddSingleton<SelectionController>();
                services.AddSingleton<BorderController>();
            })
            .Build();
        _host.Start();

        var settings = _host.Services.GetRequiredService<ISettingsStore>().Load();
        Log.Information("WinCapture started. Settings loaded (saveFolder set: {HasFolder})", !string.IsNullOrEmpty(settings.SaveFolder));

        // Creating the controller wires RegionState to the border window (hotkeys connect in later tasks).
        _host.Services.GetRequiredService<BorderController>();

        // Restore the last region (corrected into the virtual screen) and show its border.
        try
        {
            _host.Services.GetRequiredService<RegionCoordinator>().Restore(WinCapture.Native.Interop.WindowInterop.GetVirtualScreen());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Restoring last region failed");
        }

        AppTheme.Initialize();
        CreateTrayIcon();

        _hotkeys = new HotkeyService(
            _host.Services.GetRequiredService<ISettingsStore>(),
            OnHotkey,
            (title, message) => _trayIcon?.ShowNotification(title, message, H.NotifyIcon.Core.NotificationIcon.Warning));
        _hotkeys.Reapply();

        CreateControlBar(settings.CaptureMode);
        _guard.StartListening(() => Dispatcher.BeginInvoke(ShowSettingsWindow));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeys?.Dispose(); // unregisters all hotkeys
        _hotkeys = null;
        _settingsWindow?.AllowClose();
        _controlBar?.AllowClose();
        _trayIcon?.Dispose();
        _trayIcon = null;
        _host?.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
        _host?.Dispose();
        _guard?.Dispose();
        Log.Information("WinCapture exited");
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static void ConfigureLogging()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinCapture",
            "logs");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(logDir, "wincapture-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
    }

    private void CreateTrayIcon()
    {
        var menu = new ContextMenu { Style = (Style)FindResource("TrayContextMenuStyle") };

        // Order follows SPEC section 4; shortcut text shows SPEC section 7 defaults.
        // Unimplemented items are disabled placeholders.
        var selectItem = CreateMenuItem("영역 지정", "Ctrl+Alt+R", enabled: true);
        selectItem.Click += (_, _) => OnHotkey(HotkeyAction.SelectRegion);
        menu.Items.Add(selectItem);
        menu.Items.Add(CreateMenuItem("스크린샷", "Ctrl+Alt+S", enabled: false));
        menu.Items.Add(CreateMenuItem("동영상 녹화 시작/정지", "Ctrl+Alt+V", enabled: false));
        menu.Items.Add(CreateMenuItem("GIF 녹화 시작/정지", "Ctrl+Alt+G", enabled: false));
        menu.Items.Add(CreateSeparator());
        var toggleWindowItem = CreateMenuItem("창 표시/숨김", "Ctrl+Alt+W", enabled: true);
        toggleWindowItem.Click += (_, _) => OnHotkey(HotkeyAction.ToggleAppWindow);
        menu.Items.Add(toggleWindowItem);
        menu.Items.Add(CreateMenuItem("저장 폴더 열기", null, enabled: false));
        menu.Items.Add(CreateSeparator());

        var settingsItem = CreateMenuItem("설정", null, enabled: true);
        settingsItem.Click += (_, _) => ShowSettingsWindow();
        menu.Items.Add(settingsItem);

        var exitItem = CreateMenuItem("종료", null, enabled: true);
        exitItem.Click += (_, _) => ExitApplication();
        menu.Items.Add(exitItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "WinCapture",
            IconSource = (ImageSource)FindResource("TrayIconImage"),
            ContextMenu = menu,
            // Right click only: LeftOrRightClick made H.NotifyIcon defer the left-click menu by the system
            // double-click time and opened the menu on left click, which SPEC section 4 no longer wants.
            MenuActivation = H.NotifyIcon.Core.PopupActivationMode.RightClick,
        };

        // Left click shows the control bar and brings it to the front (SPEC section 4).
        // TrayLeftMouseUp fires immediately; the library's LeftClickCommand waits for the double-click time.
        _trayIcon.TrayLeftMouseUp += (_, _) => ShowControlBar();

        _trayIcon.ForceCreate();
        WarmUpMenu(menu);
    }

    // The first IsOpen of a ContextMenu builds its template and popup; do the template work up front, while idle,
    // so the first right click does not pay for it. Does not open the popup (no flicker, no focus change).
    private void WarmUpMenu(ContextMenu menu) =>
        Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.ApplicationIdle,
            () =>
            {
                try
                {
                    menu.ApplyTemplate();
                    foreach (var item in menu.Items.OfType<Control>())
                    {
                        item.ApplyTemplate();
                    }

                    menu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Tray menu warm-up failed");
                }
            });

    private MenuItem CreateMenuItem(string header, string? shortcut, bool enabled) => new()
    {
        Header = header,
        InputGestureText = shortcut,
        IsEnabled = enabled,
        Style = (Style)FindResource("TrayMenuItemStyle"),
    };

    private Separator CreateSeparator() => new() { Style = (Style)FindResource("TrayMenuSeparatorStyle") };

    // Single dispatch for hotkeys and the matching tray menu items.
    private void OnHotkey(HotkeyAction action)
    {
        var services = _host?.Services;
        if (services is null)
        {
            return;
        }

        switch (action)
        {
            case HotkeyAction.SelectRegion:
                services.GetRequiredService<SelectionController>().BeginSelection();
                break;
            case HotkeyAction.ToggleBorder:
                services.GetRequiredService<RegionState>().ToggleBorder();
                break;
            case HotkeyAction.ClearRegion:
                services.GetRequiredService<RegionCoordinator>().Clear(); // also empties saved lastRegion
                break;
            case HotkeyAction.ToggleAppWindow:
                ToggleAppWindow();
                break;
            default:
                // Screenshot / RecordVideo / RecordGif: registered only; implemented in later sessions.
                Log.Information("Hotkey {Action} pressed (not implemented yet)", action);
                break;
        }
    }

    // Single path for "show/hide windows": the [show/hide] button, the ToggleAppWindow hotkey and the tray item.
    // Hides the settings window and the control bar together and restores the ones that were visible.
    // The tray icon and the border window are not touched.
    private void ToggleAppWindow()
    {
        var settingsVisible = _settingsWindow is { IsVisible: true };
        var barVisible = _controlBar is { IsVisible: true };
        if (settingsVisible || barVisible)
        {
            _restoreSettingsWindow = settingsVisible;
            _restoreControlBar = barVisible;
            _settingsWindow?.Hide();
            _controlBar?.Hide();
            return;
        }

        // Nothing visible: bring back what was hidden last time; the control bar is the fallback.
        if (_restoreSettingsWindow)
        {
            ShowSettingsWindow();
        }

        if (_restoreControlBar || !_restoreSettingsWindow)
        {
            ShowControlBar();
        }
    }

    /// <summary>
    /// Hides the app windows (settings window and control bar) that overlap the region when autoHideAppWindow
    /// is on (F-08-3). Screenshot/recording start (sessions 3-5) calls this before capturing. Returns true when
    /// at least one window was hidden.
    /// </summary>
    internal bool HideAppWindowIfOverlapping()
    {
        if (_host is null)
        {
            return false;
        }

        var autoHide = _host.Services.GetRequiredService<ISettingsStore>().Load().AutoHideAppWindow;
        var region = _host.Services.GetRequiredService<RegionState>().Region;
        var hidAny = false;
        foreach (var window in new Window?[] { _settingsWindow, _controlBar })
        {
            if (window is not { IsVisible: true })
            {
                continue;
            }

            var bounds = WinCapture.Native.Interop.WindowInterop.GetWindowBounds(
                new System.Windows.Interop.WindowInteropHelper(window).Handle);
            if (AppWindowPolicy.ShouldAutoHide(autoHide, window.IsVisible, bounds, region))
            {
                window.Hide();
                hidAny = true;
            }
        }

        return hidAny;
    }

    private void CreateControlBar(CaptureMode initialMode)
    {
        _controlBar = new ControlBarWindow(initialMode);
        _controlBar.SelectRegionRequested += (_, _) => OnHotkey(HotkeyAction.SelectRegion);
        // Same path as the hotkey / tray item for the mode; not enabled until the mode is implemented.
        _controlBar.CaptureRequested += (_, mode) => OnHotkey(CaptureAvailability.ActionFor(mode));
        _controlBar.SettingsRequested += (_, _) => ShowSettingsWindow();
        _controlBar.ToggleWindowsRequested += (_, _) => OnHotkey(HotkeyAction.ToggleAppWindow);
        _controlBar.ModeChanged += (_, mode) => SaveCaptureMode(mode);
        ShowControlBar();
    }

    private void SaveCaptureMode(CaptureMode mode)
    {
        try
        {
            var store = _host!.Services.GetRequiredService<ISettingsStore>();
            store.Save(store.Load() with { CaptureMode = mode });
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Saving captureMode failed");
        }
    }

    // Single path for showing the control bar (startup, tray left click, window toggle).
    private void ShowControlBar()
    {
        if (_controlBar is null)
        {
            return;
        }

        if (!_controlBar.IsVisible)
        {
            _controlBar.Show();
        }

        if (_controlBar.WindowState == WindowState.Minimized)
        {
            _controlBar.WindowState = WindowState.Normal;
        }

        _controlBar.Topmost = true;
        _controlBar.Topmost = false;
        _controlBar.Activate();
    }

    // Single path for showing the settings window (tray menu and second-instance signal).
    private void ShowSettingsWindow()
    {
        // The window is created once and hidden (not closed) when the user clicks X.
        _settingsWindow ??= new SettingsWindow();

        if (!_settingsWindow.IsVisible)
        {
            _settingsWindow.Show();
        }

        if (_settingsWindow.WindowState == WindowState.Minimized)
        {
            _settingsWindow.WindowState = WindowState.Normal;
        }

        // Topmost toggle reliably brings the window to the foreground.
        _settingsWindow.Topmost = true;
        _settingsWindow.Topmost = false;
        _settingsWindow.Activate();
    }

    private void ExitApplication()
    {
        _settingsWindow?.AllowClose();
        _controlBar?.AllowClose();
        _trayIcon?.Dispose();
        _trayIcon = null;
        Shutdown();
    }
}
