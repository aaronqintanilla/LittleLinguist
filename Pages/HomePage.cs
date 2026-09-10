using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace LittleLinguist.Pages;

public class HomePage : ContentPage
{
    private SettingsPage? settingsPage;
    private StoryPage? storyPage;
    private PhotoStoryPage? PhotoStoryPage;

    public HomePage()
    {
        // ---------------------------------------------------------
        // FONDO
        // ---------------------------------------------------------

        Background = new SolidColorBrush(Color.Parse("#EDE7FF"));

        // ---------------------------------------------------------
        // BOTÓN SETTINGS
        // ---------------------------------------------------------

        var settingsIcon = new TextBlock
        {
            Text = "⚙",
            FontSize = 25,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var settingsButton = new Button
        {
            Content = settingsIcon,
            Width = 55,
            Height = 55,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Background = new SolidColorBrush(Color.Parse("#D8CCFF")),
            Foreground = new SolidColorBrush(Color.Parse("#5940A8")),
            CornerRadius = new CornerRadius(18)
        };

        settingsButton.Click += SettingsButton_Click;

        // ---------------------------------------------------------
        // TÍTULO
        // ---------------------------------------------------------

        var title = new TextBlock
        {
            Text = "✨ Little Linguist ✨",
            FontSize = 42,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#6846C7")),
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // SUBTÍTULO
        // ---------------------------------------------------------

        var subtitle = new TextBlock
        {
            Text = "Learn languages through a new adventure!",
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.Parse("#55729A")),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // ---------------------------------------------------------
        // DECORACIÓN - ESTRELLAS
        // ---------------------------------------------------------

        var star1 = new TextBlock
        {
            Text = "☆",
            FontSize = 30,
            Foreground = new SolidColorBrush(Color.Parse("#6846C7")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(85, 115, 0, 0)
        };

        var star2 = new TextBlock
        {
            Text = "✦",
            FontSize = 24,
            Foreground = new SolidColorBrush(Color.Parse("#7956D8")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 190, 135, 0)
        };

        var star3 = new TextBlock
        {
            Text = "✧",
            FontSize = 20,
            Foreground = new SolidColorBrush(Color.Parse("#65B8E8")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(180, 0, 0, 0)
        };

        var star4 = new TextBlock
        {
            Text = "☆",
            FontSize = 25,
            Foreground = new SolidColorBrush(Color.Parse("#7956D8")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 70, 75, 0)
        };

        var star5 = new TextBlock
        {
            Text = "✦",
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.Parse("#65B8E8")),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 220, 100)
        };

        var star6 = new TextBlock
        {
            Text = "✧",
            FontSize = 28,
            Foreground = new SolidColorBrush(Color.Parse("#6846C7")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(220, 0, 0, 85)
        };

        // ---------------------------------------------------------
        // DECORACIÓN - NUBES
        // ---------------------------------------------------------

        var cloud1 = new TextBlock
        {
            Text = "☁",
            FontSize = 55,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(60, 0, 0, 50)
        };

        var cloud2 = new TextBlock
        {
            Text = "☁",
            FontSize = 45,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 70, 80)
        };

        var cloud3 = new TextBlock
        {
            Text = "☁",
            FontSize = 35,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(170, 55, 0, 0)
        };

        var cloud4 = new TextBlock
        {
            Text = "☁",
            FontSize = 30,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 75, 220, 0)
        };

        // ---------------------------------------------------------
        // DECORACIÓN - LUNA
        // ---------------------------------------------------------

        var moon = new TextBlock
        {
            Text = "☾",
            FontSize = 38,
            Foreground = new SolidColorBrush(Color.Parse("#FFD966")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(75, 220, 0, 0)
        };

        // ---------------------------------------------------------
        // BOTÓN STORY
        // ---------------------------------------------------------

        var storyIcon = new TextBlock
        {
            Text = "📖",
            FontSize = 22,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(110, 0, 0, 0)
        };

        var storyText = new TextBlock
        {
            Text = "Story Adventure",
            FontSize = 21,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var storyContent = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        storyContent.Children.Add(storyText);
        storyContent.Children.Add(storyIcon);

        var storyButton = new Button
        {
            Content = storyContent,
            Height = 75,
            Background = new SolidColorBrush(Color.Parse("#7956D8")),
            CornerRadius = new CornerRadius(25),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };

        storyButton.Click += StoryButton_Click;

        // ---------------------------------------------------------
        // BOTÓN PHOTO
        // ---------------------------------------------------------

        var visionIcon = new TextBlock
        {
            Text = "📸",
            FontSize = 22,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(110, 0, 0, 0)
        };

        var visionText = new TextBlock
        {
            Text = "Photo Adventure",
            FontSize = 21,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        var visionContent = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        visionContent.Children.Add(visionText);
        visionContent.Children.Add(visionIcon);

        var visionButton = new Button
        {
            Content = visionContent,
            Height = 75,
            Background = new SolidColorBrush(Color.Parse("#65B8E8")),
            CornerRadius = new CornerRadius(25),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };

        visionButton.Click += VisionButton_Click;

        // ---------------------------------------------------------
        // PANEL CENTRAL
        // ---------------------------------------------------------

        var centerPanel = new StackPanel
        {
            Width = 500,
            Spacing = 22,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                title,
                subtitle,
                storyButton,
                visionButton
            }
        };

        // ---------------------------------------------------------
        // GRID PRINCIPAL
        // ---------------------------------------------------------

        var mainGrid = new Grid
        {
            Margin = new Thickness(40)
        };

        // Decoraciones
        mainGrid.Children.Add(star1);
        mainGrid.Children.Add(star2);
        mainGrid.Children.Add(star3);
        mainGrid.Children.Add(star4);
        mainGrid.Children.Add(star5);
        mainGrid.Children.Add(star6);

        mainGrid.Children.Add(cloud1);
        mainGrid.Children.Add(cloud2);
        mainGrid.Children.Add(cloud3);
        mainGrid.Children.Add(cloud4);

        mainGrid.Children.Add(moon);

        // Botón Settings
        mainGrid.Children.Add(settingsButton);

        // Panel principal
        mainGrid.Children.Add(centerPanel);

        Content = mainGrid;
    }

    // Abre SettingsPage.
    private async void SettingsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is not null)
        {
            await Navigation.PushAsync(
                new SettingsPage()
            );
        }
    }

    private async void StoryButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is null)
        {
            return;
        }

        if (storyPage == null)
        {
            storyPage = new StoryPage();
        }

        Session.Instance.Reset();
        StoryGenerator.Instance.StopStory();

        if (Navigation is not null)
        {
            storyPage.StartStory();
            await Navigation.PushAsync(storyPage);
        }
    }

    private async void VisionButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (Navigation is null)
        {
            return;
        }

        var cameraPage = new CameraPage();

        await Navigation.PushAsync(
            cameraPage
        );

        await cameraPage.UpdateCameraPreview();
    }
}