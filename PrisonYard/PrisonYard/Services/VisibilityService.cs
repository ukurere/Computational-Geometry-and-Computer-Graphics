using System;
using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models;

namespace PrisonYard.Services;

public static class VisibilityService
{
    private const double AngleDelta = 1e-7;
    private const double GeometryEpsilon = 1e-9;

    public static VisibilityRegion BuildVisibilityRegion(OrthogonalPolygon polygon, int cameraVertexIndex)
    {
        var camera = CameraPlacement.FromVertex(
            polygon,
            cameraVertexIndex,
            $"Камера {cameraVertexIndex}");

        return BuildVisibilityRegion(polygon, camera);
    }

    public static VisibilityRegion BuildVisibilityRegion(OrthogonalPolygon polygon, CameraPlacement camera)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        if (camera is null)
        {
            throw new ArgumentNullException(nameof(camera));
        }

        var origin = GetObservationPointInsidePolygon(polygon, camera);
        var angles = BuildRayAngles(polygon, origin);

        var intersections = new List<Point2D>();

        foreach (double angle in angles)
        {
            var hit = FindClosestIntersection(polygon, origin, angle);

            if (hit is not null)
            {
                intersections.Add(hit.Value);
            }
        }

        var boundaryPoints = RemoveNearDuplicatePoints(intersections);

        if (boundaryPoints.Count < 3)
        {
            throw new InvalidOperationException(
                $"Не вдалося побудувати область видимості для вершини {camera.VertexIndex}.");
        }

        return new VisibilityRegion(camera.VertexIndex, boundaryPoints);
    }

    private static Point2D GetObservationPointInsidePolygon(OrthogonalPolygon polygon, CameraPlacement camera)
    {
        var (minX, maxX, minY, maxY) = polygon.GetBoundingBox();
        double scale = Math.Max(Math.Max(maxX - minX, maxY - minY), 1);
        double delta = scale * 1e-4;

        var candidates = new[]
        {
            new Point2D(camera.Position.X + delta, camera.Position.Y + delta),
            new Point2D(camera.Position.X + delta, camera.Position.Y - delta),
            new Point2D(camera.Position.X - delta, camera.Position.Y + delta),
            new Point2D(camera.Position.X - delta, camera.Position.Y - delta)
        };

        foreach (var candidate in candidates)
        {
            if (polygon.ContainsPoint(candidate, includeBoundary: false))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"Не вдалося знайти внутрішню точку спостереження для вершини {camera.VertexIndex}.");
    }

    private static IReadOnlyList<double> BuildRayAngles(OrthogonalPolygon polygon, Point2D origin)
    {
        var angles = new List<double>();

        foreach (var vertex in polygon.Vertices)
        {
            double angle = Math.Atan2(vertex.Y - origin.Y, vertex.X - origin.X);
            angles.Add(angle - AngleDelta);
            angles.Add(angle);
            angles.Add(angle + AngleDelta);
        }

        angles.Sort();
        return angles;
    }

    private static Point2D? FindClosestIntersection(OrthogonalPolygon polygon, Point2D origin, double angle)
    {
        double dx = Math.Cos(angle);
        double dy = Math.Sin(angle);

        Point2D? closestPoint = null;
        double closestDistance = double.PositiveInfinity;

        foreach (var edge in polygon.GetEdges())
        {
            if (TryIntersectRayWithSegment(origin, dx, dy, edge.Start, edge.End, out var hitPoint, out var distance))
            {
                if (distance < closestDistance && distance > GeometryEpsilon)
                {
                    closestDistance = distance;
                    closestPoint = hitPoint;
                }
            }
        }

        return closestPoint;
    }

    private static bool TryIntersectRayWithSegment(
        Point2D origin,
        double dirX,
        double dirY,
        Vertex segmentStart,
        Vertex segmentEnd,
        out Point2D intersection,
        out double distance)
    {
        double rx = dirX;
        double ry = dirY;

        double sx = segmentEnd.X - segmentStart.X;
        double sy = segmentEnd.Y - segmentStart.Y;

        double qpx = segmentStart.X - origin.X;
        double qpy = segmentStart.Y - origin.Y;

        double denominator = Cross(rx, ry, sx, sy);

        if (Math.Abs(denominator) < GeometryEpsilon)
        {
            intersection = default;
            distance = 0;
            return false;
        }

        double t = Cross(qpx, qpy, sx, sy) / denominator;
        double u = Cross(qpx, qpy, rx, ry) / denominator;

        if (t < GeometryEpsilon || u < -GeometryEpsilon || u > 1 + GeometryEpsilon)
        {
            intersection = default;
            distance = 0;
            return false;
        }

        intersection = new Point2D(origin.X + t * rx, origin.Y + t * ry);
        distance = t;
        return true;
    }

    private static double Cross(double ax, double ay, double bx, double by)
    {
        return ax * by - ay * bx;
    }

    private static IReadOnlyList<Point2D> RemoveNearDuplicatePoints(IReadOnlyList<Point2D> points)
    {
        var result = new List<Point2D>();

        foreach (var point in points)
        {
            if (result.Count == 0 || !AreClose(result[^1], point))
            {
                result.Add(point);
            }
        }

        if (result.Count > 1 && AreClose(result[0], result[^1]))
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    private static bool AreClose(Point2D a, Point2D b)
    {
        return Math.Abs(a.X - b.X) < 1e-6 && Math.Abs(a.Y - b.Y) < 1e-6;
    }
}