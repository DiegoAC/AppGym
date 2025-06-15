using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using CommunityToolkit.Maui.Alerts;
using System.Collections.Generic;

namespace GymMate.ViewModels;

public partial class ClassDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IClassBookingService _service;
    private readonly IFirebaseAuthService _auth;

    [ObservableProperty]
    private GymClass gymClass = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActionText))]
    private bool isReserved;

    public string ActionText => IsReserved ? "Cancelar" : "Reservar";

    public ClassDetailViewModel(IClassBookingService service, IFirebaseAuthService auth)
    {
        _service = service;
        _auth = auth;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Class", out var value) && value is GymClass c)
        {
            GymClass = c;
            IsReserved = await _service.IsReservedAsync(c.Id, _auth.CurrentUserUid);
        }
    }

    [RelayCommand]
    private async Task ToggleReservationAsync()
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;
        try
        {
            if (IsReserved)
            {
                await _service.CancelReservationAsync(GymClass.Id, uid);
                IsReserved = false;
                if (GymClass.ReservedCount > 0)
                    GymClass.ReservedCount--;
                await Toast.Make("Reserva cancelada").Show();
            }
            else
            {
                await _service.ReserveAsync(GymClass.Id, uid);
                IsReserved = true;
                GymClass.ReservedCount++;
                await Toast.Make("Reserva realizada").Show();
            }
        }
        catch (InvalidOperationException)
        {
            await Shell.Current.DisplayAlert("Clase llena", "No hay cupo disponible", "OK");
        }
    }
}
