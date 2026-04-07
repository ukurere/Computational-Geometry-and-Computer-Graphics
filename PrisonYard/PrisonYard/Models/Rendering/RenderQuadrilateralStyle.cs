using System.Windows.Media;

namespace PrisonYard.Models.Rendering;

public sealed class RenderQuadrilateralStyle
{
    public Brush Fill { get; init; } = new SolidColorBrush(Color.FromArgb(80, 100, 149, 237));
    public Brush Stroke { get; init; } = Brushes.CornflowerBlue;
    public double StrokeThickness { get; init; } = 1.5;

    public bool ShowFill { get; init; } = true;
    public bool ShowStroke { get; init; } = true;
}