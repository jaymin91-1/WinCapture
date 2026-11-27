using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace WinCapture.App;

/// <summary>
/// Applies the app's own light/dark palette on top of WPF-UI. The palette dictionary is swapped when the
/// system theme changes, and WPF-UI's accent is pinned to the app blue instead of the system accent.
/// </summary>
public static class AppTheme
{
    private static readonly Color LightAccent = Color.FromRgb(0x2F, 0x7C, 0xF6);
    private static readonly Color DarkAccent = Color.FromRgb(0x4D, 0x8E, 0xF7);

    private static ResourceDictionary? _current;
    private static bool _hooked;

    /// <summary>Applies the current system theme and keeps following it. Call once at startup.</summary>
    public static void Initialize()
    {
        if (!_hooked)
        {
            _hooked = true;
            ApplicationThemeManager.Changed += (theme, _) => Apply(theme);
        }

        ApplicationThemeManager.ApplySystemTheme(updateAccent: false);
        Apply(ApplicationThemeManager.GetAppTheme());
    }

    public static void Apply(ApplicationTheme theme)
    {
        var dark = theme == ApplicationTheme.Dark;
        var name = dark ? "Dark" : "Light";
        var next = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/WinCapture.App;component/Themes/Colors.{name}.xaml", UriKind.Absolute),
        };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_current is not null)
        {
            merged.Remove(_current);
        }

        merged.Add(next);
        _current = next;

        ApplicationAccentColorManager.Apply(dark ? DarkAccent : LightAccent, theme);
    }
}
