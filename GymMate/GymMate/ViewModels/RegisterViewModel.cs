using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;

namespace GymMate.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
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
        // TODO: call auth service
    }
}
