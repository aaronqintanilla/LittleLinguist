using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using HomePage = LittleLinguist.Pages.HomePage;

class Program
{
    [STAThread]

    // Configures and starts the Avalonia application.
    public static void Main(string[] args)
    {
        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .Start(AppMain, args);
    }

    // Creates the main window and starts the application services.
    private static void AppMain(
        Application app,
        string[] args)
    {
        app.Styles.Add(new FluentTheme());

        app.RequestedThemeVariant = ThemeVariant.Light;

        var window = new Window
        {
            Title = "Little Linguist",
            Width = 800,
            Height = 600,
        };

        var homePage = new HomePage();
        var navigationPage = new NavigationPage();
        navigationPage.Content = homePage;

        foreach (SwipeGestureRecognizer gr in navigationPage.GestureRecognizers.Cast<SwipeGestureRecognizer>())
        {
            gr.CanHorizontallySwipe = false;
        }
        window.Content = navigationPage;

        window.Closed += (_, _) =>
        {
            StoryGenerator.Instance.StopStory();
            VisionEngine.Instance.Dispose();
            SpeechReader.Instance.Dispose();
        };

        window.Show();

        _ = LoadModelsAsync();

        app.Run(window);
    }

    // Loads the story and vision models in the background.
    private static async Task LoadModelsAsync()
    {
        try
        {
            
            await StoryGenerator.Instance.LoadModel();
            await VisionEngine.Instance.LoadModel();

            Console.WriteLine(
                "Vision model completely loaded."
            );
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                "VISION MODEL ERROR:"
            );

            Console.Error.WriteLine(
                ex.ToString()
            );
        }
    }
}