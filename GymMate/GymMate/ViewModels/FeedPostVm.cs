using System.Windows.Input;

namespace GymMate.ViewModels;

public class FeedPostVm
{
    public string? AuthorAvatarUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int LikesCount { get; set; }
    public ICommand LikeCommand { get; }
    public ICommand CommentCommand { get; }

    public FeedPostVm(string? avatarUrl, string authorName, string photoUrl, string? caption,
        int likes, ICommand likeCmd, ICommand commentCmd)
    {
        AuthorAvatarUrl = avatarUrl;
        AuthorName = authorName;
        PhotoUrl = photoUrl;
        Caption = caption;
        LikesCount = likes;
        LikeCommand = likeCmd;
        CommentCommand = commentCmd;
    }
}
