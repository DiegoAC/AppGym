using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;

namespace GymMate.ViewModels;

public partial class ProfileViewModel : ObservableObject, IQueryAttributable
{
    private readonly IFollowService _follow;
    private readonly IProgressPhotoService _photos;
    private readonly IFirebaseAuthService _auth;

    [ObservableProperty] private string uid = string.Empty;
    [ObservableProperty] private string displayName = string.Empty;
    [ObservableProperty] private string? avatarUrl;
    [ObservableProperty] private bool isMine;
    [ObservableProperty] private bool isFollowing;
    [ObservableProperty] private bool showFollowBack;
    [ObservableProperty] private int followersCount;
    [ObservableProperty] private int followingCount;

    public ObservableCollection<ProgressPhoto> Photos { get; } = new();

    public ProfileViewModel(IFollowService follow, IProgressPhotoService photos, IFirebaseAuthService auth)
    {
        _follow = follow;
        _photos = photos;
        _auth = auth;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("uid", out var value) && value is string id)
            Uid = id;
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
        IsMine = Uid == current;
        var profile = await _follow.GetProfileAsync(Uid);
        if (profile != null)
        {
            DisplayName = profile.DisplayName;
            AvatarUrl = profile.AvatarUrl;
        }

        var followers = new List<string>();
        await foreach (var f in _follow.GetFollowersAsync(Uid))
            followers.Add(f);
        FollowersCount = followers.Count;
        ShowFollowBack = !IsMine && followers.Contains(current) && !await _follow.IsFollowingAsync(Uid);

        var following = new List<string>();
        await foreach (var f in _follow.GetFollowingAsync(Uid))
            following.Add(f);
        FollowingCount = following.Count;
        IsFollowing = await _follow.IsFollowingAsync(Uid);

        Photos.Clear();
        await foreach (var p in _photos.GetPhotosAsync(Uid))
            Photos.Add(p);
    }

    [RelayCommand]
    private async Task ToggleFollowAsync()
    {
        if (IsMine) return;
        if (IsFollowing)
            await _follow.UnfollowAsync(Uid);
        else
            await _follow.FollowAsync(Uid);
        await LoadAsync();
    }
}
