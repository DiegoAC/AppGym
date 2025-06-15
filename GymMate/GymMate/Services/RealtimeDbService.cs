namespace GymMate.Services;

public interface IRealtimeDbService
{
    Task SaveUserProfileAsync(string userId, object profile);
    Task<IEnumerable<Models.WorkoutRoutine>> GetRoutinesAsync(string userId);
    Task AddOrUpdateRoutineAsync(string userId, Models.WorkoutRoutine routine);
    Task DeleteRoutineAsync(string userId, string routineId);
    Task<IEnumerable<Models.WorkoutSession>> GetSessionsAsync(string userId);
    Task AddSessionAsync(string userId, Models.WorkoutSession session);
    Task DeleteSessionAsync(string userId, string sessionId);
    event EventHandler? RoutinesChanged;
    event EventHandler? SessionsChanged;
}

public class RealtimeDbService : IRealtimeDbService
{
    private static readonly Dictionary<string, Dictionary<string, Models.WorkoutRoutine>> _routines = new();
    private static readonly Dictionary<string, Dictionary<string, Models.WorkoutSession>> _sessions = new();

    public event EventHandler? RoutinesChanged;
    public event EventHandler? SessionsChanged;

    public Task SaveUserProfileAsync(string userId, object profile)
    {
        // TODO: Integrate Firebase realtime database
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Models.WorkoutRoutine>> GetRoutinesAsync(string userId)
    {
        if (_routines.TryGetValue(userId, out var dict))
        {
            return Task.FromResult<IEnumerable<Models.WorkoutRoutine>>(dict.Values.ToList());
        }

        return Task.FromResult<IEnumerable<Models.WorkoutRoutine>>(Array.Empty<Models.WorkoutRoutine>());
    }

    public Task AddOrUpdateRoutineAsync(string userId, Models.WorkoutRoutine routine)
    {
        if (!_routines.TryGetValue(userId, out var dict))
        {
            dict = new();
            _routines[userId] = dict;
        }

        dict[routine.Id] = routine;
        RoutinesChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task DeleteRoutineAsync(string userId, string routineId)
    {
        if (_routines.TryGetValue(userId, out var dict))
        {
            dict.Remove(routineId);
        }

        RoutinesChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Models.WorkoutSession>> GetSessionsAsync(string userId)
    {
        if (_sessions.TryGetValue(userId, out var dict))
        {
            return Task.FromResult<IEnumerable<Models.WorkoutSession>>(dict.Values.ToList());
        }

        return Task.FromResult<IEnumerable<Models.WorkoutSession>>(Array.Empty<Models.WorkoutSession>());
    }

    public Task AddSessionAsync(string userId, Models.WorkoutSession session)
    {
        if (!_sessions.TryGetValue(userId, out var dict))
        {
            dict = new();
            _sessions[userId] = dict;
        }

        dict[session.Id] = session;
        SessionsChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task DeleteSessionAsync(string userId, string sessionId)
    {
        if (_sessions.TryGetValue(userId, out var dict))
        {
            dict.Remove(sessionId);
        }

        SessionsChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
