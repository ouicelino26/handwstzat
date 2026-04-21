namespace HandWStat.Services;

public sealed record ApiSession(
    bool IsAuthenticated,
    string? Username,
    string? Role,
    string? AccessToken,
    string? Message)
{
    public static ApiSession Anonymous { get; } = new(false, null, null, null, null);
}

public interface IApiAuthService
{
    ApiSession Session { get; }

    event Action? SessionChanged;

    Task<ApiSession> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    void Logout();

    void ApplyAuthorization(HttpRequestMessage request);
}
