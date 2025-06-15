using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class PhotosViewModel : ObservableObject
{
    private readonly IProgressPhotoService _service;

    public ObservableCollection<ProgressPhoto> Photos { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    public PhotosViewModel(IProgressPhotoService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task AppearingAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        Photos.Clear();
        await foreach (var p in _service.GetPhotosAsync())
            Photos.Add(p);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted) return;
        var result = await MediaPicker.PickPhotoAsync();
        if (result == null) return;
        string? caption = await Shell.Current.DisplayPromptAsync("Caption", "Añadir descripción opcional", "Guardar", "Cancelar");
        await _service.UploadAsync(result, caption);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ViewAsync(ProgressPhoto photo)
    {
        await Shell.Current.GoToAsync("photoDetail", new Dictionary<string, object> { ["Photo"] = photo });
    }
}
