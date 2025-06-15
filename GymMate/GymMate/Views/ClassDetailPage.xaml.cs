using GymMate.ViewModels;

namespace GymMate.Views;

public partial class ClassDetailPage : ContentPage
{
    public ClassDetailPage(ClassDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
