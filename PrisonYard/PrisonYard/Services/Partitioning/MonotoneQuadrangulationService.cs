using System.Collections.Generic;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Geometry;

namespace PrisonYard.Services.Partitioning;

public static class MonotoneQuadrangulationService
{
    public static IReadOnlyList<Quadrilateral> Quadrangulate(
        OrthogonalPolygon polygon,
        MonotonePiece piece)
    {
        var result = new List<Quadrilateral>();
        QuadrangulateRecursive(polygon, piece.BoundaryVertexIndices.ToList(), result);
        return result;
    }

    private static void QuadrangulateRecursive(
        OrthogonalPolygon polygon,
        List<int> boundary,
        List<Quadrilateral> result)
    {
        if (boundary.Count < 4) return;

        if (boundary.Count == 4)
        {
            result.Add(new Quadrilateral(boundary));
            return;
        }

        int n = boundary.Count;

        for (int i = 0; i < n; i++)
        {
            int i3 = (i + 3) % n;

            if (IsValidEarDiagonal(polygon, boundary, i, i3))
            {
                result.Add(new Quadrilateral(new[]
                {
                    boundary[i],
                    boundary[(i + 1) % n],
                    boundary[(i + 2) % n],
                    boundary[i3]
                }));

                QuadrangulateRecursive(polygon, RemoveEarVertices(boundary, i), result);
                return;
            }
        }

        // Fallback: force progress to avoid infinite loop on degenerate input
        result.Add(new Quadrilateral(new[] { boundary[0], boundary[1], boundary[2], boundary[3] }));
        QuadrangulateRecursive(polygon, RemoveEarVertices(boundary, 0), result);
    }

    private static bool IsValidEarDiagonal(
        OrthogonalPolygon polygon,
        List<int> boundary,
        int fromPos,
        int toPos)
    {
        int n = boundary.Count;
        int fromIdx = boundary[fromPos];
        int toIdx = boundary[toPos];

        if (fromIdx == toIdx) return false;

        // Must not be adjacent in the piece boundary
        int dist = System.Math.Abs(fromPos - toPos);
        dist = System.Math.Min(dist, n - dist);
        if (dist <= 1) return false;

        var p1 = GeometryHelper.ToPoint2D(polygon.GetVertex(fromIdx));
        var p2 = GeometryHelper.ToPoint2D(polygon.GetVertex(toIdx));

        // Midpoint must be strictly inside the piece
        var mid = new Point2D((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
        if (!IsInsidePiece(polygon, boundary, mid))
            return false;

        // Diagonal must not cross any boundary edge (skip edges that share an endpoint)
        for (int i = 0; i < n; i++)
        {
            int a = boundary[i];
            int b = boundary[(i + 1) % n];

            if (a == fromIdx || a == toIdx || b == fromIdx || b == toIdx)
                continue;

            var pa = GeometryHelper.ToPoint2D(polygon.GetVertex(a));
            var pb = GeometryHelper.ToPoint2D(polygon.GetVertex(b));

            if (GeometryHelper.SegmentsIntersect(p1, p2, pa, pb))
                return false;
        }

        return true;
    }

    private static bool IsInsidePiece(OrthogonalPolygon polygon, List<int> boundary, Point2D point)
    {
        bool inside = false;
        int n = boundary.Count;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var vi = polygon.GetVertex(boundary[i]);
            var vj = polygon.GetVertex(boundary[j]);

            bool crosses =
                ((vi.Y > point.Y) != (vj.Y > point.Y)) &&
                (point.X < (double)(vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y) + vi.X);

            if (crosses) inside = !inside;
        }

        return inside;
    }

    private static List<int> RemoveEarVertices(List<int> boundary, int earStartPos)
    {
        int n = boundary.Count;
        int skip1 = (earStartPos + 1) % n;
        int skip2 = (earStartPos + 2) % n;

        var result = new List<int>(n - 2);
        for (int i = 0; i < n; i++)
        {
            if (i != skip1 && i != skip2)
                result.Add(boundary[i]);
        }

        return result;
    }
}
