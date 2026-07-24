using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LittleLinguist.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    // Se ejecuta al pulsar el botón Back.
    private async void BackButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        // Elimina SettingsPage y vuelve a la página anterior.
        await Navigation.PopAsync();
    }
}