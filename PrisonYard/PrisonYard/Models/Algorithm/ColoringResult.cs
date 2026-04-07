using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonYard.Models.Algorithm;

public sealed class ColoringResult
{
    public IReadOnlyDictionary<int, int> VertexColors { get; }
    public int ColorCount { get; }
    public int SelectedGuardColor { get; }

    public ColoringResult(
        IReadOnlyDictionary<int, int> vertexColors,
        int colorCount,
        int selectedGuardColor)
    {
        if (vertexColors is null)
        {
            throw new ArgumentNullException(nameof(vertexColors));
        }

        if (colorCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(colorCount),
                "Кількість кольорів повинна бути додатною.");
        }

        if (selectedGuardColor < 0 || selectedGuardColor >= colorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedGuardColor),
                "Некоректний індекс вибраного кольору.");
        }

        VertexColors = vertexColors;
        ColorCount = colorCount;
        SelectedGuardColor = selectedGuardColor;
    }

    public IReadOnlyList<int> GetVerticesOfColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= ColorCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(colorIndex),
                "Некоректний індекс кольору.");
        }

        return VertexColors
            .Where(pair => pair.Value == colorIndex)
            .Select(pair => pair.Key)
            .OrderBy(index => index)
            .ToList();
    }

    public IReadOnlyList<int> GetGuardVertices()
    {
        return GetVerticesOfColor(SelectedGuardColor);
    }

    public int GetColorUsageCount(int colorIndex)
    {
        return GetVerticesOfColor(colorIndex).Count;
    }
}