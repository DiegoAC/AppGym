using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.LocalNotification;

namespace GymMate.ViewModels;

public partial class RestTimerViewModel : ObservableObject
{
    private System.Timers.Timer? _timer;

    [ObservableProperty]
    private int selectedSeconds = 5;

    [ObservableProperty]
    private int remainingSeconds;

    partial void OnSelectedSecondsChanged(int value)
    {
        RemainingSeconds = value;
    }

    [RelayCommand]
    private void Start()
    {
        RemainingSeconds = SelectedSeconds;
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += TimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (RemainingSeconds > 0)
        {
            MainThread.BeginInvokeOnMainThread(() => RemainingSeconds--);
            return;
        }

        _timer?.Stop();
        _timer = null;
        ShowNotification();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("..");
        });
    }

    private void ShowNotification()
    {
        var request = new NotificationRequest
        {
            NotificationId = 1000,
            Title = "GymMate",
            Description = "Tiempo de descanso finalizado",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now
            }
        };
        NotificationCenter.Current.Show(request);
    }
}
