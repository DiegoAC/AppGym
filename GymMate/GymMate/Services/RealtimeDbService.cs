namespace GymMate.Services;

using Plugin.Firebase.Firestore;
using System.Reactive.Linq;

public interface IRealtimeDbService
{
    Task SaveUserProfileAsync(string userId, object profile);
    Task<IEnumerable<Models.WorkoutRoutine>> GetRoutinesAsync(string userId);
    Task AddOrUpdateRoutineAsync(string userId, Models.WorkoutRoutine routine);
    Task DeleteRoutineAsync(string userId, string routineId);
    Task<IEnumerable<Models.WorkoutSession>> GetSessionsAsync(string userId);
    Task AddSessionAsync(string userId, Models.WorkoutSession session);
    Task DeleteSessionAsync(string userId, string sessionId);
    Task SaveDeviceTokenAsync(string userId, string token);
    event EventHandler? RoutinesChanged;
    event EventHandler? SessionsChanged;
}

public class RealtimeDbService : IRealtimeDbService
{
    private readonly IFirebaseFirestore _firestore;
    private readonly Dictionary<string, IDisposable> _routineSubs = new();
    private readonly Dictionary<string, IDisposable> _sessionSubs = new();

    public event EventHandler? RoutinesChanged;
    public event EventHandler? SessionsChanged;

    public RealtimeDbService(IFirebaseFirestore firestore)
    {
        _firestore = firestore;
    }

    private CollectionReference RoutineCollection(string uid)
        => _firestore.Collection($"userProfiles/{uid}/routines");

    private CollectionReference SessionCollection(string uid)
        => _firestore.Collection($"userProfiles/{uid}/sessions");

    private void EnsureRoutineListener(string uid)
    {
        if (_routineSubs.ContainsKey(uid))
            return;
        _routineSubs[uid] = RoutineCollection(uid)
            .AsObservable()
            .Subscribe(_ => RoutinesChanged?.Invoke(this, EventArgs.Empty));
    }

    private void EnsureSessionListener(string uid)
    {
        if (_sessionSubs.ContainsKey(uid))
            return;
        _sessionSubs[uid] = SessionCollection(uid)
            .AsObservable()
            .Subscribe(_ => SessionsChanged?.Invoke(this, EventArgs.Empty));
    }

    public Task SaveUserProfileAsync(string userId, object profile)
    {
        return _firestore.Collection("userProfiles")
            .Document(userId)
            .SetAsync(profile);
    }

    public async Task<IEnumerable<Models.WorkoutRoutine>> GetRoutinesAsync(string userId)
    {
        EnsureRoutineListener(userId);
        var snapshot = await RoutineCollection(userId).GetAsync();
        var list = new List<Models.WorkoutRoutine>();
        foreach (var doc in snapshot.Documents)
        {
            var routine = doc.ToObject<Models.WorkoutRoutine>();
            if (routine != null)
            {
                routine.Id = doc.Id;
                list.Add(routine);
            }
        }
        return list;
    }

    public Task AddOrUpdateRoutineAsync(string userId, Models.WorkoutRoutine routine)
    {
        EnsureRoutineListener(userId);
        return RoutineCollection(userId).Document(routine.Id).SetAsync(routine);
    }

    public Task DeleteRoutineAsync(string userId, string routineId)
    {
        EnsureRoutineListener(userId);
        return RoutineCollection(userId).Document(routineId).DeleteAsync();
    }

    public async Task<IEnumerable<Models.WorkoutSession>> GetSessionsAsync(string userId)
    {
        EnsureSessionListener(userId);
        var snapshot = await SessionCollection(userId).GetAsync();
        var list = new List<Models.WorkoutSession>();
        foreach (var doc in snapshot.Documents)
        {
            var session = doc.ToObject<Models.WorkoutSession>();
            if (session != null)
            {
                session.Id = doc.Id;
                list.Add(session);
            }
        }
        return list;
    }

    public Task AddSessionAsync(string userId, Models.WorkoutSession session)
    {
        EnsureSessionListener(userId);
        return SessionCollection(userId).Document(session.Id).SetAsync(session);
    }

    public Task DeleteSessionAsync(string userId, string sessionId)
    {
        EnsureSessionListener(userId);
        return SessionCollection(userId).Document(sessionId).DeleteAsync();
    }

    public Task SaveDeviceTokenAsync(string userId, string token)
    {
        return _firestore.Collection($"userProfiles/{userId}/deviceTokens")
            .Document(token)
            .SetAsync(new { token });
    }
}
