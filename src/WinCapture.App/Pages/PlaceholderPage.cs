using System.Windows;
using System.Windows.Controls;

namespace WinCapture.App.Pages;

/// <summary>Empty shell page for a settings tab. Real content arrives in session 6.</summary>
public abstract class PlaceholderPage : Page
{
    protected PlaceholderPage(string title)
    {
        Content = new StackPanel
        {
            Margin = new Thickness(32, 24, 32, 24),
            Children =
            {
                CreateTitle(title),
                new TextBlock
                {
                    Text = "이 탭의 설정은 이후 버전에서 제공됩니다.",
                    Margin = new Thickness(0, 12, 0, 0),
                },
            },
        };
        this.SetResourceReference(BackgroundProperty, "WindowBackgroundBrush");
        ((TextBlock)((StackPanel)Content).Children[1]).SetResourceReference(TextBlock.ForegroundProperty, "TextSecondaryBrush");
    }

    private static TextBlock CreateTitle(string title)
    {
        var block = new TextBlock { Text = title };
        block.SetResourceReference(FrameworkElement.StyleProperty, "PageTitleStyle");
        return block;
    }
}

public sealed class GeneralPage() : PlaceholderPage("일반");
public sealed class HotkeysPage() : PlaceholderPage("단축키");
public sealed class CapturePage() : PlaceholderPage("캡처");
public sealed class RecordingPage() : PlaceholderPage("녹화");
public sealed class FfmpegPage() : PlaceholderPage("FFmpeg");
public sealed class AboutPage() : PlaceholderPage("정보");
