namespace GymMate.Data;

using SQLite;
using GymMate.Models;
using Microsoft.Maui.Storage;

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
}
