namespace GymMate.Models;

public class SetRecord
{
    public string ExerciseName { get; set; } = string.Empty;
    public int Reps { get; set; }
    public double WeightKg { get; set; }
}

public class WorkoutSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RoutineId { get; set; } = string.Empty;
    public DateTime DateUtc { get; set; } = DateTime.UtcNow;
    public IList<SetRecord> Sets { get; set; } = new List<SetRecord>();
}
