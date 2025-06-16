namespace GymMate.Services;

using GymMate.Models;
using GymMate.Data;
using Microsoft.Maui.Networking;
using Plugin.Firebase.Firestore;
using System.Text.Json;

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
    private readonly IFirebaseFirestore _firestore;
    private readonly LocalDbService _localDb;

    public event EventHandler? SessionsChanged;

    public SessionsService(IFirebaseFirestore firestore, LocalDbService localDb)
    {
        _firestore = firestore;
        _localDb = localDb;
    }

    private CollectionReference GetCollection(string uid)
        => _firestore.Collection($"userProfiles/{uid}/sessions");

    public async Task<IEnumerable<WorkoutSession>> GetSessionsAsync(string uid, string? routineId = null)
    {
        if (!Connectivity.Current.IsConnected)
            return await _localDb.GetCachedSessionsAsync(routineId);

        Query query = GetCollection(uid);
        if (!string.IsNullOrEmpty(routineId))
            query = query.WhereEqualsTo("routineId", routineId);
        var snapshot = await query.GetAsync();
        var list = new List<WorkoutSession>();
        foreach (var doc in snapshot.Documents)
        {
            var session = doc.ToObject<WorkoutSession>();
            if (session != null)
            {
                session.Id = doc.Id;
                list.Add(session);
                await _localDb.SaveSessionAsync(session, false);
            }
        }
        return list;
    }

    public async Task AddSessionAsync(string uid, WorkoutSession session)
    {
        if (!Connectivity.Current.IsConnected)
        {
            await _localDb.SaveSessionAsync(session, true);
            SessionsChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        await GetCollection(uid).Document(session.Id).SetAsync(session);
        await _localDb.SaveSessionAsync(session, false);
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

        await GetCollection(uid).Document(sessionId).DeleteAsync();
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
            await GetCollection(uid).Document(session.Id).SetAsync(session);
            await _localDb.SaveSessionAsync(session, false);
        }
    }
}
