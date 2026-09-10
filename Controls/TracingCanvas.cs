using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace LittleLinguist.Controls;

// Handles the drawing canvas and evaluates how accurately the word is traced.
public class TracingCanvas : Control
{
    // Each list represents a different stroke.
    // Points are stored relative to the word.
    private readonly List<List<Point>> _strokes = new();
    private List<Point>? _currentStroke;
    private bool _isDrawing;
    public string TargetWord { get; set; } = "APPLE";
    public double GuideFontSize { get; set; } = 120;

    // Initializes the tracing canvas and pointer event handlers.
    public TracingCanvas()
    {
        MinHeight = 250;
        ClipToBounds = true;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
    }

    // Draws the guide word and the user's strokes on the canvas.
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var drawingArea = new Rect(Bounds.Size);

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

        double fontSize = GetFittedFontSize();

        FormattedText formattedWord =
            CreateFormattedText(TargetWord, fontSize);

        Point textOrigin =
            GetTextOrigin(formattedWord);

        context.DrawText(
            formattedWord,
            textOrigin
        );

        var inkBrush =
            new SolidColorBrush(Color.Parse("#872589"));

        double inkThickness =
            Math.Clamp(fontSize * 0.07, 4, 10);

        var inkPen =
            new Pen(inkBrush, inkThickness);

        foreach (List<Point> stroke in _strokes)
        {
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

    // Converts an absolute position into coordinates relative to the word.
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

    // Converts a relative position back into absolute canvas coordinates.
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

    // Starts a new stroke when the user presses the pointer.
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

        e.Pointer.Capture(this);

        e.Handled = true;

        InvalidateVisual();
    }

    // Starts a new stroke when the user presses the pointer.
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

        Point previousAbsolutePoint = ToAbsolute(
            _currentStroke[^1],
            formattedWord,
            wordOrigin
        );

        double differenceX =
            newAbsolutePoint.X - previousAbsolutePoint.X;

        double differenceY =
            newAbsolutePoint.Y - previousAbsolutePoint.Y;

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

    // Adds points to the current stroke while the user draws.
    private void OnPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        _isDrawing = false;
        _currentStroke = null;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    // Ends the current stroke when the user releases the pointer.
    public void Clear()
    {
        _strokes.Clear();
        _currentStroke = null;
        _isDrawing = false;

        InvalidateVisual();
    }

    // Clears all user strokes from the canvas.
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

        // We convert all relative points
        // to their current absolute positions.
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

        if (userPoints.Count < 20)
        {
            return 0;
        }

        // We convert the entire word into a geometry.
        Geometry? wordGeometry =
            formattedWord.BuildGeometry(wordOrigin);

        if (wordGeometry is null)
        {
            return 0;
        }

        double toleranceWidth =
            Math.Max(6, fontSize * 0.16);

        var tolerancePen =
            new Pen(Brushes.Black, toleranceWidth);

        int validPointCount = userPoints.Count(point =>
            wordGeometry.FillContains(point) ||
            wordGeometry.StrokeContains(
                tolerancePen,
                point
            )
        );

        double precision =
            validPointCount / (double)userPoints.Count;

        double totalLetterCoverage = 0;
        int evaluatedLetters = 0;

        // Maximum distance for considering an area covered.
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
                    Point samplePoint = new Point(
                        bounds.X +
                        (column + 0.5) *
                        bounds.Width / columns,

                        bounds.Y +
                        (row + 0.5) *
                        bounds.Height / rows
                    );

                    if (
                        !letterGeometry.FillContains(
                            samplePoint
                        )
                    )
                    {
                        continue;
                    }

                    ++targetCells;

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

        double score =
            precision * averageLetterCoverage;

        return Math.Clamp(score, 0, 1);
    }

    // Creates the formatted text used to display and measure the word.
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

    // Calculates the largest font size that fits inside the canvas.
    private double GetFittedFontSize()
    {
        if (string.IsNullOrWhiteSpace(TargetWord))
        {
            return GuideFontSize;
        }

        double maximumFontSize =
            Math.Max(1, GuideFontSize);

        FormattedText maximumText =
            CreateFormattedText(
                TargetWord,
                maximumFontSize
            );

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

        double scale = Math.Min(
            1,
            Math.Min(widthScale, heightScale)
        );

        return Math.Max(
            1,
            maximumFontSize * scale
        );
    }

    // Calculates the position needed to center the word on the canvas.
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

    // Measures the width of the characters before a given position.
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

    // Calculates the squared distance between two points.
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