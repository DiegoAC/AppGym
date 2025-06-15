namespace GymMate.Services;

public interface IRealtimeDbService
{
    Task SaveUserProfileAsync(string userId, object profile);
    Task<IEnumerable<Models.WorkoutRoutine>> GetRoutinesAsync(string userId);
    Task AddOrUpdateRoutineAsync(string userId, Models.WorkoutRoutine routine);
    Task DeleteRoutineAsync(string userId, string routineId);
    event EventHandler? RoutinesChanged;
}

public class RealtimeDbService : IRealtimeDbService
{
    private static readonly Dictionary<string, Dictionary<string, Models.WorkoutRoutine>> _routines = new();

    public event EventHandler? RoutinesChanged;

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
}
