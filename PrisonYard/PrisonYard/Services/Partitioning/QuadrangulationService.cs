using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Services.Partitioning;

public static class QuadrangulationService
{
    public static (
        IReadOnlyList<Diagonal> PartitionDiagonals,
        IReadOnlyList<MonotonePiece> MonotonePieces,
        IReadOnlyList<Quadrilateral> Quadrilaterals)
        Quadrangulate(OrthogonalPolygon polygon)
    {
        var partitionDiagonals = MonotonePartitionService.BuildPartitionDiagonals(polygon);
        var pieces = MonotonePartitionService.BuildPieces(polygon, partitionDiagonals);

        var quadrilaterals = new List<Quadrilateral>();

        foreach (var piece in pieces)
        {
            quadrilaterals.AddRange(MonotoneQuadrangulationService.Quadrangulate(polygon, piece));
        }

        if (quadrilaterals.Count == 0 && polygon.VertexCount == 4)
        {
            quadrilaterals.Add(new Quadrilateral(new[] { 0, 1, 2, 3 }));
        }

        return (
            partitionDiagonals,
            pieces,
            quadrilaterals
        );
    }

    public static IReadOnlyList<Diagonal> CollectQuadrangulationDiagonals(
        IReadOnlyList<Quadrilateral> quadrilaterals)
    {
        return quadrilaterals
            .SelectMany(quadrilateral => quadrilateral.GetDiagonals())
            .Distinct()
            .ToList();
    }
}