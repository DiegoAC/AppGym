using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;

[QueryProperty(nameof(Uid), nameof(Uid))]

namespace GymMate.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IFollowService _follow;
    private readonly IProgressPhotoService _photos;
    private readonly IFirebaseAuthService _auth;

    [ObservableProperty] private string uid = string.Empty;
    [ObservableProperty] private UserProfile? profile;
    [ObservableProperty] private bool isOwnProfile;
    [ObservableProperty] private bool isFollowing;
    [ObservableProperty] private int followersCount;
    [ObservableProperty] private int followingCount;

    public ObservableCollection<ProgressPhoto> Photos { get; } = new();

    public ProfileViewModel(IFollowService follow, IProgressPhotoService photos, IFirebaseAuthService auth)
    {
        _follow = follow;
        _photos = photos;
        _auth = auth;
    }


    partial void OnUidChanged(string value)
    {
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (string.IsNullOrEmpty(Uid))
            Uid = _auth.CurrentUserUid;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var current = _auth.CurrentUserUid;
        IsOwnProfile = Uid == current;

        Profile = await _follow.GetProfileAsync(Uid);

        var followers = new List<string>();
        await foreach (var f in _follow.GetFollowersAsync(Uid))
            followers.Add(f);
        FollowersCount = followers.Count;

        var following = new List<string>();
        await foreach (var f in _follow.GetFollowingAsync(Uid))
            following.Add(f);
        FollowingCount = following.Count;

        if (!IsOwnProfile)
        {
            var obs = await _follow.IsFollowingAsync(Uid);
            IsFollowing = await obs.FirstAsync();
        }
        else
        {
            IsFollowing = false;
        }

        Photos.Clear();
        await foreach (var p in _photos.GetPhotosAsync(Uid))
            Photos.Add(p);
    }

    [RelayCommand]
    private async Task FollowOrUnfollowAsync()
    {
        if (IsOwnProfile) return;
        if (IsFollowing)
        {
            IsFollowing = false;
            FollowersCount = Math.Max(0, FollowersCount - 1);
            await _follow.UnfollowAsync(Uid);
        }
        else
        {
            IsFollowing = true;
            FollowersCount++;
            await _follow.FollowAsync(Uid);
        }
    }
}
