using GymMate.ViewModels;

namespace GymMate.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Content.Opacity = 0;
        await Content.FadeTo(1, 400, Easing.SinOut);
    }
}
