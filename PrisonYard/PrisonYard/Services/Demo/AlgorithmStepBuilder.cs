using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models.Algorithm;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Coloring;
using PrisonYard.Services.Geometry;
using PrisonYard.Services.Partitioning;

namespace PrisonYard.Services.Demo;

public static class AlgorithmStepBuilder
{
    public static AlgorithmRun BuildDemoSteps(OrthogonalPolygon polygon)
    {
        var steps = new List<AlgorithmStep>();

        var reflexVertices = polygon.GetReflexVertexIndices().ToList();
        var horizontalEdges = OrthogonalEdgeAnalysisService.GetHorizontalEdges(polygon).ToList();

        var topEdges = horizontalEdges
            .Where(edge => edge.IsTop)
            .Select(edge => edge.EdgeIndex)
            .ToList();

        var bottomEdges = horizontalEdges
            .Where(edge => edge.IsBottom)
            .Select(edge => edge.EdgeIndex)
            .ToList();

        var peaks = OrthogonalEdgeAnalysisService.GetPeaks(polygon).ToList();

        steps.Add(new AlgorithmStep
        {
            ActionType = StepActionType.InputPolygon,
            Title = "Крок 1. Вхідний ортогональний багатокутник",
            Description = "Зчитано вершини багатокутника та побудовано його контур.",
            ShowVertexIndices = true
        });

        steps.Add(new AlgorithmStep
        {
            ActionType = StepActionType.DetectOrientation,
            Title = "Крок 2. Орієнтація обходу",
            Description = $"Орієнтація обходу контуру: {PolygonAnalysisService.GetOrientationLabel(polygon)}.",
            ShowVertexIndices = true
        });

        for (int i = 0; i < polygon.VertexCount; i++)
        {
            var kind = polygon.GetVertexKind(i);

            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.DetectReflexVertices,
                Title = $"Крок 3.{i + 1}. Аналіз вершини {i}",
                Description = kind == VertexKind.Reflex
                    ? $"Вершина {i} є рефлексною."
                    : $"Вершина {i} є опуклою.",
                HighlightedVertexIndices = new[] { i },
                ReflexVertexIndices = reflexVertices,
                ShowVertexIndices = true
            });
        }

        steps.Add(new AlgorithmStep
        {
            ActionType = StepActionType.DetectHorizontalEdges,
            Title = "Крок 4. Верхні горизонтальні ребра",
            Description = "Показано всі горизонтальні ребра типу Top.",
            HighlightedEdgeIndices = topEdges,
            ReflexVertexIndices = reflexVertices,
            ShowVertexIndices = true
        });

        steps.Add(new AlgorithmStep
        {
            ActionType = StepActionType.DetectHorizontalEdges,
            Title = "Крок 5. Нижні горизонтальні ребра",
            Description = "Показано всі горизонтальні ребра типу Bottom.",
            HighlightedEdgeIndices = bottomEdges,
            ReflexVertexIndices = reflexVertices,
            ShowVertexIndices = true
        });

        if (peaks.Count == 0)
        {
            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.DetectPeaks,
                Title = "Крок 6. Піки не знайдено",
                Description = "У цьому багатокутнику не виявлено top/bottom peaks.",
                ReflexVertexIndices = reflexVertices,
                ShowVertexIndices = true
            });
        }
        else
        {
            for (int i = 0; i < peaks.Count; i++)
            {
                var peak = peaks[i];

                steps.Add(new AlgorithmStep
                {
                    ActionType = StepActionType.DetectPeaks,
                    Title = $"Крок 6.{i + 1}. Знайдено peak",
                    Description =
                        $"Peak типу {peak.Kind} на ребрі {peak.EdgeIndex} між вершинами {peak.LeftVertexIndex} і {peak.RightVertexIndex}.",
                    HighlightedVertexIndices = new[] { peak.LeftVertexIndex, peak.RightVertexIndex },
                    HighlightedEdgeIndices = new[] { peak.EdgeIndex },
                    ReflexVertexIndices = reflexVertices,
                    ShowVertexIndices = true
                });
            }
        }

        var quadrangulationResult = QuadrangulationService.Quadrangulate(polygon);
        var partitionDiagonals = quadrangulationResult.PartitionDiagonals.ToList();
        var monotonePieces = quadrangulationResult.MonotonePieces.ToList();
        var quadrilaterals = quadrangulationResult.Quadrilaterals.ToList();

        var progressiveDiagonals = new List<(int FromVertexIndex, int ToVertexIndex)>();

        for (int i = 0; i < partitionDiagonals.Count; i++)
        {
            var diagonal = partitionDiagonals[i];
            progressiveDiagonals.Add((diagonal.FromVertexIndex, diagonal.ToVertexIndex));

            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.AddPartitionDiagonal,
                Title = $"Крок 7.{i + 1}. Додано діагональ розбиття",
                Description =
                    $"Додано діагональ між вершинами {diagonal.FromVertexIndex} і {diagonal.ToVertexIndex}.",
                Diagonals = progressiveDiagonals.ToList(),
                ReflexVertexIndices = reflexVertices,
                ShowVertexIndices = true
            });
        }

        if (monotonePieces.Count > 0)
        {
            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.BuildMonotonePiece,
                Title = "Крок 8. Отримано псевдо-монотонні частини",
                Description = $"Побудовано частин: {monotonePieces.Count}.",
                Diagonals = progressiveDiagonals.ToList(),
                ReflexVertexIndices = reflexVertices,
                ShowVertexIndices = true
            });
        }

        if (quadrilaterals.Count > 0)
        {
            var quadrangulationDiagonals = QuadrangulationService
                .CollectQuadrangulationDiagonals(quadrilaterals)
                .Select(diagonal => (diagonal.FromVertexIndex, diagonal.ToVertexIndex))
                .ToList();

            var quadrilateralGroups = quadrilaterals
                .Select(q => (IReadOnlyList<int>)q.VertexIndices.ToList())
                .ToList();

            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.BuildQuadrilateral,
                Title = "Крок 9. Quadrilateralization",
                Description = $"Побудовано опуклих чотирикутників: {quadrilaterals.Count}.",
                Diagonals = quadrangulationDiagonals,
                QuadrilateralVertexGroups = quadrilateralGroups,
                ReflexVertexIndices = reflexVertices,
                ShowVertexIndices = true
            });

            var coloring = FourColoringService.ColorVertices(polygon, quadrilaterals);

            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.ColorVertices,
                Title = "Крок 10. 4-розфарбування вершин",
                Description = "Вершини графа quadrilateralization пофарбовано у 4 кольори.",
                Diagonals = quadrangulationDiagonals,
                QuadrilateralVertexGroups = quadrilateralGroups,
                ReflexVertexIndices = reflexVertices,
                Coloring = coloring,
                ShowVertexIndices = true,
                ShowColors = true
            });

            var guardVertices = coloring.GetGuardVertices().ToList();
            string guardVerticesText = string.Join(", ", guardVertices);

            steps.Add(new AlgorithmStep
            {
                ActionType = StepActionType.SelectGuardColor,
                Title = "Фінальний крок. Вибір найменшого кольорового класу",
                Description =
                    $"Найменший кольоровий клас: {coloring.SelectedGuardColor}. Вершини для камер: {guardVerticesText}.",
                Diagonals = quadrangulationDiagonals,
                QuadrilateralVertexGroups = quadrilateralGroups,
                ReflexVertexIndices = reflexVertices,
                Coloring = coloring,
                HighlightedVertexIndices = guardVertices,
                ShowVertexIndices = true,
                ShowColors = true
            });
        }

        return new AlgorithmRun
        {
            Polygon = polygon,
            Steps = steps
        };
    }
}