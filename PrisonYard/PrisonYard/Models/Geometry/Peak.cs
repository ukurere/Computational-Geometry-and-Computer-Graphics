using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonYard.Models.Geometry;
public enum PeakKind
{
    Top = 0,
    Bottom = 1
}

public sealed class Peak
{
    public PeakKind Kind { get; }
    public HorizontalEdgeInfo EdgeInfo { get; }

    public int EdgeIndex => EdgeInfo.EdgeIndex;
    public int LeftVertexIndex => EdgeInfo.StartVertexIndex;
    public int RightVertexIndex => EdgeInfo.EndVertexIndex;

    public int Y => EdgeInfo.Y;
    public int LeftX => EdgeInfo.LeftX;
    public int RightX => EdgeInfo.RightX;

    public Peak(PeakKind kind, HorizontalEdgeInfo edgeInfo)
    {
        EdgeInfo = edgeInfo ?? throw new ArgumentNullException(nameof(edgeInfo));
        Kind = kind;
    }
}
