using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using System.Linq;
using System.Text.RegularExpressions;
namespace LittleLinguist.Pages;


/*
FALTA:
1. elegir pregunta ¿aleatoriamente? e ir a esa página (hacer el next button)
2. arreglar modelo LLM
- poner modelo LLM en un archivo diferente, compartido por todas las pestañas para que la historia pueda continuar
- generar historias diferentes, siempre es sobre un pájaro llamado Pip
3. cambiar el diseño de la interfaz
4. Si le da a continuar, que deje de leer el texto el SpeechReader
*/

public class PhotoStoryPage : ContentPage
{
    TextBlock storyText = new TextBlock();

    // Botón Play
    Button playButton = new Button();

    public PhotoStoryPage()
    {
        // Grid principal
        var grid = new Grid();
        grid.Margin = new Thickness(25);

        // Dos filas
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Texto de la historia
        storyText.Text = "";
        storyText.TextWrapping = TextWrapping.Wrap;
        storyText.FontSize = 18;

        // Scroll para el texto
        var scrollViewer = new ScrollViewer();
        scrollViewer.Content = storyText;
        scrollViewer.BorderThickness = new Thickness(2);
        scrollViewer.Padding = new Thickness(15);
        scrollViewer.Margin = new Thickness(0, 0, 0, 20);

        Grid.SetRow(scrollViewer, 0);

        // Panel para los botones
        var buttonPanel = new StackPanel();
        buttonPanel.Orientation = Orientation.Horizontal;
        buttonPanel.HorizontalAlignment = HorizontalAlignment.Center;
        buttonPanel.Spacing = 20;

        // Botón Finish
        var finishButton = new Button();
        finishButton.Content = "Finish Session";
        finishButton.Padding = new Thickness(20, 10);

        finishButton.Click += FinishButton_Click;

        // Botón Next
        var nextButton = new Button();
        nextButton.Content = "Next";
        nextButton.Padding = new Thickness(20, 10);

        nextButton.Click += NextButton_Click;

        // Botón Play / Pause
        playButton.Content = "Play";
        playButton.Padding = new Thickness(20, 10);
        playButton.Click += PlayButton_Click;

        // Slider de velocidad: a la derecha, más rápido
        var speedSlider = new Slider();
        speedSlider.Minimum = 0.5;
        speedSlider.Maximum = 2.5;
        speedSlider.Value = 1.5;
        speedSlider.Width = 200;
        speedSlider.TickFrequency = 0.1;
        speedSlider.IsSnapToTickEnabled = true;
        speedSlider.ValueChanged += SpeedSlider_ValueChanged;

        var speedLabel = new TextBlock();
        speedLabel.Text = "Speed";
        speedLabel.VerticalAlignment = VerticalAlignment.Center;

        var speedPanel = new StackPanel();
        speedPanel.Orientation = Orientation.Horizontal;
        speedPanel.Spacing = 10;
        speedPanel.VerticalAlignment = VerticalAlignment.Center;
        speedPanel.Children.Add(speedLabel);
        speedPanel.Children.Add(speedSlider);
        buttonPanel.Children.Add(playButton);
        buttonPanel.Children.Add(speedPanel);
        buttonPanel.Children.Add(finishButton);
        buttonPanel.Children.Add(nextButton);
        Grid.SetRow(buttonPanel, 1);

        grid.Children.Add(scrollViewer);
        grid.Children.Add(buttonPanel);

        Content = grid;
    }

    // Vuelve a la página anterior (Home)
    private async void FinishButton_Click(object? sender, RoutedEventArgs e)
    {
        if(Navigation is not null)
        {
            await Navigation.PopAsync();
        }
    }

    // Pasa a la siguiente página
    private async void NextButton_Click(object? sender, RoutedEventArgs e)
    {
        //await Navigation.PopAsync();
        //await Navigation.PushAsync(new FALTANOMBRE());
        StoryGenerator.Instance.StopStory();
        SpeechReader.Instance.Pause();
        playButton.Content = "Play";
        // StoryGenerator.Instance.StopStory()
    // Detiene la generación y la lectura.

    if (Navigation is null)
    {
        return;
    }

    string story = storyText.Text ?? "";

    // Extrae palabras de entre 3 y 10 letras.
    List<string> words = Regex
        .Matches(story, @"\b[A-Za-z]{3,10}\b")
        .Select(match => match.Value)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (words.Count == 0)
    {
        return;
    }

    // Elige una palabra aleatoria.
    string randomWord =
        words[Random.Shared.Next(words.Count)];

    Console.WriteLine($"Selected word: {randomWord}");

    await Navigation.PushAsync(
        new WritingPage(randomWord)
    );

    }

    public async void StartStory(byte[] imageData)
    {
        string respuesta = await VisionEngine.Instance.IdentifyObject(imageData);
        storyText.Text = respuesta;
    }

    // Alterna entre reproducir y pausar el audio
    private void PlayButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SpeechReader.Instance.IsPlaying)
        {
            SpeechReader.Instance.Pause();
            playButton.Content = "Play";
        }
        else
        {
            SpeechReader.Instance.Play();
            playButton.Content = "Pause";
        }
    }

    // Ajusta la velocidad de lectura.
    // El slider va de lento (izquierda) a rápido (derecha),
    // mientras que Piper usa la escala contraria.
    private void SpeedSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        SpeechReader.Instance.SetSpeed((float)(3.0 - e.NewValue));
    }
}