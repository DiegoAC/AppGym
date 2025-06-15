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

    public FirebaseAuthService(IFirebaseFirestore firestore)
    {
        _firestore = firestore;
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
        return true;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        // TODO: Integrate Firebase Auth register
        await EnsureUserProfileAsync(CurrentUserUid, email);
        return true;
    }

    public Task LogoutAsync()
    {
        // TODO: Integrate Firebase Auth logout
        return Task.CompletedTask;
    }
}
