using System.Windows;
using System.Windows.Controls;

namespace WinCapture.App.Controls;

/// <summary>
/// Hosts one child but reports a desired width of 0, so the child never widens its parent column: the column width
/// is decided by the other rows (the [Capture] button) and the child is then arranged to that width.
/// Used for the control bar status card whose text length changes with the region (coordinates, chip), which would
/// otherwise make the window width change whenever the region changes.
/// </summary>
public sealed class WidthIgnoringDecorator : Decorator
{
    protected override Size MeasureOverride(Size constraint)
    {
        if (Child is null)
        {
            return default;
        }

        Child.Measure(new Size(double.PositiveInfinity, constraint.Height));
        return new Size(0, Child.DesiredSize.Height);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Child is not null)
        {
            // Re-measure at the real column width so text trimming and the chip use the final width.
            Child.Measure(new Size(arrangeSize.Width, double.PositiveInfinity));
            Child.Arrange(new Rect(0, 0, arrangeSize.Width, Math.Max(arrangeSize.Height, Child.DesiredSize.Height)));
        }

        return arrangeSize;
    }
}
