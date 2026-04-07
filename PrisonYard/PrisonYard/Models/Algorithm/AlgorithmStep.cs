using System.Collections.Generic;
using PrisonYard.Models;
using PrisonYard.Models.Algorithm;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Demo;
using PrisonYard.Services.Parsing;
using PrisonYard.Services.Rendering;

namespace PrisonYard.Models.Algorithm;

public sealed class AlgorithmStep
{
    public StepActionType ActionType { get; init; } = StepActionType.None;

    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<int> HighlightedVertexIndices { get; init; } = [];
    public IReadOnlyList<int> ReflexVertexIndices { get; init; } = [];
    public IReadOnlyList<int> HighlightedEdgeIndices { get; init; } = [];

    public IReadOnlyList<(int FromVertexIndex, int ToVertexIndex)> Diagonals { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<int>> QuadrilateralVertexGroups { get; init; } = [];

    public IReadOnlyList<CameraPlacement> Cameras { get; init; } = [];
    public IReadOnlyList<VisibilityRegion> VisibilityRegions { get; init; } = [];

    public ColoringResult? Coloring { get; init; }

    public bool ShowPolygonFill { get; init; } = true;
    public bool ShowVertexIndices { get; init; } = true;
    public bool ShowEdges { get; init; } = true;
    public bool ShowDiagonals { get; init; } = true;
    public bool ShowQuadrilaterals { get; init; } = true;
    public bool ShowColors { get; init; } = true;
}