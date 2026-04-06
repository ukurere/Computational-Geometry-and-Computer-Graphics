using System;

namespace PrisonYard.Models;

public sealed class CameraPlacement
{
    public int VertexIndex { get; }
    public Point2D Position { get; }
    public string Name { get; }

    public CameraPlacement(int vertexIndex, Point2D position, string? name = null)
    {
        if (vertexIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vertexIndex), "Індекс вершини не може бути від'ємним.");
        }

        VertexIndex = vertexIndex;
        Position = position;
        Name = string.IsNullOrWhiteSpace(name) ? $"Camera {vertexIndex}" : name;
    }

    public static CameraPlacement FromVertex(OrthogonalPolygon polygon, int vertexIndex, string? name = null)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        if (vertexIndex < 0 || vertexIndex >= polygon.Vertices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(vertexIndex), "Некоректний індекс вершини.");
        }

        var vertex = polygon.Vertices[vertexIndex];
        return new CameraPlacement(vertexIndex, new Point2D(vertex.X, vertex.Y), name);
    }
}