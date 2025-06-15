using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;

namespace GymMate.ViewModels;

public partial class UserSearchItem : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    [ObservableProperty]
    private bool isFollowing;
    [ObservableProperty]
    private bool isFollower;
}

public partial class UserSearchViewModel : ObservableObject
{
    private readonly IFollowService _follow;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<UserSearchItem> Results { get; } = new();

    [ObservableProperty]
    private string query = string.Empty;

    public UserSearchViewModel(IFollowService follow, IFirebaseAuthService auth)
    {
        _follow = follow;
        _auth = auth;
    }

    partial void OnQueryChanged(string value)
    {
        _ = SearchAsync();
    }

    [RelayCommand]
    public async Task AppearingAsync()
    {
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        var uid = _auth.CurrentUserUid;
        var followers = new List<string>();
        if (!string.IsNullOrEmpty(uid))
        {
            await foreach (var f in _follow.GetFollowersAsync(uid))
                followers.Add(f);
        }
        Results.Clear();
        await foreach (var profile in _follow.SearchAsync(Query))
        {
            if (profile.Id == uid) continue;
            var item = new UserSearchItem
            {
                Id = profile.Id!,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.AvatarUrl,
                IsFollowing = await _follow.IsFollowingAsync(profile.Id!),
                IsFollower = followers.Contains(profile.Id!)
            };
            Results.Add(item);
        }
    }

    [RelayCommand]
    private async Task ToggleFollowAsync(UserSearchItem item)
    {
        if (item == null) return;
        if (item.IsFollowing)
            await _follow.UnfollowAsync(item.Id);
        else
            await _follow.FollowAsync(item.Id);
        item.IsFollowing = await _follow.IsFollowingAsync(item.Id);
    }

    [RelayCommand]
    private async Task OpenProfileAsync(string uid)
    {
        await Shell.Current.GoToAsync($"profile?uid={uid}");
    }
}
