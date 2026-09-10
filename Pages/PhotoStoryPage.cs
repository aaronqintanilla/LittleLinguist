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
using LittleLinguist.Services;

// Displays a photo-based story and manages the learning activities.
public class PhotoStoryPage : ContentPage
{
    TextBlock storyText = new TextBlock();
    Button playButton = new Button();

    int cont = 0;
    private List<string> _sentences = new();
    private string? _speakingSentence;

    // Initializes the photo story page and its user interface.
    public PhotoStoryPage()
    {
        NavigationPage.SetHasBackButton(this, false);
        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));
        
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

        storyText.TextWrapping = TextWrapping.Wrap;
        storyText.FontSize = 19;
        storyText.Foreground = new SolidColorBrush(
            Color.Parse("#465477"));

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

        var buttonPanel = new Grid
        {
            ColumnDefinitions =
                ColumnDefinitions.Parse("Auto,*,Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

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

        var centerControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        centerControls.Children.Add(playButton);
        centerControls.Children.Add(speedPanel);

        Grid.SetColumn(finishButton, 0);
        Grid.SetColumn(centerControls, 1);
        Grid.SetColumn(nextButton, 2);

        buttonPanel.Children.Add(finishButton);
        buttonPanel.Children.Add(centerControls);
        buttonPanel.Children.Add(nextButton);

        Grid.SetRow(buttonPanel, 2);

        grid.Children.Add(header);
        grid.Children.Add(scrollViewer);
        grid.Children.Add(buttonPanel);

        Content = grid;

        StoryGenerator.Instance.SentencesChanged += OnSentencesChanged;
        SpeechReader.Instance.SentenceChanged += OnSpeakingSentenceChanged;
    }

    // Ends the program
    private async void FinishButton_Click(object? sender, RoutedEventArgs e)
    {
        StoryGenerator.Instance.StopStory();
        if(Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }

    // Selects a learning activity based on the enabled skills.
    private async void NextButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Navigation is null)
        {
            return;
        }

        if(Session.Instance.IsFinished)
        {
            StoryGenerator.Instance.StopStory();
            await Navigation.PopAsync();
            return;
        }

        string story = string.Join(" ", _sentences);

        StoryGenerator.Instance.PauseStory();
        SpeechReader.Instance.Pause();
        playButton.Content = "Play";

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

        ContentPage? page = null;

        var settings = LearningSettings.Load();

        bool skillFound = false;

        for (int i = 0; i < 3 && !skillFound; i++)
        {
            switch (cont % 3)
            {
                // Writing
                case 0:
                    cont++;

                    if (settings.Writing)
                    {
                        page = new WritingPage(
                        randomWord,
                        ContinueStory
                        );

                        skillFound = true;
                    }
                    break;

                // Reading and Listening
                case 1:
                    cont++;

                    if (settings.Reading || settings.Listening)
                    {
                        page = new ReadingComprehensionPage(
                        story,
                        ContinueStory
                        );

                        skillFound = true;
                    }
                    break;

                // Pronunciation
                case 2:
                    cont++;

                    if (settings.Pronunciation)
                    {
                        page = new SpeechPage(
                        randomWord,
                        ContinueStory
                    );

                        skillFound = true;
                    }
                    break;
            }
        }

        if (page is not null)
        {
            await Navigation.PushAsync(page);
        }
    }

    // Identifies the photographed object and starts the story.
    public async void StartStory(byte[] imageData)
    {
        string objectDescription = await VisionEngine.Instance.IdentifyObject(imageData);
        await Session.Instance.WriteNextPart(objectDescription);
    }

    // Continues the story after completing a learning activity.
    public async void ContinueStory()
    {
        if (Session.Instance.IsFinished && Navigation is not null)
        {
            StoryGenerator.Instance.StopStory();
            cont = 0;
            await Navigation.PopToRootAsync();
            return;
        }

        await Session.Instance.WriteNextPart(null);
    }

    // Toggles story narration between play and pause.
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

    // Adjusts the narration speed.
    private void SpeedSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        SpeechReader.Instance.SetSpeed((float)(3.0 - e.NewValue));
    }

    // Updates the displayed story when new sentences are generated.
    private void OnSentencesChanged(List<string> sentences)
    {
        _sentences = sentences;
        Dispatcher.UIThread.Post(RefreshStoryText);
    }

    // Updates the sentence currently being narrated.
    private void OnSpeakingSentenceChanged(string? sentence)
    {
        _speakingSentence = sentence;
        Dispatcher.UIThread.Post(RefreshStoryText);
    }

    // Refreshes the story text and highlights the sentence being narrated.
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

    // Unsubscribes from events and stops the story.
    public void Cleanup()
    {
        StoryGenerator.Instance.SentencesChanged -= OnSentencesChanged;
        SpeechReader.Instance.SentenceChanged -= OnSpeakingSentenceChanged;

        StoryGenerator.Instance.StopStory();
    }
}