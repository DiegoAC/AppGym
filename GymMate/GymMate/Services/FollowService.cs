using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using GymMate.Models;

namespace GymMate.Services;

public interface IFollowService
{
    Task FollowAsync(string targetUid);
    Task UnfollowAsync(string targetUid);
    IAsyncEnumerable<string> GetFollowingAsync(string uid);
    IAsyncEnumerable<string> GetFollowersAsync(string uid);
    Task<IObservable<bool>> IsFollowingAsync(string targetUid);
    Task<UserProfile?> GetProfileAsync(string uid);
}

public class FollowService : IFollowService
{
    private readonly IFirebaseAuthService _auth;
    private readonly IFirebaseFirestore _firestore = CrossFirebaseFirestore.Current;
    private readonly IAnalyticsService _analytics;

    public FollowService(IFirebaseAuthService auth, IAnalyticsService analytics)
    {
        _auth = auth;
        _analytics = analytics;
    }

    public async Task FollowAsync(string targetUid)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || uid == targetUid) return;

        await _firestore.RunTransactionAsync(async transaction =>
        {
            var followingDoc = _firestore.Document($"userProfiles/{uid}/following/{targetUid}");
            var followerDoc = _firestore.Document($"userProfiles/{targetUid}/followers/{uid}");
            transaction.Set(followingDoc, new { });
            transaction.Set(followerDoc, new { });
        });
        await _analytics.LogEventAsync("follow_user");
    }

    public async Task UnfollowAsync(string targetUid)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || uid == targetUid) return;

        await _firestore.RunTransactionAsync(async transaction =>
        {
            var followingDoc = _firestore.Document($"userProfiles/{uid}/following/{targetUid}");
            var followerDoc = _firestore.Document($"userProfiles/{targetUid}/followers/{uid}");
            transaction.Delete(followingDoc);
            transaction.Delete(followerDoc);
        });
        await _analytics.LogEventAsync("follow_user");
    }

    public async IAsyncEnumerable<string> GetFollowingAsync(string uid)
    {
        var snapshot = await _firestore.Collection($"userProfiles/{uid}/following").GetAsync();
        foreach (var doc in snapshot.Documents)
        {
            yield return doc.Id;
            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<string> GetFollowersAsync(string uid)
    {
        var snapshot = await _firestore.Collection($"userProfiles/{uid}/followers").GetAsync();
        foreach (var doc in snapshot.Documents)
        {
            yield return doc.Id;
            await Task.Yield();
        }
    }

    public Task<IObservable<bool>> IsFollowingAsync(string targetUid)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid))
        {
            return Task.FromResult(Observable.Return(false));
        }

        var docRef = _firestore.Document($"userProfiles/{uid}/following/{targetUid}");
        var observable = docRef.AsObservable().Select(snapshot => snapshot.Exists);
        return Task.FromResult(observable);
    }

    public async Task<UserProfile?> GetProfileAsync(string uid)
    {
        var doc = _firestore.Document($"userProfiles/{uid}");
        var snapshot = await doc.GetAsync();
        if (snapshot.Exists)
        {
            var profile = snapshot.ToObject<UserProfile>();
            if (profile != null)
                profile.Uid = uid;
            return profile;
        }
        return null;
    }
}
