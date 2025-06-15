using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GymMate.Models;
using GymMate.Messages;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class SessionDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    [ObservableProperty]
    private WorkoutSession session = new();

    public SessionDetailViewModel(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Session", out var value) && value is WorkoutSession s)
        {
            Session = s;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Eliminar", "¿Eliminar sesión?", "Sí", "No");
        if (!confirm) return;

        WeakReferenceMessenger.Default.Send(new SessionDeletedMessage(Session.Id));
        try
        {
            await _db.DeleteSessionAsync(_auth.CurrentUserUid, Session.Id);
        }
        catch
        {
            WeakReferenceMessenger.Default.Send(new SessionsReloadMessage());
            await Shell.Current.DisplayAlert("Error", "No se pudo borrar", "OK");
        }

        await Shell.Current.GoToAsync("..");
    }
}
