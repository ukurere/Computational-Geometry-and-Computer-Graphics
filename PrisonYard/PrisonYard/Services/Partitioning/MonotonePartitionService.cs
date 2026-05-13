using System;
using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Geometry;

namespace PrisonYard.Services.Partitioning;

public static class MonotonePartitionService
{
    private const double Epsilon = 1e-9;

    public static IReadOnlyList<Diagonal> BuildPartitionDiagonals(OrthogonalPolygon polygon)
    {
        var peaks = OrthogonalEdgeAnalysisService.GetPeaks(polygon);
        var result = new List<Diagonal>();

        foreach (var peak in peaks)
        {
            if (!TryBuildDiagonalForPeak(polygon, peak, out var diagonal, out _))
            {
                continue;
            }

            diagonal = diagonal.Normalize();

            if (result.Contains(diagonal))
            {
                continue;
            }

            if (!CanAddPartitionDiagonal(polygon, diagonal, result))
            {
                continue;
            }

            result.Add(diagonal);
        }

        return result
            .OrderBy(d => d.MinVertexIndex)
            .ThenBy(d => d.MaxVertexIndex)
            .ToList();
    }

    public static bool TryBuildDiagonalForPeak(
        OrthogonalPolygon polygon,
        Peak peak,
        out Diagonal diagonal,
        out int targetEdgeIndex)
    {
        diagonal = default;
        targetEdgeIndex = -1;

        var candidateEdges = polygon.GetHorizontalEdges()
            .Where(edge => edge.EdgeIndex != peak.EdgeIndex)
            .Where(edge => IsCandidateTargetEdge(peak, edge))
            .OrderBy(edge => Math.Abs(edge.Y - peak.Y))
            .ThenBy(edge => edge.LeftX)
            .ToList();

        foreach (var targetEdge in candidateEdges)
        {
            if (!AreEdgesVerticallyVisible(polygon, peak.EdgeInfo, targetEdge))
            {
                continue;
            }

            var endpointPairs = BuildEndpointDiagonalCandidates(polygon, peak, targetEdge);

            foreach (var candidate in endpointPairs)
            {
                if (!GeometryHelper.IsValidInternalDiagonal(
                        polygon,
                        candidate.FromVertexIndex,
                        candidate.ToVertexIndex))
                {
                    continue;
                }

                diagonal = candidate.Normalize();
                targetEdgeIndex = targetEdge.EdgeIndex;
                return true;
            }
        }

        return false;
    }

    public static bool CanAddPartitionDiagonal(
        OrthogonalPolygon polygon,
        Diagonal candidate,
        IReadOnlyList<Diagonal> existingDiagonals)
    {
        var candidateStart = GeometryHelper.ToPoint2D(polygon.GetVertex(candidate.FromVertexIndex));
        var candidateEnd = GeometryHelper.ToPoint2D(polygon.GetVertex(candidate.ToVertexIndex));

        foreach (var existing in existingDiagonals)
        {
            if (ShareEndpoint(candidate, existing))
            {
                continue;
            }

            var existingStart = GeometryHelper.ToPoint2D(polygon.GetVertex(existing.FromVertexIndex));
            var existingEnd = GeometryHelper.ToPoint2D(polygon.GetVertex(existing.ToVertexIndex));

            if (GeometryHelper.SegmentsIntersect(candidateStart, candidateEnd, existingStart, existingEnd))
            {
                return false;
            }
        }

        return true;
    }

    public static IReadOnlyList<MonotonePiece> BuildPieces(
        OrthogonalPolygon polygon,
        IReadOnlyList<Diagonal> partitionDiagonals)
    {
        if (partitionDiagonals.Count == 0)
        {
            return new[]
            {
                new MonotonePiece(
                    Enumerable.Range(0, polygon.VertexCount),
                    partitionDiagonals,
                    isVerticalMonotone: false,
                    isPseudoMonotone: true)
            };
        }

        int n = polygon.VertexCount;

        // Build adjacency from polygon edges + diagonals (both directions)
        var neighbors = new List<List<int>>(n);
        for (int i = 0; i < n; i++)
            neighbors.Add(new List<int>());

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            neighbors[i].Add(j);
            neighbors[j].Add(i);
        }

        foreach (var d in partitionDiagonals)
        {
            int u = d.FromVertexIndex, v = d.ToVertexIndex;
            if (!neighbors[u].Contains(v)) neighbors[u].Add(v);
            if (!neighbors[v].Contains(u)) neighbors[v].Add(u);
        }

        // For each half-edge (u→v), find next half-edge (v→w)
        // by selecting w (≠u) with the smallest counterclockwise angle from the reversed incoming direction.
        // This traces the face to the left of each directed edge.
        var nextHE = new Dictionary<(int, int), (int, int)>();

        for (int v = 0; v < n; v++)
        {
            var posV = polygon.GetVertex(v);

            foreach (int u in neighbors[v])
            {
                var posU = polygon.GetVertex(u);
                double inAngle = Math.Atan2(posV.Y - posU.Y, posV.X - posU.X);
                double reversedAngle = inAngle + Math.PI;

                int bestW = -1;
                double bestTurn = double.MaxValue;

                foreach (int w in neighbors[v])
                {
                    if (w == u) continue;

                    var posW = polygon.GetVertex(w);
                    double outAngle = Math.Atan2(posW.Y - posV.Y, posW.X - posV.X);

                    double turn = outAngle - reversedAngle;
                    turn %= 2 * Math.PI;
                    if (turn < 0) turn += 2 * Math.PI;

                    if (turn < bestTurn)
                    {
                        bestTurn = turn;
                        bestW = w;
                    }
                }

                if (bestW < 0) bestW = u; // fallback for isolated vertex (shouldn't occur)
                nextHE[(u, v)] = (v, bestW);
            }
        }

        // Trace faces by following nextHE until cycle closes
        var visited = new HashSet<(int, int)>();
        var faces = new List<List<int>>();

        foreach (var startHE in nextHE.Keys)
        {
            if (visited.Contains(startHE)) continue;

            var face = new List<int>();
            var cur = startHE;

            for (int iter = 0; iter < n * 3; iter++)
            {
                if (visited.Contains(cur)) break;
                visited.Add(cur);
                face.Add(cur.Item1);

                if (!nextHE.TryGetValue(cur, out var next)) break;
                cur = next;
                if (cur == startHE) break;
            }

            if (face.Count >= 4)
                faces.Add(face);
        }

        bool polygonIsCcw = polygon.IsCounterClockwise();

        var pieces = faces
            .Select(face => (face, area: ComputeSignedArea(face, polygon)))
            .Where(fa => Math.Abs(fa.area) > 0.5 && (fa.area > 0) == polygonIsCcw)
            .Select(fa => new MonotonePiece(fa.face, Array.Empty<Diagonal>()))
            .ToList();

        if (pieces.Count == 0)
        {
            return new[]
            {
                new MonotonePiece(
                    Enumerable.Range(0, polygon.VertexCount),
                    partitionDiagonals,
                    isVerticalMonotone: false,
                    isPseudoMonotone: true)
            };
        }

        return pieces;
    }

    private static double ComputeSignedArea(IReadOnlyList<int> vertexIndices, OrthogonalPolygon polygon)
    {
        double area = 0;
        int n = vertexIndices.Count;
        for (int i = 0; i < n; i++)
        {
            var a = polygon.GetVertex(vertexIndices[i]);
            var b = polygon.GetVertex(vertexIndices[(i + 1) % n]);
            area += (double)a.X * b.Y - (double)b.X * a.Y;
        }
        return area / 2.0;
    }

    private static bool IsCandidateTargetEdge(Peak peak, HorizontalEdgeInfo edge)
    {
        if (peak.Kind == PeakKind.Bottom)
        {
            return edge.Y > peak.Y;
        }

        return edge.Y < peak.Y;
    }

    private static bool AreEdgesVerticallyVisible(
        OrthogonalPolygon polygon,
        HorizontalEdgeInfo source,
        HorizontalEdgeInfo target)
    {
        double overlapLeft = Math.Max(source.LeftX, target.LeftX);
        double overlapRight = Math.Min(source.RightX, target.RightX);

        if (overlapLeft - overlapRight > Epsilon)
        {
            return false;
        }

        double xSample = (overlapLeft + overlapRight) / 2.0;
        double y1 = source.Y;
        double y2 = target.Y;

        int samples = 12;

        for (int i = 1; i < samples; i++)
        {
            double t = i / (double)samples;
            double y = y1 + (y2 - y1) * t;

            var point = new Point2D(xSample, y);

            if (!polygon.ContainsPoint(point, includeBoundary: true))
            {
                return false;
            }
        }

        return true;
    }

    private static IReadOnlyList<Diagonal> BuildEndpointDiagonalCandidates(
        OrthogonalPolygon polygon,
        Peak peak,
        HorizontalEdgeInfo targetEdge)
    {
        int[] sourceVertices =
        [
            peak.LeftVertexIndex,
            peak.RightVertexIndex
        ];

        int[] targetVertices =
        [
            targetEdge.StartVertexIndex,
            targetEdge.EndVertexIndex
        ];

        var candidates = new List<(Diagonal Diagonal, int Score)>();

        foreach (int sourceVertexIndex in sourceVertices)
        {
            var sourceVertex = polygon.GetVertex(sourceVertexIndex);

            foreach (int targetVertexIndex in targetVertices)
            {
                if (sourceVertexIndex == targetVertexIndex)
                {
                    continue;
                }

                var targetVertex = polygon.GetVertex(targetVertexIndex);

                int score =
                    Math.Abs(sourceVertex.X - targetVertex.X) +
                    Math.Abs(sourceVertex.Y - targetVertex.Y);

                candidates.Add((
                    new Diagonal(sourceVertexIndex, targetVertexIndex),
                    score));
            }
        }

        return candidates
            .OrderBy(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Diagonal.MinVertexIndex)
            .ThenBy(candidate => candidate.Diagonal.MaxVertexIndex)
            .Select(candidate => candidate.Diagonal)
            .Distinct()
            .ToList();
    }

    private static bool ShareEndpoint(Diagonal first, Diagonal second)
    {
        return first.FromVertexIndex == second.FromVertexIndex ||
               first.FromVertexIndex == second.ToVertexIndex ||
               first.ToVertexIndex == second.FromVertexIndex ||
               first.ToVertexIndex == second.ToVertexIndex;
    }
}