using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using GymMate.Models;
using GymMate.Services;

namespace GymMate.ViewModels;

public partial class ProgressViewModel : ObservableObject
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<string> Exercises { get; } = new();

    [ObservableProperty]
    private string? selectedExercise;

    [ObservableProperty]
    private Chart? chart;

    public ProgressViewModel(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [RelayCommand]
    public async Task AppearingAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;
        var sessions = await _db.GetSessionsAsync(uid);
        var names = sessions.SelectMany(s => s.Sets).Select(s => s.ExerciseName).Distinct().OrderBy(x => x);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Exercises.Clear();
            foreach (var n in names) Exercises.Add(n);
            if (SelectedExercise == null && Exercises.Count > 0) SelectedExercise = Exercises[0];
        });
        await UpdateChartAsync(sessions);
    }

    partial void OnSelectedExerciseChanged(string? value)
    {
        _ = RefreshChartAsync();
    }

    private async Task RefreshChartAsync()
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(SelectedExercise)) return;
        var sessions = await _db.GetSessionsAsync(uid);
        await UpdateChartAsync(sessions);
    }

    private Task UpdateChartAsync(IEnumerable<WorkoutSession> sessions)
    {
        if (string.IsNullOrEmpty(SelectedExercise)) return Task.CompletedTask;
        var from = DateTime.UtcNow.AddDays(-30);
        var data = sessions.Where(s => s.DateUtc >= from)
            .SelectMany(s => s.Sets.Where(set => set.ExerciseName == SelectedExercise)
            .Select(set => new { s.DateUtc, set.WeightKg }))
            .OrderBy(d => d.DateUtc)
            .ToList();
        var entries = data.Select(d => new ChartEntry((float)d.WeightKg)
        {
            Label = d.DateUtc.ToString("MM-dd"),
            ValueLabel = d.WeightKg.ToString("0.##")
        }).ToList();
        Chart = new LineChart { Entries = entries, LineMode = LineMode.Straight };
        return Task.CompletedTask;
    }
}
