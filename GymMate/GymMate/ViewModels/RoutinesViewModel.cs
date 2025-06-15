using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GymMate.Models;
using GymMate.Messages;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class RoutinesViewModel : ObservableObject
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<WorkoutRoutine> Routines { get; } = new();

    public RoutinesViewModel(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
        _db.RoutinesChanged += Db_RoutinesChanged;
        WeakReferenceMessenger.Default.Register<RoutineUpdatedMessage>(this, (r, m) => LocalAddOrUpdate(m.Routine));
        WeakReferenceMessenger.Default.Register<RoutineDeletedMessage>(this, (r, m) => LocalDelete(m.RoutineId));
        WeakReferenceMessenger.Default.Register<RoutinesReloadMessage>(this, (r, m) => LoadAsync());
    }

    private async void Db_RoutinesChanged(object? sender, EventArgs e)
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
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;
        var routines = await _db.GetRoutinesAsync(uid);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Routines.Clear();
            foreach (var r in routines.OrderByDescending(x => x.CreatedUtc))
                Routines.Add(r);
        });
    }

    private void LocalAddOrUpdate(WorkoutRoutine routine)
    {
        var existing = Routines.FirstOrDefault(r => r.Id == routine.Id);
        if (existing == null)
        {
            Routines.Add(routine);
        }
        else
        {
            var index = Routines.IndexOf(existing);
            Routines[index] = routine;
        }
    }

    private void LocalDelete(string id)
    {
        var existing = Routines.FirstOrDefault(r => r.Id == id);
        if (existing != null)
            Routines.Remove(existing);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await Shell.Current.GoToAsync("routineDetail");
    }

    [RelayCommand]
    private async Task EditAsync(WorkoutRoutine routine)
    {
        await Shell.Current.GoToAsync("routineDetail", new Dictionary<string, object> { ["Routine"] = routine });
    }

    [RelayCommand]
    private async Task DeleteAsync(WorkoutRoutine routine)
    {
        bool confirm = await Shell.Current.DisplayAlert("Eliminar", $"¿Eliminar {routine.Name}?", "Sí", "No");
        if (!confirm) return;

        LocalDelete(routine.Id);
        try
        {
            await _db.DeleteRoutineAsync(_auth.CurrentUserUid, routine.Id);
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo borrar", "OK");
            await LoadAsync();
        }
    }
}
