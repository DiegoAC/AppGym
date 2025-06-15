namespace GymMate.Models;

public class UserProfile
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
