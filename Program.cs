using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using HomePage = LittleLinguist.Pages.HomePage;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .Start(AppMain, args);
    }

    // Este método ya no es async.
    private static void AppMain(
        Application app,
        string[] args)
    {
        app.Styles.Add(new FluentTheme());

        var window = new Window
        {
            Title = "Little Linguist",
            Width = 800,
            Height = 600
        };

        var navigationPage = new NavigationPage
        {
            Content = new HomePage()
        };

        window.Content = navigationPage;

        window.Closed += (_, _) =>
        {
            StoryGenerator.Instance.StopStory();
            VisionEngine.Instance.Dispose();
            SpeechReader.Instance.Dispose();
        };

        // Primero mostramos la ventana.
        window.Show();

        // Empezamos la carga sin detener el arranque de Avalonia.
        _ = LoadModelsAsync();

        // Iniciamos el bucle de la aplicación.
        app.Run(window);
    }

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