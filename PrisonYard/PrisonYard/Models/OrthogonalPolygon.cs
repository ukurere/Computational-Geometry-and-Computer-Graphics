using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonYard.Models;

public sealed class OrthogonalPolygon
{
    public IReadOnlyList<Vertex> Vertices { get; }

    public OrthogonalPolygon(IEnumerable<Vertex> vertices)
    {
        var list = vertices.ToList();

        if (list.Count >= 2 && list[0] == list[^1])
        {
            list.RemoveAt(list.Count - 1);
        }

        if (list.Count < 4)
        {
            throw new ArgumentException("Багатокутник повинен мати щонайменше 4 вершини.");
        }

        Validate(list);
        Vertices = list;
    }

    public IReadOnlyList<Edge> GetEdges()
    {
        var edges = new List<Edge>();

        for (int i = 0; i < Vertices.Count; i++)
        {
            var start = Vertices[i];
            var end = Vertices[(i + 1) % Vertices.Count];
            edges.Add(new Edge(start, end));
        }

        return edges;
    }

    public int GetSignedDoubleArea()
    {
        int sum = 0;

        for (int i = 0; i < Vertices.Count; i++)
        {
            var a = Vertices[i];
            var b = Vertices[(i + 1) % Vertices.Count];
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum;
    }

    public bool IsCounterClockwise() => GetSignedDoubleArea() > 0;

    public VertexKind GetVertexKind(int index)
    {
        int n = Vertices.Count;

        var prev = Vertices[(index - 1 + n) % n];
        var current = Vertices[index];
        var next = Vertices[(index + 1) % n];

        int dx1 = current.X - prev.X;
        int dy1 = current.Y - prev.Y;

        int dx2 = next.X - current.X;
        int dy2 = next.Y - current.Y;

        int cross = dx1 * dy2 - dy1 * dx2;

        // Для CCW: left turn => convex, right turn => reflex
        // Для CW: навпаки
        bool isCcw = IsCounterClockwise();

        if (cross == 0)
        {
            throw new InvalidOperationException($"Вершина {index} вироджена: сусідні ребра колінеарні.");
        }

        if (isCcw)
        {
            return cross > 0 ? VertexKind.Convex : VertexKind.Reflex;
        }

        return cross < 0 ? VertexKind.Convex : VertexKind.Reflex;
    }

    public IReadOnlyList<int> GetReflexVertexIndices()
    {
        var result = new List<int>();

        for (int i = 0; i < Vertices.Count; i++)
        {
            if (GetVertexKind(i) == VertexKind.Reflex)
            {
                result.Add(i);
            }
        }

        return result;
    }

    private static void Validate(IReadOnlyList<Vertex> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];

            if (current == next)
            {
                throw new ArgumentException($"Сусідні вершини {i} і {(i + 1) % vertices.Count} збігаються.");
            }

            bool sameX = current.X == next.X;
            bool sameY = current.Y == next.Y;

            if (!(sameX ^ sameY))
            {
                throw new ArgumentException(
                    $"Ребро між вершинами {i} і {(i + 1) % vertices.Count} не є ортогональним.");
            }
        }
    }
}