using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GymMate.Models;
using GymMate.Messages;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class SessionEditorViewModel : ObservableObject
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<WorkoutRoutine> Routines { get; } = new();
    public ObservableCollection<SetRecord> Sets { get; } = new();

    [ObservableProperty]
    private WorkoutRoutine? selectedRoutine;

    [ObservableProperty]
    private DateTime date = DateTime.Today;

    public SessionEditorViewModel(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [RelayCommand]
    public async Task AppearingAsync()
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;
        var routines = await _db.GetRoutinesAsync(uid);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Routines.Clear();
            foreach (var r in routines.OrderBy(x => x.Name))
                Routines.Add(r);
        });
    }

    [RelayCommand]
    private void AddSet()
    {
        Sets.Add(new SetRecord());
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedRoutine == null)
        {
            await Shell.Current.DisplayAlert("Error", "Seleccione rutina", "OK");
            return;
        }

        var session = new WorkoutSession
        {
            Id = Guid.NewGuid().ToString(),
            RoutineId = SelectedRoutine.Id,
            DateUtc = Date.ToUniversalTime(),
            Sets = Sets.ToList()
        };

        WeakReferenceMessenger.Default.Send(new SessionUpdatedMessage(session));
        try
        {
            await _db.AddSessionAsync(_auth.CurrentUserUid, session);
        }
        catch
        {
            WeakReferenceMessenger.Default.Send(new SessionsReloadMessage());
            await Shell.Current.DisplayAlert("Error", "No se pudo guardar", "OK");
        }

        await Shell.Current.GoToAsync("..");
    }
}
