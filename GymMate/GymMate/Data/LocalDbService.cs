namespace GymMate.Data;

using SQLite;
using GymMate.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

public class LocalDbService
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public LocalDbService()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "gymmate.db");
        _db = new SQLiteAsyncConnection(path);
    }

    public async Task InitAsync()
    {
        if (_initialized) return;
        await _db.CreateTableAsync<FeedPostDto>();
        await _db.CreateTableAsync<WorkoutRoutineDto>();
        await _db.CreateTableAsync<WorkoutSessionDto>();
        _initialized = true;
    }

    public async Task SavePostsAsync(IEnumerable<FeedPost> posts)
    {
        await InitAsync();
        foreach (var p in posts)
        {
            var dto = new FeedPostDto
            {
                Id = p.Id,
                AuthorUid = p.AuthorUid,
                PhotoUrl = p.PhotoUrl,
                Caption = p.Caption,
                UploadedUtc = p.UploadedUtc,
                LikesCount = p.LikesCount
            };
            await _db.InsertOrReplaceAsync(dto);
        }
    }

    public async Task<List<FeedPost>> GetCachedPostsAsync(int take = 30)
    {
        await InitAsync();
        var dtos = await _db.Table<FeedPostDto>()
            .OrderByDescending(x => x.UploadedUtc)
            .Take(take)
            .ToListAsync();
        return dtos.Select(d => new FeedPost
        {
            Id = d.Id,
            AuthorUid = d.AuthorUid,
            PhotoUrl = d.PhotoUrl,
            Caption = d.Caption,
            UploadedUtc = d.UploadedUtc,
            LikesCount = d.LikesCount
        }).ToList();
    }

    public Task SaveRoutineAsync(WorkoutRoutine routine)
        => SaveRoutineAsync(routine, false);

    public async Task SaveRoutineAsync(WorkoutRoutine routine, bool pendingSync)
    {
        await InitAsync();
        var dto = new WorkoutRoutineDto
        {
            Id = routine.Id,
            Name = routine.Name,
            Description = routine.Description,
            CreatedUtc = routine.CreatedUtc.Ticks,
            IsPendingSync = pendingSync
        };
        await _db.InsertOrReplaceAsync(dto);
    }

    public async Task DeleteRoutineAsync(string id)
    {
        await InitAsync();
        await _db.DeleteAsync<WorkoutRoutineDto>(id);
    }

    public async Task<List<WorkoutRoutine>> GetCachedRoutinesAsync()
    {
        await InitAsync();
        var dtos = await _db.Table<WorkoutRoutineDto>()
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync();
        return dtos.Select(d => new WorkoutRoutine
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            CreatedUtc = new DateTime(d.CreatedUtc, DateTimeKind.Utc)
        }).ToList();
    }

    public async Task<List<WorkoutRoutineDto>> GetPendingRoutineDtosAsync()
    {
        await InitAsync();
        return await _db.Table<WorkoutRoutineDto>()
            .Where(r => r.IsPendingSync)
            .ToListAsync();
    }

    public async Task SaveSessionAsync(WorkoutSession session, bool pendingSync)
    {
        await InitAsync();
        var dto = new WorkoutSessionDto
        {
            Id = session.Id,
            RoutineId = session.RoutineId,
            DateUtc = session.DateUtc.Ticks,
            JsonSets = JsonSerializer.Serialize(session.Sets),
            IsPendingSync = pendingSync
        };
        await _db.InsertOrReplaceAsync(dto);
    }

    public async Task DeleteSessionAsync(string id)
    {
        await InitAsync();
        await _db.DeleteAsync<WorkoutSessionDto>(id);
    }

    public async Task<List<WorkoutSession>> GetCachedSessionsAsync(string? routineId = null)
    {
        await InitAsync();
        var query = _db.Table<WorkoutSessionDto>();
        if (!string.IsNullOrEmpty(routineId))
            query = query.Where(s => s.RoutineId == routineId);
        var dtos = await query.OrderByDescending(x => x.DateUtc).ToListAsync();
        return dtos.Select(d => new WorkoutSession
        {
            Id = d.Id,
            RoutineId = d.RoutineId,
            DateUtc = new DateTime(d.DateUtc, DateTimeKind.Utc),
            Sets = JsonSerializer.Deserialize<List<SetRecord>>(d.JsonSets) ?? new List<SetRecord>()
        }).ToList();
    }

    public async Task<List<WorkoutSessionDto>> GetPendingSessionsAsync()
    {
        await InitAsync();
        return await _db.Table<WorkoutSessionDto>()
            .Where(s => s.IsPendingSync)
            .ToListAsync();
    }
}
