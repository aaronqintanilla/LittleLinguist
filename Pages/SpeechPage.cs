using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using System.IO;
using LittleLinguist.Services;
using Avalonia.Threading;

namespace LittleLinguist.Pages;

// Handles the pronunciation activity and speech recognition.
public class SpeechPage : ContentPage
{
    private readonly string targetWord;

    private readonly SpeechRecognizer speechRecognizer;
    private TextBlock resultText;
    private readonly Action? _onCompleted;

    // Initializes the pronunciation page and its user interface.
    public SpeechPage(string word, Action? onCompleted = null)
    {
        targetWord = word;
        _onCompleted = onCompleted;

        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/vosk-model-small-en-us-0.15";

        speechRecognizer = new SpeechRecognizer(modelPath);
        SpeechReader.Instance.SentenceChanged += OnSentenceChanged;

        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));

        var stars = new TextBlock
        {
            Text = "✦  ✧  ☆",
            FontSize = 20,
            Foreground = new SolidColorBrush(
                Color.Parse("#FFD966")),

            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,

            Margin = new Thickness(
                10, 0, 0, 5)
        };

        var cloud = new TextBlock
        {
            Text = "☁",
            FontSize = 32,
            Foreground = Brushes.White,

            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,

            Margin = new Thickness(
                0, 0, 15, 5)
        };

        var title = new TextBlock
        {
            Text = "🗣 Pronunciation",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#6846C7")),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var wordText = new TextBlock
        {
            Text = targetWord,
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#465477")),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var wordBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#C9BBF5")),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = wordText
        };

        var playButton = new Button
        {
            Content = "🔊 Hear the word",
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#7956D8")),
            Padding = new Thickness(18, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(18)
        };

        playButton.Click += PlayButton_Click;

        var speedSlider = new Slider
        {
            Minimum = 0.1,
            Maximum = 1.5,
            Value = 1.0,
            TickFrequency = 0.1,
            IsSnapToTickEnabled = true,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        speedSlider.ValueChanged += (_, e) =>
        {
            double speed = e.NewValue;

            // Piper works in reverse: a smaller lengthScale means faster speech.
            SpeechReader.Instance.SetSpeed(
                (float)(1.0 / speed)
            );
        };

        var speedLabel = new TextBlock
        {
            Text = "🐢  Speed  🐇",
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(
                Color.Parse("#55729A")),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var speedPanel = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        speedPanel.Children.Add(speedLabel);
        speedPanel.Children.Add(speedSlider);

        var speakButton = new Button
        {
            Content = "🎤 Speak",
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#65B8E8")),
            Padding = new Thickness(18, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(18)
        };

        speakButton.Click += SpeakButton_Click;

        var skipButton = new Button
        {
            Content = "➡️ Skip",

            Padding = new Thickness(18, 10),

            FontSize = 15,
            FontWeight = FontWeight.Bold,

            Foreground = Brushes.White,

            Background = new SolidColorBrush(
                Color.Parse("#E98BA5")),

            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(18),

            HorizontalAlignment =
                HorizontalAlignment.Center,

            VerticalAlignment =
                VerticalAlignment.Center
        };

        skipButton.Click +=
            SkipButton_Click;

        resultText = new TextBlock
        {
            Text = "",
            FontSize = 17,
            Foreground = new SolidColorBrush(Color.Parse("#55729A")),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var resultBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#C9BBF5")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(15, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = resultText
        };

        var listenPanel = new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        listenPanel.Children.Add(playButton);
        listenPanel.Children.Add(speedPanel);

        var speakPanel = new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        speakPanel.Children.Add(speakButton);
        speakPanel.Children.Add(resultBorder);

        var exerciseGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 15,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        Grid.SetColumn(listenPanel, 0);
        Grid.SetColumn(speakPanel, 1);

        exerciseGrid.Children.Add(listenPanel);
        exerciseGrid.Children.Add(speakPanel);

        var topPanel = new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        topPanel.Children.Add(title);
        topPanel.Children.Add(wordBorder);

        var mainGrid = new Grid
        {
            Margin = new Thickness(25, 15),
            RowDefinitions = RowDefinitions.Parse("Auto,*,Auto")
        };

        Grid.SetRow(topPanel, 0);
        Grid.SetRow(exerciseGrid, 1);
        Grid.SetRow(skipButton, 2);

        mainGrid.Children.Add(topPanel);
        mainGrid.Children.Add(exerciseGrid);
        mainGrid.Children.Add(skipButton);

        mainGrid.Children.Add(stars);
        mainGrid.Children.Add(cloud);

        Content = mainGrid;
    }

    // Plays the target word using speech synthesis.
    private void PlayButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        SpeechReader.Instance.Stop();

        SpeechReader.Instance.Speak(targetWord);
        SpeechReader.Instance.Play();
    }

    // Records the user's speech and checks the pronunciation.
    private async void SpeakButton_Click(
    object? sender,
        RoutedEventArgs e)
    {
        try
        {
            Console.WriteLine("Starting speech recognition...");

            resultText.Text = "🎤 Listening...";

            string recognizedText =
                await ListenAndRecognize();

            Console.WriteLine(
                $"Recognized: {recognizedText}"
            );

            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                resultText.Text = $"🎤 No speech detected.\n Try again!";
                return;
            }

            if (IsCorrect(recognizedText))
            {
                resultText.Text =
                    $"You said: {recognizedText}\n" +
                    "✅ Correct!";

                await Task.Delay(1200);

                resultText.Text =
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
                resultText.Text =
                    $"❌ Incorrect.\n Try again!";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                "SPEECH RECOGNITION ERROR:"
            );

            Console.Error.WriteLine(
                ex.ToString()
            );

            resultText.Text =
                "❌ Speech recognition error.";
        }
    }

    // Checks whether the recognized word matches the target word.
    private bool IsCorrect(string recognizedText)
    {
        string expected = Normalize(targetWord);
        string actual = Normalize(recognizedText);

        return expected == actual;
    }

    // Normalizes text before comparing words.
    private string Normalize(string text)
    {
        return text
            .Trim()
            .ToLowerInvariant();
    }

    // Records and recognizes the user's speech.
    private async Task<string> ListenAndRecognize()
    {
        return await speechRecognizer
            .RecognizeAsync(targetWord);
    }

    // Updates the result text while the word is being played.
    private void OnSentenceChanged(string? sentence)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (sentence is null)
            {
                resultText.Text = "";
            }
            else
            {
                resultText.Text = "🔊 Playing...";
            }
        });
    }

    // Skips the pronunciation activity and continues the story.
    private async void SkipButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }

        _onCompleted?.Invoke();
    }
}