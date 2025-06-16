namespace GymMate.Services;

using GymMate.Models;
using GymMate.Data;
using Microsoft.Maui.Networking;

public interface IFeedService
{
    IAsyncEnumerable<FeedPost> GetLatestAsync(int pageSize = 20, DateTime? startAfter = null);
    Task CreatePostAsync(ProgressPhoto photo);
    Task LikeAsync(string postId, string uid);
    Task UnlikeAsync(string postId, string uid);
    Task<bool> IsLikedAsync(string postId, string uid);
    IAsyncEnumerable<FeedComment> GetCommentsAsync(string postId);
    Task AddCommentAsync(string postId, string text, string uid);
    Task DeleteCommentAsync(string postId, string commentId, string uid);
    event EventHandler<string>? CommentsChanged;
    event EventHandler<FeedPost>? PostUpdated;
}

public class FeedService : IFeedService
{
    private static readonly Dictionary<string, FeedPost> _posts = new();
    private static readonly Dictionary<string, HashSet<string>> _likes = new();
    private static readonly Dictionary<string, Dictionary<string, FeedComment>> _comments = new();
    private readonly IFirebaseAuthService _auth;
    private readonly INotificationService _notifications;
    private readonly IFollowService _follow;
    private readonly LocalDbService _localDb;

    public event EventHandler<FeedPost>? PostUpdated;
    public event EventHandler<string>? CommentsChanged;

    public FeedService(IFirebaseAuthService auth, INotificationService notifications, IFollowService follow, LocalDbService localDb)
    {
        _auth = auth;
        _notifications = notifications;
        _follow = follow;
        _localDb = localDb;
    }

    public async IAsyncEnumerable<FeedPost> GetLatestAsync(int pageSize = 20, DateTime? startAfter = null)
    {
        var cached = await _localDb.GetCachedPostsAsync(pageSize);
        if (cached.Any() && !Connectivity.Current.IsConnected)
        {
            foreach (var c in cached)
            {
                yield return c;
                await Task.Yield();
            }
            yield break;
        }

        var meUid = _auth.CurrentUserUid;
        var followUids = new List<string>();

        if (!string.IsNullOrEmpty(meUid))
        {
            await foreach (var f in _follow.GetFollowingAsync(meUid))
            {
                followUids.Add(f);
                if (followUids.Count >= 100)
                    break;
            }
        }

        IEnumerable<FeedPost> query = _posts.Values;

        if (followUids.Count > 0)
            query = query.Where(p => followUids.Contains(p.AuthorUid));

        query = query.OrderByDescending(p => p.UploadedUtc);

        if (startAfter != null)
            query = query.Where(p => p.UploadedUtc < startAfter.Value);

        var list = query.Take(pageSize).ToList();
        await _localDb.SavePostsAsync(list);

        foreach (var post in list)
        {
            yield return post;
            await Task.Yield();
        }
    }

    public async Task CreatePostAsync(ProgressPhoto photo)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return Task.CompletedTask;
        var profile = await _follow.GetProfileAsync(uid) ?? new UserProfile { Uid = uid };
        var post = new FeedPost
        {
            AuthorUid = uid,
            PhotoUrl = photo.Url,
            Caption = photo.Caption,
            UploadedUtc = DateTime.UtcNow,
            LikesCount = 0,
            AuthorName = profile.DisplayName,
            AuthorAvatarUrl = profile.AvatarUrl
        };
        _posts[post.Id] = post;
        PostUpdated?.Invoke(this, post);
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

    public async IAsyncEnumerable<FeedComment> GetCommentsAsync(string postId)
    {
        if (_comments.TryGetValue(postId, out var dict))
        {
            foreach (var c in dict.Values.OrderBy(c => c.CreatedUtc))
            {
                yield return c;
                await Task.Yield();
            }
        }
    }

    public Task AddCommentAsync(string postId, string text, string uid)
    {
        if (!_posts.ContainsKey(postId))
            return Task.CompletedTask;

        if (!_comments.TryGetValue(postId, out var dict))
        {
            dict = new();
            _comments[postId] = dict;
        }

        var comment = new FeedComment
        {
            AuthorUid = uid,
            Text = text,
            CreatedUtc = DateTime.UtcNow
        };
        dict[comment.Id] = comment;
        CommentsChanged?.Invoke(this, postId);

        if (_posts.TryGetValue(postId, out var post) && post.AuthorUid == uid)
        {
            _notifications.ScheduleLocalAsync(DateTime.Now, "Nuevo comentario", text, comment.Id);
        }

        return Task.CompletedTask;
    }

    public Task DeleteCommentAsync(string postId, string commentId, string uid)
    {
        if (_comments.TryGetValue(postId, out var dict) && dict.TryGetValue(commentId, out var comment))
        {
            if (comment.AuthorUid == uid)
            {
                dict.Remove(commentId);
                CommentsChanged?.Invoke(this, postId);
            }
        }
        return Task.CompletedTask;
    }
}

