using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace PrisonYard.Models.Geometry;

public readonly record struct Edge(Vertex Start, Vertex End)
{
    public bool IsHorizontal => Start.Y == End.Y;
    public bool IsVertical => Start.X == End.X;

    public int Length
    {
        get
        {
            if (IsHorizontal)
            {
                return Math.Abs(End.X - Start.X);
            }

            if (IsVertical)
            {
                return Math.Abs(End.Y - Start.Y);
            }

            return 0;
        }
    }
}