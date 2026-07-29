using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace LittleLinguist.Pages;

/*
FALTA:
1. que cambie la pagina hacia la que se mueve según las competencias
2. cambiar el diseño de la interfaz
*/

public class HomePage : ContentPage
{
    private SettingsPage? settingsPage;
    private StoryPage? storyPage;

    public HomePage()
    {
        //Background =
            //new SolidColorBrush(Color.Parse("#FFF7FC"));

        // ---------------------------------------------------------
        // BOTÓN SETTINGS
        // ---------------------------------------------------------

        var settingsButton = new Button
        {
            Content = "⚙ Settings",
            FontSize = 16,
            Padding = new Thickness(18, 10),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };

        settingsButton.Click += SettingsButton_Click;

        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "Little Linguist",
            FontSize = 38,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // SUBTÍTULO
        // ---------------------------------------------------------

        var subtitle = new TextBlock
        {
            Text = "Learn languages through a new adventure!",
            FontSize = 18,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // BOTÓN START
        // ---------------------------------------------------------

        // var startButton = new Button
        // {
        //     Content = "Start your adventure!",
        //     FontSize = 20,
        //     Padding = new Thickness(30, 14),
        //     HorizontalAlignment = HorizontalAlignment.Center
        // };

        // startButton.Click += StartButton_Click;

        // ---------------------------------------------------------
// BOTÓN STORY
// ---------------------------------------------------------

    var storyButton = new Button
    {
        Content = "Create a story",
        FontSize = 20,
        Padding = new Thickness(30, 14),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center
    };

    storyButton.Click += StoryButton_Click;

        // ---------------------------------------------------------
        // PANEL CENTRAL
        // ---------------------------------------------------------

        var centerPanel = new StackPanel
        {
            Width = 450,
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                title,
                subtitle,
                storyButton
            }
        };

        // ---------------------------------------------------------
        // GRID PRINCIPAL
        // ---------------------------------------------------------

        var mainGrid = new Grid
        {
            Margin = new Thickness(25)
        };

        mainGrid.Children.Add(settingsButton);
        mainGrid.Children.Add(centerPanel);

        Content = mainGrid;
    }

    // Abre SettingsPage.
    private async void SettingsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PushAsync(
                new SettingsPage()
            );
        }
    }

    // Abre la página para practicar escritura.
    // private async void StartButton_Click(
    //     object? sender,
    //     RoutedEventArgs e)
    // {
    //     if (Navigation is not null)
    //     {
    //         await Navigation.PushAsync(
    //             new WritingPage("APPLE")
    //         );
    // }

  private async void StoryButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (Navigation is null)
        {
            return;
        }

        if(storyPage == null)
            {
                storyPage = new StoryPage();
                storyPage.StartStory();
            }

            if (Navigation is not null)
            {
                // De momento también abre SettingsPage.
                await Navigation.PushAsync(storyPage);
            }
    }

    

    private async void WritingButton_Click(
        object? sender,
        RoutedEventArgs e)
    {

        if (Navigation is not null)
        {
            await Navigation.PushAsync(
                new WritingPage("APPLE")
            );
        }
    }
}