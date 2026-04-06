using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PrisonYard.Models;

namespace PrisonYard.Services;

public static class PolygonRenderer
{
    private static readonly Color[] RegionPalette =
    [
        Color.FromRgb(255, 99, 132),
        Color.FromRgb(54, 162, 235),
        Color.FromRgb(255, 206, 86),
        Color.FromRgb(75, 192, 192),
        Color.FromRgb(153, 102, 255),
        Color.FromRgb(255, 159, 64)
    ];

    public static void Draw(Canvas canvas, OrthogonalPolygon polygon)
    {
        canvas.Children.Clear();

        if (polygon.Vertices.Count == 0)
        {
            return;
        }

        var transform = BuildTransform(canvas, polygon);
        if (transform is null)
        {
            return;
        }

        DrawPolygonFill(canvas, polygon, transform);
        DrawPolygonEdges(canvas, polygon, transform);
        DrawVertices(
            canvas,
            polygon,
            transform,
            highlightedVertexIndices: Array.Empty<int>(),
            reflexVertexIndices: polygon.GetReflexVertexIndices(),
            cameraVertexIndices: Array.Empty<int>(),
            showVertexIndices: false);
    }

    public static void DrawStep(Canvas canvas, OrthogonalPolygon polygon, AlgorithmStep step)
    {
        canvas.Children.Clear();

        if (polygon.Vertices.Count == 0)
        {
            return;
        }

        var transform = BuildTransform(canvas, polygon);
        if (transform is null)
        {
            return;
        }

        if (step.ShowPolygonFill)
        {
            DrawPolygonFill(canvas, polygon, transform);
        }

        DrawVisibilityRegions(canvas, transform, step.VisibilityRegions);

        if (step.ShowEdges)
        {
            DrawPolygonEdges(canvas, polygon, transform);
        }

        var cameraVertexIndices = step.Cameras
            .Select(camera => camera.VertexIndex)
            .Distinct()
            .ToArray();

        DrawVertices(
            canvas,
            polygon,
            transform,
            highlightedVertexIndices: step.HighlightedVertexIndices,
            reflexVertexIndices: step.ReflexVertexIndices,
            cameraVertexIndices: cameraVertexIndices,
            showVertexIndices: step.ShowVertexIndices);

        DrawCameraLabels(canvas, transform, step.Cameras);
    }

    private static ViewTransform? BuildTransform(Canvas canvas, OrthogonalPolygon polygon)
    {
        double canvasWidth = canvas.ActualWidth;
        double canvasHeight = canvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            return null;
        }

        const double padding = 20.0;

        var (minX, maxX, minY, maxY) = polygon.GetBoundingBox();

        double logicalWidth = Math.Max(1, maxX - minX);
        double logicalHeight = Math.Max(1, maxY - minY);

        double availableWidth = Math.Max(1, canvasWidth - 2 * padding);
        double availableHeight = Math.Max(1, canvasHeight - 2 * padding);

        double scaleX = availableWidth / logicalWidth;
        double scaleY = availableHeight / logicalHeight;
        double scale = Math.Min(scaleX, scaleY);

        double scaledWidth = logicalWidth * scale;
        double scaledHeight = logicalHeight * scale;

        double offsetX = (canvasWidth - scaledWidth) / 2.0;
        double offsetY = (canvasHeight - scaledHeight) / 2.0;

        return new ViewTransform(minX, maxY, offsetX, offsetY, scale);
    }

    private static void DrawPolygonFill(Canvas canvas, OrthogonalPolygon polygon, ViewTransform transform)
    {
        var points = new PointCollection(polygon.Vertices.Select(transform.ToScreen));

        var polygonShape = new Polygon
        {
            Points = points,
            Fill = Brushes.LightGray,
            Stroke = Brushes.Transparent,
            StrokeThickness = 0
        };

        canvas.Children.Add(polygonShape);
    }

    private static void DrawPolygonEdges(Canvas canvas, OrthogonalPolygon polygon, ViewTransform transform)
    {
        var points = new PointCollection(polygon.Vertices.Select(transform.ToScreen));

        var polygonShape = new Polygon
        {
            Points = points,
            Fill = Brushes.Transparent,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        canvas.Children.Add(polygonShape);
    }

    private static void DrawVisibilityRegions(
        Canvas canvas,
        ViewTransform transform,
        IReadOnlyList<VisibilityRegion> regions)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            var region = regions[i];

            if (region.BoundaryPoints.Count < 3)
            {
                continue;
            }

            Color baseColor = RegionPalette[i % RegionPalette.Length];
            Brush fillBrush = new SolidColorBrush(Color.FromArgb(120, baseColor.R, baseColor.G, baseColor.B));
            Brush strokeBrush = new SolidColorBrush(baseColor);

            var regionShape = new Polygon
            {
                Points = new PointCollection(region.BoundaryPoints.Select(transform.ToScreen)),
                Fill = fillBrush,
                Stroke = strokeBrush,
                StrokeThickness = 1.5
            };

            canvas.Children.Add(regionShape);
        }
    }

    private static void DrawVertices(
        Canvas canvas,
        OrthogonalPolygon polygon,
        ViewTransform transform,
        IReadOnlyList<int> highlightedVertexIndices,
        IReadOnlyList<int> reflexVertexIndices,
        IReadOnlyList<int> cameraVertexIndices,
        bool showVertexIndices)
    {
        var highlighted = new HashSet<int>(highlightedVertexIndices);
        var reflex = new HashSet<int>(reflexVertexIndices);
        var cameras = new HashSet<int>(cameraVertexIndices);

        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            var point = transform.ToScreen(polygon.Vertices[i]);
            Brush fill = GetVertexBrush(i, highlighted, cameras, reflex, polygon);

            double size = cameras.Contains(i) ? 14 : 10;

            var marker = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = fill,
                Stroke = Brushes.Black,
                StrokeThickness = 1.2
            };

            Canvas.SetLeft(marker, point.X - size / 2.0);
            Canvas.SetTop(marker, point.Y - size / 2.0);
            canvas.Children.Add(marker);

            if (showVertexIndices)
            {
                var label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.Black,
                    Background = Brushes.White
                };

                Canvas.SetLeft(label, point.X + 8);
                Canvas.SetTop(label, point.Y - 12);
                canvas.Children.Add(label);
            }
        }
    }

    private static void DrawCameraLabels(
        Canvas canvas,
        ViewTransform transform,
        IReadOnlyList<CameraPlacement> cameras)
    {
        foreach (var camera in cameras)
        {
            var point = transform.ToScreen(camera.Position);

            var label = new TextBlock
            {
                Text = camera.Name,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkGreen,
                Background = Brushes.White
            };

            Canvas.SetLeft(label, point.X + 10);
            Canvas.SetTop(label, point.Y + 6);
            canvas.Children.Add(label);
        }
    }

    private static Brush GetVertexBrush(
        int index,
        HashSet<int> highlighted,
        HashSet<int> cameras,
        HashSet<int> reflex,
        OrthogonalPolygon polygon)
    {
        if (highlighted.Contains(index))
        {
            return Brushes.DarkGoldenrod;
        }

        if (cameras.Contains(index))
        {
            return Brushes.DarkGreen;
        }

        if (reflex.Contains(index))
        {
            return Brushes.Blue;
        }

        return polygon.GetVertexKind(index) == VertexKind.Reflex
            ? Brushes.Blue
            : Brushes.DarkRed;
    }

    private sealed class ViewTransform
    {
        private readonly int _minX;
        private readonly int _maxY;
        private readonly double _offsetX;
        private readonly double _offsetY;
        private readonly double _scale;

        public ViewTransform(int minX, int maxY, double offsetX, double offsetY, double scale)
        {
            _minX = minX;
            _maxY = maxY;
            _offsetX = offsetX;
            _offsetY = offsetY;
            _scale = scale;
        }

        public Point ToScreen(Vertex vertex)
        {
            return ToScreen(new Point2D(vertex.X, vertex.Y));
        }

        public Point ToScreen(Point2D point)
        {
            return new Point(
                _offsetX + (point.X - _minX) * _scale,
                _offsetY + (_maxY - point.Y) * _scale);
        }
    }
}