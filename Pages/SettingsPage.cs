using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace LittleLinguist.Pages;

/*
FALTA:
1. guardar els settings en un fitxer json
2. que no es pugui començar amb 0 competencies (que hace back vs que hace guardar, etc)
3. cambiar el diseño de la interfaz
*/

public class SettingsPage : ContentPage
{
    // Guardamos las casillas porque necesitaremos consultar su valor.
    private readonly CheckBox _pronunciationCheckBox;
    private readonly CheckBox _writingCheckBox;
    private readonly CheckBox _readingCheckBox;
    private readonly CheckBox _listeningCheckBox;

    private readonly TextBlock _statusText;

    public SettingsPage()
    {
        // ---------------------------------------------------------
        // FONDO
        // ---------------------------------------------------------

        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));

        // ---------------------------------------------------------
        // BOTÓN BACK
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // DECORACIÓN
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // MODO DE APRENDIZAJE
        // ---------------------------------------------------------

        var defaultStory = new RadioButton
        {
            Content = "Default story",
            GroupName = "LearningMode",
            IsChecked = true,
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        var createStory = new RadioButton
        {
            Content = "Create your own story",
            GroupName = "LearningMode",
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        var defaultStoryPanel = new StackPanel
        {
            Spacing = 5,
            Children =
            {
                defaultStory,
                new TextBlock
                {
                    Text = "Start a ready-made adventure created just for you.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.Parse("#55729A"))
                }
            }
        };

        var createStoryPanel = new StackPanel
        {
            Spacing = 5,
            Children =
            {
                createStory,
                new TextBlock
                {
                    Text = "Show an object and make it part of your adventure.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.Parse("#55729A"))
                }
            }
        };

        var modeGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,*"),
            ColumnSpacing = 40
        };

        Grid.SetColumn(defaultStoryPanel, 0);
        Grid.SetColumn(createStoryPanel, 1);

        modeGrid.Children.Add(defaultStoryPanel);
        modeGrid.Children.Add(createStoryPanel);

        var modeSection = CreateSection(
            "📚 Select your learning mode:",
            modeGrid
        );

        // ---------------------------------------------------------
        // HABILIDADES
        // ---------------------------------------------------------

        _pronunciationCheckBox = new CheckBox
        {
            Content = "🗣Pronunciation",
            FontSize = 18,
            IsChecked = true,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        _writingCheckBox = new CheckBox
        {
            Content = "✏️Writing",
            FontSize = 18,
            IsChecked = true,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        _readingCheckBox = new CheckBox
        {
            Content = "📖Reading comprehension",
            FontSize = 18,
            IsChecked = true,
            Foreground = new SolidColorBrush(Color.Parse("#5940A8"))
        };

        _listeningCheckBox = new CheckBox
        {
            Content = "👂Listening comprehension",
            FontSize = 18,
            IsChecked = true,
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

        // ---------------------------------------------------------
        // MENSAJE Y BOTÓN START
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // CONTENIDO COMPLETO
        // ---------------------------------------------------------

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(35, 20),
            Spacing = 25,
            Children =
            {
                backButton,
                titleBorder,
                decoration,
                modeSection,
                skillsSection,
                bottomGrid
            }
        };

        Content = new ScrollViewer
        {
            Content = mainPanel
        };
    }

    // Crea un recuadro con título.
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

    // Crea una habilidad con casilla y descripción.
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

    // Vuelve a la página anterior.
    private async void BackButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }

    // Comprueba que se haya seleccionado alguna habilidad.
    private void StartButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        bool hasSelectedSkill =
            _pronunciationCheckBox.IsChecked == true ||
            _writingCheckBox.IsChecked == true ||
            _readingCheckBox.IsChecked == true ||
            _listeningCheckBox.IsChecked == true;

        if (hasSelectedSkill)
        {
            _statusText.Text =
                "🌟 Your learning adventure is ready!";
        }
        else
        {
            _statusText.Text =
                "You must select at least one skill to improve!";
        }
    }
}