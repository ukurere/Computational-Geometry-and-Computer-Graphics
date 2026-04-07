using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Services.Partitioning;

public static class OrthogonalEdgeAnalysisService
{
    public static IReadOnlyList<HorizontalEdgeInfo> GetHorizontalEdges(OrthogonalPolygon polygon)
    {
        return polygon.GetHorizontalEdges();
    }

    public static IReadOnlyList<HorizontalEdgeInfo> GetTopEdges(OrthogonalPolygon polygon)
    {
        return polygon.GetHorizontalEdges()
            .Where(edge => edge.IsTop)
            .ToList();
    }

    public static IReadOnlyList<HorizontalEdgeInfo> GetBottomEdges(OrthogonalPolygon polygon)
    {
        return polygon.GetHorizontalEdges()
            .Where(edge => edge.IsBottom)
            .ToList();
    }

    public static IReadOnlyList<Peak> GetPeaks(OrthogonalPolygon polygon)
    {
        var horizontalEdges = polygon.GetHorizontalEdges();
        var peaks = new List<Peak>();

        foreach (var edgeInfo in horizontalEdges)
        {
            bool leftReflex = polygon.GetVertexKind(edgeInfo.StartVertexIndex) == VertexKind.Reflex;
            bool rightReflex = polygon.GetVertexKind(edgeInfo.EndVertexIndex) == VertexKind.Reflex;

            if (!leftReflex || !rightReflex)
            {
                continue;
            }

            if (edgeInfo.Kind == EdgeKind.Top)
            {
                peaks.Add(new Peak(PeakKind.Top, edgeInfo));
            }
            else if (edgeInfo.Kind == EdgeKind.Bottom)
            {
                peaks.Add(new Peak(PeakKind.Bottom, edgeInfo));
            }
        }

        return peaks;
    }
}