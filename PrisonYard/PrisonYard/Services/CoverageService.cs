using System;
using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models;

namespace PrisonYard.Services;

public static class CoverageService
{
    private sealed class Candidate
    {
        public Candidate(CameraPlacement camera, VisibilityRegion region, ulong[] coverageMask, int coverageCount)
        {
            Camera = camera;
            Region = region;
            CoverageMask = coverageMask;
            CoverageCount = coverageCount;
        }

        public CameraPlacement Camera { get; }
        public VisibilityRegion Region { get; }
        public ulong[] CoverageMask { get; }
        public int CoverageCount { get; }
    }

    public static IReadOnlyList<CameraPlacement> BuildCamerasFromVertexIndices(
        OrthogonalPolygon polygon,
        IEnumerable<int> vertexIndices)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        if (vertexIndices is null)
        {
            throw new ArgumentNullException(nameof(vertexIndices));
        }

        return vertexIndices
            .Distinct()
            .OrderBy(index => index)
            .Select(index => CameraPlacement.FromVertex(polygon, index, $"Камера {index}"))
            .ToList();
    }

    public static IReadOnlyList<VisibilityRegion> BuildVisibilityRegions(
        OrthogonalPolygon polygon,
        IEnumerable<CameraPlacement> cameras)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        if (cameras is null)
        {
            throw new ArgumentNullException(nameof(cameras));
        }

        return cameras
            .Select(camera => VisibilityService.BuildVisibilityRegion(polygon, camera))
            .ToList();
    }

    public static IReadOnlyList<VisibilityRegion> BuildVisibilityRegions(
        OrthogonalPolygon polygon,
        IEnumerable<int> cameraVertexIndices)
    {
        var cameras = BuildCamerasFromVertexIndices(polygon, cameraVertexIndices);
        return BuildVisibilityRegions(polygon, cameras);
    }

    public static (IReadOnlyList<CameraPlacement> Cameras, IReadOnlyList<VisibilityRegion> Regions)
        FindMinimumCameraCover(OrthogonalPolygon polygon)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        var witnesses = BuildWitnessPoints(polygon);

        if (witnesses.Count == 0)
        {
            return (Array.Empty<CameraPlacement>(), Array.Empty<VisibilityRegion>());
        }

        var candidates = BuildCandidates(polygon, witnesses)
            .Where(candidate => candidate.CoverageCount > 0)
            .OrderByDescending(candidate => candidate.CoverageCount)
            .ThenBy(candidate => candidate.Camera.VertexIndex)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("Не вдалося побудувати жодного кандидата на камеру.");
        }

        for (int cameraCount = 1; cameraCount <= candidates.Count; cameraCount++)
        {
            var initialMask = CreateEmptyMask(witnesses.Count);
            var selected = new List<Candidate>();

            if (TryFindCombination(
                    candidates,
                    witnessCount: witnesses.Count,
                    startIndex: 0,
                    remainingToPick: cameraCount,
                    currentMask: initialMask,
                    selected: selected,
                    out var solution))
            {
                var orderedSolution = solution
                    .OrderBy(candidate => candidate.Camera.VertexIndex)
                    .ToList();

                return (
                    orderedSolution.Select(candidate => candidate.Camera).ToList(),
                    orderedSolution.Select(candidate => candidate.Region).ToList()
                );
            }
        }

        throw new InvalidOperationException("Не вдалося знайти покриття для всіх контрольних областей.");
    }

    private static IReadOnlyList<Candidate> BuildCandidates(
        OrthogonalPolygon polygon,
        IReadOnlyList<Point2D> witnesses)
    {
        var result = new List<Candidate>();

        for (int vertexIndex = 0; vertexIndex < polygon.VertexCount; vertexIndex++)
        {
            var camera = CameraPlacement.FromVertex(polygon, vertexIndex, $"Камера {vertexIndex}");
            var region = VisibilityService.BuildVisibilityRegion(polygon, camera);
            var mask = BuildCoverageMask(region, witnesses);
            int coverageCount = CountBits(mask);

            result.Add(new Candidate(camera, region, mask, coverageCount));
        }

        return result;
    }

    private static IReadOnlyList<Point2D> BuildWitnessPoints(OrthogonalPolygon polygon)
    {
        var uniqueX = polygon.Vertices
            .Select(vertex => (double)vertex.X)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var uniqueY = polygon.Vertices
            .Select(vertex => (double)vertex.Y)
            .Distinct()
            .OrderBy(y => y)
            .ToList();

        var xSamples = BuildMidpointSamples(uniqueX);
        var ySamples = BuildMidpointSamples(uniqueY);

        var witnesses = new List<Point2D>();

        foreach (double x in xSamples)
        {
            foreach (double y in ySamples)
            {
                var point = new Point2D(x, y);

                if (polygon.ContainsPoint(point, includeBoundary: false))
                {
                    witnesses.Add(point);
                }
            }
        }

        return DeduplicatePoints(witnesses);
    }

    private static IReadOnlyList<double> BuildMidpointSamples(IReadOnlyList<double> values)
    {
        var result = new List<double>();

        for (int i = 0; i < values.Count - 1; i++)
        {
            result.Add((values[i] + values[i + 1]) / 2.0);
        }

        return result;
    }

    private static IReadOnlyList<Point2D> DeduplicatePoints(IReadOnlyList<Point2D> points)
    {
        var result = new List<Point2D>();
        var seen = new HashSet<string>();

        foreach (var point in points)
        {
            string key = $"{Math.Round(point.X, 8)}|{Math.Round(point.Y, 8)}";

            if (seen.Add(key))
            {
                result.Add(point);
            }
        }

        return result;
    }

    private static ulong[] BuildCoverageMask(VisibilityRegion region, IReadOnlyList<Point2D> witnesses)
    {
        var mask = CreateEmptyMask(witnesses.Count);

        for (int i = 0; i < witnesses.Count; i++)
        {
            if (region.ContainsPoint(witnesses[i], includeBoundary: true))
            {
                SetBit(mask, i);
            }
        }

        return mask;
    }

    private static bool TryFindCombination(
        IReadOnlyList<Candidate> candidates,
        int witnessCount,
        int startIndex,
        int remainingToPick,
        ulong[] currentMask,
        List<Candidate> selected,
        out List<Candidate> solution)
    {
        if (CoversAll(currentMask, witnessCount))
        {
            solution = new List<Candidate>(selected);
            return true;
        }

        if (remainingToPick == 0 || startIndex >= candidates.Count)
        {
            solution = null!;
            return false;
        }

        if (candidates.Count - startIndex < remainingToPick)
        {
            solution = null!;
            return false;
        }

        var maxPossibleMask = CloneMask(currentMask);

        for (int i = startIndex; i < candidates.Count; i++)
        {
            OrInPlace(maxPossibleMask, candidates[i].CoverageMask);
        }

        if (!CoversAll(maxPossibleMask, witnessCount))
        {
            solution = null!;
            return false;
        }

        for (int i = startIndex; i <= candidates.Count - remainingToPick; i++)
        {
            var nextMask = OrMasks(currentMask, candidates[i].CoverageMask);

            if (MasksEqual(nextMask, currentMask))
            {
                continue;
            }

            selected.Add(candidates[i]);

            if (TryFindCombination(
                    candidates,
                    witnessCount,
                    i + 1,
                    remainingToPick - 1,
                    nextMask,
                    selected,
                    out solution))
            {
                return true;
            }

            selected.RemoveAt(selected.Count - 1);
        }

        solution = null!;
        return false;
    }

    private static ulong[] CreateEmptyMask(int bitCount)
    {
        return new ulong[(bitCount + 63) / 64];
    }

    private static void SetBit(ulong[] mask, int bitIndex)
    {
        int wordIndex = bitIndex / 64;
        int bitOffset = bitIndex % 64;
        mask[wordIndex] |= 1UL << bitOffset;
    }

    private static int CountBits(ulong[] mask)
    {
        int count = 0;

        foreach (ulong word in mask)
        {
            count += System.Numerics.BitOperations.PopCount(word);
        }

        return count;
    }

    private static ulong[] CloneMask(ulong[] source)
    {
        var clone = new ulong[source.Length];
        Array.Copy(source, clone, source.Length);
        return clone;
    }

    private static ulong[] OrMasks(ulong[] left, ulong[] right)
    {
        var result = new ulong[left.Length];

        for (int i = 0; i < left.Length; i++)
        {
            result[i] = left[i] | right[i];
        }

        return result;
    }

    private static void OrInPlace(ulong[] target, ulong[] source)
    {
        for (int i = 0; i < target.Length; i++)
        {
            target[i] |= source[i];
        }
    }

    private static bool MasksEqual(ulong[] left, ulong[] right)
    {
        for (int i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool CoversAll(ulong[] mask, int witnessCount)
    {
        int fullWords = witnessCount / 64;
        int remainingBits = witnessCount % 64;

        for (int i = 0; i < fullWords; i++)
        {
            if (mask[i] != ulong.MaxValue)
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        ulong expected = (1UL << remainingBits) - 1;
        return mask[fullWords] == expected;
    }
}