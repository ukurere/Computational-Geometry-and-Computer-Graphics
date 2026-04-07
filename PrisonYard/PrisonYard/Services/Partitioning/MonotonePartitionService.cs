using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Geometry;

namespace PrisonYard.Services.Partitioning;

public static class MonotonePartitionService
{
    public static IReadOnlyList<Diagonal> BuildPartitionDiagonals(OrthogonalPolygon polygon)
    {
        var peaks = OrthogonalEdgeAnalysisService.GetPeaks(polygon);
        var result = new List<Diagonal>();

        foreach (var peak in peaks)
        {
            TryAddDiagonalFromPeak(polygon, peak, result);
        }

        return result
            .Distinct()
            .OrderBy(d => d.MinVertexIndex)
            .ThenBy(d => d.MaxVertexIndex)
            .ToList();
    }

    public static IReadOnlyList<MonotonePiece> BuildPieces(
        OrthogonalPolygon polygon,
        IReadOnlyList<Diagonal> partitionDiagonals)
    {
        // Базовий крок: поки що повертаємо один шматок —
        // увесь полігон. Діагоналі вже можна відмалювати
        // покроково в UI, а справжнє розбиття на підполігони
        // допишемо наступним етапом.
        return new[]
        {
            new MonotonePiece(
                boundaryVertexIndices: Enumerable.Range(0, polygon.VertexCount).ToList(),
                partitionDiagonals: partitionDiagonals,
                isVerticalMonotone: false,
                isPseudoMonotone: true)
        };
    }

    private static void TryAddDiagonalFromPeak(
        OrthogonalPolygon polygon,
        Peak peak,
        List<Diagonal> result)
    {
        int[] candidateEndpoints = [peak.LeftVertexIndex, peak.RightVertexIndex];

        foreach (int endpoint in candidateEndpoints)
        {
            int endpointX = polygon.GetVertex(endpoint).X;
            int endpointY = polygon.GetVertex(endpoint).Y;

            var candidates = new List<(int VertexIndex, int Distance)>();

            for (int i = 0; i < polygon.VertexCount; i++)
            {
                if (i == endpoint)
                {
                    continue;
                }

                var vertex = polygon.GetVertex(i);

                if (vertex.X != endpointX)
                {
                    continue;
                }

                bool correctDirection =
                    peak.Kind == PeakKind.Bottom
                        ? vertex.Y > endpointY
                        : vertex.Y < endpointY;

                if (!correctDirection)
                {
                    continue;
                }

                int distance = System.Math.Abs(vertex.Y - endpointY);
                candidates.Add((i, distance));
            }

            foreach (var candidate in candidates.OrderBy(c => c.Distance))
            {
                var diagonal = new Diagonal(endpoint, candidate.VertexIndex).Normalize();

                if (!result.Contains(diagonal) &&
                    GeometryHelper.IsValidInternalDiagonal(
                        polygon,
                        diagonal.FromVertexIndex,
                        diagonal.ToVertexIndex))
                {
                    result.Add(diagonal);
                    return;
                }
            }
        }
    }
}