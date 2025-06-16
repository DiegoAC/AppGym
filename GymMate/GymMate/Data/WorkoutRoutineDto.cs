namespace GymMate.Data;

using SQLite;

public class WorkoutRoutineDto
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long CreatedUtc { get; set; }
    public bool IsPendingSync { get; set; }
}
