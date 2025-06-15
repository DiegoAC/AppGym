namespace GymMate.Services;

using GymMate.Models;
using Plugin.Firebase.Firestore;

public interface IClassBookingService
{
    IAsyncEnumerable<GymClass> GetUpcomingClassesAsync();
    Task ReserveAsync(string classId, string uid);
    Task CancelReservationAsync(string classId, string uid);
    Task<bool> IsReservedAsync(string classId, string uid);
    event EventHandler? ClassesChanged;
}

public class ClassBookingService : IClassBookingService
{
    private static readonly Dictionary<string, GymClass> _classes = new();
    private static readonly Dictionary<string, HashSet<string>> _reservations = new();

    public event EventHandler? ClassesChanged;

    public ClassBookingService()
    {
        // sample data
        if (_classes.Count == 0)
        {
            var now = DateTime.UtcNow;
            _classes["1"] = new GymClass { Id = "1", Name = "Yoga", StartsUtc = now.AddHours(2), DurationMin = 60, Capacity = 10 };
            _classes["2"] = new GymClass { Id = "2", Name = "HIIT", StartsUtc = now.AddHours(5), DurationMin = 45, Capacity = 15 };
        }
    }

    public async IAsyncEnumerable<GymClass> GetUpcomingClassesAsync()
    {
        var list = _classes.Values
            .Where(c => c.StartsUtc >= DateTime.UtcNow)
            .OrderBy(c => c.StartsUtc)
            .ToList();
        foreach (var c in list)
        {
            yield return c;
            await Task.Yield();
        }
    }

    public Task<bool> IsReservedAsync(string classId, string uid)
    {
        if (_reservations.TryGetValue(classId, out var set))
            return Task.FromResult(set.Contains(uid));
        return Task.FromResult(false);
    }

    public Task ReserveAsync(string classId, string uid)
    {
        lock (_classes)
        {
            if (!_classes.TryGetValue(classId, out var gc))
                return Task.CompletedTask;
            if (gc.ReservedCount >= gc.Capacity)
                throw new InvalidOperationException("full");
            if (!_reservations.TryGetValue(classId, out var set))
            {
                set = new();
                _reservations[classId] = set;
            }
            if (set.Add(uid))
            {
                gc.ReservedCount++;
                ClassesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        return Task.CompletedTask;
    }

    public Task CancelReservationAsync(string classId, string uid)
    {
        lock (_classes)
        {
            if (!_classes.TryGetValue(classId, out var gc))
                return Task.CompletedTask;
            if (_reservations.TryGetValue(classId, out var set) && set.Remove(uid))
            {
                if (gc.ReservedCount > 0)
                    gc.ReservedCount--;
                ClassesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        return Task.CompletedTask;
    }
}
