using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace LittleLinguist.Controls;

public class Touch : Control
{
    public Touch()
    {
        MinHeight = 250;
        ClipToBounds = true;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var drawingArea = new Rect(Bounds.Size);

        // Fondo blanco y borde rosa.
        context.DrawRectangle(
            Brushes.White,
            new Pen(
                new SolidColorBrush(Color.Parse("#D87DDE")),
                2
            ),
            drawingArea,
            15,
            15
        );
    }

    private void OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {

        var position = e.GetPosition(this);

        Console.WriteLine(
            $"PRESS | Type={e.Pointer.Type} | " +
            $"Id={e.Pointer.Id} | " +
            $"Position=({position.X:F1}, {position.Y:F1})");

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }
    }

    private void OnPointerMoved(
        object? sender,
        PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        Console.WriteLine(
        $"MOVE | Type={e.Pointer.Type} | " +
        $"Id={e.Pointer.Id} | " +
        $"Position=({position.X:F1}, {position.Y:F1})");
    }

    private void OnPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        Console.WriteLine(
            $"RELEASE | Type={e.Pointer.Type} | " +
            $"Id={e.Pointer.Id}");
    }
}