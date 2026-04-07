using System.Collections.Generic;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Services.Partitioning;

public static class MonotoneQuadrangulationService
{
    public static IReadOnlyList<Quadrilateral> Quadrangulate(
        OrthogonalPolygon polygon,
        MonotonePiece piece)
    {
        var result = new List<Quadrilateral>();

        // Базовий варіант:
        // якщо шматок уже є 4-кутником — повертаємо його.
        if (piece.BoundaryVertexIndices.Count == 4)
        {
            result.Add(new Quadrilateral(piece.BoundaryVertexIndices));
        }

        return result;
    }
}