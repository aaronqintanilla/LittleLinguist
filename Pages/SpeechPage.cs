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

public class SpeechPage : ContentPage
{
    private readonly string targetWord;

    private readonly SpeechRecognizer speechRecognizer;
    private TextBlock resultText;
    private readonly Action? _onCompleted;

    public SpeechPage(string word, Action? onCompleted = null)
    {
        targetWord = word;
        _onCompleted = onCompleted;

        //string modelPath = Path.Combine(
        //AppContext.BaseDirectory,
       // "models",
        //"vosk-model-small-en-us-0.15");

        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/vosk-model-small-en-us-0.15";

        speechRecognizer = new SpeechRecognizer(modelPath);
        SpeechReader.Instance.SentenceChanged += OnSentenceChanged;

        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));

        // ---------------------------------------------------------
        // DECORACIÓN SUPERIOR
        // ---------------------------------------------------------

        var decoration = new Grid
        {
            Height = 50
        };

        var stars = new TextBlock
        {
            Text = "✦  ✧  ☆",
            FontSize = 26,
            Foreground = new SolidColorBrush(Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 0)
        };

        var cloud = new TextBlock
        {
            Text = "☁",
            FontSize = 42,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 25, 0)
        };

        decoration.Children.Add(stars);
        decoration.Children.Add(cloud);

        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "🗣 Pronunciation",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#6846C7")),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        // ---------------------------------------------------------
        // PALABRA
        // ---------------------------------------------------------

        var wordText = new TextBlock
        {
            Text = targetWord,
            FontSize = 48,
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
            CornerRadius = new CornerRadius(25),
            Padding = new Thickness(35, 20),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = wordText
        };

        // ---------------------------------------------------------
        // BOTÓN REPRODUCIR
        // ---------------------------------------------------------

        var playButton = new Button
        {
            Content = "🔊 Hear the word",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#7956D8")),
            Padding = new Thickness(25, 15),
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

            // Piper funciona al revés:
            // lengthScale pequeño = más rápido.
            SpeechReader.Instance.SetSpeed(
                (float)(1.0 / speed)
            );
        };

        var speedLabel = new TextBlock
        {
            Text = "🐢  Speed  🐇",
            FontSize = 15,
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

        // ---------------------------------------------------------
        // BOTÓN MICRÓFONO
        // ---------------------------------------------------------

        var speakButton = new Button
        {
            Content = "🎤 Speak",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#65B8E8")),
            Padding = new Thickness(25, 15),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(18)
        };

        speakButton.Click += SpeakButton_Click;

        // ---------------------------------------------------------
        // RESULTADO
        // ---------------------------------------------------------

        resultText = new TextBlock
        {
            Text = "",
            FontSize = 20,
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
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(20, 15),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = resultText
        };

        // ---------------------------------------------------------
        // PANEL IZQUIERDO: ESCUCHAR
        // ---------------------------------------------------------

        var listenPanel = new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        listenPanel.Children.Add(playButton);
        listenPanel.Children.Add(speedPanel);


        // ---------------------------------------------------------
        // PANEL DERECHO: HABLAR
        // ---------------------------------------------------------

        var speakPanel = new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        speakPanel.Children.Add(speakButton);
        speakPanel.Children.Add(resultBorder);


        // ---------------------------------------------------------
        // EJERCICIO: DOS COLUMNAS
        // ---------------------------------------------------------

        var exerciseGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 30,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        Grid.SetColumn(listenPanel, 0);
        Grid.SetColumn(speakPanel, 1);

        exerciseGrid.Children.Add(listenPanel);
        exerciseGrid.Children.Add(speakPanel);


        // ---------------------------------------------------------
        // CONTENIDO CENTRAL
        // ---------------------------------------------------------

        var contentPanel = new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        contentPanel.Children.Add(title);
        contentPanel.Children.Add(wordBorder);
        contentPanel.Children.Add(exerciseGrid);


        // ---------------------------------------------------------
        // GRID PRINCIPAL
        // ---------------------------------------------------------

        var mainGrid = new Grid
        {
            Margin = new Thickness(35, 20),
            RowDefinitions = RowDefinitions.Parse("Auto,*")
        };

        Grid.SetRow(decoration, 0);
        Grid.SetRow(contentPanel, 1);

        mainGrid.Children.Add(decoration);
        mainGrid.Children.Add(contentPanel);

        Content = mainGrid;
    }

    // ---------------------------------------------------------
    // REPRODUCIR PALABRA
    // ---------------------------------------------------------
    private void PlayButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        // Eliminamos cualquier reproducción anterior.
        SpeechReader.Instance.Stop();

        SpeechReader.Instance.Speak(targetWord);
        SpeechReader.Instance.Play();
    }

    // ---------------------------------------------------------
    // SPEECH TO TEXT
    // ---------------------------------------------------------

    // private async void SpeakButton_Click(
    // object? sender,
    // RoutedEventArgs e)
    // {
    //     resultText.Text = "🎤 Listening...";

    //     string recognizedText = await ListenAndRecognize();

    //     resultText.Text = "You said: " + recognizedText;

    //     if (IsCorrect(recognizedText))
    //     {
    //         resultText.Text += "\n✅ Correct!";
    //     }
    //     else
    //     {
    //         resultText.Text += "\n❌ Try again!";
    //     }
    // }
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
            resultText.Text = "🎤 No speech detected. Try again!";
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
            resultText.Text += "\n❌ Try again!";
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

    private bool IsCorrect(string recognizedText)
    {
        string expected = Normalize(targetWord);
        string actual = Normalize(recognizedText);

        return expected == actual;
    }

    private string Normalize(string text)
    {
        return text
            .Trim()
            .ToLowerInvariant();
    }

    private async Task<string> ListenAndRecognize()
    {
        return await speechRecognizer
            .RecognizeAsync(targetWord);
    }

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
}