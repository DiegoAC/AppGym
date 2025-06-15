using GymMate.ViewModels;

namespace GymMate.Views;

public partial class RoutinesPage : ContentPage
{
    public RoutinesPage(RoutinesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RoutinesViewModel vm)
            vm.AppearingCommand.Execute(null);
    }
}
