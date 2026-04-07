using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonYard.Models.Geometry;
public sealed class Quadrilateral
{
    public IReadOnlyList<int> VertexIndices { get; }

    public Quadrilateral(IEnumerable<int> vertexIndices)
    {
        if (vertexIndices is null)
        {
            throw new ArgumentNullException(nameof(vertexIndices));
        }

        var indices = vertexIndices.ToList();

        if (indices.Count != 4)
        {
            throw new ArgumentException(
                "Quadrilateral повинен містити рівно 4 вершини у циклічному порядку.",
                nameof(vertexIndices));
        }

        VertexIndices = indices;
    }

    public int A => VertexIndices[0];
    public int B => VertexIndices[1];
    public int C => VertexIndices[2];
    public int D => VertexIndices[3];

    public IReadOnlyList<Diagonal> GetDiagonals()
    {
        return new[]
        {
            new Diagonal(A, C),
            new Diagonal(B, D)
        };
    }

    public IReadOnlyList<(int From, int To)> GetBoundaryEdges()
    {
        return new[]
        {
            (A, B),
            (B, C),
            (C, D),
            (D, A)
        };
    }

    public bool ContainsVertex(int vertexIndex)
    {
        return VertexIndices.Contains(vertexIndex);
    }
}