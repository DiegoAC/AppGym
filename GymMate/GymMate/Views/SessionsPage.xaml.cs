using GymMate.ViewModels;

namespace GymMate.Views;

public partial class SessionsPage : ContentPage
{
    public SessionsPage(SessionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SessionsViewModel vm)
            vm.AppearingCommand.Execute(null);
    }
}
