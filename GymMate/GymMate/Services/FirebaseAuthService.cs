namespace GymMate.Services;

public interface IFirebaseAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string password);
    Task LogoutAsync();
    string CurrentUserUid { get; }
}

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly IFirebaseFirestore _firestore;
    private readonly Lazy<INotificationService> _notificationService;
    private readonly IAnalyticsService _analytics;

    public FirebaseAuthService(IFirebaseFirestore firestore, Lazy<INotificationService> notificationService, IAnalyticsService analytics)
    {
        _firestore = firestore;
        _notificationService = notificationService;
        _analytics = analytics;
    }

    public string CurrentUserUid { get; private set; } = "debug-user";

    private async Task EnsureUserProfileAsync(string uid, string email)
    {
        var doc = _firestore.Document($"userProfiles/{uid}");
        var snapshot = await doc.GetAsync();
        if (!snapshot.Exists)
        {
            var profile = new Models.UserProfile
            {
                Uid = uid,
                DisplayName = email.Split('@')[0],
                AvatarUrl = string.Empty,
                CreatedUtc = DateTime.UtcNow
            };
            await doc.SetAsync(profile);
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        // TODO: Integrate Firebase Auth login
        await EnsureUserProfileAsync(CurrentUserUid, email);
        await _notificationService.Value.InitialiseAsync();
        await _notificationService.Value.SubscribeAsync($"user_{CurrentUserUid}");
        await _analytics.SetUserIdAsync(CurrentUserUid);
        await _analytics.LogEventAsync("login_success");
        return true;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        // TODO: Integrate Firebase Auth register
        await EnsureUserProfileAsync(CurrentUserUid, email);
        await _analytics.SetUserIdAsync(CurrentUserUid);
        await _analytics.LogEventAsync("login_success");
        return true;
    }

    public Task LogoutAsync()
    {
        // TODO: Integrate Firebase Auth logout
        return Task.CompletedTask;
    }
}
