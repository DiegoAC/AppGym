using GymMate.ViewModels;

namespace GymMate.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SettingsViewModel vm)
            vm.AppearingCommand.Execute(null);
    }

    private async void OnLanguageTapped(object sender, EventArgs e)
    {
        if (BindingContext is not SettingsViewModel vm)
            return;

        var picker = new Picker
        {
            ItemsSource = vm.LanguageNames,
            SelectedItem = vm.CurrentLanguageName
        };

        picker.SelectedIndexChanged += async (_, __) =>
        {
            await vm.ChangeLanguageCommand.ExecuteAsync(picker.SelectedItem);
            if (Application.Current.MainPage is IPage page)
                page.HideBottomSheet();
        };

        if (Application.Current.MainPage is IPage p)
            await p.ShowBottomSheetAsync(picker);
    }
}
