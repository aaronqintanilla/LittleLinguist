using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace LittleLinguist.Controls;

public class TracingCanvas : Control
{
    // Todos los trazos realizados.
    // Cada elemento de la lista representa un trazo distinto.
    private readonly List<List<Point>> _strokes = new();

    // Trazo que el usuario está dibujando actualmente.
    private List<Point>? _currentStroke;

    private bool _isDrawing;

    // Palabra que se debe repasar.
    public string TargetWord { get; set; } = "APPLE"; 

    public double GuideFontSize { get; set; } = 120;

    public TracingCanvas()
    {
        MinHeight = 250;
        ClipToBounds = true;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    // ---------------------------------------------------------
    // DIBUJO DEL CONTROL
    // ---------------------------------------------------------

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var drawingArea = new Rect(Bounds.Size);

        // Fondo y borde de la zona de escritura.
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

        // Palabra gris que el niño debe repasar.
        var formattedWord = CreateFormattedWord();
        var textOrigin = GetTextOrigin(formattedWord);

        context.DrawText(formattedWord, textOrigin);

        // Trazos dibujados por el usuario.
        var inkBrush =
            new SolidColorBrush(Color.Parse("#872589"));

        var inkPen = new Pen(inkBrush, 8);

        foreach (var stroke in _strokes)
        {
            // Un único punto.
            if (stroke.Count == 1)
            {
                context.DrawEllipse(
                    inkBrush,
                    null,
                    stroke[0],
                    4,
                    4
                );

                continue;
            }

            // Unimos los puntos consecutivos.
            for (int i = 1; i < stroke.Count; ++i)
            {
                context.DrawLine(
                    inkPen,
                    stroke[i - 1],
                    stroke[i]
                );
            }
        }
    }

    // ---------------------------------------------------------
    // EVENTOS TÁCTILES
    // ---------------------------------------------------------

    private void OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        _isDrawing = true;

        _currentStroke = new List<Point>
        {
            e.GetPosition(this)
        };

        _strokes.Add(_currentStroke);

        // Seguimos recibiendo movimientos aunque el dedo
        // se desplace ligeramente fuera del control.
        e.Pointer.Capture(this);

        e.Handled = true;

        InvalidateVisual();
    }

    private void OnPointerMoved(
        object? sender,
        PointerEventArgs e)
    {
        if (!_isDrawing || _currentStroke is null)
        {
            return;
        }

        Point newPoint = e.GetPosition(this);
        Point previousPoint = _currentStroke[^1]; // Obtiene el último punto de la lista

        double differenceX = newPoint.X - previousPoint.X;
        double differenceY = newPoint.Y - previousPoint.Y;

        // Evitamos guardar demasiados puntos prácticamente iguales.
        if (
            differenceX * differenceX +
            differenceY * differenceY >= 1
        )
        {
            _currentStroke.Add(newPoint);
            InvalidateVisual();
        }

        e.Handled = true;
    }

    private void OnPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        _isDrawing = false;
        _currentStroke = null;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    // ---------------------------------------------------------
    // BORRAR
    // ---------------------------------------------------------

    public void Clear()
    {
        _strokes.Clear();
        _currentStroke = null;
        _isDrawing = false;

        InvalidateVisual();
    }

    // ---------------------------------------------------------
    // EVALUAR EL TRAZADO
    // ---------------------------------------------------------

    public double CalculateScore()  // REVISAR COMO HACERLO PARA QUE REPRESENTE
                                    // CORRECTAMENTE EL TRAZADO DE LA PALABRA
    {
        List<Point> userPoints =
            _strokes.SelectMany(stroke => stroke).ToList();

        // No se ha escrito suficiente.
        if (userPoints.Count < 20)
        {
            return 0;
        }

        FormattedText formattedWord = CreateFormattedWord();
        Point origin = GetTextOrigin(formattedWord);

        // Forma geométrica exacta de las letras.
        Geometry wordGeometry =
            formattedWord.BuildGeometry(origin);

        // Permitimos una pequeña desviación alrededor de la letra.
        var tolerancePen = new Pen(Brushes.Black, 20);

        List<Point> validPoints = userPoints
            .Where(point =>
                wordGeometry.FillContains(point) ||
                wordGeometry.StrokeContains(
                    tolerancePen,
                    point
                )
            )
            .ToList();

        // Porcentaje de puntos que están encima o cerca de las letras.
        double precision =
            validPoints.Count / (double)userPoints.Count;

        // Comprobamos que haya trazos en todas las letras.
        int coveredLetters = 0;

        for (int i = 0; i < TargetWord.Length; ++i)
        {
            double letterStart =
                origin.X + MeasurePrefixWidth(i);

            double letterEnd =
                origin.X + MeasurePrefixWidth(i + 1);

            Rect letterArea = new Rect(
                letterStart,
                wordGeometry.Bounds.Y,
                Math.Max(1, letterEnd - letterStart),
                wordGeometry.Bounds.Height
            );

            int pointsInLetter =
                validPoints.Count(letterArea.Contains);

            if (pointsInLetter >= 4)
            {
                ++coveredLetters;
            }
        }

        double letterCoverage =
            coveredLetters / (double)TargetWord.Length;

        /*
         * 70 %: los puntos siguen la forma de las letras.
         * 30 %: se han repasado todas las letras.
         */
        double score =
            0.50 * precision +
            0.50 * letterCoverage;

        return Math.Clamp(score, 0, 1);
    }

    // ---------------------------------------------------------
    // CREACIÓN DEL TEXTO
    // ---------------------------------------------------------

    private FormattedText CreateFormattedWord()
    {
        var typeface = new Typeface(
            FontFamily.Default,
            FontStyle.Normal,
            FontWeight.Bold,
            FontStretch.Normal
        );

        return new FormattedText(
            TargetWord,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            GuideFontSize,
            new SolidColorBrush(
                Color.Parse("#D5D5D5")
            )
        );
    }

    private Point GetTextOrigin(FormattedText text)
    {
        return new Point(
            Math.Max(10, (Bounds.Width - text.Width) / 2),
            Math.Max(10, (Bounds.Height - text.Height) / 2)
        );
    }

    private double MeasurePrefixWidth(int characterCount)
    {
        if (characterCount <= 0)
        {
            return 0;
        }

        string prefix =
            TargetWord[..characterCount];

        var typeface = new Typeface(
            FontFamily.Default,
            FontStyle.Normal,
            FontWeight.Bold,
            FontStretch.Normal
        );

        var formattedPrefix = new FormattedText(
            prefix,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            GuideFontSize,
            Brushes.Transparent
        );

        return formattedPrefix.Width;
    }
}