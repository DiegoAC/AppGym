using Plugin.Firebase.Analytics;

namespace GymMate.Services;

public interface IAnalyticsService
{
    Task LogEventAsync(string name, IDictionary<string, object>? parameters = null);
    Task SetUserIdAsync(string uid);
}

class AnalyticsService : IAnalyticsService
{
    readonly IFirebaseAnalytics _fa = CrossFirebaseAnalytics.Current;
    public Task LogEventAsync(string n, IDictionary<string, object>? p = null)
        => _fa.LogEventAsync(n, p);
    public Task SetUserIdAsync(string uid) => _fa.SetUserIdAsync(uid);
}
