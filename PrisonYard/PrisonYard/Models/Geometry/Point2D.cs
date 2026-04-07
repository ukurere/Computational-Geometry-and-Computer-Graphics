namespace PrisonYard.Models.Geometry;
public readonly record struct Point2D(double X, double Y)
{
    public override string ToString() => $"({X:0.##}, {Y:0.##})";
}