namespace GymMate.Services;

using GymMate.Models;
using GymMate.Data;
using Microsoft.Maui.Networking;

public interface IRoutinesService
{
    Task<IEnumerable<WorkoutRoutine>> GetRoutinesAsync(string uid);
    Task AddOrUpdateRoutineAsync(string uid, WorkoutRoutine routine);
    Task DeleteRoutineAsync(string uid, string routineId);
    Task SyncPendingAsync(string uid);
    event EventHandler? RoutinesChanged;
}

public class RoutinesService : IRoutinesService
{
    private readonly LocalDbService _localDb;
    private readonly IAnalyticsService _analytics;
    private readonly IRealtimeDbService _db;
    private string? _currentUid;

    public event EventHandler? RoutinesChanged;

    public RoutinesService(LocalDbService localDb, IAnalyticsService analytics, IRealtimeDbService db)
    {
        _localDb = localDb;
        _analytics = analytics;
        _db = db;
        _db.RoutinesChanged += async (_, _) =>
        {
            if (!string.IsNullOrEmpty(_currentUid))
            {
                var routines = await _db.GetRoutinesAsync(_currentUid);
                foreach (var r in routines)
                    await _localDb.SaveRoutineAsync(r);
                RoutinesChanged?.Invoke(this, EventArgs.Empty);
            }
        };
    }

    public async Task<IEnumerable<WorkoutRoutine>> GetRoutinesAsync(string uid)
    {
        if (!Connectivity.Current.IsConnected)
            return await _localDb.GetCachedRoutinesAsync();

        _currentUid = uid;
        var snapshot = await _db.GetRoutinesAsync(uid);
        foreach (var r in snapshot)
            await _localDb.SaveRoutineAsync(r);
        return snapshot;
    }

    public async Task AddOrUpdateRoutineAsync(string uid, WorkoutRoutine routine)
    {
        if (!Connectivity.Current.IsConnected)
        {
            await _localDb.SaveRoutineAsync(routine, true);
            await _analytics.LogEventAsync("routine_created");
            RoutinesChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        await _db.AddOrUpdateRoutineAsync(uid, routine);
        await _localDb.SaveRoutineAsync(routine);
        await _analytics.LogEventAsync("routine_created");
        RoutinesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DeleteRoutineAsync(string uid, string routineId)
    {
        if (!Connectivity.Current.IsConnected)
        {
            await _localDb.DeleteRoutineAsync(routineId);
            RoutinesChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        await _db.DeleteRoutineAsync(uid, routineId);
        await _localDb.DeleteRoutineAsync(routineId);
        RoutinesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SyncPendingAsync(string uid)
    {
        if (!Connectivity.Current.IsConnected)
            return;

        var pending = await _localDb.GetPendingRoutineDtosAsync();
        foreach (var dto in pending)
        {
            var routine = new WorkoutRoutine
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                CreatedUtc = new DateTime(dto.CreatedUtc, DateTimeKind.Utc)
            };
            await _db.AddOrUpdateRoutineAsync(uid, routine);
            await _localDb.SaveRoutineAsync(routine);
        }
    }
}
