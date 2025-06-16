using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;

using GymMate.Services;

namespace GymMate.ViewModels;

public partial class SettingsViewModel(INotificationService notifications) : ObservableObject
{
    private readonly INotificationService _notifications = notifications;

    [ObservableProperty]
    private bool dailyRemindersEnabled;

    [ObservableProperty]
    private TimeSpan dailyReminderTime = new(8,0,0);

    [ObservableProperty]
    private bool newRoutinesAlertsEnabled;

    partial void OnDailyRemindersEnabledChanged(bool value)
    {
        Preferences.Set(nameof(DailyRemindersEnabled), value);
        if (value)
            ScheduleReminder();
    }

    partial void OnDailyReminderTimeChanged(TimeSpan value)
    {
        Preferences.Set(nameof(DailyReminderTime), value.ToString());
        if (DailyRemindersEnabled)
            ScheduleReminder();
    }

    partial void OnNewRoutinesAlertsEnabledChanged(bool value)
    {
        Preferences.Set(nameof(NewRoutinesAlertsEnabled), value);
        if (value)
            _notifications.SubscribeAsync("new-routines");
        else
            _notifications.UnsubscribeAsync("new-routines");
    }

    [RelayCommand]
    private Task RequestPermissionsAsync() => _notifications.RequestPermissionsAsync();

    private void ScheduleReminder()
    {
        var when = DateTime.Today.Add(DailyReminderTime);
        if (when <= DateTime.Now)
            when = when.AddDays(1);
        _notifications.ScheduleLocalAsync(when, "GymMate", "Hora de entrenar", "daily_reminder");
    }

    public void Load()
    {
        DailyRemindersEnabled = Preferences.Get(nameof(DailyRemindersEnabled), false);
        var time = Preferences.Get(nameof(DailyReminderTime), "08:00:00");
        TimeSpan.TryParse(time, out dailyReminderTime);
        OnPropertyChanged(nameof(DailyReminderTime));
        NewRoutinesAlertsEnabled = Preferences.Get(nameof(NewRoutinesAlertsEnabled), false);
    }
}
