using GymMate.ViewModels;

namespace GymMate.Views;

public partial class SessionEditorPage : ContentPage
{
    public SessionEditorPage(SessionEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SessionEditorViewModel vm)
            vm.AppearingCommand.Execute(null);
    }
}
