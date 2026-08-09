using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Controls;
using System.Threading.Tasks;
using System;

namespace LittleLinguist.Pages;

/*
FALTA:
1. al hacer finish (añadir boton para cerrar sesion de aprendizaje), que vuelva a HomePage y se reinicie todo
2. cambiar el diseño ajustandolo al resto
*/

public class TouchscreenPage : ContentPage
{
    private readonly Touch touch;

    public TouchscreenPage()
    {
        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "Touchscreen",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // ZONA TÁCTIL
        // ---------------------------------------------------------

        /*_tracingCanvas = new TracingCanvas
        {
            TargetWord = word.ToUpperInvariant(),

            // Ya no tiene una altura fija.
            // Ocupará el espacio restante de la ventana.
            MinHeight = 160,

            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };*/

        touch = new Touch
        {
            MinHeight = 160,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        // ---------------------------------------------------------
        // GRID PRINCIPAL
        // ---------------------------------------------------------

        var mainGrid = new Grid
        {
            Margin = new Thickness(30),

            // Título: altura automática.
            // Instrucciones: altura automática.
            // Canvas: ocupa todo el espacio restante.
            // Mensaje: altura automática.
            // Botones: altura automática.
            RowDefinitions =
                RowDefinitions.Parse("Auto,Auto,*,Auto,Auto"),

            RowSpacing = 20
        };

        Grid.SetRow(title, 0);
        Grid.SetRow(touch, 2);

        mainGrid.Children.Add(title);
        mainGrid.Children.Add(touch);

        // No utilizamos ScrollViewer.
        Content = mainGrid;
    }
}