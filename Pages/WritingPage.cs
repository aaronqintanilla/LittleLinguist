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
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
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
        // BOTÓN CONTINUE
        // ---------------------------------------------------------

        var continueButton = new Button
        {
            Content = "Continue",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        continueButton.Click += ContinueButton_Click;

        // ---------------------------------------------------------
        // GRID DE BOTONES
        // ---------------------------------------------------------

        var buttonsGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,*"),
            ColumnSpacing = 20
        };

        Grid.SetColumn(clearButton, 0);
        Grid.SetColumn(continueButton, 1);

        buttonsGrid.Children.Add(clearButton);
        buttonsGrid.Children.Add(continueButton);

        // ---------------------------------------------------------
        // CONTENIDO COMPLETO
        // ---------------------------------------------------------

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(30),
            Spacing = 20,

            Children =
            {
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

    // Borra los trazos y el mensaje.
    private void ClearButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _tracingCanvas.Clear();
        _feedbackText.Text = "";
    }

    // Comprueba la palabra antes de continuar.
    private async void ContinueButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        double score = _tracingCanvas.CalculateScore();

        if (score >= 0.80)
        {
            // La palabra está bien: vuelve a StoryPage.
            if (Navigation is not null)
            {
                await Navigation.PopAsync();
            }
        }
        else
        {
            // La palabra está mal: borra y obliga a repetirla.
            _tracingCanvas.Clear();

            _feedbackText.Text =
                $"✏️ Try again. Write the whole word carefully. Score: {score:P0}";
        }
    }
}