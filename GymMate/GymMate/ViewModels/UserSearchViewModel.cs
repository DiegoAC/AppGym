using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;
using Plugin.Firebase.Firestore;

namespace GymMate.ViewModels;

public partial class UserSearchItem : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    [ObservableProperty]
    private bool isFollowing;
}

public partial class UserSearchViewModel : ObservableObject
{
    private readonly IFirebaseFirestore _firestore;
    private readonly IFollowService _follow;

    public ObservableCollection<UserSearchItem> Results { get; } = new();

    [ObservableProperty]
    private string query = string.Empty;

    public UserSearchViewModel(IFirebaseFirestore firestore, IFollowService follow)
    {
        _firestore = firestore;
        _follow = follow;
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
        Results.Clear();
        var snapshot = await _firestore.Collection("userProfiles")
            .WhereGreaterThanOrEqualTo("DisplayName", Query)
            .OrderBy("DisplayName")
            .Limit(20)
            .GetAsync();

        foreach (var doc in snapshot.Documents)
        {
            var profile = doc.ToObject<UserProfile>();
            profile.Uid = doc.Id;
            var followingObs = await _follow.IsFollowingAsync(profile.Uid);
            var isFollowing = await followingObs.FirstAsync();
            Results.Add(new UserSearchItem
            {
                Id = profile.Uid,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.AvatarUrl,
                IsFollowing = isFollowing
            });
        }
    }

    [RelayCommand]
    private async Task ToggleFollowAsync(UserSearchItem item)
    {
        if (item == null) return;
        if (item.IsFollowing)
        {
            item.IsFollowing = false;
            await _follow.UnfollowAsync(item.Id);
        }
        else
        {
            item.IsFollowing = true;
            await _follow.FollowAsync(item.Id);
        }
    }

    [RelayCommand]
    private async Task OpenProfileAsync(string uid)
    {
        await Shell.Current.GoToAsync($"profile?Uid={uid}");
    }
}
