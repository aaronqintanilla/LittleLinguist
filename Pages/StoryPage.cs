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

namespace LittleLinguist.Pages;


/*
FALTA:
1. elegir pregunta ¿aleatoriamente? e ir a esa página (hacer el next button)
2. arreglar modelo LLM
- poner modelo LLM en un archivo diferente, compartido por todas las pestañas para que la historia pueda continuar
- generar historias diferentes, siempre es sobre un pájaro llamado Pip
3. cambiar el diseño de la interfaz
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

    public async void StartStory()
    {
        await StoryGenerator.Instance.WriteStory(storyText);
    }
}