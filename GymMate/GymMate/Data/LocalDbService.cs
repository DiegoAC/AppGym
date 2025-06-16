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
}
