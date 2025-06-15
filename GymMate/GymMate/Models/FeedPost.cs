namespace GymMate.Models;

public class FeedPost
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AuthorUid { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
    public int LikesCount { get; set; }
}
