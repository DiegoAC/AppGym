using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GymMate.Models;
using GymMate.Messages;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class SessionsViewModel : ObservableObject
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<WorkoutSession> Sessions { get; } = new();

    public SessionsViewModel(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
        _db.SessionsChanged += Db_SessionsChanged;
        WeakReferenceMessenger.Default.Register<SessionUpdatedMessage>(this, (r, m) => LocalAddOrUpdate(m.Session));
        WeakReferenceMessenger.Default.Register<SessionDeletedMessage>(this, (r, m) => LocalDelete(m.SessionId));
        WeakReferenceMessenger.Default.Register<SessionsReloadMessage>(this, (r, m) => LoadAsync());
    }

    private async void Db_SessionsChanged(object? sender, EventArgs e)
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
        var sessions = await _db.GetSessionsAsync(uid);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Sessions.Clear();
            foreach (var s in sessions.OrderByDescending(x => x.DateUtc))
                Sessions.Add(s);
        });
    }

    private void LocalAddOrUpdate(WorkoutSession session)
    {
        var existing = Sessions.FirstOrDefault(s => s.Id == session.Id);
        if (existing == null)
        {
            Sessions.Insert(0, session);
        }
        else
        {
            var index = Sessions.IndexOf(existing);
            Sessions[index] = session;
        }
    }

    private void LocalDelete(string id)
    {
        var existing = Sessions.FirstOrDefault(s => s.Id == id);
        if (existing != null)
            Sessions.Remove(existing);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await Shell.Current.GoToAsync("sessionEditor");
    }

    [RelayCommand]
    private async Task ViewAsync(WorkoutSession session)
    {
        await Shell.Current.GoToAsync("sessionDetail", new Dictionary<string, object> { ["Session"] = session });
    }

    [RelayCommand]
    private async Task DeleteAsync(WorkoutSession session)
    {
        bool confirm = await Shell.Current.DisplayAlert("Eliminar", "¿Eliminar sesión?", "Sí", "No");
        if (!confirm) return;

        LocalDelete(session.Id);
        try
        {
            await _db.DeleteSessionAsync(_auth.CurrentUserUid, session.Id);
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo borrar", "OK");
            await LoadAsync();
        }
    }
}
