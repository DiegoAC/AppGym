using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GymMate.Models;
using GymMate.Messages;
using GymMate.Services;
using System.Collections.ObjectModel;

namespace GymMate.ViewModels;

public partial class RoutinesViewModel : ObservableObject
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<RoutineCardVm> Routines { get; } = new();

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
                Routines.Add(ToVm(r));
        });
    }

    private RoutineCardVm ToVm(WorkoutRoutine r)
        => new(r.Name, r.Focus, r.Difficulty, r.CreatedUtc, r);

    private void LocalAddOrUpdate(WorkoutRoutine routine)
    {
        var existing = Routines.FirstOrDefault(r => r.CommandParameter.Id == routine.Id);
        var vm = ToVm(routine);
        if (existing == null)
        {
            Routines.Add(vm);
        }
        else
        {
            var index = Routines.IndexOf(existing);
            Routines[index] = vm;
        }
    }

    private void LocalDelete(string id)
    {
        var existing = Routines.FirstOrDefault(r => r.CommandParameter.Id == id);
        if (existing != null)
            Routines.Remove(existing);
    }

    [RelayCommand]
    private async Task AddRoutineAsync()
    {
        await Shell.Current.GoToAsync("routineDetail");
    }

    [RelayCommand]
    private async Task EditRoutineAsync(RoutineCardVm card)
    {
        await Shell.Current.GoToAsync("routineDetail", new Dictionary<string, object> { ["Routine"] = card.CommandParameter });
    }

    [RelayCommand]
    private async Task DeleteAsync(RoutineCardVm card)
    {
        bool confirm = await Shell.Current.DisplayAlert("Eliminar", $"¿Eliminar {card.CommandParameter.Name}?", "Sí", "No");
        if (!confirm) return;

        LocalDelete(card.CommandParameter.Id);
        try
        {
            await _db.DeleteRoutineAsync(_auth.CurrentUserUid, card.CommandParameter.Id);
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo borrar", "OK");
            await LoadAsync();
        }
    }
}
