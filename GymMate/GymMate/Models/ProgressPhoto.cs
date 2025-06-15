namespace GymMate.Models;

public class ProgressPhoto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
    public string? Caption { get; set; }
}
