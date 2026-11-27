using FluentAssertions;
using WinCapture.Core.Presentation;

namespace WinCapture.Core.Tests;

public class FixedSizeWindowPolicyTests
{
    [Theory]
    [InlineData(4, true)]   // HTSIZE
    [InlineData(9, true)]   // HTMAXBUTTON
    [InlineData(10, true)]  // HTLEFT
    [InlineData(12, true)]  // HTTOP
    [InlineData(15, true)]  // HTBOTTOM
    [InlineData(17, true)]  // HTBOTTOMRIGHT
    [InlineData(1, false)]  // HTCLIENT
    [InlineData(2, false)]  // HTCAPTION (drag to move stays)
    [InlineData(8, false)]  // HTMINBUTTON
    [InlineData(20, false)] // HTCLOSE
    public void IsSizingHit_matches_resize_and_maximize_codes(int hit, bool expected) =>
        FixedSizeWindowPolicy.IsSizingHit(hit).Should().Be(expected);

    [Theory]
    [InlineData(0xF000, true)]  // SC_SIZE
    [InlineData(0xF008, true)]  // SC_SIZE | WMSZ_BOTTOMRIGHT
    [InlineData(0xF032, true)]  // SC_MAXIMIZE | HTCAPTION flag bits
    [InlineData(0xF030, true)]  // SC_MAXIMIZE
    [InlineData(0xF010, false)] // SC_MOVE
    [InlineData(0xF020, false)] // SC_MINIMIZE
    [InlineData(0xF060, false)] // SC_CLOSE
    [InlineData(0xF120, false)] // SC_RESTORE (needed to leave minimized)
    public void IsBlockedSysCommand_blocks_only_size_and_maximize(int wParam, bool expected) =>
        FixedSizeWindowPolicy.IsBlockedSysCommand(wParam).Should().Be(expected);

    [Fact]
    public void Caption_double_click_is_blocked_but_client_is_not()
    {
        FixedSizeWindowPolicy.IsBlockedDoubleClick(2).Should().BeTrue();
        FixedSizeWindowPolicy.IsBlockedDoubleClick(1).Should().BeFalse();
    }

    [Theory]
    [InlineData(100, 100, true)]   // top-left corner
    [InlineData(105, 300, true)]   // left edge band
    [InlineData(419, 300, true)]   // right edge band
    [InlineData(300, 419, true)]   // bottom edge band
    [InlineData(300, 103, true)]   // top edge band
    [InlineData(300, 300, false)]  // interior
    [InlineData(106, 106, false)]  // just inside the 6 px band
    [InlineData(420, 300, false)]  // right exclusive
    [InlineData(99, 300, false)]   // outside
    public void IsInEdgeBand_uses_a_band_inside_the_rectangle(int x, int y, bool expected) =>
        FixedSizeWindowPolicy.IsInEdgeBand(x, y, 100, 100, 420, 420, 6).Should().Be(expected);

    [Theory]
    [InlineData(320.0, 96u, 320)]
    [InlineData(320.0, 144u, 480)]
    [InlineData(333.5, 144u, 501)]
    public void ToPixelsCeiling_scales_dips_by_dpi(double dips, uint dpi, int expected) =>
        FixedSizeWindowPolicy.ToPixelsCeiling(dips, dpi).Should().Be(expected);
}
