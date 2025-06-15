﻿namespace GymMate.Views
{
    public partial class HomePage : ContentPage
    {
        int count = 0;

        public HomePage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnRestTimerClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("resttimer");
        }

        private async void OnRoutinesClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//routines");
        }

        private async void OnSessionsClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//sessions");
        }

        private async void OnClassesClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//classes");
        }

        private async void OnProgressClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("progress");
        }

        private async void OnPhotosClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//photos");
        }

        private async void OnUsersClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//userSearch");
        }

        private async void OnFeedClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//feed");
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("settings");
        }
    }
}
