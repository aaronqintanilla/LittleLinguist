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

    public SpeechPage(string word)
    {
        targetWord = word;

        //string modelPath = Path.Combine(
        //AppContext.BaseDirectory,
       // "models",
        //"vosk-model-small-en-us-0.15");

        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/vosk-model-small-en-us-0.15";

        speechRecognizer = new SpeechRecognizer(modelPath);
        SpeechReader.Instance.SentenceChanged += OnSentenceChanged;

        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "Pronunciation",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // PALABRA
        // ---------------------------------------------------------

        var wordText = new TextBlock
        {
            Text = targetWord,
            FontSize = 48,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // BOTÓN REPRODUCIR
        // ---------------------------------------------------------

        var playButton = new Button
        {
            Content = "🔊 Hear the word",
            FontSize = 20,
            Padding = new Thickness(25, 15),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        playButton.Click += PlayButton_Click;

        var speedLabel = new TextBlock
        {
            Text = "Speed: 1.0x",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center
        };

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

            speedLabel.Text = $"Speed: {speed:F1}x";

            // Piper funciona al revés:
            // lengthScale pequeño = más rápido.
            SpeechReader.Instance.SetSpeed(
                (float)(1.0 / speed)
            );
        };

        // ---------------------------------------------------------
        // BOTÓN MICROFONO
        // ---------------------------------------------------------

        var speakButton = new Button
        {
            Content = "🎤 Speak",
            FontSize = 20,
            Padding = new Thickness(25, 15),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        speakButton.Click += SpeakButton_Click;

        // ---------------------------------------------------------
        // RESULTADO
        // ---------------------------------------------------------

        resultText = new TextBlock
        {
            Text = "Your answer will appear here.",
            FontSize = 20,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // PANEL CENTRAL
        // ---------------------------------------------------------

        var centerPanel = new StackPanel
        {
            Width = 450,
            Spacing = 25,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                title,
                wordText,
                playButton,
                speedLabel,
                speedSlider,
                speakButton,
                resultText
            }
        };

        // ---------------------------------------------------------
        // GRID PRINCIPAL
        // ---------------------------------------------------------

        var mainGrid = new Grid
        {
            Margin = new Thickness(25)
        };

        mainGrid.Children.Add(centerPanel);

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
            resultText.Text += "\n✅ Correct!";
            await Task.Delay(1200);
            resultText.Text =
                "📖 Back to the story...";

            await Task.Delay(800);

            if (Navigation is not null)
            {
                await Navigation.PopAsync();
            }
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