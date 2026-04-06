using System.Collections.Generic;

namespace PrisonYard.Models;

public sealed class AlgorithmRun
{
    public OrthogonalPolygon Polygon { get; init; } = null!;
    public IReadOnlyList<AlgorithmStep> Steps { get; init; } = [];
}