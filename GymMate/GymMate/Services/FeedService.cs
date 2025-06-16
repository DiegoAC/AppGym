namespace GymMate.Services;

using GymMate.Models;
using GymMate.Data;
using Microsoft.Maui.Networking;
using Plugin.Firebase.Firestore;

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
    private readonly IAnalyticsService _analytics;
    private readonly IFirebaseFirestore _firestore = CrossFirebaseFirestore.Current;

    public event EventHandler<FeedPost>? PostUpdated;
    public event EventHandler<string>? CommentsChanged;

    public FeedService(IFirebaseAuthService auth, INotificationService notifications, IFollowService follow, LocalDbService localDb, IAnalyticsService analytics)
    {
        _auth = auth;
        _notifications = notifications;
        _follow = follow;
        _localDb = localDb;
        _analytics = analytics;
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
                if (followUids.Count >= 10)
                    break;
            }
            // incluir el propio uid para mostrar posts personales
            followUids.Add(meUid);
        }

        if (followUids.Count == 0)
            yield break;

        var query = _firestore.Collection("feedPosts")
            .WhereIn("AuthorUid", followUids.ToArray())
            .OrderByDescending("UploadedUtc");

        if (startAfter != null)
            query = query.WhereLessThan("UploadedUtc", startAfter.Value);

        var snapshot = await query.Limit(pageSize).GetAsync();
        var list = new List<FeedPost>();
        foreach (var doc in snapshot.Documents)
        {
            var post = doc.ToObject<FeedPost>();
            if (post != null)
            {
                post.Id = doc.Id;
                _posts[post.Id] = post;
                list.Add(post);
            }
        }
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
        if (string.IsNullOrEmpty(uid))
            return;
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
        await _firestore.Collection("feedPosts").Document(post.Id).SetAsync(post);
        _posts[post.Id] = post;
        await _localDb.SavePostsAsync(new[] { post });
        PostUpdated?.Invoke(this, post);
    }

    public async Task LikeAsync(string postId, string uid)
    {
        var postRef = _firestore.Document($"feedPosts/{postId}");
        await postRef.Collection("likes").Document(uid).SetAsync(new { });
        await postRef.UpdateAsync("LikesCount", FieldValue.Increment(1));

        if (_posts.TryGetValue(postId, out var post))
        {
            post.LikesCount++;
            await _localDb.SavePostsAsync(new[] { post });
            PostUpdated?.Invoke(this, post);
        }
        await _analytics.LogEventAsync("post_like");
    }

    public async Task UnlikeAsync(string postId, string uid)
    {
        var postRef = _firestore.Document($"feedPosts/{postId}");
        await postRef.Collection("likes").Document(uid).DeleteAsync();
        await postRef.UpdateAsync("LikesCount", FieldValue.Increment(-1));

        if (_posts.TryGetValue(postId, out var post))
        {
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
            await _localDb.SavePostsAsync(new[] { post });
            PostUpdated?.Invoke(this, post);
        }
        await _analytics.LogEventAsync("post_like");
    }

    public async Task<bool> IsLikedAsync(string postId, string uid)
    {
        var doc = await _firestore.Document($"feedPosts/{postId}/likes/{uid}").GetAsync();
        return doc.Exists;
    }

    public async IAsyncEnumerable<FeedComment> GetCommentsAsync(string postId)
    {
        var snapshot = await _firestore.Collection($"feedPosts/{postId}/comments")
            .OrderBy("CreatedUtc")
            .GetAsync();

        var dict = _comments.GetValueOrDefault(postId) ?? new Dictionary<string, FeedComment>();
        _comments[postId] = dict;

        foreach (var doc in snapshot.Documents)
        {
            var comment = doc.ToObject<FeedComment>();
            if (comment != null)
            {
                comment.Id = doc.Id;
                dict[comment.Id] = comment;
                yield return comment;
                await Task.Yield();
            }
        }
    }

    public async Task AddCommentAsync(string postId, string text, string uid)
    {
        if (!_posts.ContainsKey(postId))
            return;

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

        await _firestore.Collection($"feedPosts/{postId}/comments")
            .Document(comment.Id)
            .SetAsync(comment);

        CommentsChanged?.Invoke(this, postId);

        if (_posts.TryGetValue(postId, out var post) && post.AuthorUid != uid)
        {
            await _notifications.ScheduleLocalAsync(DateTime.Now, "Nuevo comentario", text, comment.Id);
        }
    }

    public async Task DeleteCommentAsync(string postId, string commentId, string uid)
    {
        var docRef = _firestore.Document($"feedPosts/{postId}/comments/{commentId}");
        var snap = await docRef.GetAsync();
        if (snap.Exists)
        {
            var comment = snap.ToObject<FeedComment>();
            if (comment != null && comment.AuthorUid == uid)
            {
                await docRef.DeleteAsync();
                if (_comments.TryGetValue(postId, out var dict))
                    dict.Remove(commentId);
                CommentsChanged?.Invoke(this, postId);
            }
        }
    }
}

