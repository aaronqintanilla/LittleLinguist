using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Services;
using System.Threading.Tasks;

namespace LittleLinguist.Pages;

// Displays the learning settings and allows the user to select skills.
public class SettingsPage : ContentPage
{
    private readonly CheckBox _pronunciationCheckBox;
    private readonly CheckBox _writingCheckBox;
    private readonly CheckBox _readingCheckBox;
    private readonly CheckBox _listeningCheckBox;

    private readonly TextBlock _statusText;
    private readonly LearningSettings _settings;

    // Initializes the settings page and its user interface.
    public SettingsPage()
    {
        _settings = LearningSettings.Load();

        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));

        var backButton = new Button
        {
            Content = "← Back",
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8")),
            Background = new SolidColorBrush(Color.Parse("#D8CCFF")),
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(18, 10),
            CornerRadius = new CornerRadius(16)
        };

        backButton.Click += BackButton_Click;

        var title = new TextBlock
        {
            Text = "⚙ Little Linguist",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var titleBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#6846C7")),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(20, 14),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = title
        };

        var decoration = new Grid
        {
            Height = 45
        };

        var star = new TextBlock
        {
            Text = "✦  ✧  ☆",
            FontSize = 27,
            Foreground = new SolidColorBrush(Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 0, 0, 0)
        };

        var cloud = new TextBlock
        {
            Text = "☁",
            FontSize = 40,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 35, 0)
        };

        decoration.Children.Add(star);
        decoration.Children.Add(cloud);

        _pronunciationCheckBox = new CheckBox
        {
            Content = "🗣Pronunciation",
            FontSize = 18,
            IsChecked = _settings.Pronunciation,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        _writingCheckBox = new CheckBox
        {
            Content = "✏️Writing",
            FontSize = 18,
            IsChecked = _settings.Writing,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        _readingCheckBox = new CheckBox
        {
            Content = "📖Reading comprehension",
            FontSize = 18,
            IsChecked = _settings.Reading,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        _listeningCheckBox = new CheckBox
        {
            Content = "👂Listening comprehension",
            FontSize = 18,
            IsChecked = _settings.Listening,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        var pronunciationPanel = CreateSkill(
            _pronunciationCheckBox,
            "Practise saying words clearly and confidently."
        );

        var writingPanel = CreateSkill(
            _writingCheckBox,
            "Trace and write words from the story."
        );

        var readingPanel = CreateSkill(
            _readingCheckBox,
            "Read short passages and discover their meaning."
        );

        var listeningPanel = CreateSkill(
            _listeningCheckBox,
            "Listen carefully and understand what happens in the story."
        );

        var skillsGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,*"),
            RowDefinitions = RowDefinitions.Parse("Auto,Auto"),
            ColumnSpacing = 40,
            RowSpacing = 25
        };

        AddToGrid(skillsGrid, pronunciationPanel, 0, 0);
        AddToGrid(skillsGrid, writingPanel, 0, 1);
        AddToGrid(skillsGrid, readingPanel, 1, 0);
        AddToGrid(skillsGrid, listeningPanel, 1, 1);

        var skillsSection = CreateSection(
            "🌟 Which skills do you want to strengthen?",
            skillsGrid
        );

        _statusText = new TextBlock
        {
            Text = "",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#55729A")),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        var startButton = new Button
        {
            Content = "Save configuration ✨",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#7956D8")),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(20, 14),
            CornerRadius = new CornerRadius(20)
        };

        startButton.Click += StartButton_Click;

        var bottomGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,*"),
            ColumnSpacing = 30
        };

        Grid.SetColumn(_statusText, 0);
        Grid.SetColumn(startButton, 1);

        bottomGrid.Children.Add(_statusText);
        bottomGrid.Children.Add(startButton);

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(35, 20),
            Spacing = 25,
            Children =
            {
                backButton,
                titleBorder,
                decoration,
                skillsSection,
                bottomGrid
            }
        };

        Content = new ScrollViewer
        {
            Content = mainPanel
        };
    }

    // Creates a bordered section with a title and content.
    private static StackPanel CreateSection(string title, Control content)
    {
        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock
                {
                    Text = title,
                    FontSize = 21,
                    FontWeight = FontWeight.Bold,
                    Foreground =
                        new SolidColorBrush(Color.Parse("#6846C7"))
                },

                new Border
                {
                    Background = Brushes.White,
                    BorderBrush =
                        new SolidColorBrush(Color.Parse("#C9BBF5")),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(18),
                    Padding = new Thickness(20),
                    Child = content
                }
            }
        };
    }

    // Creates a skill option with a checkbox and description.
    private static StackPanel CreateSkill(
        CheckBox checkBox,
        string description)
    {
        return new StackPanel
        {
            Spacing = 4,
            Children =
            {
                checkBox,
                new TextBlock
                {
                    Text = description,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground =
                        new SolidColorBrush(Color.Parse("#55729A"))
                }
            }
        };
    }

    // Adds a control to the specified grid position.
    private static void AddToGrid(
        Grid grid,
        Control control,
        int row,
        int column)
    {
        Grid.SetRow(control, row);
        Grid.SetColumn(control, column);
        grid.Children.Add(control);
    }

    /// Returns to the previous page.
    private async void BackButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }

    // Validates and saves the selected learning skills.
    private async void StartButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        bool hasSelectedSkill =
            _pronunciationCheckBox.IsChecked == true ||
            _writingCheckBox.IsChecked == true ||
            _readingCheckBox.IsChecked == true ||
            _listeningCheckBox.IsChecked == true;

        if (!hasSelectedSkill)
        {
            _statusText.Text =
                "⚠️ You must select at least one skill to continue!";

            return;
        }

        _settings.Pronunciation =
            _pronunciationCheckBox.IsChecked == true;

        _settings.Writing =
            _writingCheckBox.IsChecked == true;

        _settings.Reading =
            _readingCheckBox.IsChecked == true;

        _settings.Listening =
            _listeningCheckBox.IsChecked == true;

        _settings.Save();

        _statusText.Text =
            "🌟 Your learning configuration has been saved!";

        await Task.Delay(800);

        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }
}