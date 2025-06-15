using GymMate.ViewModels;

namespace GymMate.Views;

public partial class RoutineDetailPage : ContentPage
{
    public RoutineDetailPage(RoutineDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
