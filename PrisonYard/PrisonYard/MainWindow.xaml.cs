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
            InputTextBox.Text = File.ReadAllText(dialog.FileName);
            ParseAndDraw(InputTextBox.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ParseAndDraw(string text)
    {
        _currentPolygon = PolygonParser.ParseFromText(text);
        PolygonRenderer.Draw(DrawingCanvas, _currentPolygon);
    }

    private void RedrawIfPossible()
    {
        if (_currentPolygon is not null)
        {
            PolygonRenderer.Draw(DrawingCanvas, _currentPolygon);
        }
    }
}