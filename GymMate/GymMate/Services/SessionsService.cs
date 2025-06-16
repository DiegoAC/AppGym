namespace GymMate.Services;

using GymMate.Models;
using GymMate.Data;
using Microsoft.Maui.Networking;
using System.Text.Json;
using System.Linq;

public interface ISessionsService
{
    Task<IEnumerable<WorkoutSession>> GetSessionsAsync(string uid, string? routineId = null);
    Task AddSessionAsync(string uid, WorkoutSession session);
    Task DeleteSessionAsync(string uid, string sessionId);
    Task SyncPendingAsync(string uid);
    event EventHandler? SessionsChanged;
}

public class SessionsService : ISessionsService
{
    private readonly LocalDbService _localDb;
    private readonly IAnalyticsService _analytics;
    private readonly IRealtimeDbService _db;
    private string? _currentUid;

    public event EventHandler? SessionsChanged;

    public SessionsService(LocalDbService localDb, IAnalyticsService analytics, IRealtimeDbService db)
    {
        _localDb = localDb;
        _analytics = analytics;
        _db = db;
        _db.SessionsChanged += async (_, _) =>
        {
            if (!string.IsNullOrEmpty(_currentUid))
            {
                var sessions = await _db.GetSessionsAsync(_currentUid);
                foreach (var s in sessions)
                    await _localDb.SaveSessionAsync(s, false);
                SessionsChanged?.Invoke(this, EventArgs.Empty);
            }
        };
    }


    public async Task<IEnumerable<WorkoutSession>> GetSessionsAsync(string uid, string? routineId = null)
    {
        if (!Connectivity.Current.IsConnected)
            return await _localDb.GetCachedSessionsAsync(routineId);

        _currentUid = uid;
        var all = await _db.GetSessionsAsync(uid);
        IEnumerable<WorkoutSession> result = all;
        if (!string.IsNullOrEmpty(routineId))
            result = all.Where(s => s.RoutineId == routineId);
        foreach (var s in result)
            await _localDb.SaveSessionAsync(s, false);
        return result.ToList();
    }

    public async Task AddSessionAsync(string uid, WorkoutSession session)
    {
        if (!Connectivity.Current.IsConnected)
        {
            await _localDb.SaveSessionAsync(session, true);
            await _analytics.LogEventAsync("session_created");
            SessionsChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        await _db.AddSessionAsync(uid, session);
        await _localDb.SaveSessionAsync(session, false);
        await _analytics.LogEventAsync("session_created");
        SessionsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DeleteSessionAsync(string uid, string sessionId)
    {
        if (!Connectivity.Current.IsConnected)
        {
            await _localDb.DeleteSessionAsync(sessionId);
            SessionsChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        await _db.DeleteSessionAsync(uid, sessionId);
        await _localDb.DeleteSessionAsync(sessionId);
        SessionsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SyncPendingAsync(string uid)
    {
        if (!Connectivity.Current.IsConnected)
            return;

        var pending = await _localDb.GetPendingSessionsAsync();
        foreach (var dto in pending)
        {
            var session = new WorkoutSession
            {
                Id = dto.Id,
                RoutineId = dto.RoutineId,
                DateUtc = new DateTime(dto.DateUtc, DateTimeKind.Utc),
                Sets = JsonSerializer.Deserialize<List<SetRecord>>(dto.JsonSets) ?? new List<SetRecord>()
            };
            await _db.AddSessionAsync(uid, session);
            await _localDb.SaveSessionAsync(session, false);
        }
    }
}
