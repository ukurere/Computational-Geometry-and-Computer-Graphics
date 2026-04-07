using System.Collections.Generic;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Services.Geometry;

public static class PolygonAnalysisService
{
    public static string GetOrientationLabel(OrthogonalPolygon polygon)
    {
        return polygon.IsCounterClockwise() ? "CCW" : "CW";
    }

    public static int GetReflexCount(OrthogonalPolygon polygon)
    {
        return polygon.GetReflexVertexIndices().Count;
    }

    public static int GetConvexCount(OrthogonalPolygon polygon)
    {
        return polygon.GetConvexVertexIndices().Count;
    }

    public static IReadOnlyList<int> GetReflexVertexIndices(OrthogonalPolygon polygon)
    {
        return polygon.GetReflexVertexIndices();
    }

    public static IReadOnlyList<int> GetConvexVertexIndices(OrthogonalPolygon polygon)
    {
        return polygon.GetConvexVertexIndices();
    }

    public static IReadOnlyList<HorizontalEdgeInfo> GetHorizontalEdges(OrthogonalPolygon polygon)
    {
        return polygon.GetHorizontalEdges();
    }
}