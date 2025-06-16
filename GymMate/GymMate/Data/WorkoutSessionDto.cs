namespace GymMate.Data;

using SQLite;

public class WorkoutSessionDto
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string RoutineId { get; set; } = string.Empty;
    public long DateUtc { get; set; }
    public string JsonSets { get; set; } = string.Empty;
    public bool IsPendingSync { get; set; }
}
