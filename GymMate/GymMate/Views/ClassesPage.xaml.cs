using GymMate.ViewModels;

namespace GymMate.Views;

public partial class ClassesPage : ContentPage
{
    public ClassesPage(ClassesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ClassesViewModel vm)
            vm.AppearingCommand.Execute(null);
    }
}
