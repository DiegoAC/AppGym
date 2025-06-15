using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;

namespace GymMate.ViewModels;

public partial class ClassesViewModel : ObservableObject
{
    private readonly IClassBookingService _service;

    public ObservableCollection<GymClass> Classes { get; } = new();

    public ClassesViewModel(IClassBookingService service)
    {
        _service = service;
        _service.ClassesChanged += Service_ClassesChanged;
    }

    private async void Service_ClassesChanged(object? sender, EventArgs e)
    {
        await LoadAsync();
    }

    [RelayCommand]
    public async Task AppearingAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = new List<GymClass>();
        await foreach (var c in _service.GetUpcomingClassesAsync())
            list.Add(c);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Classes.Clear();
            foreach (var c in list)
                Classes.Add(c);
        });
    }

    [RelayCommand]
    private async Task DetailsAsync(GymClass gymClass)
    {
        await Shell.Current.GoToAsync("classDetail", new Dictionary<string, object> { ["Class"] = gymClass });
    }
}
