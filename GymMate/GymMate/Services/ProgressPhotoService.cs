namespace GymMate.Services;

using GymMate.Models;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Storage;
using System.IO;

public interface IProgressPhotoService
{
    Task UploadAsync(FileResult file, string? caption);
    IAsyncEnumerable<ProgressPhoto> GetPhotosAsync(string? uid = null);
    Task DeleteAsync(string photoId);
}

public class ProgressPhotoService : IProgressPhotoService
{
    private readonly IFirebaseAuthService _auth;
    private readonly IFirebaseStorage _storage = CrossFirebaseStorage.Current;
    private readonly IFirebaseFirestore _firestore = CrossFirebaseFirestore.Current;

    public ProgressPhotoService(IFirebaseAuthService auth)
    {
        _auth = auth;
    }

    public async Task UploadAsync(FileResult file, string? caption)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || file == null)
            return;

        var id = Guid.NewGuid().ToString();

        using var stream = await file.OpenReadAsync();
        var storageRef = _storage
            .GetRootReference()
            .GetChild($"users/{uid}/photos/{id}");
        await storageRef.PutStream(stream).AwaitAsync();
        var url = await storageRef.GetDownloadUrlAsync();

        var photo = new ProgressPhoto
        {
            Id = id,
            Url = url,
            UploadedUtc = DateTime.UtcNow,
            Caption = caption
        };

        await _firestore.Document($"users/{uid}/photos/{id}").SetAsync(photo);
    }

    public async IAsyncEnumerable<ProgressPhoto> GetPhotosAsync(string? uid = null)
    {
        uid ??= _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) yield break;

        var snapshot = await _firestore
            .Collection($"users/{uid}/photos")
            .OrderByDescending("UploadedUtc")
            .GetAsync();

        foreach (var doc in snapshot.Documents)
        {
            var photo = doc.ToObject<ProgressPhoto>();
            if (photo != null)
            {
                photo.Id = doc.Id;
                yield return photo;
                await Task.Yield();
            }
        }
    }

    public async Task DeleteAsync(string photoId)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;

        var storageRef = _storage
            .GetRootReference()
            .GetChild($"users/{uid}/photos/{photoId}");
        await storageRef.DeleteAsync();
        await _firestore.Document($"users/{uid}/photos/{photoId}").DeleteAsync();
    }
}
