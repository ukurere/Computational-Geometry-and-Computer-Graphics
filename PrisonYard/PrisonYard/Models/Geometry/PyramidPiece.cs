using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonYard.Models.Geometry;
public sealed class PyramidPiece
{
    public IReadOnlyList<int> BoundaryVertexIndices { get; }

    public IReadOnlyList<int> LeftStaircaseReflexVertexIndices { get; }
    public IReadOnlyList<int> RightStaircaseReflexVertexIndices { get; }

    public int BaseLeftVertexIndex { get; }
    public int BaseRightVertexIndex { get; }

    public PyramidPiece(
        IEnumerable<int> boundaryVertexIndices,
        IEnumerable<int> leftStaircaseReflexVertexIndices,
        IEnumerable<int> rightStaircaseReflexVertexIndices,
        int baseLeftVertexIndex,
        int baseRightVertexIndex)
    {
        if (boundaryVertexIndices is null)
        {
            throw new ArgumentNullException(nameof(boundaryVertexIndices));
        }

        if (leftStaircaseReflexVertexIndices is null)
        {
            throw new ArgumentNullException(nameof(leftStaircaseReflexVertexIndices));
        }

        if (rightStaircaseReflexVertexIndices is null)
        {
            throw new ArgumentNullException(nameof(rightStaircaseReflexVertexIndices));
        }

        var boundary = boundaryVertexIndices.ToList();

        if (boundary.Count < 4)
        {
            throw new ArgumentException(
                "PyramidPiece повинен містити щонайменше 4 вершини.",
                nameof(boundaryVertexIndices));
        }

        BoundaryVertexIndices = boundary;
        LeftStaircaseReflexVertexIndices = leftStaircaseReflexVertexIndices.ToList();
        RightStaircaseReflexVertexIndices = rightStaircaseReflexVertexIndices.ToList();
        BaseLeftVertexIndex = baseLeftVertexIndex;
        BaseRightVertexIndex = baseRightVertexIndex;
    }
}