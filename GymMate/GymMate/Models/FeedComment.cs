namespace GymMate.Models;

public class FeedComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AuthorUid { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsMine { get; set; }
}
