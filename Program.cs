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
using Avalonia.Themes.Fluent;
using LittleLinguist.Controls;
using LittleLinguist.Pages;
using HomePage = LittleLinguist.Pages.HomePage;

/*
FALTA:
1. quitar el comentario de arriba
2. comentar las clases y los métodos de "todos" los archivos
*/

class Program
{
    static Window? window;

    public static void Main(string[] args)
    {
        AppBuilder.Configure<Application>()
                  .UsePlatformDetect()
                  .Start(AppMain, args);
    }

    static async void AppMain(Application app, string[] args)
    {
        app.Styles.Add(new FluentTheme());

        window = new Window
        {
            Title = "Little Linguist",
            Width = 800,
            Height = 600,
            Cursor = new Cursor(StandardCursorType.None)
        };

        window.AddHandler(
            InputElement.PointerMovedEvent,
            OnPointerMoved,
            Avalonia.Interactivity.RoutingStrategies.Bubble | Avalonia.Interactivity.RoutingStrategies.Tunnel
        );

        var homePage = new HomePage();
        var navigationPage = new NavigationPage();
        navigationPage.Content = homePage;
        foreach (SwipeGestureRecognizer gr in navigationPage.GestureRecognizers.Cast<SwipeGestureRecognizer>())
        {
            gr.CanHorizontallySwipe = false;
        }
        window.Content = navigationPage;
        await StoryGenerator.Instance.LoadModel();

        window.Closed += (_, _) =>
        {
            StoryGenerator.Instance.StopStory();
            SpeechReader.Instance.Dispose();
        };

        window.Show();
        app.Run(window);
    }
    
    private static void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if(e.Pointer.Type == PointerType.Mouse)
        {
            if (window is null) return;
           // window.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }
}