using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using System.Linq;
using System.Text.RegularExpressions;
namespace LittleLinguist.Pages;
using Avalonia.Controls.Documents;
using Avalonia.Threading;

/*
FALTA:
1. cambiar pruebas a las que ir según la competencia
2. historias diferentes
*/

public class StoryPage : ContentPage
{
    TextBlock storyText = new TextBlock();

    // Botón Play
    Button playButton = new Button();

    int cont = 0;
    private List<string> _sentences = new();
    private string? _speakingSentence;

    public StoryPage()
    {
        NavigationPage.SetHasBackButton(this, false);
        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));
        
        // Grid principal
        var grid = new Grid
        {
            Margin = new Thickness(35, 20)
        };
        grid.RowDefinitions.Add(
            new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(
            new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(
            new RowDefinition(GridLength.Auto));

        // Decoración
        var header = new Grid
        {
            Height = 80
        };

        var starsLeft = new TextBlock
        {
            Text = "✦  ✧",
            FontSize = 30,
            Foreground = new SolidColorBrush(
                Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        var title = new TextBlock
        {
            Text = "📖",
            FontSize = 34,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(
                Color.Parse("#6846C7")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var starsRight = new TextBlock
        {
            Text = "✧  ✦",
            FontSize = 30,
            Foreground = new SolidColorBrush(
                Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        header.Children.Add(starsLeft);
        header.Children.Add(title);
        header.Children.Add(starsRight);

        Grid.SetRow(header, 0);

        // Texto de la historia
        storyText.TextWrapping = TextWrapping.Wrap;
        storyText.FontSize = 19;
        storyText.Foreground = new SolidColorBrush(
            Color.Parse("#465477"));

        // Scroll para el texto
        var scrollViewer = new ScrollViewer
        {
            Content = storyText,
            Padding = new Thickness(25),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(
                Color.Parse("#C9BBF5")),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(25),
            Margin = new Thickness(0, 5, 0, 20)
        };

        Grid.SetRow(scrollViewer, 1);

        // Panel para los botones
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 15
        };

        // Botón Finish
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
            CornerRadius = new CornerRadius(18)
        };

        finishButton.Click += FinishButton_Click;

        // Botón Next
        var nextButton = new Button
        {
            Content = "⭐ Next",
            Padding = new Thickness(22, 13),
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(
                Color.Parse("#65B8E8")),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(18)
        };

        nextButton.Click += NextButton_Click;

        // Botón Play / Pause
        playButton.Content = "▶ Play";
        playButton.Padding = new Thickness(22, 13);
        playButton.FontSize = 17;
        playButton.FontWeight = FontWeight.Bold;
        playButton.Foreground = Brushes.White;
        playButton.Background = new SolidColorBrush(
            Color.Parse("#7956D8"));
        playButton.BorderThickness = new Thickness(0);
        playButton.CornerRadius = new CornerRadius(18);

        playButton.Click += PlayButton_Click;

        // Slider de velocidad: a la derecha, más rápido
        var speedSlider = new Slider
        {
            Minimum = 0.5,
            Maximum = 2.5,
            Value = 1.5,
            Width = 160,
            TickFrequency = 0.1,
            IsSnapToTickEnabled = true
        };

        speedSlider.ValueChanged += SpeedSlider_ValueChanged;

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

        buttonPanel.Children.Add(playButton);
        buttonPanel.Children.Add(speedPanel);
        buttonPanel.Children.Add(finishButton);
        buttonPanel.Children.Add(nextButton);
        Grid.SetRow(buttonPanel, 2);

        grid.Children.Add(header);
        grid.Children.Add(scrollViewer);
        grid.Children.Add(buttonPanel);

        Content = grid;

        StoryGenerator.Instance.SentencesChanged += OnSentencesChanged;
        SpeechReader.Instance.SentenceChanged += OnSpeakingSentenceChanged;
    }

    // Vuelve a la página anterior (Home)
    private async void FinishButton_Click(object? sender, RoutedEventArgs e)
    {
        StoryGenerator.Instance.StopStory();
        if(Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }

    // Pasa a la siguiente página con una palabra de la historia
    private async void NextButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Navigation is null)
        {
            return;
        }

        if(Session.Instance.IsFinished)
        {
            Console.WriteLine("Fin de la historia");
            StoryGenerator.Instance.StopStory();
            await Navigation.PopAsync();
            return;
        }

        // Guardamos el texto antes de parar, porque StopStory
        // vacía la lista de frases.
        string story = string.Join(" ", _sentences);

        // Detiene la generación y la lectura.
        StoryGenerator.Instance.PauseStory();
        //SpeechReader.Instance.Pause();
        playButton.Content = "Play";

        // Extrae palabras de entre 3 y 10 letras.
        List<string> words = Regex
            .Matches(story, @"\b[A-Za-z]{3,10}\b")
            .Select(match => match.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (words.Count == 0)
        {
            Console.WriteLine("No words found in the story.");
            return;
        }

        string randomWord = words[Random.Shared.Next(words.Count)];

        Console.WriteLine($"Selected word: {randomWord}");

        ContentPage page;

        switch (cont % 3)
        {
            case 0:
                page = new WritingPage(
                    randomWord,
                    StartStory
                );
                break;

            case 1:
                page =
                    new ReadingComprehensionPage(
                        story,
                        StartStory
                    );
                break;

            default:
                page = new SpeechPage(
                    randomWord,
                    StartStory
                );
                break;
        }

    cont++;

    await Navigation.PushAsync(page);

        //BUG: await Navigation.PushAsync(new WritingPage(randomWord, StartStory));
    }

    public async void StartStory()
    {
        await Session.Instance.WriteNextPart(null);
    }

    // Alterna entre reproducir y pausar el audio
    private void PlayButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SpeechReader.Instance.IsPlaying)
        {
            SpeechReader.Instance.Pause();
            playButton.Content = "Play";
        }
        else
        {
            SpeechReader.Instance.Play();
            playButton.Content = "Pause";
        }
    }

    // Ajusta la velocidad de lectura.
    // El slider va de lento (izquierda) a rápido (derecha),
    // mientras que Piper usa la escala contraria.
    private void SpeedSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        SpeechReader.Instance.SetSpeed((float)(3.0 - e.NewValue));
    }

    // Llega una nueva lista de frases desde el generador.
    private void OnSentencesChanged(List<string> sentences)
    {
        _sentences = sentences;
        Dispatcher.UIThread.Post(RefreshStoryText);
    }

    // Cambia la frase que se está leyendo en voz alta.
    private void OnSpeakingSentenceChanged(string? sentence)
    {
        _speakingSentence = sentence;
        Dispatcher.UIThread.Post(RefreshStoryText);
    }

    // Redibuja la historia, resaltando la frase que se está leyendo.
    private void RefreshStoryText()
    {
        if (storyText.Inlines is null) return;

        storyText.Inlines.Clear();

        foreach (string sentence in _sentences)
        {
            string text = sentence.TrimEnd();

            bool isSpeaking = text == _speakingSentence;

            string separator = sentence.EndsWith("\n") ? "\n\n" : " ";

            storyText.Inlines.Add(new Run(text + separator)
            {
                Foreground = isSpeaking
                    ? new SolidColorBrush(Color.Parse("#872589"))
                    : Brushes.Black,

                FontWeight = isSpeaking ? FontWeight.Bold : FontWeight.Normal
            });
        }
    }

    public void Cleanup()
    {
        StoryGenerator.Instance.SentencesChanged -= OnSentencesChanged;
        SpeechReader.Instance.SentenceChanged -= OnSpeakingSentenceChanged;

        StoryGenerator.Instance.StopStory();
    }
}