using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using Avalonia.Interactivity;

namespace LittleLinguist.Pages;

/*
FALTA:
1. elegir pregunta ¿aleatoriamente? e ir a esa página (hacer el next button)
2. traer el texto del LLM
3. cambiar el diseño, que es feísimo
*/

public class StoryPage : ContentPage
{
    public StoryPage()
    {
        // Grid principal
        var grid = new Grid();
        grid.Margin = new Thickness(25);

        // Dos filas
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Texto de la historia
        var storyText = new TextBlock();
        storyText.Text = " TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA  TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA TEXTO DE PRUEBA ";
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
        await Navigation.PopAsync();
    }

    // Pasa a la siguiente página
    private async void NextButton_Click(object? sender, RoutedEventArgs e)
    {
        //await Navigation.PopAsync();
        //await Navigation.PushAsync(new FALTANOMBRE());
    }
}