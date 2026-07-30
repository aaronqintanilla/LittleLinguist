using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LittleLinguist.Controls;
using OpenCvSharp;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LittleLinguist.Pages;

public class CameraPage : ContentPage
{
    private readonly Image _cameraPreview;
    private readonly Image _photo;
    private CancellationTokenSource? _cameraCancellation;
    private Task? _cameraTask;
    private Button retakeButton;
    private Button continueButton;
    private bool _photoTaken;
    public CameraPage()
    {
        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------
        _photoTaken = false;
        _photo = new Image
        {
            Width = 480,
            Height = 300,
            Stretch = Stretch.UniformToFill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var title = new TextBlock
        {
            Text = "Take a picture!",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // INSTRUCCIONES
        // ---------------------------------------------------------

        // var instructions = new TextBlock
        // {
        //     Text = "Press the button below to take a picture!",
        //     FontSize = 18,
        //     TextAlignment = TextAlignment.Center,
        //     TextWrapping = TextWrapping.Wrap,
        //     HorizontalAlignment = HorizontalAlignment.Center
        // };

        // ---------------------------------------------------------
        // BOTÓN CLEAR
        // ---------------------------------------------------------

        // var clearButton = new Button
        // {
        //     Content = "Clear",
        //     FontSize = 18,
        //     Padding = new Thickness(25, 12),
        //     HorizontalAlignment = HorizontalAlignment.Stretch,
        //     HorizontalContentAlignment = HorizontalAlignment.Center
        // };

        // clearButton.Click += ClearButton_Click;

        // ---------------------------------------------------------
        // BOTÓN CONTINUE
        // ---------------------------------------------------------

        continueButton = new Button
        {
            Content = "Continue",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center

        };

        continueButton.Click += ContinueButton_Click;

        retakeButton = new Button
        {
            Content = "Retake",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            IsVisible = false
        };

        retakeButton.Click += RetakeButton_Click;

        _cameraPreview = new Image
        {
            Width = 480,
            Height = 300,
            Stretch = Stretch.UniformToFill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var cameraBorder = new Border
        {
            Width = 500,
            Height = 320,
            CornerRadius = new CornerRadius(15),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush(Color.Parse("#D87DDE")),
            ClipToBounds = true,
            Child = _cameraPreview
        };

        // ---------------------------------------------------------
        // GRID DE BOTONES
        // ---------------------------------------------------------

        var buttonsGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,*"),
            ColumnSpacing = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Grid.SetColumn(clearButton, 0);
        Grid.SetColumn(continueButton, 1);
        Grid.SetColumn(retakeButton, 0);

        // buttonsGrid.Children.Add(clearButton);
        buttonsGrid.Children.Add(continueButton);
        buttonsGrid.Children.Add(retakeButton);

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
        //Grid.SetRow(instructions, 1);
        // Grid.SetRow(_tracingCanvas, 2);
        // Grid.SetRow(_feedbackText, 3);
        Grid.SetRow(cameraBorder, 1);
        mainGrid.Children.Add(cameraBorder);
        Grid.SetRow(buttonsGrid, 4);

        mainGrid.Children.Add(title);
        // mainGrid.Children.Add(instructions);
        // mainGrid.Children.Add(_tracingCanvas);
        // mainGrid.Children.Add(_feedbackText);
        mainGrid.Children.Add(buttonsGrid);

        // No utilizamos ScrollViewer.
        Content = mainGrid;
    }

    // ---------------------------------------------------------
    // CLEAR
    // ---------------------------------------------------------

    // private void ClearButton_Click(
    //     object? sender,
    //     RoutedEventArgs e)
    // {
    //     _tracingCanvas.Clear();
    //     _feedbackText.Text = "";
    // }

    // ---------------------------------------------------------
    // CONTINUE
    // ---------------------------------------------------------

    private async void ContinueButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!_photoTaken){
            _photo.Source = _cameraPreview.Source;
            // Detenemos el bucle de la webcam.
            _cameraCancellation?.Cancel();
            if (_cameraTask is not null)
            {
                try
                {
                    await _cameraTask;
                }
                catch (OperationCanceledException)
                {
                }
                retakeButton.IsVisible = true;
            }
            continueButton.Content = "Confirm";

            Console.WriteLine("Camera stopped.");
            _photoTaken = true;
        }
        else{
            // Aquí puedes agregar la lógica para continuar a la siguiente página
            // Por ejemplo, podrías navegar a otra página o realizar alguna acción con la foto tomada.
            var photoStoryPage = new PhotoStoryPage();
            using var memoryStream = new MemoryStream();
            (_photo.Source as Bitmap)?.Save(memoryStream);
            byte[] imageData = memoryStream.ToArray();
            photoStoryPage.StartStory(imageData);

            if (Navigation is not null)
            {
                // De momento también abre SettingsPage.
                await Navigation.PushAsync(photoStoryPage);
            }
        }
        
    }

    private async void RetakeButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _photo.Source = null;
        retakeButton.IsVisible = false;
        _photoTaken = false;
        continueButton.Content = "Continue";
        await UpdateCameraPreview();
    }

    private static Avalonia.Media.Imaging.Bitmap MatToAvaloniaBitmap(
    Mat frame)
    {
        Cv2.ImEncode(
            ".jpg",
            frame,
            out byte[] imageData
        );

        using var stream =
            new MemoryStream(imageData);

        return new Avalonia.Media.Imaging.Bitmap(stream);
    }

    public Task UpdateCameraPreview()
    {
        // Evita iniciar la cámara dos veces.
        if (_cameraTask is not null &&
            !_cameraTask.IsCompleted)
        {
            return _cameraTask;
        }

        _cameraCancellation =
            new CancellationTokenSource();

        _cameraTask =
            CameraLoop(_cameraCancellation.Token);

        return _cameraTask;
    }

    public async Task CameraLoop(CancellationToken token)
    {

        using var capture = new VideoCapture(0);
        if (!capture.IsOpened())
        {
            throw new IOException("Could not open camera 0.");
        }

        capture.FrameWidth = (int)_cameraPreview.Width;
        capture.FrameHeight = (int)_cameraPreview.Height;

        using var frame = new Mat();
        try{
            while (capture.Read(frame) && !token.IsCancellationRequested && !frame.Empty())
            {
                Bitmap bitmap = MatToAvaloniaBitmap(frame);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Liberamos la imagen anterior.
                    if (_cameraPreview.Source
                        is IDisposable previousImage)
                    {
                        previousImage.Dispose();
                    }

                    _cameraPreview.Source =
                        bitmap;
                });

                // Aproximadamente 30 fotogramas por segundo.
                await Task.Delay(33);
            }
        }
        catch (OperationCanceledException)
        {
            // La tarea fue cancelada, salimos del bucle.
        }
    }
}