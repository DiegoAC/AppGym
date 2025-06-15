using GymMate.Models;

namespace GymMate.Services;

public interface IFollowService
{
    IAsyncEnumerable<string> GetFollowersAsync(string uid);
    IAsyncEnumerable<string> GetFollowingAsync(string uid);
    Task FollowAsync(string targetUid);
    Task UnfollowAsync(string targetUid);
    Task<bool> IsFollowingAsync(string targetUid);
    IAsyncEnumerable<UserProfile> SearchAsync(string? query);
    Task<UserProfile?> GetProfileAsync(string uid);
    event EventHandler? FollowingChanged;
}

public class FollowService : IFollowService
{
    private readonly IFirebaseAuthService _auth;
    private readonly INotificationService _notifications;

    private static readonly Dictionary<string, UserProfile> _profiles = new();
    private static readonly Dictionary<string, HashSet<string>> _followers = new();
    private static readonly Dictionary<string, HashSet<string>> _following = new();

    public event EventHandler? FollowingChanged;

    public FollowService(IFirebaseAuthService auth, INotificationService notifications)
    {
        _auth = auth;
        _notifications = notifications;

        if (_profiles.Count == 0)
        {
            _profiles["debug-user"] = new UserProfile { Id = "debug-user", DisplayName = "Debug User", AvatarUrl = null };
            _profiles["user1"] = new UserProfile { Id = "user1", DisplayName = "Alice", AvatarUrl = null };
            _profiles["user2"] = new UserProfile { Id = "user2", DisplayName = "Bob", AvatarUrl = null };
        }
    }

    public async IAsyncEnumerable<UserProfile> SearchAsync(string? query)
    {
        var list = _profiles.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query))
            list = list.Where(p => p.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach (var p in list)
        {
            yield return p;
            await Task.Yield();
        }
    }

    public Task<UserProfile?> GetProfileAsync(string uid)
    {
        _profiles.TryGetValue(uid, out var profile);
        return Task.FromResult(profile);
    }

    public async IAsyncEnumerable<string> GetFollowersAsync(string uid)
    {
        if (_followers.TryGetValue(uid, out var set))
        {
            foreach (var f in set)
            {
                yield return f;
                await Task.Yield();
            }
        }
    }

    public async IAsyncEnumerable<string> GetFollowingAsync(string uid)
    {
        if (_following.TryGetValue(uid, out var set))
        {
            foreach (var f in set)
            {
                yield return f;
                await Task.Yield();
            }
        }
    }

    public Task<bool> IsFollowingAsync(string targetUid)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return Task.FromResult(false);
        return Task.FromResult(_following.TryGetValue(uid, out var set) && set.Contains(targetUid));
    }

    public async Task FollowAsync(string targetUid)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || uid == targetUid) return;
        lock (_followers)
        {
            if (!_following.TryGetValue(uid, out var fset))
            {
                fset = new HashSet<string>();
                _following[uid] = fset;
            }
            if (!fset.Add(targetUid)) return;
            if (!_followers.TryGetValue(targetUid, out var folset))
            {
                folset = new HashSet<string>();
                _followers[targetUid] = folset;
            }
            folset.Add(uid);
        }
        FollowingChanged?.Invoke(this, EventArgs.Empty);
        await _notifications.ScheduleLocalAsync(DateTime.Now, "Nuevo seguidor", $"{uid} comenzó a seguirte", Guid.NewGuid().ToString());
    }

    public Task UnfollowAsync(string targetUid)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || uid == targetUid) return Task.CompletedTask;
        lock (_followers)
        {
            if (_following.TryGetValue(uid, out var fset))
                fset.Remove(targetUid);
            if (_followers.TryGetValue(targetUid, out var folset))
                folset.Remove(uid);
        }
        FollowingChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
