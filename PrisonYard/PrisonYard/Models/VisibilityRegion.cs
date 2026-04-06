using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonYard.Models;

public sealed class VisibilityRegion
{
    public int CameraVertexIndex { get; }
    public IReadOnlyList<Point2D> BoundaryPoints { get; }

    public VisibilityRegion(int cameraVertexIndex, IEnumerable<Point2D> boundaryPoints)
    {
        if (cameraVertexIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cameraVertexIndex), "Індекс вершини камери не може бути від'ємним.");
        }

        if (boundaryPoints is null)
        {
            throw new ArgumentNullException(nameof(boundaryPoints));
        }

        var points = boundaryPoints.ToList();

        if (points.Count < 3)
        {
            throw new ArgumentException("Область видимості повинна містити щонайменше 3 точки.", nameof(boundaryPoints));
        }

        CameraVertexIndex = cameraVertexIndex;
        BoundaryPoints = points;
    }

    public double GetSignedDoubleArea()
    {
        double sum = 0;

        for (int i = 0; i < BoundaryPoints.Count; i++)
        {
            var a = BoundaryPoints[i];
            var b = BoundaryPoints[(i + 1) % BoundaryPoints.Count];
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum;
    }

    public bool IsCounterClockwise() => GetSignedDoubleArea() > 0;
}