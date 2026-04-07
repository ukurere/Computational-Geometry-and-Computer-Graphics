using System;
using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models.Algorithm;
using PrisonYard.Models.Geometry;

namespace PrisonYard.Services.Coloring;

public static class FourColoringService
{
    public static ColoringResult ColorVertices(
        OrthogonalPolygon polygon,
        IReadOnlyList<Quadrilateral> quadrilaterals)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        var adjacency = BuildAdjacency(polygon, quadrilaterals);
        var order = Enumerable.Range(0, polygon.VertexCount)
            .OrderByDescending(v => adjacency[v].Count)
            .ToList();

        var colors = Enumerable.Repeat(-1, polygon.VertexCount).ToArray();

        bool success = ColorBacktracking(0, order, colors, adjacency);

        if (!success)
        {
            throw new InvalidOperationException("Не вдалося виконати 4-розфарбування графа.");
        }

        var colorMap = Enumerable.Range(0, polygon.VertexCount)
            .ToDictionary(i => i, i => colors[i]);

        int selectedColor = Enumerable.Range(0, 4)
            .OrderBy(color => colorMap.Count(pair => pair.Value == color))
            .First();

        return new ColoringResult(
            vertexColors: colorMap,
            colorCount: 4,
            selectedGuardColor: selectedColor);
    }

    private static List<HashSet<int>> BuildAdjacency(
        OrthogonalPolygon polygon,
        IReadOnlyList<Quadrilateral> quadrilaterals)
    {
        var adjacency = Enumerable.Range(0, polygon.VertexCount)
            .Select(_ => new HashSet<int>())
            .ToList();

        for (int i = 0; i < polygon.VertexCount; i++)
        {
            int j = (i + 1) % polygon.VertexCount;
            AddEdge(adjacency, i, j);
        }

        foreach (var quadrilateral in quadrilaterals)
        {
            foreach (var diagonal in quadrilateral.GetDiagonals())
            {
                AddEdge(adjacency, diagonal.FromVertexIndex, diagonal.ToVertexIndex);
            }
        }

        return adjacency;
    }

    private static void AddEdge(List<HashSet<int>> adjacency, int a, int b)
    {
        if (a == b)
        {
            return;
        }

        adjacency[a].Add(b);
        adjacency[b].Add(a);
    }

    private static bool ColorBacktracking(
        int position,
        IReadOnlyList<int> order,
        int[] colors,
        List<HashSet<int>> adjacency)
    {
        if (position == order.Count)
        {
            return true;
        }

        int vertex = order[position];

        for (int color = 0; color < 4; color++)
        {
            if (CanUseColor(vertex, color, colors, adjacency))
            {
                colors[vertex] = color;

                if (ColorBacktracking(position + 1, order, colors, adjacency))
                {
                    return true;
                }

                colors[vertex] = -1;
            }
        }

        return false;
    }

    private static bool CanUseColor(
        int vertex,
        int color,
        int[] colors,
        List<HashSet<int>> adjacency)
    {
        foreach (int neighbor in adjacency[vertex])
        {
            if (colors[neighbor] == color)
            {
                return false;
            }
        }

        return true;
    }
}