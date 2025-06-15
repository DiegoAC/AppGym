using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymMate.Models;
using GymMate.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace GymMate.ViewModels;

public partial class CommentsViewModel : ObservableObject, IQueryAttributable
{
    private readonly IFeedService _service;
    private readonly IFirebaseAuthService _auth;

    public ObservableCollection<FeedComment> Comments { get; } = new();

    [ObservableProperty]
    private string postId = string.Empty;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public string CurrentUid => _auth.CurrentUserUid ?? string.Empty;

    public CommentsViewModel(IFeedService service, IFirebaseAuthService auth)
    {
        _service = service;
        _auth = auth;
        _service.CommentsChanged += Service_CommentsChanged;
    }

    private async void Service_CommentsChanged(object? sender, string e)
    {
        if (e == PostId)
            await LoadAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("postId", out var value) && value is string id)
        {
            PostId = id;
        }
    }

    partial void OnPostIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            LoadAsync();
    }

    [RelayCommand]
    private Task AppearingAsync()
    {
        return LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        Comments.Clear();
        await foreach (var c in _service.GetCommentsAsync(PostId))
        {
            c.IsMine = c.AuthorUid == _auth.CurrentUserUid;
            Comments.Add(c);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid) || string.IsNullOrWhiteSpace(Text)) return;
        await _service.AddCommentAsync(PostId, Text.Trim(), uid);
        Text = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteAsync(FeedComment comment)
    {
        var uid = _auth.CurrentUserUid;
        if (comment == null || string.IsNullOrEmpty(uid)) return;
        await _service.DeleteCommentAsync(PostId, comment.Id, uid);
    }
}
