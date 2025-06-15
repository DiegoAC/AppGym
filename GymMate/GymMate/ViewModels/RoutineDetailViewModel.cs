using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GymMate.Models;
using GymMate.Messages;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class RoutineDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    [ObservableProperty]
    private WorkoutRoutine routine = new();

    public RoutineDetailViewModel(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Routine", out var value) && value is WorkoutRoutine r)
        {
            Routine = new WorkoutRoutine
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedUtc = r.CreatedUtc
            };
        }
        else
        {
            Routine = new WorkoutRoutine();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;

        bool isNew = string.IsNullOrEmpty(Routine.Id);
        if (isNew)
        {
            Routine.Id = Guid.NewGuid().ToString();
            Routine.CreatedUtc = DateTime.UtcNow;
        }

        WeakReferenceMessenger.Default.Send(new RoutineUpdatedMessage(Routine));

        try
        {
            await _db.AddOrUpdateRoutineAsync(uid, Routine);
        }
        catch
        {
            WeakReferenceMessenger.Default.Send(new RoutinesReloadMessage());
            await Shell.Current.DisplayAlert("Error", "No se pudo guardar la rutina", "OK");
        }

        await Shell.Current.GoToAsync("..");
    }
}
