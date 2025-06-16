namespace GymMate.Services;

using Plugin.Firebase.Auth;
using Plugin.Firebase.Firestore;
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
    private readonly IFirebaseAuth _auth;

    public FirebaseAuthService(IFirebaseFirestore firestore, Lazy<INotificationService> notificationService, IAnalyticsService analytics)
    {
        _firestore = firestore;
        _notificationService = notificationService;
        _analytics = analytics;
        _auth = CrossFirebaseAuth.Current;
    }

    public string CurrentUserUid { get; private set; } = string.Empty;

    private async Task EnsureUserProfileAsync(string uid, string email)
    {
        var doc = _firestore.Document($"userProfiles/{uid}");
        var profile = new Models.UserProfile
        {
            Uid = uid,
            DisplayName = email.Split('@')[0],
            AvatarUrl = string.Empty,
            CreatedUtc = DateTime.UtcNow
        };
        await doc.SetAsync(profile, SetOptions.Merge());
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _auth.SignInWithEmailAndPasswordAsync(email, password, false);
            CurrentUserUid = user?.Uid ?? string.Empty;
            if (string.IsNullOrEmpty(CurrentUserUid))
                return false;

            await EnsureUserProfileAsync(CurrentUserUid, email);
            await _notificationService.Value.InitialiseAsync();
            await _notificationService.Value.SubscribeAsync($"user_{CurrentUserUid}");
            await _analytics.SetUserIdAsync(CurrentUserUid);
            await _analytics.LogEventAsync("login_success");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        try
        {
            var user = await _auth.CreateUserAsync(email, password);
            CurrentUserUid = user?.Uid ?? string.Empty;
            if (string.IsNullOrEmpty(CurrentUserUid))
                return false;

            await EnsureUserProfileAsync(CurrentUserUid, email);
            await _notificationService.Value.InitialiseAsync();
            await _notificationService.Value.SubscribeAsync($"user_{CurrentUserUid}");
            await _analytics.SetUserIdAsync(CurrentUserUid);
            await _analytics.LogEventAsync("login_success");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _auth.SignOutAsync();
        var uid = CurrentUserUid;
        CurrentUserUid = string.Empty;
        if (!string.IsNullOrEmpty(uid))
        {
            await _notificationService.Value.UnsubscribeAsync($"user_{uid}");
        }
    }
}
