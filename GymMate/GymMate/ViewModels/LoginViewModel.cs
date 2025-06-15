using CommunityToolkit.Mvvm.ComponentModel;

namespace GymMate.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;
}
