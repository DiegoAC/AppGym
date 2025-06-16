using GymMate.ViewModels;
using System.Threading.Tasks;

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

        foreach (var v in RoutinesCV.VisibleViews)
        {
            v.TranslationX = 60;
            v.Opacity = 0;
        }
        foreach (var v in RoutinesCV.VisibleViews)
        {
            _ = Task.WhenAll(v.TranslateTo(0, 0, 250), v.FadeTo(1, 250));
        }
    }
}
