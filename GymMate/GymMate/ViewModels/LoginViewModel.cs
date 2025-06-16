using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IFirebaseAuthService _auth;

    public LoginViewModel(IFirebaseAuthService auth)
    {
        _auth = auth;
    }
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [RelayCommand]
    private async Task NavigateRegisterAsync()
        => await Shell.Current.GoToAsync("//register");

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Toast.Make("Email/Password required").Show();
            return;
        }

        var ok = await _auth.LoginAsync(Email, Password);
        if (ok)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await Toast.Make("Login failed").Show();
        }
    }
}
