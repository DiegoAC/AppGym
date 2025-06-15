namespace GymMate.Services;

public interface IRealtimeDbService
{
    Task SaveUserProfileAsync(string userId, object profile);
}

public class RealtimeDbService : IRealtimeDbService
{
    public Task SaveUserProfileAsync(string userId, object profile)
    {
        // TODO: Integrate Firebase realtime database
        return Task.CompletedTask;
    }
}
