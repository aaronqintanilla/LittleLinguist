using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Controls;
using System.Threading.Tasks;
using System;

namespace LittleLinguist.Pages;

// Handles the writing activity and checks the traced word.
public class WritingPage : ContentPage
{
    private readonly TracingCanvas _tracingCanvas;
    private readonly TextBlock _feedbackText;
    private readonly Action? _onCompleted;
    private bool _isCompleting = false;

    // Initializes the writing page and its user interface.
    public WritingPage(string word = "APPLE", Action? onCompleted = null)
    {
        _onCompleted = onCompleted;
        NavigationPage.SetHasBackButton(this, false);

        Background = new SolidColorBrush(
            Color.Parse("#EDE7FF"));

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

        _tracingCanvas = new TracingCanvas
        {
            TargetWord = word.ToUpperInvariant(),

            MinHeight = 160,

            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

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

        var skipButton = new Button
        {
            Content = "➡️ Skip",
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

        skipButton.Click += SkipButton_Click;

        var buttonsGrid = new Grid
        {
            ColumnDefinitions =
                ColumnDefinitions.Parse("*,*,*"),
            ColumnSpacing = 20,
            HorizontalAlignment =
                HorizontalAlignment.Stretch
        };

        Grid.SetColumn(skipButton, 0);
        Grid.SetColumn(clearButton, 1);
        Grid.SetColumn(continueButton, 2);

        buttonsGrid.Children.Add(skipButton);
        buttonsGrid.Children.Add(clearButton);
        buttonsGrid.Children.Add(continueButton);

        var header = new Grid
        {
            Height = 80
        };

        Grid.SetRow(decoration, 0);
        Grid.SetRow(title, 0);

        header.Children.Add(decoration);
        header.Children.Add(title);

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

    // Clears the user's drawing and feedback message.
    private void ClearButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _tracingCanvas.Clear();
        _feedbackText.Text = "";
    }

    // Skips the writing activity and continues the story.
    private async void SkipButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }

        _onCompleted?.Invoke();
    }

    // Checks the tracing score and continues if the word is correct.
    private async void ContinueButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (_isCompleting)
            return;

        double score =
            _tracingCanvas.CalculateScore();

        if (score >= 0.80)
        {
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

            _onCompleted?.Invoke();
        }
        else
        {
            _tracingCanvas.Clear();

            _feedbackText.Text =
                $"✏️ Try again. Write the whole word carefully. Score: {score:P0}";
        }
    }
}