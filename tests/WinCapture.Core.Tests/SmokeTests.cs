using FluentAssertions;
using WinCapture.Core;

namespace WinCapture.Core.Tests;

public class SmokeTests
{
    [Fact]
    public void AppInfo_Name_IsWinCapture()
    {
        AppInfo.Name.Should().Be("WinCapture");
    }
}
