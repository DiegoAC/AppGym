using GymMate.ViewModels;

namespace GymMate.Views;

public partial class CommentsPage : ContentPage
{
    public CommentsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GymMate.ViewModels.CommentsViewModel vm)
            vm.AppearingCommand.Execute(null);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is GymMate.ViewModels.CommentsViewModel vm)
        {
            vm.Comments.CollectionChanged += Comments_CollectionChanged;
        }
    }

    private void Comments_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            var item = e.NewItems[e.NewItems.Count - 1];
            commentsView.ScrollTo(item, ScrollToPosition.End, true);
        }
    }
}
