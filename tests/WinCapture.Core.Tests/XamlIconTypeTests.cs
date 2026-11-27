using System.Xml.Linq;
using FluentAssertions;

namespace WinCapture.Core.Tests;

/// <summary>
/// Guards against putting a non-IconElement (e.g. Viewbox) into an Icon property. WPF-UI's
/// NavigationViewItem.Icon is a DependencyProperty of type IconElement whose coerce callback throws
/// ArgumentException for anything else, but XAML compilation does not catch it (it fails when the window is created).
/// Pure XML parsing of the App's XAML files; no WPF reference.
/// </summary>
public class XamlIconTypeTests
{
    // Wpf.Ui IconElement types (and IconSourceElement, which Coerce converts) plus the app's own PathIcon.
    private static readonly string[] AllowedIconElements =
    {
        "SymbolIcon", "FontIcon", "ImageIcon", "PathIcon", "SymbolIconSource", "FontIconSource", "ImageIconSource",
    };

    private const string OldPattern = """
        <ui:NavigationViewItem xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Content="x">
            <ui:NavigationViewItem.Icon>
                <Viewbox Width="18" Height="18"><Path Data="M0,0" /></Viewbox>
            </ui:NavigationViewItem.Icon>
        </ui:NavigationViewItem>
        """;

    [Fact]
    public void Detector_flags_the_old_Viewbox_pattern()
    {
        FindViolations(XDocument.Parse(OldPattern), "old.xaml").Should().ContainSingle()
            .Which.Should().Contain("Viewbox");
    }

    [Fact]
    public void Detector_accepts_PathIcon()
    {
        var xaml = OldPattern.Replace(
            "<Viewbox Width=\"18\" Height=\"18\"><Path Data=\"M0,0\" /></Viewbox>",
            "<controls:PathIcon Width=\"18\" Height=\"18\" Data=\"M0,0\" />")
            .Replace("Content=\"x\"", "Content=\"x\" xmlns:controls=\"clr-namespace:WinCapture.App.Controls\"");
        FindViolations(XDocument.Parse(xaml), "new.xaml").Should().BeEmpty();
    }

    [Fact]
    public void App_xaml_files_only_put_icon_elements_into_Icon_properties()
    {
        var appDir = Path.Combine(FindRepoRoot(), "src", "WinCapture.App");
        var files = Directory.GetFiles(appDir, "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();
        files.Should().NotBeEmpty();

        var violations = files.SelectMany(f => FindViolations(XDocument.Load(f), Path.GetFileName(f))).ToList();
        violations.Should().BeEmpty();
    }

    [Fact]
    public void Settings_window_has_six_tab_icons_all_PathIcon()
    {
        var doc = XDocument.Load(Path.Combine(FindRepoRoot(), "src", "WinCapture.App", "SettingsWindow.xaml"));
        var icons = doc.Descendants().Where(e => e.Name.LocalName == "NavigationViewItem.Icon").ToList();
        icons.Should().HaveCount(6);
        icons.Select(i => i.Elements().Single().Name.LocalName).Should().OnlyContain(n => n == "PathIcon");
    }

    private static IEnumerable<string> FindViolations(XDocument doc, string file)
    {
        // Property-element syntax: <Anything.Icon> must contain exactly one allowed element.
        foreach (var prop in doc.Descendants().Where(e => e.Name.LocalName.EndsWith(".Icon", StringComparison.Ordinal)))
        {
            foreach (var child in prop.Elements())
            {
                if (!AllowedIconElements.Contains(child.Name.LocalName))
                {
                    yield return $"{file}: <{prop.Name.LocalName}> contains <{child.Name.LocalName}>, not an IconElement";
                }
            }
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "WinCapture.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("WinCapture.sln not found above test output");
    }
}
