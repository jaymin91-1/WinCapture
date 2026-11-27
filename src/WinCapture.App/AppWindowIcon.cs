using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Serilog;
using WinCapture.Native.Interop;

namespace WinCapture.App;

/// <summary>
/// Makes sure an app window shows the app icon in the taskbar and title bar. Window.Icon (XAML) is not enough:
/// WPF-UI's backdrop / title bar handling sends WM_SETICON with a null icon to FluentWindow windows, which clears
/// the icon WPF set from Window.Icon (taskbar button shows the default icon). So the icon is also set explicitly
/// with WM_SETICON once the window handle exists and again after the window has loaded and rendered.
/// </summary>
internal static class AppWindowIcon
{
    private static readonly Uri IconUri = new("pack://application:,,,/Assets/AppIcon.ico", UriKind.Absolute);
    private static byte[]? _iconBytes;

    /// <summary>Call from the window constructor (before the window is shown).</summary>
    public static void Attach(Window window)
    {
        var icon = new WindowIcon(LoadBytes());
        var warned = false;

        void Apply(string stage)
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                var error = icon.Apply(hwnd);
                if (error is not null && !warned)
                {
                    warned = true;
                    Log.Warning("Setting the window icon failed at {Stage} for {Window}: {Error}", stage, window.GetType().Name, error);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Setting the window icon threw at {Stage} for {Window}", stage, window.GetType().Name);
            }
        }

        try
        {
            window.Icon = BitmapFrame.Create(IconUri);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Loading Window.Icon failed for {Window}", window.GetType().Name);
        }

        window.SourceInitialized += (_, _) => Apply("SourceInitialized");
        window.Loaded += (_, _) => Apply("Loaded");
        window.ContentRendered += (_, _) => Apply("ContentRendered");
        window.Closed += (_, _) => icon.Dispose();
    }

    private static byte[] LoadBytes()
    {
        if (_iconBytes is not null)
        {
            return _iconBytes;
        }

        try
        {
            var info = Application.GetResourceStream(IconUri);
            if (info is null)
            {
                Log.Warning("App icon resource not found: {Uri}", IconUri);
                return _iconBytes = Array.Empty<byte>();
            }

            using var stream = info.Stream;
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return _iconBytes = memory.ToArray();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Reading the app icon resource failed");
            return Array.Empty<byte>();
        }
    }
}
