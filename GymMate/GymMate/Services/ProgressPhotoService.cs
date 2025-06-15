namespace GymMate.Services;

using GymMate.Models;

public interface IProgressPhotoService
{
    Task UploadAsync(FileResult file, string? caption);
    IAsyncEnumerable<ProgressPhoto> GetPhotosAsync(string? uid = null);
    Task DeleteAsync(string photoId);
}

public class ProgressPhotoService : IProgressPhotoService
{
    private readonly IFirebaseAuthService _auth;
    private static readonly Dictionary<string, Dictionary<string, ProgressPhoto>> _photos = new();

    public ProgressPhotoService(IFirebaseAuthService auth)
    {
        _auth = auth;
    }

    public Task UploadAsync(FileResult file, string? caption)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || file == null)
            return Task.CompletedTask;

        if (!_photos.TryGetValue(uid, out var dict))
        {
            dict = new();
            _photos[uid] = dict;
        }

        var id = Guid.NewGuid().ToString();
        dict[id] = new ProgressPhoto
        {
            Id = id,
            Url = file.FullPath ?? string.Empty,
            UploadedUtc = DateTime.UtcNow,
            Caption = caption
        };

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<ProgressPhoto> GetPhotosAsync(string? uid = null)
    {
        uid ??= _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) yield break;

        if (_photos.TryGetValue(uid, out var dict))
        {
            foreach (var p in dict.Values.OrderByDescending(p => p.UploadedUtc))
            {
                yield return p;
                await Task.Yield();
            }
        }
    }

    public Task DeleteAsync(string photoId)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return Task.CompletedTask;
        if (_photos.TryGetValue(uid, out var dict))
            dict.Remove(photoId);
        return Task.CompletedTask;
    }
}
