using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;
using WinCapture.App.Pages;

namespace WinCapture.App;

public partial class SettingsWindow : FluentWindow
{
    private bool _allowClose;
    private Type? _pendingPage;

    public SettingsWindow()
    {
        // The theme is fixed to light (AppTheme); the system theme is not followed.
        AppWindowIcon.Attach(this);
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

    /// <summary>Selects a settings tab (page type). Before the first load the choice is applied when the window loads.</summary>
    public void SelectPage(Type pageType)
    {
        if (IsLoaded)
        {
            RootNavigation.Navigate(pageType);
        }
        else
        {
            _pendingPage = pageType;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RootNavigation.Navigate(_pendingPage ?? typeof(GeneralPage));
        _pendingPage = null;
    }
}
