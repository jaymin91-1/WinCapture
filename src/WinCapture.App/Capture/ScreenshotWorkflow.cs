using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Serilog;
using WinCapture.App.Toast;
using WinCapture.Core.Capture;
using WinCapture.Core.Geometry;
using WinCapture.Core.Presentation;
using WinCapture.Core.Settings;
using WinCapture.Core.Storage;

namespace WinCapture.App.Capture;

/// <summary>
/// The single screenshot entry point (SPEC F-09). The control bar [Capture] button, the tray "스크린샷" item and the
/// Screenshot hotkey all call <see cref="RunAsync"/> (through App.OnHotkey). App windows are never hidden here:
/// whatever is visible in the region ends up in the file (SPEC F-08).
/// Must be called on the UI thread; capture, encoding and saving run on background threads.
/// </summary>
public sealed class ScreenshotWorkflow(
    RegionState region,
    ISettingsStore settingsStore,
    IToastService toast,
    ISaveFileSystem fileSystem)
{
    private readonly SingleFlightGate _gate = new();

    public async Task RunAsync()
    {
        if (!_gate.TryEnter())
        {
            Log.Information("Screenshot ignored: another capture is still running");
            return;
        }

        try
        {
            await RunCoreAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Screenshot failed unexpectedly");
            toast.Show(ToastKind.Error, ScreenshotMessages.ErrorTitle, ScreenshotMessages.ErrorMessage(ex.Message));
        }
        finally
        {
            _gate.Exit();
        }
    }

    private async Task RunCoreAsync()
    {
        var settings = settingsStore.Load();
        var plan = ScreenshotPlan.Decide(region.CaptureRect, settings.Capture.CopyToClipboard);
        if (!plan.CanCapture)
        {
            Log.Warning("Screenshot not possible: {Error}", plan.Error);
            toast.Show(ToastKind.Error, ScreenshotMessages.ErrorTitle, plan.Error!);
            return;
        }

        // Capture + PNG encode on a background thread (BitBlt of the clipped rectangle).
        var result = await ScreenshotService.CaptureAsync(plan.Area!.Value, settings.Capture.IncludeCursor);
        if (!result.Success)
        {
            toast.Show(ToastKind.Error, ScreenshotMessages.ErrorTitle, ScreenshotMessages.ErrorMessage(result.Error ?? ""));
            return;
        }

        var png = result.Png!;
        var size = result.Area; // saved size; smaller than the region when it was clipped to the virtual screen
        string path;
        try
        {
            path = await Task.Run(() => Save(settings, size, png));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Saving screenshot failed");
            toast.Show(ToastKind.Error, ScreenshotMessages.ErrorTitle, ScreenshotMessages.ErrorMessage(ex.Message));
            return;
        }

        Log.Information("Screenshot saved: {Width}x{Height}", size.Width, size.Height);

        // Clipboard needs the UI thread (this continuation is on it).
        if (plan.CopyToClipboard && !TryCopyToClipboard(png))
        {
            toast.Show(ToastKind.Warning, ScreenshotMessages.ClipboardWarningTitle, ScreenshotMessages.ClipboardWarningMessage);
        }

        toast.Show(
            ToastKind.Success,
            ScreenshotMessages.SuccessTitle,
            ScreenshotMessages.SuccessMessage(ScreenshotMessages.FileNameOf(path), size.Width, size.Height),
            () => RevealInExplorer(path));
    }

    private string Save(AppSettings settings, Core.Geometry.Rect size, byte[] png)
    {
        var path = CaptureFileNamer.BuildPath(
            settings.FileNamePattern,
            DateTime.Now,
            size.Width,
            size.Height,
            "png",
            settings.SaveFolder,
            settings.UseTypeSubfolders,
            Environment.ExpandEnvironmentVariables,
            fileSystem);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, png);
        return path;
    }

    private static bool TryCopyToClipboard(byte[] png)
    {
        try
        {
            using var stream = new MemoryStream(png);
            var frame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            frame.Freeze();
            Clipboard.SetImage(frame);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Copying screenshot to clipboard failed");
            return false;
        }
    }

    private static void RevealInExplorer(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = ScreenshotMessages.ExplorerSelectArguments(path),
                UseShellExecute = false,
            })?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Opening explorer for the screenshot failed");
        }
    }
}
