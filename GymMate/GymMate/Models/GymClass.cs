namespace GymMate.Models;

public class GymClass
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime StartsUtc { get; set; }
    public int DurationMin { get; set; }
    public int Capacity { get; set; }
    public int ReservedCount { get; set; }

    public double Occupancy => Capacity == 0 ? 0 : (double)ReservedCount / Capacity;
}
