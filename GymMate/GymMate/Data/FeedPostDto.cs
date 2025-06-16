namespace GymMate.Data;

using SQLite;

public class FeedPostDto
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string AuthorUid { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime UploadedUtc { get; set; }
    public int LikesCount { get; set; }
}
