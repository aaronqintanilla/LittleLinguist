/* using Avalonia;
using System;

namespace LittleLinguist;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}

*/
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using LittleLinguist.Controls;
using LittleLinguist.Pages;
using Vosk;
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

        // Force light theme
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

        // Fix touchscreen by disabling swipe to go back
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