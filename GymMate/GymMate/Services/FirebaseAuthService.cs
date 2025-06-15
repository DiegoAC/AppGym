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
    public string CurrentUserUid { get; private set; } = "debug-user";
    public Task<bool> LoginAsync(string email, string password)
    {
        // TODO: Integrate Firebase Auth login
        return Task.FromResult(true);
    }

    public Task<bool> RegisterAsync(string email, string password)
    {
        // TODO: Integrate Firebase Auth register
        return Task.FromResult(true);
    }

    public Task LogoutAsync()
    {
        // TODO: Integrate Firebase Auth logout
        return Task.CompletedTask;
    }
}
