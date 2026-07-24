using Avalonia;
using System;

namespace LittleLinguist;

sealed class Program
{
    // Código de inicialización. No uses APIs de Avalonia antes de llamar a AppMain.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Configuración de Avalonia (utilizada también por el previsualizador visual)
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}