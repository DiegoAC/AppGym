using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class PhotoDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IProgressPhotoService _service;
    private readonly IFeedService _feed;

    [ObservableProperty]
    private ProgressPhoto photo = new();

    public PhotoDetailViewModel(IProgressPhotoService service, IFeedService feed)
    {
        _service = service;
        _feed = feed;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Photo", out var value) && value is ProgressPhoto p)
            Photo = p;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Eliminar", "¿Eliminar foto?", "Sí", "No");
        if (!confirm) return;

        await _service.DeleteAsync(Photo.Id);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        await _feed.CreatePostAsync(Photo);
        await Shell.Current.DisplayAlert("Compartido", "Se publicó en el feed", "OK");
    }
}
