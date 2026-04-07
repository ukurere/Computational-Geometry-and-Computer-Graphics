using System.Windows.Media;

namespace PrisonYard.Models.Rendering;

public sealed class RenderPolygonStyle
{
    public Brush Fill { get; init; } = Brushes.LightGray;
    public Brush Stroke { get; init; } = Brushes.Black;
    public double StrokeThickness { get; init; } = 2.0;

    public bool ShowFill { get; init; } = true;
    public bool ShowStroke { get; init; } = true;
}