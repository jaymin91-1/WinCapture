using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace WinCapture.App.Controls;

/// <summary>
/// Line icon drawn from a <see cref="Geometry"/> (24x24 coordinate space). It derives from WPF-UI's
/// <see cref="IconElement"/> because NavigationViewItem.Icon only accepts IconElement (or IconSourceElement).
/// The stroke follows the inherited <see cref="IconElement.Foreground"/> (theme / selection aware); no fill.
/// </summary>
public sealed class PathIcon : IconElement
{
    /// <summary>Identifies the <see cref="Data"/> dependency property.</summary>
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data),
        typeof(Geometry),
        typeof(PathIcon),
        new PropertyMetadata(null));

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override UIElement InitializeChildren()
    {
        var path = new Path
        {
            Width = 24,
            Height = 24,
            Fill = null,
            StrokeThickness = 1.6,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
        };
        path.SetBinding(Shape.StrokeProperty, new Binding(nameof(Foreground)) { Source = this });
        path.SetBinding(Path.DataProperty, new Binding(nameof(Data)) { Source = this });

        // Scales the 24x24 drawing to the icon element size (18x18 set by the owner).
        return new Viewbox { Stretch = Stretch.Uniform, Child = path };
    }
}
