using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Threading.Tasks;

namespace LittleLinguist.Pages;


/*
FALTA:
1. elegir pregunta ¿aleatoriamente? e ir a esa página (hacer el next button)
2. traer el texto del LLM
3. cambiar el diseño, que es feísimo
*/

public class StoryPage : ContentPage
{
    TextBlock storyText = new TextBlock();

    public StoryPage()
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
    }

    public async Task GenerateStory()
    {
        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/gemma-3-1b-it-q4_0.gguf";

        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 2048,
            GpuLayerCount = 0
        };

        Console.WriteLine("Loading model...");
        var model = LLamaWeights.LoadFromFile(parameters);
        var context = model.CreateContext(parameters);

        var executor = new InteractiveExecutor(context);
        var session = new ChatSession(executor);

        var inferenceParams = new InferenceParams
        {
            MaxTokens = -1,
            AntiPrompts = new List<string> { "<|im_end|>", "<|im_start|>" },
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = 0.7f, MinP = 0.3f }
        };

        string? input = "Write a story about Laia, my friend";

        await foreach (var token in session.ChatAsync(new ChatHistory.Message(AuthorRole.User, input), inferenceParams))
        {
            // Aquí puedes actualizar la interfaz de usuario con el token generado.
            // Por ejemplo, podrías agregarlo a un TextBlock o similar.
            storyText.Text += token;
        }
    }
}