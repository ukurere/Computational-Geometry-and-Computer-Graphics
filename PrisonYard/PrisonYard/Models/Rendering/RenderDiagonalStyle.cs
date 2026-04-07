using System.Windows.Media;

namespace PrisonYard.Models.Rendering;

public sealed class RenderDiagonalStyle
{
    public Brush Stroke { get; init; } = Brushes.DarkSlateGray;
    public double StrokeThickness { get; init; } = 2.0;

    public bool IsDashed { get; init; } = true;

    public DoubleCollection DashPattern { get; init; } = [6, 4];
}