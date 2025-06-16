using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace GymMate.ViewModels;

public record HomeButtonVm(string Text, string Icon, string Route);

public partial class HomeViewModel : ObservableObject
{
    public ObservableCollection<HomeButtonVm> HomeButtons { get; } = new()
    {
        new("Rutinas", "fitness_center", "//routines"),
        new("Sesiones", "timer", "//sessions"),
        new("Fotos", "photo_camera", "//photos"),
        new("Feed", "rss_feed", "//feed"),
        new("Clases", "event", "//classes"),
        new("Ajustes", "settings", "//settings"),
    };

    [RelayCommand]
    private Task NavigateAsync(string route)
        => Shell.Current.GoToAsync(route);
}
