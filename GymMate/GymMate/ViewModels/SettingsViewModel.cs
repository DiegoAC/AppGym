using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Storage;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class SettingsViewModel(INotificationService notifications, IFirebaseAuthService auth, IPreferences preferences, IAnalyticsService analytics) : ObservableObject
{
    private readonly INotificationService _notifications = notifications;
    private readonly IFirebaseAuthService _auth = auth;
    private readonly IPreferences _preferences = preferences;
    private readonly IAnalyticsService _analytics = analytics;

    [ObservableProperty]
    private bool isFeedPushEnabled;

    [ObservableProperty]
    private bool isDailyReminderEnabled;

    [ObservableProperty]
    private TimeSpan reminderTime;

    [RelayCommand]
    private void Appearing()
    {
        IsFeedPushEnabled = _preferences.Get(nameof(IsFeedPushEnabled), false);
        IsDailyReminderEnabled = _preferences.Get(nameof(IsDailyReminderEnabled), false);
        var time = _preferences.Get(nameof(ReminderTime), "08:00:00");
        TimeSpan.TryParse(time, out reminderTime);
        OnPropertyChanged(nameof(ReminderTime));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        _preferences.Set(nameof(IsFeedPushEnabled), IsFeedPushEnabled);
        _preferences.Set(nameof(IsDailyReminderEnabled), IsDailyReminderEnabled);
        _preferences.Set(nameof(ReminderTime), ReminderTime.ToString());

        var uid = _auth.CurrentUserUid;
        if (!string.IsNullOrEmpty(uid))
        {
            if (IsFeedPushEnabled)
                await _notifications.SubscribeAsync($"user_{uid}");
            else
                await _notifications.UnsubscribeAsync($"user_{uid}");
        }

        if (IsDailyReminderEnabled)
        {
            var when = DateTime.Today.AddDays(1).Add(ReminderTime);
            await _notifications.ScheduleLocalAsync(when, "Hora de entrenar", "¡No olvides tu sesión de hoy!", "daily_reminder");
        }
        else
        {
            await _notifications.CancelLocalAsync("daily_reminder");
        }

        await _analytics.LogEventAsync("settings_saved");
        await Toast.Make("Ajustes guardados").Show();
    }
}
