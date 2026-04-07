using System;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Services.Geometry;

public static class GeometryHelper
{
    public const double Epsilon = 1e-9;

    public static Point2D ToPoint2D(Vertex vertex)
    {
        return new Point2D(vertex.X, vertex.Y);
    }

    public static double Cross(Point2D a, Point2D b, Point2D c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }

    public static bool AreAdjacentVertices(OrthogonalPolygon polygon, int i, int j)
    {
        int n = polygon.VertexCount;

        if (i == j)
        {
            return true;
        }

        int diff = Math.Abs(i - j);
        return diff == 1 || diff == n - 1;
    }

    public static bool IsValidInternalDiagonal(OrthogonalPolygon polygon, int fromVertexIndex, int toVertexIndex)
    {
        if (fromVertexIndex == toVertexIndex)
        {
            return false;
        }

        if (AreAdjacentVertices(polygon, fromVertexIndex, toVertexIndex))
        {
            return false;
        }

        var a = polygon.GetVertex(fromVertexIndex);
        var b = polygon.GetVertex(toVertexIndex);

        if (!IsSegmentProperlyInsidePolygon(polygon, ToPoint2D(a), ToPoint2D(b)))
        {
            return false;
        }

        for (int edgeIndex = 0; edgeIndex < polygon.VertexCount; edgeIndex++)
        {
            var edge = polygon.GetEdge(edgeIndex);

            if (edgeIndex == fromVertexIndex || edgeIndex == toVertexIndex)
            {
                continue;
            }

            int prevEdgeIndex = ((fromVertexIndex - 1) % polygon.VertexCount + polygon.VertexCount) % polygon.VertexCount;
            int prevEdgeIndex2 = ((toVertexIndex - 1) % polygon.VertexCount + polygon.VertexCount) % polygon.VertexCount;

            if (edgeIndex == prevEdgeIndex || edgeIndex == prevEdgeIndex2)
            {
                continue;
            }

            if (SegmentsIntersect(
                    ToPoint2D(a),
                    ToPoint2D(b),
                    ToPoint2D(edge.Start),
                    ToPoint2D(edge.End)))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsSegmentProperlyInsidePolygon(OrthogonalPolygon polygon, Point2D a, Point2D b)
    {
        var midpoint = new Point2D((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);

        if (!polygon.ContainsPoint(midpoint, includeBoundary: false))
        {
            return false;
        }

        int samples = 9;

        for (int i = 1; i < samples; i++)
        {
            double t = i / (double)samples;
            var sample = new Point2D(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t);

            if (!polygon.ContainsPoint(sample, includeBoundary: true))
            {
                return false;
            }
        }

        return true;
    }

    public static bool SegmentsIntersect(Point2D a, Point2D b, Point2D c, Point2D d)
    {
        double o1 = Cross(a, b, c);
        double o2 = Cross(a, b, d);
        double o3 = Cross(c, d, a);
        double o4 = Cross(c, d, b);

        if (Math.Abs(o1) < Epsilon && OnSegment(a, c, b)) return true;
        if (Math.Abs(o2) < Epsilon && OnSegment(a, d, b)) return true;
        if (Math.Abs(o3) < Epsilon && OnSegment(c, a, d)) return true;
        if (Math.Abs(o4) < Epsilon && OnSegment(c, b, d)) return true;

        return (o1 > 0) != (o2 > 0) && (o3 > 0) != (o4 > 0);
    }

    public static bool OnSegment(Point2D a, Point2D p, Point2D b)
    {
        return p.X >= Math.Min(a.X, b.X) - Epsilon &&
               p.X <= Math.Max(a.X, b.X) + Epsilon &&
               p.Y >= Math.Min(a.Y, b.Y) - Epsilon &&
               p.Y <= Math.Max(a.Y, b.Y) + Epsilon;
    }
}