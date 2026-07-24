using CommunityToolkit.Mvvm.ComponentModel;

namespace LittleLinguist.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // En .NET 8 con CommunityToolkit, se usa un campo privado con "_" 
    // y la librería genera automáticamente la propiedad "Greeting" en segundo plano.
    [ObservableProperty]
    private string _greeting = "¡Bienvenidos a LittleLinguist!";
}