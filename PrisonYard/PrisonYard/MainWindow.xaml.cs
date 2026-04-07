using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using PrisonYard.Models.Algorithm;
using PrisonYard.Models.Geometry;
using PrisonYard.Services.Demo;
using PrisonYard.Services.Parsing;
using PrisonYard.Services.Rendering;

namespace PrisonYard;

public partial class MainWindow : Window
{
    private OrthogonalPolygon? _currentPolygon;
    private AlgorithmRun? _algorithmRun;
    private int _currentStepIndex = -1;

    private readonly DispatcherTimer _inputDebounceTimer;

    private bool _isUpdatingTextProgrammatically;
    private bool _currentInputIsValid;
    private string? _currentErrorMessage;

    public MainWindow()
    {
        InitializeComponent();

        _inputDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _inputDebounceTimer.Tick += InputDebounceTimer_Tick;

        Loaded += MainWindow_Loaded;
        DrawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;
        InputTextBox.TextChanged += InputTextBox_TextChanged;

        ResetStatus();
        ClearStepInfo();
        UpdateStepNavigationButtons();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TryParseAndRefresh(showModalError: false);
    }

    private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RedrawCurrentView();
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingTextProgrammatically)
        {
            return;
        }

        _inputDebounceTimer.Stop();
        _inputDebounceTimer.Start();
    }

    private void InputDebounceTimer_Tick(object? sender, EventArgs e)
    {
        _inputDebounceTimer.Stop();
        TryParseAndRefresh(showModalError: false);
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

            _isUpdatingTextProgrammatically = true;
            InputTextBox.Text = text;
            _isUpdatingTextProgrammatically = false;

            TryParseAndRefresh(showModalError: true);
        }
        catch (Exception ex)
        {
            _isUpdatingTextProgrammatically = false;
            SetInvalidState(ex.Message);

            MessageBox.Show(
                ex.Message,
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void BuildSteps_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentInputIsValid || _currentPolygon is null)
        {
            MessageBox.Show(
                "Неможливо побудувати кроки алгоритму, поки поточне введення не є коректним.",
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            _algorithmRun = AlgorithmStepBuilder.BuildDemoSteps(_currentPolygon);
            _currentStepIndex = 0;
            ShowCurrentStep();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не вдалося побудувати кроки алгоритму: {ex.Message}",
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void DrawPolygonOnly_Click(object sender, RoutedEventArgs e)
    {
        _algorithmRun = null;
        _currentStepIndex = -1;

        ClearStepInfo();
        UpdateStepNavigationButtons();
        RenderPolygonOnly();
    }

    private void PreviousStep_Click(object sender, RoutedEventArgs e)
    {
        if (_algorithmRun is null || _currentStepIndex <= 0)
        {
            return;
        }

        _currentStepIndex--;
        ShowCurrentStep();
    }

    private void NextStep_Click(object sender, RoutedEventArgs e)
    {
        if (_algorithmRun is null || _currentStepIndex >= _algorithmRun.Steps.Count - 1)
        {
            return;
        }

        _currentStepIndex++;
        ShowCurrentStep();
    }

    private void TryParseAndRefresh(bool showModalError)
    {
        string text = InputTextBox.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            _currentInputIsValid = false;
            _currentErrorMessage = null;
            _currentPolygon = null;

            _algorithmRun = null;
            _currentStepIndex = -1;

            DrawingCanvas.Children.Clear();
            ResetStatus();
            ClearStepInfo();
            UpdateStepNavigationButtons();
            return;
        }

        try
        {
            var polygon = PolygonParser.ParseFromText(text);

            _currentPolygon = polygon;
            _currentInputIsValid = true;
            _currentErrorMessage = null;

            InvalidateStepMode();
            UpdateStatus(polygon);
            SetValidInputVisualState();
            RenderPolygonOnly();
        }
        catch (Exception ex)
        {
            _currentInputIsValid = false;
            _currentErrorMessage = ex.Message;

            InvalidateStepMode();
            SetInvalidState(ex.Message);

            if (showModalError)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void InvalidateStepMode()
    {
        _algorithmRun = null;
        _currentStepIndex = -1;

        ClearStepInfo();
        UpdateStepNavigationButtons();
    }

    private void ShowCurrentStep()
    {
        if (_algorithmRun is null ||
            _algorithmRun.Steps.Count == 0 ||
            _currentStepIndex < 0 ||
            _currentStepIndex >= _algorithmRun.Steps.Count)
        {
            return;
        }

        var step = _algorithmRun.Steps[_currentStepIndex];

        StepTitleTextBlock.Text = step.Title;
        StepDescriptionTextBlock.Text = step.Description;
        StepCounterTextBlock.Text = $"Крок: {_currentStepIndex + 1} / {_algorithmRun.Steps.Count}";

        PolygonRenderer.DrawStep(DrawingCanvas, _algorithmRun.Polygon, step);

        UpdateStepNavigationButtons();
    }

    private void RedrawCurrentView()
    {
        if (_algorithmRun is not null &&
            _currentStepIndex >= 0 &&
            _currentStepIndex < _algorithmRun.Steps.Count)
        {
            ShowCurrentStep();
            return;
        }

        RenderPolygonOnly();

        if (_currentInputIsValid && _currentPolygon is not null)
        {
            UpdateStatus(_currentPolygon);
        }
        else if (!string.IsNullOrWhiteSpace(_currentErrorMessage))
        {
            SetInvalidState(_currentErrorMessage);
        }
        else
        {
            ResetStatus();
        }
    }

    private void RenderPolygonOnly()
    {
        if (_currentPolygon is null)
        {
            DrawingCanvas.Children.Clear();
            return;
        }

        PolygonRenderer.Draw(DrawingCanvas, _currentPolygon);
    }

    private void UpdateStatus(OrthogonalPolygon polygon)
    {
        int n = polygon.VertexCount;
        int reflexCount = polygon.GetReflexVertexIndices().Count;
        int guardBound = n / 4;
        string orientation = polygon.IsCounterClockwise() ? "CCW" : "CW";

        VertexCountTextBlock.Text = n.ToString();
        ReflexCountTextBlock.Text = reflexCount.ToString();
        GuardBoundTextBlock.Text = guardBound.ToString();
        OrientationTextBlock.Text = orientation;

        StatusTextBlock.Text = "Багатокутник успішно побудовано";
        StatusTextBlock.Foreground = Brushes.DarkGreen;
    }

    private void ResetStatus()
    {
        VertexCountTextBlock.Text = "-";
        ReflexCountTextBlock.Text = "-";
        GuardBoundTextBlock.Text = "-";
        OrientationTextBlock.Text = "-";

        StatusTextBlock.Text = "Очікування введення";
        StatusTextBlock.Foreground = Brushes.Black;

        InputTextBox.ClearValue(BorderBrushProperty);
        InputTextBox.ClearValue(BorderThicknessProperty);
        InputTextBox.ClearValue(BackgroundProperty);
    }

    private void SetInvalidState(string errorMessage)
    {
        VertexCountTextBlock.Text = "-";
        ReflexCountTextBlock.Text = "-";
        GuardBoundTextBlock.Text = "-";
        OrientationTextBlock.Text = "-";

        StatusTextBlock.Text = $"Помилка: {errorMessage}";
        StatusTextBlock.Foreground = Brushes.DarkRed;

        SetInvalidInputVisualState();
    }

    private void ClearStepInfo()
    {
        StepTitleTextBlock.Text = "Крок алгоритму";
        StepDescriptionTextBlock.Text =
            "Натисніть «Показати кроки алгоритму», щоб перейти до покрокового перегляду.";
        StepCounterTextBlock.Text = "Крок: - / -";
    }

    private void UpdateStepNavigationButtons()
    {
        bool hasSteps = _algorithmRun is not null && _algorithmRun.Steps.Count > 0;

        PreviousStepButton.IsEnabled = hasSteps && _currentStepIndex > 0;
        NextStepButton.IsEnabled = hasSteps &&
                                   _currentStepIndex >= 0 &&
                                   _currentStepIndex < _algorithmRun.Steps.Count - 1;
    }

    private void SetValidInputVisualState()
    {
        InputTextBox.BorderBrush = Brushes.SeaGreen;
        InputTextBox.BorderThickness = new Thickness(2);
        InputTextBox.Background = Brushes.White;
    }

    private void SetInvalidInputVisualState()
    {
        InputTextBox.BorderBrush = Brushes.IndianRed;
        InputTextBox.BorderThickness = new Thickness(2);
        InputTextBox.Background = new SolidColorBrush(Color.FromRgb(255, 245, 245));
    }
}