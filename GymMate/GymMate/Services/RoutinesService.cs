namespace GymMate.Services;

using GymMate.Models;
using GymMate.Data;
using Microsoft.Maui.Networking;
using Plugin.Firebase.Firestore;

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
    private readonly IFirebaseFirestore _firestore;
    private readonly LocalDbService _localDb;

    public event EventHandler? RoutinesChanged;

    public RoutinesService(IFirebaseFirestore firestore, LocalDbService localDb)
    {
        _firestore = firestore;
        _localDb = localDb;
    }

    private CollectionReference GetCollection(string uid)
        => _firestore.Collection($"userProfiles/{uid}/routines");

    public async Task<IEnumerable<WorkoutRoutine>> GetRoutinesAsync(string uid)
    {
        if (!Connectivity.Current.IsConnected)
            return await _localDb.GetCachedRoutinesAsync();

        var snapshot = await GetCollection(uid).GetAsync();
        var list = new List<WorkoutRoutine>();
        foreach (var doc in snapshot.Documents)
        {
            var routine = doc.ToObject<WorkoutRoutine>();
            if (routine != null)
            {
                routine.Id = doc.Id;
                list.Add(routine);
                await _localDb.SaveRoutineAsync(routine);
            }
        }
        return list;
    }

    public async Task AddOrUpdateRoutineAsync(string uid, WorkoutRoutine routine)
    {
        if (!Connectivity.Current.IsConnected)
        {
            await _localDb.SaveRoutineAsync(routine, true);
            RoutinesChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        await GetCollection(uid).Document(routine.Id).SetAsync(routine);
        await _localDb.SaveRoutineAsync(routine);
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

        await GetCollection(uid).Document(routineId).DeleteAsync();
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
            await GetCollection(uid).Document(routine.Id).SetAsync(routine);
            await _localDb.SaveRoutineAsync(routine);
        }
    }
}
