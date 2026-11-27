using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using WinCapture.App.Pages;

namespace WinCapture.App;

public partial class SettingsWindow : FluentWindow
{
    private bool _allowClose;

    public SettingsWindow()
    {
        // Follow the system light/dark theme, including live changes.
        // Solid background (no Mica) and the app's own accent: WPF-UI accent updates are disabled.
        SystemThemeWatcher.Watch(this, WindowBackdropType.None, updateAccents: false);
        InitializeComponent();
    }

    /// <summary>Lets the window really close (called only when the app is exiting).</summary>
    public void AllowClose() => _allowClose = true;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_allowClose)
        {
            // X hides the window; the app keeps running in the tray.
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RootNavigation.Navigate(typeof(GeneralPage));
    }
}
