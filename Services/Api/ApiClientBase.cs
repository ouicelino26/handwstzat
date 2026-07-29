using System.Net;
using System.Net.Http.Json;
using System.Diagnostics;
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

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiRequestException(
                "Le service met trop de temps a repondre. Reessayez dans quelques instants.",
                "API_TIMEOUT",
                null,
                retryable: true,
                statusCode: null);
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[HandWStat API] Network failure for {request.RequestUri}: {ex}");
            throw new ApiRequestException(
                "Le service statistique est momentanement inaccessible.",
                "API_NETWORK_ERROR",
                null,
                retryable: true,
                ex.StatusCode,
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateRequestExceptionAsync(request, response, cancellationToken);
            }

            return await response.Content.ReadFromJsonAsync<T>(SerializerOptions, cancellationToken);
        }
    }

    private static async Task<ApiRequestException> CreateRequestExceptionAsync(
        HttpRequestMessage request,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var details = await response.Content.ReadAsStringAsync(cancellationToken);
        var correlationId = TryReadProblemString(details, "correlationId")
            ?? TryReadHeader(response, "X-Correlation-ID")
            ?? TryReadHeader(response, "X-Request-ID");
        var technicalCode = TryReadProblemString(details, "code")
            ?? TryReadProblemString(details, "type")
            ?? $"HTTP_{(int)response.StatusCode}";
        var retryable = response.StatusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            || (int)response.StatusCode >= 500;

        var userMessage = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                "Connexion non autorisee. Utilisez un compte habilite.",
            HttpStatusCode.BadRequest =>
                "Le filtre statistique n'a pas pu etre traite. Verifiez le perimetre choisi.",
            HttpStatusCode.NotFound =>
                "Les donnees demandees sont introuvables dans ce perimetre.",
            HttpStatusCode.TooManyRequests =>
                "Le service recoit trop de demandes. Reessayez dans quelques instants.",
            _ when (int)response.StatusCode >= 500 =>
                "Le service statistique rencontre un probleme temporaire.",
            _ =>
                "Impossible de recuperer les donnees demandees."
        };

        Debug.WriteLine(
            $"[HandWStat API] {(int)response.StatusCode} {response.ReasonPhrase} for {request.RequestUri}; "
            + $"code={technicalCode}; correlationId={correlationId}; body={details}");

        return new ApiRequestException(
            userMessage,
            technicalCode,
            correlationId,
            retryable,
            response.StatusCode);
    }

    private static string? TryReadProblemString(string json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty(propertyName, out var value)
                && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
        }
        catch (JsonException)
        {
            // The raw body remains available in development diagnostics only.
        }

        return null;
    }

    private static string? TryReadHeader(HttpResponseMessage response, string name)
    {
        return response.Headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault()
            : null;
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
