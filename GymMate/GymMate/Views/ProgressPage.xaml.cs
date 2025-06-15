using GymMate.ViewModels;

namespace GymMate.Views;

public partial class ProgressPage : ContentPage
{
    public ProgressPage(ProgressViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProgressViewModel vm)
            vm.AppearingCommand.Execute(null);
    }
}
