using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using PrisonYard.Models;
using PrisonYard.Services;

namespace PrisonYard;

public partial class MainWindow : Window
{
    private OrthogonalPolygon? _currentPolygon;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => RedrawIfPossible();
        DrawingCanvas.SizeChanged += (_, _) => RedrawIfPossible();

        ResetStatus();
    }

    private void LoadFromFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            string text = File.ReadAllText(dialog.FileName);
            InputTextBox.Text = text;
            ParseAndDraw(text);
        }
        catch (Exception ex)
        {
            _currentPolygon = null;
            DrawingCanvas.Children.Clear();
            ResetStatus();
            StatusTextBlock.Text = $"Помилка: {ex.Message}";

            MessageBox.Show(
                ex.Message,
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void DrawFromText_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ParseAndDraw(InputTextBox.Text);
        }
        catch (Exception ex)
        {
            _currentPolygon = null;
            DrawingCanvas.Children.Clear();
            ResetStatus();
            StatusTextBlock.Text = $"Помилка: {ex.Message}";

            MessageBox.Show(
                ex.Message,
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ParseAndDraw(string text)
    {
        _currentPolygon = PolygonParser.ParseFromText(text);

        PolygonRenderer.Draw(DrawingCanvas, _currentPolygon);
        UpdateStatus(_currentPolygon);
    }

    private void RedrawIfPossible()
    {
        if (_currentPolygon is null)
        {
            return;
        }

        PolygonRenderer.Draw(DrawingCanvas, _currentPolygon);
        UpdateStatus(_currentPolygon);
    }

    private void UpdateStatus(OrthogonalPolygon polygon)
    {
        int n = polygon.Vertices.Count;
        int reflexCount = polygon.GetReflexVertexIndices().Count;
        int guardBound = n / 4;
        string orientation = polygon.IsCounterClockwise() ? "CCW" : "CW";

        VertexCountTextBlock.Text = n.ToString();
        ReflexCountTextBlock.Text = reflexCount.ToString();
        GuardBoundTextBlock.Text = guardBound.ToString();
        OrientationTextBlock.Text = orientation;
        StatusTextBlock.Text = "Багатокутник успішно побудовано";
    }

    private void ResetStatus()
    {
        VertexCountTextBlock.Text = "-";
        ReflexCountTextBlock.Text = "-";
        GuardBoundTextBlock.Text = "-";
        OrientationTextBlock.Text = "-";
        StatusTextBlock.Text = "Очікування введення";
    }
}