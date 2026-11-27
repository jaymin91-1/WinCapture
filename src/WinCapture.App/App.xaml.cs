using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinCapture.Core.Settings;

namespace WinCapture.App;

public partial class App : Application
{
    private SingleInstanceGuard? _guard;
    private IHost? _host;
    private TaskbarIcon? _trayIcon;
    private SettingsWindow? _settingsWindow;

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
            })
            .Build();
        _host.Start();

        var settings = _host.Services.GetRequiredService<ISettingsStore>().Load();
        Log.Information("WinCapture started. Settings loaded (saveFolder set: {HasFolder})", !string.IsNullOrEmpty(settings.SaveFolder));

        CreateTrayIcon();
        _guard.StartListening(() => Dispatcher.BeginInvoke(ShowSettingsWindow));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _settingsWindow?.AllowClose();
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
        var menu = new ContextMenu();

        // Order follows SPEC section 4. Unimplemented items are disabled placeholders.
        menu.Items.Add(new MenuItem { Header = "영역 지정", IsEnabled = false });
        menu.Items.Add(new MenuItem { Header = "스크린샷", IsEnabled = false });
        menu.Items.Add(new MenuItem { Header = "동영상 녹화 시작/정지", IsEnabled = false });
        menu.Items.Add(new MenuItem { Header = "GIF 녹화 시작/정지", IsEnabled = false });
        menu.Items.Add(new MenuItem { Header = "창 표시/숨김", IsEnabled = false });
        menu.Items.Add(new MenuItem { Header = "저장 폴더 열기", IsEnabled = false });

        var settingsItem = new MenuItem { Header = "설정" };
        settingsItem.Click += (_, _) => ShowSettingsWindow();
        menu.Items.Add(settingsItem);

        var exitItem = new MenuItem { Header = "종료" };
        exitItem.Click += (_, _) => ExitApplication();
        menu.Items.Add(exitItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "WinCapture",
            IconSource = (ImageSource)FindResource("TrayIconImage"),
            ContextMenu = menu,
            MenuActivation = H.NotifyIcon.Core.PopupActivationMode.LeftOrRightClick,
        };
        _trayIcon.ForceCreate();
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
        _trayIcon?.Dispose();
        _trayIcon = null;
        Shutdown();
    }
}
