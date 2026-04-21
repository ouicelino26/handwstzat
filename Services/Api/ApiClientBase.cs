using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HandWStat.Configuration;

namespace HandWStat.Services.Api;

public abstract class ApiClientBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _settings;
    private readonly IApiAuthService _authService;

    protected ApiClientBase(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
    {
        _httpClient = httpClient;
        _settings = settings;
        _authService = authService;
    }

    protected async Task<T?> GetAsync<T>(string relativePath, ApiQueryBuilder? query = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(relativePath, query));
        return await SendAsync<T>(request, cancellationToken);
    }

    protected async Task<IReadOnlyList<T>> GetListAsync<T>(string relativePath, ApiQueryBuilder? query = null, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<T>>(relativePath, query, cancellationToken) ?? [];
    }

    protected async Task<T?> PostAsync<TRequest, T>(string relativePath, TRequest requestBody, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(relativePath));
        request.Content = JsonContent.Create(requestBody, options: SerializerOptions);
        return await SendAsync<T>(request, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _authService.ApplyAuthorization(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException("Connexion non autorisee. Utilisez un compte habilite.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Impossible de recuperer les donnees demandees : {(int)response.StatusCode} {response.ReasonPhrase}. {details}".Trim());
        }

        return await response.Content.ReadFromJsonAsync<T>(SerializerOptions, cancellationToken);
    }

    private Uri BuildUri(string relativePath, ApiQueryBuilder? query = null)
    {
        var baseUrl = NormalizeBaseUrl(_settings.BaseUrl);
        var normalizedPath = query?.BuildRelativePath(relativePath) ?? relativePath;
        return new Uri(new Uri(baseUrl), normalizedPath);
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var normalized = string.IsNullOrWhiteSpace(baseUrl)
            ? ApiSettings.DefaultBaseUrl
            : baseUrl.Trim();

        return normalized.EndsWith("/", StringComparison.Ordinal) ? normalized : normalized + "/";
    }
}
