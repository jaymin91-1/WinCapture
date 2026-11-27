using System.Windows;
using Wpf.Ui.Controls;
using WinCapture.Core.Presentation;

namespace WinCapture.App;

/// <summary>
/// Read-only popup listing the seven global hotkeys (SPEC section 4 "단축키 팝업"). A normal app window owned by
/// the control bar (not an OverlayWindowBase). Rows are computed by Core <see cref="HotkeyRows"/> when opened.
/// </summary>
public partial class HotkeyPopupWindow : FluentWindow
{
    public HotkeyPopupWindow(IReadOnlyList<HotkeyRow> rows)
    {
        AppWindowIcon.Attach(this);
        InitializeComponent();
        FixedSizeWindow.Attach(this);
        RowsList.ItemsSource = rows;
    }

    /// <summary>Raised when the user clicks [Open settings].</summary>
    public event EventHandler? OpenSettingsRequested;

    private void OnOpenSettingsClick(object sender, RoutedEventArgs e) => OpenSettingsRequested?.Invoke(this, EventArgs.Empty);

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
