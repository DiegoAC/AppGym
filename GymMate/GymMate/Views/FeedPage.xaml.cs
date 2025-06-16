using GymMate.ViewModels;
using System.Threading.Tasks;

namespace GymMate.Views;

public partial class FeedPage : ContentPage
{
    public FeedPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GymMate.ViewModels.FeedViewModel vm)
            await vm.AppearingAsync();

        foreach (var element in PostsCollection.VisibleViews)
        {
            if (element is VisualElement ve)
            {
                ve.TranslationY = 60;
                ve.Opacity = 0;
            }
        }

        await Task.Delay(100);

        foreach (var element in PostsCollection.VisibleViews)
        {
            if (element is VisualElement ve)
                await Task.WhenAll(ve.TranslateTo(0, 0, 300), ve.FadeTo(1, 300));
        }
    }
}
