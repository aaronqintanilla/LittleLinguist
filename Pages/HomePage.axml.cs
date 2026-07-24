using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LittleLinguist.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    // Se ejecuta al pulsar el botón Settings.
    private async void SettingsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        // Abre SettingsPage y conserva HomePage en el historial.
        await Navigation.PushAsync(new SettingsPage());
    }
}