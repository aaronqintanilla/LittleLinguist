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
    // Cada lista representa un trazo diferente.
    // Los puntos se guardan relativamente a la palabra.
    private readonly List<List<Point>> _strokes = new();

    private List<Point>? _currentStroke;
    private bool _isDrawing;

    // Palabra que debe repasarse.
    public string TargetWord { get; set; } = "APPLE";

    // Tamaño máximo de la palabra.
    public double GuideFontSize { get; set; } = 120;

    public TracingCanvas()
    {
        MinHeight = 250;
        ClipToBounds = true;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    // =========================================================
    // DIBUJAR EL CONTROL
    // =========================================================

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

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

        // Calculamos un tamaño de letra que quepa en el lienzo.
        double fontSize = GetFittedFontSize();

        FormattedText formattedWord =
            CreateFormattedText(TargetWord, fontSize);

        Point textOrigin =
            GetTextOrigin(formattedWord);

        // Dibujamos la palabra guía.
        context.DrawText(
            formattedWord,
            textOrigin
        );

        var inkBrush =
            new SolidColorBrush(Color.Parse("#872589"));

        // El grosor también se adapta al tamaño de la palabra.
        double inkThickness =
            Math.Clamp(fontSize * 0.07, 4, 10);

        var inkPen =
            new Pen(inkBrush, inkThickness);

        foreach (List<Point> stroke in _strokes)
        {
            // Convertimos los puntos relativos a posiciones reales.
            List<Point> absolutePoints = stroke
                .Select(point =>
                    ToAbsolute(
                        point,
                        formattedWord,
                        textOrigin
                    )
                )
                .ToList();

            if (absolutePoints.Count == 0)
            {
                continue;
            }

            // Si solo hay un punto, dibujamos un pequeño círculo.
            if (absolutePoints.Count == 1)
            {
                double radius = inkThickness / 2;

                context.DrawEllipse(
                    inkBrush,
                    null,
                    absolutePoints[0],
                    radius,
                    radius
                );

                continue;
            }

            // Unimos los puntos consecutivos.
            for (int i = 1; i < absolutePoints.Count; ++i)
            {
                context.DrawLine(
                    inkPen,
                    absolutePoints[i - 1],
                    absolutePoints[i]
                );
            }
        }
    }

    // =========================================================
    // COORDENADAS RELATIVAS
    // =========================================================

    // Convierte una posición real en una posición relativa
    // respecto a la palabra.
    private static Point ToRelative(
        Point absolutePoint,
        FormattedText formattedWord,
        Point wordOrigin)
    {
        double wordWidth =
            Math.Max(1, formattedWord.Width);

        double wordHeight =
            Math.Max(1, formattedWord.Height);

        return new Point(
            (absolutePoint.X - wordOrigin.X) / wordWidth,
            (absolutePoint.Y - wordOrigin.Y) / wordHeight
        );
    }

    // Convierte una posición relativa en una posición real.
    private static Point ToAbsolute(
        Point relativePoint,
        FormattedText formattedWord,
        Point wordOrigin)
    {
        return new Point(
            wordOrigin.X +
            relativePoint.X * formattedWord.Width,

            wordOrigin.Y +
            relativePoint.Y * formattedWord.Height
        );
    }

    // =========================================================
    // EVENTOS DEL RATÓN, DEDO O LÁPIZ
    // =========================================================

    private void OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        double fontSize = GetFittedFontSize();

        FormattedText formattedWord =
            CreateFormattedText(TargetWord, fontSize);

        Point wordOrigin =
            GetTextOrigin(formattedWord);

        Point absolutePoint =
            e.GetPosition(this);

        // Creamos un nuevo trazo.
        _currentStroke = new List<Point>
        {
            ToRelative(
                absolutePoint,
                formattedWord,
                wordOrigin
            )
        };

        _strokes.Add(_currentStroke);
        _isDrawing = true;

        // Seguimos recibiendo eventos aunque el puntero
        // se desplace fuera del control.
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

        double fontSize = GetFittedFontSize();

        FormattedText formattedWord =
            CreateFormattedText(TargetWord, fontSize);

        Point wordOrigin =
            GetTextOrigin(formattedWord);

        Point newAbsolutePoint =
            e.GetPosition(this);

        // Convertimos el punto anterior a coordenadas reales.
        Point previousAbsolutePoint = ToAbsolute(
            _currentStroke[^1],
            formattedWord,
            wordOrigin
        );

        double differenceX =
            newAbsolutePoint.X - previousAbsolutePoint.X;

        double differenceY =
            newAbsolutePoint.Y - previousAbsolutePoint.Y;

        // Evitamos almacenar puntos prácticamente iguales.
        if (
            differenceX * differenceX +
            differenceY * differenceY >= 1
        )
        {
            _currentStroke.Add(
                ToRelative(
                    newAbsolutePoint,
                    formattedWord,
                    wordOrigin
                )
            );

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

    // =========================================================
    // BORRAR
    // =========================================================

    public void Clear()
    {
        _strokes.Clear();
        _currentStroke = null;
        _isDrawing = false;

        InvalidateVisual();
    }

    // =========================================================
    // CALCULAR LA PUNTUACIÓN
    // =========================================================

    public double CalculateScore()
    {
        if (
            string.IsNullOrWhiteSpace(TargetWord) ||
            Bounds.Width <= 0 ||
            Bounds.Height <= 0
        )
        {
            return 0;
        }

        double fontSize = GetFittedFontSize();

        FormattedText formattedWord =
            CreateFormattedText(TargetWord, fontSize);

        Point wordOrigin =
            GetTextOrigin(formattedWord);

        // Convertimos todos los puntos relativos
        // a sus posiciones reales actuales.
        List<Point> userPoints = _strokes
            .SelectMany(stroke => stroke)
            .Select(point =>
                ToAbsolute(
                    point,
                    formattedWord,
                    wordOrigin
                )
            )
            .ToList();

        // No se ha escrito suficiente.
        if (userPoints.Count < 20)
        {
            return 0;
        }

        // Convertimos la palabra completa en una geometría.
        Geometry? wordGeometry =
            formattedWord.BuildGeometry(wordOrigin);

        if (wordGeometry is null)
        {
            return 0;
        }

        // La tolerancia se adapta al tamaño de las letras.
        double toleranceWidth =
            Math.Max(6, fontSize * 0.16);

        var tolerancePen =
            new Pen(Brushes.Black, toleranceWidth);

        // Contamos los puntos que están dentro
        // o cerca de las letras.
        int validPointCount = userPoints.Count(point =>
            wordGeometry.FillContains(point) ||
            wordGeometry.StrokeContains(
                tolerancePen,
                point
            )
        );

        double precision =
            validPointCount / (double)userPoints.Count;

        // -----------------------------------------------------
        // COBERTURA DE LAS LETRAS
        // -----------------------------------------------------

        double totalLetterCoverage = 0;
        int evaluatedLetters = 0;

        // Distancia máxima para considerar cubierta una zona.
        double coverageRadius =
            Math.Max(6, fontSize * 0.15);

        for (
            int letterIndex = 0;
            letterIndex < TargetWord.Length;
            ++letterIndex
        )
        {
            string letter =
                TargetWord[letterIndex].ToString();

            FormattedText formattedLetter =
                CreateFormattedText(letter, fontSize);

            Point letterOrigin = new Point(
                wordOrigin.X +
                MeasurePrefixWidth(
                    letterIndex,
                    fontSize
                ),
                wordOrigin.Y
            );

            Geometry? letterGeometry =
                formattedLetter.BuildGeometry(letterOrigin);

            if (letterGeometry is null)
            {
                continue;
            }

            Rect bounds =
                letterGeometry.Bounds;

            // Cuadrícula invisible de cada letra.
            const int rows = 8;
            const int columns = 6;

            int targetCells = 0;
            int coveredCells = 0;

            for (int row = 0; row < rows; ++row)
            {
                for (
                    int column = 0;
                    column < columns;
                    ++column
                )
                {
                    // Centro de la casilla actual.
                    Point samplePoint = new Point(
                        bounds.X +
                        (column + 0.5) *
                        bounds.Width / columns,

                        bounds.Y +
                        (row + 0.5) *
                        bounds.Height / rows
                    );

                    // Ignoramos las casillas que no forman
                    // parte de la letra.
                    if (
                        !letterGeometry.FillContains(
                            samplePoint
                        )
                    )
                    {
                        continue;
                    }

                    ++targetCells;

                    // Miramos si el niño ha dibujado
                    // cerca de esta zona.
                    bool isCovered = userPoints.Any(point =>
                        DistanceSquared(
                            point,
                            samplePoint
                        )
                        <=
                        coverageRadius * coverageRadius
                    );

                    if (isCovered)
                    {
                        ++coveredCells;
                    }
                }
            }

            if (targetCells > 0)
            {
                double currentLetterCoverage =
                    coveredCells /
                    (double)targetCells;

                totalLetterCoverage +=
                    currentLetterCoverage;

                ++evaluatedLetters;
            }
        }

        double averageLetterCoverage =
            evaluatedLetters == 0
                ? 0
                : totalLetterCoverage /
                  evaluatedLetters;

        // Debe estar cerca de las letras y cubrirlas.
        double score =
            precision * averageLetterCoverage;

        return Math.Clamp(score, 0, 1);
    }

    // =========================================================
    // CREACIÓN Y ADAPTACIÓN DEL TEXTO
    // =========================================================

    private FormattedText CreateFormattedText(
        string text,
        double fontSize)
    {
        var typeface = new Typeface(
            FontFamily.Default,
            FontStyle.Normal,
            FontWeight.Bold,
            FontStretch.Normal
        );

        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            new SolidColorBrush(
                Color.Parse("#D5D5D5")
            )
        );
    }

    // Calcula el tamaño máximo que cabe dentro del lienzo.
    private double GetFittedFontSize()
    {
        if (string.IsNullOrWhiteSpace(TargetWord))
        {
            return GuideFontSize;
        }

        double maximumFontSize =
            Math.Max(1, GuideFontSize);

        // Medimos primero la palabra con el tamaño máximo.
        FormattedText maximumText =
            CreateFormattedText(
                TargetWord,
                maximumFontSize
            );

        // Dejamos 20 píxeles de margen en cada lado.
        double availableWidth =
            Math.Max(1, Bounds.Width - 40);

        double availableHeight =
            Math.Max(1, Bounds.Height - 40);

        double widthScale =
            availableWidth /
            Math.Max(1, maximumText.Width);

        double heightScale =
            availableHeight /
            Math.Max(1, maximumText.Height);

        // Elegimos la reducción más restrictiva.
        double scale = Math.Min(
            1,
            Math.Min(widthScale, heightScale)
        );

        return Math.Max(
            1,
            maximumFontSize * scale
        );
    }

    // Centra la palabra en el lienzo.
    private Point GetTextOrigin(
        FormattedText text)
    {
        return new Point(
            Math.Max(
                10,
                (Bounds.Width - text.Width) / 2
            ),
            Math.Max(
                10,
                (Bounds.Height - text.Height) / 2
            )
        );
    }

    // Mide cuánto ocupan las letras anteriores.
    private double MeasurePrefixWidth(
        int characterCount,
        double fontSize)
    {
        if (characterCount <= 0)
        {
            return 0;
        }

        string prefix =
            TargetWord[..characterCount];

        FormattedText formattedPrefix =
            CreateFormattedText(
                prefix,
                fontSize
            );

        return formattedPrefix.Width;
    }

    private static double DistanceSquared(
        Point first,
        Point second)
    {
        double differenceX =
            first.X - second.X;

        double differenceY =
            first.Y - second.Y;

        return
            differenceX * differenceX +
            differenceY * differenceY;
    }
}