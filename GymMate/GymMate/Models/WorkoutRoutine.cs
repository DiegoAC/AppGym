namespace GymMate.Models;

public class WorkoutRoutine
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Focus { get; set; } = "General";
    public string Difficulty { get; set; } = "Básica";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
