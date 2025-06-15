namespace GymMate.Services;

using GymMate.Models;

public interface IFeedService
{
    IAsyncEnumerable<FeedPost> GetLatestAsync(int pageSize = 20, DateTime? startAfter = null);
    Task CreatePostAsync(ProgressPhoto photo);
    Task LikeAsync(string postId, string uid);
    Task UnlikeAsync(string postId, string uid);
    Task<bool> IsLikedAsync(string postId, string uid);
    event EventHandler<FeedPost>? PostUpdated;
}

public class FeedService : IFeedService
{
    private static readonly Dictionary<string, FeedPost> _posts = new();
    private static readonly Dictionary<string, HashSet<string>> _likes = new();
    private readonly IFirebaseAuthService _auth;

    public event EventHandler<FeedPost>? PostUpdated;

    public FeedService(IFirebaseAuthService auth)
    {
        _auth = auth;
    }

    public async IAsyncEnumerable<FeedPost> GetLatestAsync(int pageSize = 20, DateTime? startAfter = null)
    {
        var list = _posts.Values
            .OrderByDescending(p => p.UploadedUtc)
            .ToList();
        if (startAfter != null)
            list = list.Where(p => p.UploadedUtc < startAfter).ToList();
        foreach (var p in list.Take(pageSize))
        {
            yield return p;
            await Task.Yield();
        }
    }

    public Task CreatePostAsync(ProgressPhoto photo)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return Task.CompletedTask;
        var post = new FeedPost
        {
            AuthorUid = uid,
            PhotoUrl = photo.Url,
            Caption = photo.Caption,
            UploadedUtc = DateTime.UtcNow,
            LikesCount = 0
        };
        _posts[post.Id] = post;
        PostUpdated?.Invoke(this, post);
        return Task.CompletedTask;
    }

    public Task LikeAsync(string postId, string uid)
    {
        if (!_posts.TryGetValue(postId, out var post))
            return Task.CompletedTask;
        if (!_likes.TryGetValue(postId, out var set))
        {
            set = new();
            _likes[postId] = set;
        }
        if (set.Add(uid))
        {
            post.LikesCount = set.Count;
            PostUpdated?.Invoke(this, post);
        }
        return Task.CompletedTask;
    }

    public Task UnlikeAsync(string postId, string uid)
    {
        if (!_posts.TryGetValue(postId, out var post))
            return Task.CompletedTask;
        if (_likes.TryGetValue(postId, out var set) && set.Remove(uid))
        {
            post.LikesCount = set.Count;
            PostUpdated?.Invoke(this, post);
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsLikedAsync(string postId, string uid)
    {
        if (_likes.TryGetValue(postId, out var set))
            return Task.FromResult(set.Contains(uid));
        return Task.FromResult(false);
    }
}

