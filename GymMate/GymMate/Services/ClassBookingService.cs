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
    private readonly IFirebaseFirestore _firestore = CrossFirebaseFirestore.Current;
    private readonly IDisposable _subscription;

    public event EventHandler? ClassesChanged;

    public ClassBookingService()
    {
        _subscription = _firestore.Collection("classes")
            .AsObservable()
            .Subscribe(_ => ClassesChanged?.Invoke(this, EventArgs.Empty));
    }

    public async IAsyncEnumerable<GymClass> GetUpcomingClassesAsync()
    {
        var snapshot = await _firestore.Collection("classes").GetAsync();
        var list = new List<GymClass>();
        foreach (var doc in snapshot.Documents)
        {
            var gymClass = doc.ToObject<GymClass>();
            if (gymClass != null && gymClass.StartsUtc >= DateTime.UtcNow)
            {
                gymClass.Id = doc.Id;
                list.Add(gymClass);
            }
        }
        foreach (var gc in list.OrderBy(c => c.StartsUtc))
        {
            yield return gc;
            await Task.Yield();
        }
    }

    public async Task<bool> IsReservedAsync(string classId, string uid)
    {
        var doc = await _firestore.Document($"classes/{classId}/reservations/{uid}").GetAsync();
        return doc.Exists;
    }

    public async Task ReserveAsync(string classId, string uid)
    {
        var classRef = _firestore.Document($"classes/{classId}");
        var resRef = classRef.Collection("reservations").Document(uid);

        await _firestore.RunTransactionAsync(async transaction =>
        {
            var snap = await transaction.GetDocumentAsync(classRef);
            if (!snap.Exists)
                return;

            var gymClass = snap.ToObject<GymClass>();
            if (gymClass == null)
                return;

            if (gymClass.ReservedCount >= gymClass.Capacity)
                throw new InvalidOperationException("full");

            transaction.Set(resRef, new { });
            transaction.Update(classRef, new Dictionary<string, object>
            {
                { "ReservedCount", FieldValue.Increment(1) }
            });
        });
    }

    public async Task CancelReservationAsync(string classId, string uid)
    {
        var classRef = _firestore.Document($"classes/{classId}");
        var resRef = classRef.Collection("reservations").Document(uid);

        await _firestore.RunTransactionAsync(async transaction =>
        {
            var resSnap = await transaction.GetDocumentAsync(resRef);
            if (!resSnap.Exists)
                return;

            transaction.Delete(resRef);
            transaction.Update(classRef, new Dictionary<string, object>
            {
                { "ReservedCount", FieldValue.Increment(-1) }
            });
        });
    }
}
