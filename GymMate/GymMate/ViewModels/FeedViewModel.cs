using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;

namespace GymMate.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly IFeedService _service;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<FeedPost> Posts { get; } = new();

    private DateTime? _lastDate;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public FeedViewModel(IFeedService service, IFirebaseAuthService auth)
    {
        _service = service;
        _auth = auth;
        _service.PostUpdated += Service_PostUpdated;
    }

    private void Service_PostUpdated(object? sender, FeedPost e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existing = Posts.FirstOrDefault(p => p.Id == e.Id);
            if (existing == null)
            {
                Posts.Insert(0, e);
            }
            else
            {
                var index = Posts.IndexOf(existing);
                Posts[index] = e;
            }
        });
    }

    [RelayCommand]
    public async Task AppearingAsync()
    {
        if (Posts.Count == 0)
            await LoadMoreAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        Posts.Clear();
        _lastDate = null;
        await LoadMoreAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    public async Task LoadMoreAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        var list = new List<FeedPost>();
        await foreach (var p in _service.GetLatestAsync(20, _lastDate))
            list.Add(p);
        if (list.Count > 0)
            _lastDate = list.Last().UploadedUtc;
        foreach (var p in list)
            Posts.Add(p);
        IsBusy = false;
    }

    [RelayCommand]
    private Task ToggleLikeAsync(FeedPost post)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return Task.CompletedTask;
        if (post == null) return Task.CompletedTask;
        return ToggleAsync(post, uid);
    }

    private async Task ToggleAsync(FeedPost post, string uid)
    {
        if (await _service.IsLikedAsync(post.Id, uid))
            await _service.UnlikeAsync(post.Id, uid);
        else
            await _service.LikeAsync(post.Id, uid);
    }

    [RelayCommand]
    private async Task OpenCommentsAsync(string postId)
    {
        await Shell.Current.GoToAsync($"comments?postId={postId}");
    }
}
