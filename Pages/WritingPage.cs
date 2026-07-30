using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Controls;
using System.Threading.Tasks;
using System;

namespace LittleLinguist.Pages;

/*
FALTA:
1. al hacer finish (añadir boton para cerrar sesion de aprendizaje), que vuelva a HomePage y se reinicie todo
2. cambiar el diseño ajustandolo al resto
*/

public class WritingPage : ContentPage
{
    private readonly TracingCanvas _tracingCanvas;
    private readonly TextBlock _feedbackText;
    private readonly Action? _onCompleted;

    public WritingPage(string word = "APPLE", Action? onCompleted = null)
    {
        _onCompleted = onCompleted;
        NavigationPage.SetHasBackButton(this, false);

        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "Writing practice",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // INSTRUCCIONES
        // ---------------------------------------------------------

        var instructions = new TextBlock
        {
            Text = "Trace the word with your finger or digital pen.",
            FontSize = 18,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // ZONA TÁCTIL
        // ---------------------------------------------------------

        _tracingCanvas = new TracingCanvas
        {
            TargetWord = word.ToUpperInvariant(),

            // Ya no tiene una altura fija.
            // Ocupará el espacio restante de la ventana.
            MinHeight = 160,

            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
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
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch
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
            ColumnSpacing = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        Grid.SetColumn(clearButton, 0);
        Grid.SetColumn(continueButton, 1);

        buttonsGrid.Children.Add(clearButton);
        buttonsGrid.Children.Add(continueButton);

        // ---------------------------------------------------------
        // GRID PRINCIPAL
        // ---------------------------------------------------------

        var mainGrid = new Grid
        {
            Margin = new Thickness(30),

            // Título: altura automática.
            // Instrucciones: altura automática.
            // Canvas: ocupa todo el espacio restante.
            // Mensaje: altura automática.
            // Botones: altura automática.
            RowDefinitions =
                RowDefinitions.Parse("Auto,Auto,*,Auto,Auto"),

            RowSpacing = 20
        };

        Grid.SetRow(title, 0);
        Grid.SetRow(instructions, 1);
        Grid.SetRow(_tracingCanvas, 2);
        Grid.SetRow(_feedbackText, 3);
        Grid.SetRow(buttonsGrid, 4);

        mainGrid.Children.Add(title);
        mainGrid.Children.Add(instructions);
        mainGrid.Children.Add(_tracingCanvas);
        mainGrid.Children.Add(_feedbackText);
        mainGrid.Children.Add(buttonsGrid);

        // No utilizamos ScrollViewer.
        Content = mainGrid;
    }

    // ---------------------------------------------------------
    // CLEAR
    // ---------------------------------------------------------

    private void ClearButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _tracingCanvas.Clear();
        _feedbackText.Text = "";
    }

    // ---------------------------------------------------------
    // CONTINUE
    // ---------------------------------------------------------

    private async void ContinueButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        double score =
            _tracingCanvas.CalculateScore();

        if (score >= 0.80)
        {
            _feedbackText.Text = $"✅ Great job! Score: {score:P0}";
            await Task.Delay(1200);

            _feedbackText.Text = "📖 Back to the story...";
            await Task.Delay(800);

            if (Navigation is not null)
            {
                await Navigation.PopAsync();
            }

            // La historia continúa. Sin await: no bloqueamos la vuelta.
            _onCompleted?.Invoke();
        }
        else
        {
            // La palabra está mal: borramos el trazado.
            _tracingCanvas.Clear();

            _feedbackText.Text =
                $"✏️ Try again. Write the whole word carefully. Score: {score:P0}";
        }
    }
}