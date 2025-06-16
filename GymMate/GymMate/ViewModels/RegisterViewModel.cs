using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IFirebaseAuthService _auth;

    public RegisterViewModel(IFirebaseAuthService auth)
    {
        _auth = auth;
    }
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [RelayCommand]
    private async Task NavigateBackAsync()
        => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            await Toast.Make("Complete all fields").Show();
            return;
        }
        if (Password != ConfirmPassword)
        {
            await Toast.Make("Passwords do not match").Show();
            return;
        }

        var ok = await _auth.RegisterAsync(Email, Password);
        if (ok)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Toast.Make("Registration failed").Show();
        }
    }
}
