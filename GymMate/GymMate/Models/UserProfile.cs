namespace GymMate.Models;

public class UserProfile
{
    public string Uid { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
