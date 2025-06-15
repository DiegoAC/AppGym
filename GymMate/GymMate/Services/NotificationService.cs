namespace GymMate.Services;

using Plugin.Firebase.CloudMessaging;
using Plugin.LocalNotification;

public interface INotificationService
{
    Task RequestPermissionsAsync();
    Task ScheduleLocalAsync(DateTime when, string title, string body, string id);
    Task SubscribeToTopicAsync(string topic);
    Task UnsubscribeFromTopicAsync(string topic);
}

public class NotificationService : INotificationService
{
    private readonly IRealtimeDbService _db;
    private readonly IFirebaseAuthService _auth;

    public NotificationService(IRealtimeDbService db, IFirebaseAuthService auth)
    {
        _db = db;
        _auth = auth;
        CrossFirebaseCloudMessaging.Current.TokenRefreshed += OnTokenRefreshed;
    }

    private async void OnTokenRefreshed(object? sender, string token)
    {
        var uid = _auth.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;
        await _db.SaveDeviceTokenAsync(uid, token);
    }

    public Task RequestPermissionsAsync()
        => CrossFirebaseCloudMessaging.Current.RequestPermissionAsync();

    public Task ScheduleLocalAsync(DateTime when, string title, string body, string id)
    {
        var request = new NotificationRequest
        {
            NotificationId = id.GetHashCode(),
            Title = title,
            Description = body,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = when,
                RepeatType = NotificationRepeat.Daily
            }
        };
        NotificationCenter.Current.Show(request);
        return Task.CompletedTask;
    }

    public Task SubscribeToTopicAsync(string topic)
        => CrossFirebaseCloudMessaging.Current.SubscribeToTopic(topic);

    public Task UnsubscribeFromTopicAsync(string topic)
        => CrossFirebaseCloudMessaging.Current.UnsubscribeFromTopic(topic);
}
