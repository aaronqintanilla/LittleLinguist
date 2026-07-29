using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Controls;

namespace LittleLinguist.Pages;

public class WritingPage : ContentPage
{
    private readonly TracingCanvas _tracingCanvas;
    private readonly TextBlock _feedbackText;

    public WritingPage(string word = "APPLE")
    {
        //Background =
            //new SolidColorBrush(Color.Parse("#FFF7FC"));

        // ---------------------------------------------------------
        // BOTÓN BACK
        // ---------------------------------------------------------

        var backButton = new Button
        {
            Content = "← Back",
            Padding = new Thickness(18, 10),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        backButton.Click += BackButton_Click;

        // ---------------------------------------------------------
        // TÍTULO E INSTRUCCIONES
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "Writing practice",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var instructions = new TextBlock
        {
            Text = "Trace the word with your finger or digital pen.",
            FontSize = 18,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // ZONA TÁCTIL
        // ---------------------------------------------------------

        _tracingCanvas = new TracingCanvas
        {
            TargetWord = word.ToUpperInvariant(),
            Height = 280,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // ---------------------------------------------------------
        // MENSAJE
        // ---------------------------------------------------------

        _feedbackText = new TextBlock
        {
            Text = "",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // BOTÓN CLEAR
        // ---------------------------------------------------------

        var clearButton = new Button
        {
            Content = "Clear",
            FontSize = 18,
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        clearButton.Click += ClearButton_Click;

        // ---------------------------------------------------------
        // BOTÓN CHECK
        // ---------------------------------------------------------

        var checkButton = new Button
        {
            Content = "Check",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        checkButton.Click += CheckButton_Click;

        var buttonsGrid = new Grid
        {
            ColumnDefinitions =
                ColumnDefinitions.Parse("*,*"),

            ColumnSpacing = 20
        };

        Grid.SetColumn(clearButton, 0);
        Grid.SetColumn(checkButton, 1);

        buttonsGrid.Children.Add(clearButton);
        buttonsGrid.Children.Add(checkButton);

        // ---------------------------------------------------------
        // CONTENIDO COMPLETO
        // ---------------------------------------------------------

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(30),
            Spacing = 20,

            Children =
            {
                backButton,
                title,
                instructions,
                _tracingCanvas,
                _feedbackText,
                buttonsGrid
            }
        };

        Content = new ScrollViewer
        {
            Content = mainPanel
        };
    }

    private void ClearButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _tracingCanvas.Clear();
        _feedbackText.Text = "";
    }

    private void CheckButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        double score = _tracingCanvas.CalculateScore();

        if (score >= 0.80)
        {
            _feedbackText.Text =
                $"✅ Great job! Score: {score:P0}";
        }
        else
        {
            _feedbackText.Text =
                $"✏️ Try again. Score: {score:P0}";
        }
    }

    private async void BackButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }
}