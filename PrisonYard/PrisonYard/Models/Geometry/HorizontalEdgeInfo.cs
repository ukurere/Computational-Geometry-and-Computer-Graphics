using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonYard.Models.Geometry;
public sealed class HorizontalEdgeInfo
{
    public int EdgeIndex { get; }
    public int StartVertexIndex { get; }
    public int EndVertexIndex { get; }

    public Edge Edge { get; }
    public EdgeKind Kind { get; }

    public int Y => Edge.Start.Y;
    public int LeftX => Math.Min(Edge.Start.X, Edge.End.X);
    public int RightX => Math.Max(Edge.Start.X, Edge.End.X);
    public int Length => RightX - LeftX;

    public bool IsTop => Kind == EdgeKind.Top;
    public bool IsBottom => Kind == EdgeKind.Bottom;

    public HorizontalEdgeInfo(
        int edgeIndex,
        int startVertexIndex,
        int endVertexIndex,
        Edge edge,
        EdgeKind kind)
    {
        if (!edge.IsHorizontal)
        {
            throw new ArgumentException(
                "HorizontalEdgeInfo може описувати лише горизонтальне ребро.",
                nameof(edge));
        }

        if (kind != EdgeKind.Top && kind != EdgeKind.Bottom)
        {
            throw new ArgumentException(
                "Для HorizontalEdgeInfo тип ребра повинен бути Top або Bottom.",
                nameof(kind));
        }

        EdgeIndex = edgeIndex;
        StartVertexIndex = startVertexIndex;
        EndVertexIndex = endVertexIndex;
        Edge = edge;
        Kind = kind;
    }
}