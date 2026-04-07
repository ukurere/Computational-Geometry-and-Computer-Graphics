using System.Collections.Generic;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Models.Algorithm;
public sealed class AlgorithmRun
{
    public OrthogonalPolygon Polygon { get; init; } = null!;
    public IReadOnlyList<AlgorithmStep> Steps { get; init; } = [];

    public int StepCount => Steps.Count;
    public bool IsEmpty => Steps.Count == 0;
}