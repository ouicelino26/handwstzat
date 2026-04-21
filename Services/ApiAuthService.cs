using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HandWStat.Configuration;
using HandWStat.Models.Api;

namespace HandWStat.Services;

public sealed class ApiAuthService : IApiAuthService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _settings;

    public ApiAuthService(HttpClient httpClient, ApiSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public ApiSession Session { get; private set; } = ApiSession.Anonymous;

    public event Action? SessionChanged;

    public async Task<ApiSession> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        Logout(silent: true);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Session = ApiSession.Anonymous with { Message = "Renseignez un identifiant et un mot de passe." };
            SessionChanged?.Invoke();
            return Session;
        }

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                BuildUri("auth/login"),
                new LoginRequest(username.Trim(), password),
                SerializerOptions,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Session = ApiSession.Anonymous with { Message = $"Connexion impossible : {ex.Message}" };
            SessionChanged?.Invoke();
            return Session;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            Session = ApiSession.Anonymous with
            {
                Message = string.IsNullOrWhiteSpace(errorMessage)
                    ? "Identifiants invalides."
                    : errorMessage
            };
            SessionChanged?.Invoke();
            return Session;
        }

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(SerializerOptions, cancellationToken);

        if (string.IsNullOrWhiteSpace(payload?.accesstoken))
        {
            Session = ApiSession.Anonymous with { Message = "La session n'a pas pu etre ouverte." };
            SessionChanged?.Invoke();
            return Session;
        }

        Session = new ApiSession(
            true,
            payload.username,
            payload.role,
            payload.accesstoken,
            "Connexion etablie.");

        SessionChanged?.Invoke();
        return Session;
    }

    public void Logout()
    {
        Logout(silent: false);
    }

    public void ApplyAuthorization(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(Session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.AccessToken);
        }
    }

    private void Logout(bool silent)
    {
        Session = ApiSession.Anonymous;

        if (!silent)
        {
            SessionChanged?.Invoke();
        }
    }

    private Uri BuildUri(string relativePath)
    {
        return new Uri(new Uri(_settings.BaseUrl.TrimEnd('/') + "/"), relativePath);
    }
}
