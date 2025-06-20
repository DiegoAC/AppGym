namespace GymMate.Services;

using Plugin.Firebase.CloudMessaging;
using Plugin.LocalNotification;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public interface INotificationService
{
    Task RequestPermissionsAsync();
    Task ScheduleLocalAsync(DateTime when, string title, string body, string id);
    Task SubscribeAsync(string topic);
    Task UnsubscribeAsync(string topic);
    Task CancelLocalAsync(string id);
    Task SendAsync(string topic, string title, string body);
    Task InitialiseAsync();
}

public class NotificationService : INotificationService
{
    private readonly IRealtimeDbService _db;
    private readonly Lazy<IFirebaseAuthService> _auth;

    public NotificationService(IRealtimeDbService db, Lazy<IFirebaseAuthService> auth)
    {
        _db = db;
        _auth = auth;
        FirebaseMessaging.TokenRefreshed += OnTokenRefreshed;
    }

    private async void OnTokenRefreshed(object? sender, string token)
    {
        var uid = _auth.Value.CurrentUserUid;
        if (string.IsNullOrEmpty(uid)) return;
        await _db.SaveDeviceTokenAsync(uid, token);
    }

    public Task RequestPermissionsAsync()
        => FirebaseMessaging.RequestPermissionAsync();

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

    public Task SubscribeAsync(string topic)
        => FirebaseMessaging.SubscribeToTopic(topic);

    public Task UnsubscribeAsync(string topic)
        => FirebaseMessaging.UnsubscribeFromTopic(topic);

    public Task CancelLocalAsync(string id)
    {
        NotificationCenter.Current.Cancel(id.GetHashCode());
        return Task.CompletedTask;
    }

    public async Task SendAsync(string topic, string title, string body)
    {
        var serverKey = Environment.GetEnvironmentVariable("FCM_SERVER_KEY");
        if (string.IsNullOrEmpty(serverKey))
            return;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={serverKey}");

        var payload = new
        {
            to = $"/topics/{topic}",
            data = new { title, body }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            await client.PostAsync("https://fcm.googleapis.com/fcm/send", content);
        }
        catch
        {
            // ignore errors
        }
    }

    public Task InitialiseAsync()
    {
        FirebaseMessaging.OnMessageReceived += (s, m) =>
        {
            if (m.Data.TryGetValue("title", out var title) &&
                m.Data.TryGetValue("body", out var body))
            {
                var request = new NotificationRequest
                {
                    NotificationId = (title + body).GetHashCode(),
                    Title = title,
                    Description = body
                };
                NotificationCenter.Current.Show(request);
            }
        };
        return RequestPermissionsAsync();
    }
}
