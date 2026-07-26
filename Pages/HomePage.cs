using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace LittleLinguist.Pages;

/*
FALTA:
1. cambiar el botón start para que depende de las competencias seleccionadas vaya a una página u otra 
2. cambiar el diseño de la interfaz
*/

public class HomePage : ContentPage
{
    private SettingsPage? settingsPage;
    private StoryPage? storyPage;

    public HomePage()
    {
        // Fondo de la página.
        // Como el tema por defecto es el del ordenador, esto puede hacer que se vea raro si esta en modo oscuro
        // Habria que definir el estilo de la aplicacion en Program.cs
        //Background = new SolidColorBrush(Color.Parse("#FFF7FC"));

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
            TextWrapping = TextWrapping.Wrap
        };

        // ---------------------------------------------------------
        // BOTÓN START
        // ---------------------------------------------------------

        var startButton = new Button
        {
            Content = "Start your adventure!",
            FontSize = 20,
            Padding = new Thickness(30, 14),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        startButton.Click += StartButton_Click;

        // Panel que coloca título, subtítulo y botón verticalmente.
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
                startButton
            }
        };

        // Grid principal.
        var mainGrid = new Grid
        {
            Margin = new Thickness(25)
        };

        mainGrid.Children.Add(settingsButton);
        mainGrid.Children.Add(centerPanel);

        // Mostramos el Grid como contenido de HomePage.
        Content = mainGrid;
    }

    private async void SettingsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if(settingsPage == null)
        {
            settingsPage = new SettingsPage();
        }

        if (Navigation is not null)
        {
            await Navigation.PushAsync(settingsPage);

            System.Console.WriteLine("Settings button clicked");
        }
    }

    private async void StartButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
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
}