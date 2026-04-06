using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonYard.Models;

public sealed class VisibilityRegion
{
    private const double Epsilon = 1e-9;

    public int CameraVertexIndex { get; }
    public IReadOnlyList<Point2D> BoundaryPoints { get; }

    public VisibilityRegion(int cameraVertexIndex, IEnumerable<Point2D> boundaryPoints)
    {
        if (cameraVertexIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cameraVertexIndex),
                "Індекс вершини камери не може бути від'ємним.");
        }

        if (boundaryPoints is null)
        {
            throw new ArgumentNullException(nameof(boundaryPoints));
        }

        var points = boundaryPoints.ToList();

        if (points.Count < 3)
        {
            throw new ArgumentException(
                "Область видимості повинна містити щонайменше 3 точки.",
                nameof(boundaryPoints));
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

    public bool IsPointOnBoundary(Point2D point, double epsilon = Epsilon)
    {
        for (int i = 0; i < BoundaryPoints.Count; i++)
        {
            var start = BoundaryPoints[i];
            var end = BoundaryPoints[(i + 1) % BoundaryPoints.Count];

            if (IsPointOnSegment(point, start, end, epsilon))
            {
                return true;
            }
        }

        return false;
    }

    public bool ContainsPoint(Point2D point, bool includeBoundary = true, double epsilon = Epsilon)
    {
        if (includeBoundary && IsPointOnBoundary(point, epsilon))
        {
            return true;
        }

        bool inside = false;

        for (int i = 0, j = BoundaryPoints.Count - 1; i < BoundaryPoints.Count; j = i++)
        {
            var pi = BoundaryPoints[i];
            var pj = BoundaryPoints[j];

            bool intersects =
                ((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X);

            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static bool IsPointOnSegment(Point2D point, Point2D start, Point2D end, double epsilon)
    {
        double cross =
            (point.X - start.X) * (end.Y - start.Y) -
            (point.Y - start.Y) * (end.X - start.X);

        if (Math.Abs(cross) > epsilon)
        {
            return false;
        }

        double dot =
            (point.X - start.X) * (end.X - start.X) +
            (point.Y - start.Y) * (end.Y - start.Y);

        if (dot < -epsilon)
        {
            return false;
        }

        double squaredLength =
            (end.X - start.X) * (end.X - start.X) +
            (end.Y - start.Y) * (end.Y - start.Y);

        return dot <= squaredLength + epsilon;
    }
}