using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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