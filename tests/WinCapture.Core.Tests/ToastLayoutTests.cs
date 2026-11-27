using FluentAssertions;
using WinCapture.Core.Geometry;
using WinCapture.Core.Presentation;

namespace WinCapture.Core.Tests;

public sealed class ToastLayoutTests
{
    private static readonly PixelSize Toast = new(360, 80);

    [Theory]
    [InlineData(0, 1544, 944)]
    [InlineData(1, 1544, 856)]
    [InlineData(2, 1544, 768)]
    public void SlotPosition_StacksUpwardFromBottomRight(int index, int x, int y)
    {
        var pos = ToastLayout.SlotPosition(new Rect(0, 0, 1920, 1040), Toast, 16, 8, index);
        pos.Should().Be(new PixelPoint(x, y));
    }

    [Fact]
    public void SlotPosition_WorksWithNegativeWorkArea()
    {
        // Work area entirely at negative coordinates (monitor left of and above the primary).
        var work = new Rect(-1920, -1040, 1920, 1000);
        ToastLayout.SlotPosition(work, Toast, 16, 8, 0).Should().Be(new PixelPoint(-376, -136));
        ToastLayout.SlotPosition(work, Toast, 16, 8, 1).Should().Be(new PixelPoint(-376, -224));
    }

    [Fact]
    public void Truncate_KeepsShortTextAndCutsLongTextWithEllipsis()
    {
        ToastLayout.Truncate("abc", 5).Should().Be("abc");
        ToastLayout.Truncate("abcde", 5).Should().Be("abcde");
        ToastLayout.Truncate("abcdef", 5).Should().Be("abcd…");
        ToastLayout.Truncate(null, 5).Should().BeEmpty();
    }
}
