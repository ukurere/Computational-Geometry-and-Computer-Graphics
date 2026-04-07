using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonYard.Models.Geometry;
public sealed class MonotonePiece
{
    public IReadOnlyList<int> BoundaryVertexIndices { get; }
    public IReadOnlyList<Diagonal> PartitionDiagonals { get; }

    public bool IsVerticalMonotone { get; }
    public bool IsPseudoMonotone { get; }

    public MonotonePiece(
        IEnumerable<int> boundaryVertexIndices,
        IEnumerable<Diagonal>? partitionDiagonals = null,
        bool isVerticalMonotone = true,
        bool isPseudoMonotone = true)
    {
        if (boundaryVertexIndices is null)
        {
            throw new ArgumentNullException(nameof(boundaryVertexIndices));
        }

        var boundary = boundaryVertexIndices.ToList();

        if (boundary.Count < 4)
        {
            throw new ArgumentException(
                "MonotonePiece повинен містити щонайменше 4 вершини.",
                nameof(boundaryVertexIndices));
        }

        BoundaryVertexIndices = boundary;
        PartitionDiagonals = partitionDiagonals?.ToList() ?? new List<Diagonal>();

        IsVerticalMonotone = isVerticalMonotone;
        IsPseudoMonotone = isPseudoMonotone;
    }

    public int VertexCount => BoundaryVertexIndices.Count;
}