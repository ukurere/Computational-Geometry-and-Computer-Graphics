using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonYard.Models.Geometry;

public sealed class OrthogonalPolygon
{
    private const double Epsilon = 1e-9;

    public IReadOnlyList<Vertex> Vertices { get; }

    public int VertexCount => Vertices.Count;

    public OrthogonalPolygon(IEnumerable<Vertex> vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        var list = vertices.ToList();

        if (list.Count >= 2 && list[0] == list[^1])
        {
            list.RemoveAt(list.Count - 1);
        }

        if (list.Count < 4)
        {
            throw new ArgumentException(
                "Багатокутник повинен мати щонайменше 4 вершини.",
                nameof(vertices));
        }

        Validate(list);
        Vertices = list;
    }

    public Vertex GetVertex(int index)
    {
        return Vertices[NormalizeIndex(index)];
    }

    public Vertex GetPreviousVertex(int index)
    {
        return GetVertex(index - 1);
    }

    public Vertex GetNextVertex(int index)
    {
        return GetVertex(index + 1);
    }

    public Edge GetEdge(int index)
    {
        return new Edge(GetVertex(index), GetNextVertex(index));
    }

    public IReadOnlyList<Edge> GetEdges()
    {
        var edges = new List<Edge>(VertexCount);

        for (int i = 0; i < VertexCount; i++)
        {
            edges.Add(GetEdge(i));
        }

        return edges;
    }

    public (int MinX, int MaxX, int MinY, int MaxY) GetBoundingBox()
    {
        return (
            Vertices.Min(v => v.X),
            Vertices.Max(v => v.X),
            Vertices.Min(v => v.Y),
            Vertices.Max(v => v.Y)
        );
    }

    public long GetSignedDoubleArea()
    {
        long sum = 0;

        for (int i = 0; i < VertexCount; i++)
        {
            var a = Vertices[i];
            var b = Vertices[(i + 1) % VertexCount];
            sum += (long)a.X * b.Y - (long)b.X * a.Y;
        }

        return sum;
    }

    public bool IsCounterClockwise()
    {
        return GetSignedDoubleArea() > 0;
    }

    public VertexKind GetVertexKind(int index)
    {
        var prev = GetPreviousVertex(index);
        var current = GetVertex(index);
        var next = GetNextVertex(index);

        long dx1 = current.X - prev.X;
        long dy1 = current.Y - prev.Y;

        long dx2 = next.X - current.X;
        long dy2 = next.Y - current.Y;

        long cross = dx1 * dy2 - dy1 * dx2;

        if (cross == 0)
        {
            throw new InvalidOperationException(
                $"Вершина {NormalizeIndex(index)} вироджена: сусідні ребра колінеарні.");
        }

        bool isCcw = IsCounterClockwise();

        if (isCcw)
        {
            return cross > 0 ? VertexKind.Convex : VertexKind.Reflex;
        }

        return cross < 0 ? VertexKind.Convex : VertexKind.Reflex;
    }

    public IReadOnlyList<int> GetReflexVertexIndices()
    {
        var result = new List<int>();

        for (int i = 0; i < VertexCount; i++)
        {
            if (GetVertexKind(i) == VertexKind.Reflex)
            {
                result.Add(i);
            }
        }

        return result;
    }

    public IReadOnlyList<int> GetConvexVertexIndices()
    {
        var result = new List<int>();

        for (int i = 0; i < VertexCount; i++)
        {
            if (GetVertexKind(i) == VertexKind.Convex)
            {
                result.Add(i);
            }
        }

        return result;
    }

    public EdgeKind GetEdgeKind(int edgeIndex)
    {
        var edge = GetEdge(edgeIndex);

        if (edge.IsHorizontal)
        {
            bool leftToRight = edge.End.X > edge.Start.X;

            if (IsCounterClockwise())
            {
                return leftToRight ? EdgeKind.Bottom : EdgeKind.Top;
            }

            return leftToRight ? EdgeKind.Top : EdgeKind.Bottom;
        }

        if (edge.IsVertical)
        {
            bool bottomToTop = edge.End.Y > edge.Start.Y;

            if (IsCounterClockwise())
            {
                return bottomToTop ? EdgeKind.Right : EdgeKind.Left;
            }

            return bottomToTop ? EdgeKind.Left : EdgeKind.Right;
        }

        return EdgeKind.Unknown;
    }

    public IReadOnlyList<HorizontalEdgeInfo> GetHorizontalEdges()
    {
        var result = new List<HorizontalEdgeInfo>();

        for (int i = 0; i < VertexCount; i++)
        {
            var edge = GetEdge(i);

            if (!edge.IsHorizontal)
            {
                continue;
            }

            result.Add(new HorizontalEdgeInfo(
                edgeIndex: i,
                startVertexIndex: i,
                endVertexIndex: NormalizeIndex(i + 1),
                edge: edge,
                kind: GetEdgeKind(i)));
        }

        return result;
    }

    public bool IsPointOnBoundary(Point2D point, double epsilon = Epsilon)
    {
        for (int i = 0; i < VertexCount; i++)
        {
            var edge = GetEdge(i);

            if (IsPointOnSegment(point, edge.Start, edge.End, epsilon))
            {
                return true;
            }
        }

        return false;
    }

    public bool ContainsPoint(Point2D point, bool includeBoundary = true, double epsilon = Epsilon)
    {
        if (includeBoundary && IsPointOnBoundary(point, epsilon))
        {
            return true;
        }

        bool inside = false;

        for (int i = 0, j = VertexCount - 1; i < VertexCount; j = i++)
        {
            var vi = Vertices[i];
            var vj = Vertices[j];

            bool intersects =
                ((vi.Y > point.Y) != (vj.Y > point.Y)) &&
                (point.X < (double)(vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y) + vi.X);

            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private int NormalizeIndex(int index)
    {
        int normalized = index % VertexCount;
        return normalized < 0 ? normalized + VertexCount : normalized;
    }

    private static void Validate(IReadOnlyList<Vertex> vertices)
    {
        ValidateEdges(vertices);
        ValidateAngles(vertices);
        ValidateArea(vertices);
        ValidateSimple(vertices);
    }

    private static void ValidateEdges(IReadOnlyList<Vertex> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];

            if (current == next)
            {
                throw new ArgumentException(
                    $"Сусідні вершини {i} і {(i + 1) % vertices.Count} збігаються.");
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

    private static void ValidateAngles(IReadOnlyList<Vertex> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            var prev = vertices[(i - 1 + vertices.Count) % vertices.Count];
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];

            long dx1 = current.X - prev.X;
            long dy1 = current.Y - prev.Y;

            long dx2 = next.X - current.X;
            long dy2 = next.Y - current.Y;

            long cross = dx1 * dy2 - dy1 * dx2;

            if (cross == 0)
            {
                throw new ArgumentException(
                    $"Вершина {i} вироджена: сусідні ребра лежать на одній прямій.");
            }
        }
    }

    private static void ValidateArea(IReadOnlyList<Vertex> vertices)
    {
        long area = GetSignedDoubleArea(vertices);

        if (area == 0)
        {
            throw new ArgumentException("Площа багатокутника дорівнює нулю.");
        }
    }

    private static void ValidateSimple(IReadOnlyList<Vertex> vertices)
    {
        int n = vertices.Count;

        for (int i = 0; i < n; i++)
        {
            var a1 = vertices[i];
            var a2 = vertices[(i + 1) % n];

            for (int j = i + 1; j < n; j++)
            {
                if (AreAdjacentEdges(i, j, n))
                {
                    continue;
                }

                var b1 = vertices[j];
                var b2 = vertices[(j + 1) % n];

                if (SegmentsIntersect(a1, a2, b1, b2))
                {
                    throw new ArgumentException(
                        $"Багатокутник не є простим: ребра {i} та {j} перетинаються.");
                }
            }
        }
    }

    private static bool AreAdjacentEdges(int firstEdgeIndex, int secondEdgeIndex, int edgeCount)
    {
        if (firstEdgeIndex == secondEdgeIndex)
        {
            return true;
        }

        if (Math.Abs(firstEdgeIndex - secondEdgeIndex) == 1)
        {
            return true;
        }

        return Math.Abs(firstEdgeIndex - secondEdgeIndex) == edgeCount - 1;
    }

    private static long GetSignedDoubleArea(IReadOnlyList<Vertex> vertices)
    {
        long sum = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            sum += (long)a.X * b.Y - (long)b.X * a.Y;
        }

        return sum;
    }

    private static bool SegmentsIntersect(Vertex a1, Vertex a2, Vertex b1, Vertex b2)
    {
        bool aVertical = a1.X == a2.X;
        bool bVertical = b1.X == b2.X;

        if (aVertical && bVertical)
        {
            if (a1.X != b1.X)
            {
                return false;
            }

            return RangesOverlap(a1.Y, a2.Y, b1.Y, b2.Y);
        }

        if (!aVertical && !bVertical)
        {
            if (a1.Y != b1.Y)
            {
                return false;
            }

            return RangesOverlap(a1.X, a2.X, b1.X, b2.X);
        }

        var verticalStart = aVertical ? a1 : b1;
        var verticalEnd = aVertical ? a2 : b2;
        var horizontalStart = aVertical ? b1 : a1;
        var horizontalEnd = aVertical ? b2 : a2;

        int x = verticalStart.X;
        int y = horizontalStart.Y;

        return IsBetween(x, horizontalStart.X, horizontalEnd.X)
            && IsBetween(y, verticalStart.Y, verticalEnd.Y);
    }

    private static bool RangesOverlap(int a1, int a2, int b1, int b2)
    {
        int minA = Math.Min(a1, a2);
        int maxA = Math.Max(a1, a2);
        int minB = Math.Min(b1, b2);
        int maxB = Math.Max(b1, b2);

        return Math.Max(minA, minB) <= Math.Min(maxA, maxB);
    }

    private static bool IsBetween(int value, int bound1, int bound2)
    {
        int min = Math.Min(bound1, bound2);
        int max = Math.Max(bound1, bound2);
        return value >= min && value <= max;
    }

    private static bool IsPointOnSegment(Point2D point, Vertex start, Vertex end, double epsilon)
    {
        if (start.X == end.X)
        {
            if (Math.Abs(point.X - start.X) > epsilon)
            {
                return false;
            }

            double minY = Math.Min(start.Y, end.Y) - epsilon;
            double maxY = Math.Max(start.Y, end.Y) + epsilon;
            return point.Y >= minY && point.Y <= maxY;
        }

        if (start.Y == end.Y)
        {
            if (Math.Abs(point.Y - start.Y) > epsilon)
            {
                return false;
            }

            double minX = Math.Min(start.X, end.X) - epsilon;
            double maxX = Math.Max(start.X, end.X) + epsilon;
            return point.X >= minX && point.X <= maxX;
        }

        return false;
    }
}