using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace LittleLinguist.Pages;

// Displays reading comprehension questions about the story.
public class ReadingComprehensionPage : ContentPage
{
    private readonly Action? _onCompleted;
    private readonly TextBlock _questionText;
    private readonly TextBlock _feedbackText;
    private readonly StackPanel _optionsPanel;
    private readonly Button _continueButton;
    private List<ReadingQuestion> _questions = new();
    private int _currentQuestion = 0;
    private bool _answered = false;
    private bool _isFinishing = false;

    // Initializes the reading comprehension page and its user interface.
    public ReadingComprehensionPage(string story, Action? onCompleted = null)
    {
        _onCompleted = onCompleted;

        NavigationPage.SetHasBackButton(
            this,
            false
        );

        Background =
            new SolidColorBrush(
                Color.Parse("#EDE7FF")
            );

        var title =
            new TextBlock
            {
                Text = "📚 Reading Time!",

                FontSize = 34,

                FontWeight =
                    FontWeight.Bold,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#6846C7")
                    ),

                TextAlignment =
                    TextAlignment.Center,

                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

        _questionText =
            new TextBlock
            {
                Text = "✨ Preparing your question...",

                FontSize = 25,

                FontWeight =
                    FontWeight.Bold,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#6846C7")
                    ),

                TextAlignment =
                    TextAlignment.Center,

                TextWrapping =
                    TextWrapping.Wrap,

                HorizontalAlignment =
                    HorizontalAlignment.Stretch
            };

        var subtitle =
            new TextBlock
            {
                Text = "Let's see how well you followed the story!",

                FontSize = 17,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#55729A")
                    ),

                TextAlignment =
                    TextAlignment.Center,

                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

        _optionsPanel =
            new StackPanel
            {
                Spacing = 14,

                HorizontalAlignment =
                    HorizontalAlignment.Stretch
            };

        _feedbackText =
            new TextBlock
            {
                Text = "",

                FontSize = 18,

                FontWeight =
                    FontWeight.SemiBold,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#6846C7")
                    ),

                TextAlignment =
                    TextAlignment.Center,

                TextWrapping =
                    TextWrapping.Wrap,

                HorizontalAlignment =
                    HorizontalAlignment.Stretch
            };

        _continueButton =
            new Button
            {
                Content = "Continue  ➜",

                FontSize = 17,

                FontWeight =
                    FontWeight.Bold,

                Foreground =
                    Brushes.White,

                Background =
                    new SolidColorBrush(
                        Color.Parse("#65B8E8")
                    ),

                CornerRadius =
                    new CornerRadius(18),

                Padding =
                    new Thickness(
                        25,
                        12
                    ),

                MinWidth = 150,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                HorizontalContentAlignment =
                    HorizontalAlignment.Center,

                IsVisible = false
            };

        _continueButton.Click +=
            ContinueButton_Click;

        var skipButton =
            new Button
            {
                Content = "➡️ Skip",

                Padding =
                    new Thickness(
                        22,
                        13
                    ),

                FontSize = 17,

                FontWeight =
                    FontWeight.Bold,

                Foreground =
                    Brushes.White,

                Background =
                    new SolidColorBrush(
                        Color.Parse("#E98BA5")
                    ),

                BorderThickness =
                    new Thickness(0),

                CornerRadius =
                    new CornerRadius(18),

                HorizontalAlignment =
                    HorizontalAlignment.Left,

                VerticalAlignment =
                    VerticalAlignment.Center
            };

        skipButton.Click +=
            SkipButton_Click;

        var questionPanel =
            new StackPanel
            {
                Spacing = 22
            };

        questionPanel.Children.Add(
            _questionText
        );

        questionPanel.Children.Add(
            _optionsPanel
        );

        questionPanel.Children.Add(
            _feedbackText
        );

        questionPanel.Children.Add(
            _continueButton
        );

        var questionCard =
            new Border
            {
                Width = 540,

                Background =
                    Brushes.White,

                BorderBrush =
                    new SolidColorBrush(
                        Color.Parse("#D8CCFF")
                    ),

                BorderThickness =
                    new Thickness(2),

                CornerRadius =
                    new CornerRadius(28),

                Padding =
                    new Thickness(
                        35,
                        30
                    ),

                Child =
                    questionPanel
            };

        var centerPanel =
            new StackPanel
            {
                Spacing = 20,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                VerticalAlignment =
                    VerticalAlignment.Center
            };

        centerPanel.Children.Add(
            title
        );

        centerPanel.Children.Add(
            subtitle
        );

        centerPanel.Children.Add(
            questionCard
        );

        var star1 =
            new TextBlock
            {
                Text = "☆",

                FontSize = 28,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#7956D8")
                    ),

                HorizontalAlignment =
                    HorizontalAlignment.Left,

                VerticalAlignment =
                    VerticalAlignment.Top,

                Margin =
                    new Thickness(
                        75,
                        100,
                        0,
                        0
                    )
            };

        var star2 =
            new TextBlock
            {
                Text = "✦",

                FontSize = 23,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#65B8E8")
                    ),

                HorizontalAlignment =
                    HorizontalAlignment.Right,

                VerticalAlignment =
                    VerticalAlignment.Top,

                Margin =
                    new Thickness(
                        0,
                        150,
                        80,
                        0
                    )
            };

        var star3 =
            new TextBlock
            {
                Text = "✧",

                FontSize = 22,

                Foreground =
                    new SolidColorBrush(
                        Color.Parse("#7956D8")
                    ),

                HorizontalAlignment =
                    HorizontalAlignment.Left,

                VerticalAlignment =
                    VerticalAlignment.Bottom,

                Margin =
                    new Thickness(
                        100,
                        0,
                        0,
                        90
                    )
            };

        var cloud1 =
            new TextBlock
            {
                Text = "☁",

                FontSize = 48,

                Foreground =
                    Brushes.White,

                HorizontalAlignment =
                    HorizontalAlignment.Left,

                VerticalAlignment =
                    VerticalAlignment.Bottom,

                Margin =
                    new Thickness(
                        35,
                        0,
                        0,
                        35
                    )
            };

        var cloud2 =
            new TextBlock
            {
                Text = "☁",

                FontSize = 38,

                Foreground =
                    Brushes.White,

                HorizontalAlignment =
                    HorizontalAlignment.Right,

                VerticalAlignment =
                    VerticalAlignment.Bottom,

                Margin =
                    new Thickness(
                        0,
                        0,
                        50,
                        65
                    )
            };

        var mainGrid =
            new Grid
            {
                Margin =
                    new Thickness(30),

                RowDefinitions =
                    RowDefinitions.Parse("*,Auto")
            };

        Grid.SetRow(centerPanel, 0);

        Grid.SetRow(skipButton, 1);

        mainGrid.Children.Add(
            star1
        );

        mainGrid.Children.Add(
            star2
        );

        mainGrid.Children.Add(
            star3
        );

        mainGrid.Children.Add(
            cloud1
        );

        mainGrid.Children.Add(
            cloud2
        );

        mainGrid.Children.Add(
            centerPanel
        );

        mainGrid.Children.Add(
            skipButton
        );

        Content =
            mainGrid;

        _ = LoadQuestions(story);
    }

    // Generates the reading questions from the story.
    private async Task LoadQuestions(
        string story)
    {
        try
        {
            _questions =
                await StoryGenerator.Instance
                    .GenerateReadingQuestions(story);

            if (_questions.Count == 0)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _questionText.Text =
                        "😕 I couldn't prepare the question.";

                    _feedbackText.Text =
                        "Let's continue the adventure!";

                    _continueButton.IsVisible =
                        true;
                });

                return;
            }

            _currentQuestion = 0;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowQuestion();
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                "READING COMPREHENSION ERROR:"
            );

            Console.Error.WriteLine(ex);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _questionText.Text =
                    "😕 I couldn't prepare the question.";

                _continueButton.IsVisible =
                    true;
            });
        }
    }

    // Displays the current question and its answer options.
    private void ShowQuestion()
    {
        _answered = false;

        _feedbackText.Text = "";
        _continueButton.IsVisible = false;

        _optionsPanel.Children.Clear();

        ReadingQuestion question =
            _questions[_currentQuestion];

        _questionText.Text =
            question.Question;

        for (int i = 0;
             i < question.Options.Count;
             ++i)
        {
            int optionIndex = i;

            var button =
        new Button
        {
            Content =
                question.Options[i],

            FontSize = 18,

            FontWeight =
                FontWeight.SemiBold,

            Foreground =
                new SolidColorBrush(
                    Color.Parse("#4B416D")
                ),

            Background =
                new SolidColorBrush(
                    Color.Parse("#F4F0FF")
                ),

            BorderBrush =
                new SolidColorBrush(
                    Color.Parse("#D8CCFF")
                ),

            BorderThickness =
                new Thickness(2),

            CornerRadius =
                new CornerRadius(18),

            Padding =
                new Thickness(
                    20,
                    14
                ),

            HorizontalAlignment =
                HorizontalAlignment.Stretch,

            HorizontalContentAlignment =
                HorizontalAlignment.Center,

            Tag =
                optionIndex
        };

            button.Click +=
                AnswerButton_Click;

            _optionsPanel.Children.Add(
                button
            );
        }
    }

    // Checks the selected answer and shows feedback.
    private void AnswerButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_answered)
            return;

        if (sender is not Button button)
            return;

        if (button.Tag is not int selected)
            return;

        _answered = true;

        ReadingQuestion question =
            _questions[_currentQuestion];

        bool correct =
            selected ==
            question.CorrectAnswer;

        if (correct)
        {
            _feedbackText.Text =
                "✅ Correct!";
        }
        else
        {
            string answer =
                question.Options[
                    question.CorrectAnswer
                ];

            _feedbackText.Text =
                $"❌ Not quite. The answer is: {answer}";
        }

        foreach (
            Button optionButton
            in _optionsPanel.Children
                .OfType<Button>())
        {
            optionButton.IsEnabled =
                false;
        }

        _continueButton.IsVisible =
            true;
    }

    // Moves to the next question or finishes the activity.
    private async void ContinueButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_isFinishing)
            return;

        if (_currentQuestion <
            _questions.Count - 1)
        {
            ++_currentQuestion;

            ShowQuestion();

            return;
        }

        _isFinishing = true;

        _continueButton.IsEnabled =
            false;

        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }

        _onCompleted?.Invoke();
    }

    // Skips the activity and continues the story.
    private async void SkipButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }

        _onCompleted?.Invoke();
    }
}