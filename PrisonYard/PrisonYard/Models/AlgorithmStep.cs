using System.Collections.Generic;

namespace PrisonYard.Models;

public sealed class AlgorithmStep
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<int> HighlightedVertexIndices { get; init; } = [];
    public IReadOnlyList<int> ReflexVertexIndices { get; init; } = [];
    public IReadOnlyList<CameraPlacement> Cameras { get; init; } = [];
    public IReadOnlyList<VisibilityRegion> VisibilityRegions { get; init; } = [];

    public bool ShowPolygonFill { get; init; } = true;
    public bool ShowVertexIndices { get; init; } = true;
    public bool ShowEdges { get; init; } = true;
}