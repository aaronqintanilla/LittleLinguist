using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Controls;
using System.Threading.Tasks;
using System;

namespace LittleLinguist.Pages;

public class WritingPage : ContentPage
{
    private readonly TracingCanvas _tracingCanvas;
    private readonly TextBlock _feedbackText;
    private readonly Action? _onCompleted;
    private bool _isCompleting = false;

    public WritingPage(string word = "APPLE", Action? onCompleted = null)
    {
        _onCompleted = onCompleted;
        NavigationPage.SetHasBackButton(this, false);

        // =========================================================
        // FONDO
        // =========================================================

        Background = new SolidColorBrush(
            Color.Parse("#EDE7FF"));

        // =========================================================
        // TÍTULO
        // =========================================================

        var title = new TextBlock
        {
            Text = "✏️ Writing Practice",
            FontSize = 34,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(
                Color.Parse("#6846C7")),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // =========================================================
        // DECORACIÓN
        // =========================================================

        var decoration = new Grid
        {
            Height = 40
        };

        var starsLeft = new TextBlock
        {
            Text = "✦  ✧",
            FontSize = 28,
            Foreground = new SolidColorBrush(
                Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        var starsRight = new TextBlock
        {
            Text = "✧  ✦",
            FontSize = 28,
            Foreground = new SolidColorBrush(
                Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        decoration.Children.Add(starsLeft);
        decoration.Children.Add(starsRight);

        // =========================================================
        // INSTRUCCIONES
        // =========================================================

        var instructions = new TextBlock
        {
            Text = "Trace the word with your finger or digital pen ✨",
            FontSize = 18,
            Foreground = new SolidColorBrush(
                Color.Parse("#55729A")),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // =========================================================
        // ZONA TÁCTIL
        // =========================================================

        _tracingCanvas = new TracingCanvas
        {
            TargetWord = word.ToUpperInvariant(),

            MinHeight = 160,

            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        // =========================================================
        // TARJETA DEL ÁREA DE ESCRITURA
        // =========================================================

        var canvasBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(
                Color.Parse("#C9BBF5")),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(25),
            Padding = new Thickness(15),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = _tracingCanvas
        };

        // =========================================================
        // MENSAJE
        // =========================================================

        _feedbackText = new TextBlock
        {
            Text = "",
            FontSize = 19,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(
                Color.Parse("#55729A")),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        // =========================================================
        // BOTÓN CLEAR
        // =========================================================

        var clearButton = new Button
        {
            Content = "🗑 Clear",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(
                Color.Parse("#7956D8")),
            Padding = new Thickness(25, 13),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(18)
        };

        clearButton.Click += ClearButton_Click;

        // =========================================================
        // BOTÓN CONTINUE
        // =========================================================

        var continueButton = new Button
        {
            Content = "⭐ Continue",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(
                Color.Parse("#65B8E8")),
            Padding = new Thickness(25, 13),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(18)
        };

        continueButton.Click += ContinueButton_Click;

        // =========================================================
        // BOTÓN FINISH
        // =========================================================

        var finishButton = new Button
        {
            Content = "🏠 Finish",
            Padding = new Thickness(22, 13),
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(
                Color.Parse("#E98BA5")),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(18),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        finishButton.Click += FinishButton_Click;

        // =========================================================
        // GRID DE BOTONES
        // =========================================================

        var buttonsGrid = new Grid
        {
            ColumnDefinitions =
                ColumnDefinitions.Parse("*,*,*"),
            ColumnSpacing = 20,
            HorizontalAlignment =
                HorizontalAlignment.Stretch
        };

        Grid.SetColumn(finishButton, 0);
        Grid.SetColumn(clearButton, 1);
        Grid.SetColumn(continueButton, 2);

        buttonsGrid.Children.Add(finishButton);
        buttonsGrid.Children.Add(clearButton);
        buttonsGrid.Children.Add(continueButton);

        // =========================================================
        // CABECERA
        // =========================================================

        var header = new Grid
        {
            Height = 80
        };

        Grid.SetRow(decoration, 0);
        Grid.SetRow(title, 0);

        header.Children.Add(decoration);
        header.Children.Add(title);

        // =========================================================
        // GRID PRINCIPAL
        // =========================================================

        var mainGrid = new Grid
        {
            Margin = new Thickness(35, 15),

            RowDefinitions =
                RowDefinitions.Parse(
                    "Auto,Auto,*,Auto,Auto"),

            RowSpacing = 15
        };

        Grid.SetRow(header, 0);
        Grid.SetRow(instructions, 1);
        Grid.SetRow(canvasBorder, 2);
        Grid.SetRow(_feedbackText, 3);
        Grid.SetRow(buttonsGrid, 4);

        mainGrid.Children.Add(header);
        mainGrid.Children.Add(instructions);
        mainGrid.Children.Add(canvasBorder);
        mainGrid.Children.Add(_feedbackText);
        mainGrid.Children.Add(buttonsGrid);

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
        // Si ya estamos volviendo a StoryPage,
        // ignoramos cualquier clic adicional.
        if (_isCompleting)
            return;

        double score =
            _tracingCanvas.CalculateScore();

        if (score >= 0.80)
        {
            // Desde este momento no permitimos más clics.
            _isCompleting = true;

            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            _feedbackText.Text =
                $"✅ Great job! Score: {score:P0}";

            await Task.Delay(1200);

            _feedbackText.Text =
                "📖 Back to the story...";

            await Task.Delay(800);

            if (Navigation is not null)
            {
                await Navigation.PopAsync();
            }

            // Solo se ejecutará UNA vez.
            _onCompleted?.Invoke();
        }
        else
        {
            _tracingCanvas.Clear();

            _feedbackText.Text =
                $"✏️ Try again. Write the whole word carefully. Score: {score:P0}";
        }
    }

    private void FinishButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        //FALTA
    }
}