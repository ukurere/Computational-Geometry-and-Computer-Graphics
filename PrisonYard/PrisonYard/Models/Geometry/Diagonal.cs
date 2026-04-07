using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace PrisonYard.Models.Geometry;

public readonly record struct Diagonal(int FromVertexIndex, int ToVertexIndex)
{
    public int MinVertexIndex => Math.Min(FromVertexIndex, ToVertexIndex);
    public int MaxVertexIndex => Math.Max(FromVertexIndex, ToVertexIndex);

    public bool IsDegenerate => FromVertexIndex == ToVertexIndex;

    public Diagonal Normalize()
    {
        return MinVertexIndex <= MaxVertexIndex
            ? new Diagonal(MinVertexIndex, MaxVertexIndex)
            : new Diagonal(MaxVertexIndex, MinVertexIndex);
    }

    public bool ContainsVertex(int vertexIndex)
    {
        return FromVertexIndex == vertexIndex || ToVertexIndex == vertexIndex;
    }
}