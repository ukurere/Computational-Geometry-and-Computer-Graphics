using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PrisonYard.Models;

namespace PrisonYard.Services;

public static class PolygonRenderer
{
    public static void Draw(Canvas canvas, OrthogonalPolygon polygon)
    {
        canvas.Children.Clear();

        if (polygon.Vertices.Count == 0)
        {
            return;
        }

        double canvasWidth = canvas.ActualWidth;
        double canvasHeight = canvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            return;
        }

        const double padding = 20.0;

        int minX = polygon.Vertices.Min(v => v.X);
        int maxX = polygon.Vertices.Max(v => v.X);
        int minY = polygon.Vertices.Min(v => v.Y);
        int maxY = polygon.Vertices.Max(v => v.Y);

        double logicalWidth = Math.Max(1, maxX - minX);
        double logicalHeight = Math.Max(1, maxY - minY);

        double scaleX = (canvasWidth - 2 * padding) / logicalWidth;
        double scaleY = (canvasHeight - 2 * padding) / logicalHeight;
        double scale = Math.Min(scaleX, scaleY);

        var points = new PointCollection(
            polygon.Vertices.Select(v => new Point(
                padding + (v.X - minX) * scale,
                canvasHeight - padding - (v.Y - minY) * scale))
        );

        var polygonShape = new Polygon
        {
            Points = points,
            Stroke = Brushes.Black,
            Fill = Brushes.LightGray,
            StrokeThickness = 2
        };

        canvas.Children.Add(polygonShape);

        foreach (var point in points)
        {
            var marker = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.DarkRed
            };

            Canvas.SetLeft(marker, point.X - 3);
            Canvas.SetTop(marker, point.Y - 3);

            canvas.Children.Add(marker);
        }
    }
}