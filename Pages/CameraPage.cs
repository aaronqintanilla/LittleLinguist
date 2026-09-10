using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using OpenCvSharp;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LittleLinguist.Pages;

// Handles the camera interface, photo capture, and camera preview.
public class CameraPage : ContentPage
{
    private readonly Image _cameraPreview;
    private readonly Image _photo;
    private CancellationTokenSource? _cameraCancellation;
    private Task? _cameraTask;
    private Button retakeButton;
    private Button continueButton;
    private bool _photoTaken;

    // Initializes the camera page and its user interface.
    public CameraPage()
    {
        NavigationPage.SetHasBackButton(this, false);
        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));

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
            Text = "📸 Take a picture!",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#6846C7")),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var decoration = new Grid
        {
            Height = 45
        };

        var stars = new TextBlock
        {
            Text = "✦  ✧  ☆",
            FontSize = 26,
            Foreground = new SolidColorBrush(Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 0)
        };

        var cloud = new TextBlock
        {
            Text = "☁",
            FontSize = 42,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 25, 0)
        };

        decoration.Children.Add(stars);
        decoration.Children.Add(cloud);

        continueButton = new Button
        {
            Content = "Continue ➜",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#6846C7")),
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(18)

        };

        continueButton.Click += ContinueButton_Click;

        retakeButton = new Button
        {
            Content = "🔄 Retake",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#65B8E8")),
            Padding = new Thickness(25, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(18),
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
            Background = Brushes.White,
            CornerRadius = new CornerRadius(25),
            BorderThickness = new Thickness(4),
            BorderBrush = new SolidColorBrush(Color.Parse("#C9BBF5")),
            Padding = new Thickness(8),
            ClipToBounds = true,
            Child = _cameraPreview
        };

        var buttonsGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,*"),
            ColumnSpacing = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        Grid.SetColumn(continueButton, 1);
        Grid.SetColumn(retakeButton, 0);

        buttonsGrid.Children.Add(continueButton);
        buttonsGrid.Children.Add(retakeButton);

        var mainGrid = new Grid
        {
            Margin = new Thickness(30),
            RowDefinitions =
                RowDefinitions.Parse("Auto,Auto,*,Auto,Auto"),

            RowSpacing = 15
        };

        Grid.SetRow(decoration, 0);
        mainGrid.Children.Add(decoration);

        Grid.SetRow(title, 1);
        mainGrid.Children.Add(title);

        Grid.SetRow(cameraBorder, 2);
        mainGrid.Children.Add(cameraBorder);

        Grid.SetRow(buttonsGrid, 3);
        mainGrid.Children.Add(buttonsGrid);

        Content = mainGrid;
    }

    // Captures the current frame or confirms the photo and continues.
    private async void ContinueButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (!_photoTaken){
            _photo.Source = _cameraPreview.Source;
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
            continueButton.Content = "Confirm ✅";

            Console.WriteLine("Camera stopped.");
            _photoTaken = true;
        }
        else{
            var photoStoryPage = new PhotoStoryPage();
            using var memoryStream = new MemoryStream();
            (_photo.Source as Bitmap)?.Save(memoryStream, PngBitmapEncoderOptions.Default);
            byte[] imageData = memoryStream.ToArray();
            photoStoryPage.StartStory(imageData);

            if (Navigation is not null)
            {
                await Navigation.PushAsync(photoStoryPage);
            }
        }
        
    }

    // Clears the current photo and restarts the camera preview.
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

    // Converts an OpenCV frame into an Avalonia bitmap.
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

    // Starts the camera preview if it is not already running.
    public Task UpdateCameraPreview()
    {
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

    // Continuously captures frames from the camera and updates the preview.
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
                    if (_cameraPreview.Source
                        is IDisposable previousImage)
                    {
                        previousImage.Dispose();
                    }

                    _cameraPreview.Source =
                        bitmap;
                });

                await Task.Delay(33);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}