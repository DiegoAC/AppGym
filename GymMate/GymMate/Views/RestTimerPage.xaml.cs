using GymMate.ViewModels;

namespace GymMate.Views;

public partial class RestTimerPage : ContentPage
{
    public RestTimerPage(RestTimerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
