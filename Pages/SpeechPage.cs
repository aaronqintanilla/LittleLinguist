using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using System.IO;
using LittleLinguist.Services;

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
        resultText.Text = "🔊 Playing...";

        SpeechReader.Instance.Speak(targetWord);
        SpeechReader.Instance.Play();
    }

    // ---------------------------------------------------------
    // SPEECH TO TEXT
    // ---------------------------------------------------------

    private async void SpeakButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        resultText.Text = "🎤 Listening...";

        string recognizedText = await ListenAndRecognize();

        resultText.Text = "You said: " + recognizedText;

        if (IsCorrect(recognizedText))
        {
            resultText.Text += "\n✅ Correct!";
        }
        else
        {
            resultText.Text += "\n❌ Try again!";
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
        return await speechRecognizer.RecognizeAsync();
    }

}