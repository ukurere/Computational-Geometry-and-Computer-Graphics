using System.Collections.Generic;
using System.Linq;
using PrisonYard.Models;

namespace PrisonYard.Services;

public static class AlgorithmStepBuilder
{
    public static AlgorithmRun BuildDemoSteps(OrthogonalPolygon polygon)
    {
        var steps = new List<AlgorithmStep>();
        var reflex = polygon.GetReflexVertexIndices();

        steps.Add(new AlgorithmStep
        {
            Title = "Крок 1. Початковий багатокутник",
            Description = "Зчитано вершини ортогонального багатокутника та побудовано його контур."
        });

        steps.Add(new AlgorithmStep
        {
            Title = "Крок 2. Орієнтація обходу",
            Description = polygon.IsCounterClockwise()
                ? "Обхід контуру здійснюється проти годинникової стрілки (CCW)."
                : "Обхід контуру здійснюється за годинниковою стрілкою (CW)."
        });

        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            var kind = polygon.GetVertexKind(i);

            steps.Add(new AlgorithmStep
            {
                Title = $"Крок 3.{i + 1}. Аналіз вершини {i}",
                Description = kind == VertexKind.Reflex
                    ? $"Вершина {i} є рефлексною, тобто внутрішній кут дорівнює 270°."
                    : $"Вершина {i} є опуклою, тобто внутрішній кут дорівнює 90°.",
                HighlightedVertexIndices = new[] { i },
                ReflexVertexIndices = reflex,
                ShowVertexIndices = true
            });
        }

        steps.Add(new AlgorithmStep
        {
            Title = "Крок 4. Рефлексні вершини",
            Description =
                $"Знайдено {reflex.Count} рефлексних вершин. Саме вони створюють западини контуру і ускладнюють видимість.",
            ReflexVertexIndices = reflex,
            ShowVertexIndices = true
        });

        steps.Add(new AlgorithmStep
        {
            Title = "Крок 5. Пошук мінімального покриття",
            Description =
                "Далі перевіряються допустимі набори камер у вершинах та обирається найменший набір, який покриває всі контрольні області багатокутника.",
            ReflexVertexIndices = reflex,
            ShowVertexIndices = true
        });

        var solution = CoverageService.FindMinimumCameraCover(polygon);
        var cameras = solution.Cameras.ToList();
        var regions = solution.Regions.ToList();

        for (int i = 0; i < cameras.Count; i++)
        {
            var camera = cameras[i];
            var region = regions.Single(r => r.CameraVertexIndex == camera.VertexIndex);

            steps.Add(new AlgorithmStep
            {
                Title = $"Крок {6 + i}. Область видимості камери у вершині {camera.VertexIndex}",
                Description =
                    $"Побудовано область, яку бачить камера, встановлена у вершині {camera.VertexIndex}. Виділена кольором саме її зона покриття.",
                HighlightedVertexIndices = new[] { camera.VertexIndex },
                ReflexVertexIndices = reflex,
                Cameras = new[] { camera },
                VisibilityRegions = new[] { region },
                ShowVertexIndices = true
            });
        }

        string vertexList = string.Join(", ", cameras.Select(camera => camera.VertexIndex));

        steps.Add(new AlgorithmStep
        {
            Title = "Фінальний крок. Мінімальне покриття знайдено",
            Description =
                $"Мінімальна кількість камер для цього багатокутника: {cameras.Count}. Камери потрібно розмістити у вершинах: {vertexList}. Нижче показано сумарне покриття всіх вибраних камер.",
            ReflexVertexIndices = reflex,
            Cameras = cameras,
            VisibilityRegions = regions,
            ShowVertexIndices = true
        });

        return new AlgorithmRun
        {
            Polygon = polygon,
            Steps = steps
        };
    }
}