using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrisonYard.Models;

namespace PrisonYard.Services;

public static class PolygonParser
{
    public static OrthogonalPolygon ParseFromFile(string path)
    {
        var text = File.ReadAllText(path);
        return ParseFromText(text);
    }

    public static OrthogonalPolygon ParseFromText(string text)
    {
        var lines = text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count == 0)
        {
            throw new ArgumentException("Вхідні дані порожні.");
        }

        // Підтримуються 2 формати:
        // 1) перший рядок = кількість вершин n, далі n рядків "x y"
        // 2) просто список рядків "x y"

        List<string> vertexLines;

        if (int.TryParse(lines[0], out int n))
        {
            if (n <= 0)
            {
                throw new ArgumentException("Кількість вершин повинна бути додатною.");
            }

            if (lines.Count != n + 1)
            {
                throw new ArgumentException(
                    $"Очікувалось {n} рядків з вершинами після першого рядка, але отримано {lines.Count - 1}.");
            }

            vertexLines = lines.Skip(1).ToList();
        }
        else
        {
            vertexLines = lines;
        }

        var vertices = new List<Vertex>();

        for (int i = 0; i < vertexLines.Count; i++)
        {
            vertices.Add(ParseVertex(vertexLines[i], i + 1));
        }

        return new OrthogonalPolygon(vertices);
    }

    private static Vertex ParseVertex(string line, int lineNumber)
    {
        var parts = line.Split(new[] { ' ', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            throw new ArgumentException($"Рядок {lineNumber}: очікувався формат 'x y'.");
        }

        if (!int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y))
        {
            throw new ArgumentException($"Рядок {lineNumber}: координати повинні бути цілими числами.");
        }

        return new Vertex(x, y);
    }
}