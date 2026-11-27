using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace WinCapture.App;

/// <summary>
/// Fixes the app to the light theme (SPEC section 3): WPF-UI theme and the app's own palette are both Light and the
/// accent is the app blue. The Windows light/dark setting is not followed. Colors.Dark.xaml is kept for later use.
/// </summary>
public static class AppTheme
{
    private static readonly Color LightAccent = Color.FromRgb(0x2F, 0x7C, 0xF6);

    private static ResourceDictionary? _current;

    /// <summary>Applies the light theme. Call once at startup.</summary>
    public static void Initialize()
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.None, updateAccent: false);

        var next = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/WinCapture.App;component/Themes/Colors.Light.xaml", UriKind.Absolute),
        };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_current is not null)
        {
            merged.Remove(_current);
        }

        merged.Add(next);
        _current = next;

        ApplicationAccentColorManager.Apply(LightAccent, ApplicationTheme.Light);
    }
}
