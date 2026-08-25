using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Concurrent;
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

    private readonly ConcurrentDictionary<string, string> _etagCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, object?> _responseCache = new(StringComparer.Ordinal);
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
        var result = await GetConditionalAsync<T>(relativePath, query, cancellationToken);
        return result.Value;
    }

    protected async Task<ApiGetResult<T>> GetConditionalAsync<T>(string relativePath, ApiQueryBuilder? query = null, CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(relativePath, query);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var cacheKey = uri.AbsoluteUri;
        if (_etagCache.TryGetValue(cacheKey, out var cachedEtag))
        {
            request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cachedEtag, isWeak: false));
        }

        return await SendConditionalAsync<T>(request, cacheKey, cancellationToken);
    }

    protected async Task<IReadOnlyList<T>> GetListAsync<T>(string relativePath, ApiQueryBuilder? query = null, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<T>>(relativePath, query, cancellationToken) ?? [];
    }

    protected async Task<T?> PostAsync<TRequest, T>(string relativePath, TRequest requestBody, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(relativePath));
        request.Content = JsonContent.Create(requestBody, options: SerializerOptions);
        var result = await SendConditionalAsync<T>(request, cacheKey: null, cancellationToken);
        return result.Value;
    }

    protected async Task<(byte[]? Content, string? FileName, string? ContentType)> PostDownloadAsync<TRequest>(
        string relativePath,
        TRequest requestBody,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(relativePath));
        request.Content = JsonContent.Create(requestBody, options: SerializerOptions);
        _authService.ApplyAuthorization(request);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiRequestException("Le service met trop de temps a repondre. Reessayez dans quelques instants.", "API_TIMEOUT", null, retryable: true, statusCode: null);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiRequestException("Le service statistique est momentanement inaccessible.", "API_NETWORK_ERROR", null, retryable: true, ex.StatusCode, innerException: ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw await CreateRequestExceptionAsync(request, response, cancellationToken);

            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType;
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName;
            return (content, fileName, contentType);
        }
    }

    private async Task<ApiGetResult<T>> SendConditionalAsync<T>(HttpRequestMessage request, string? cacheKey, CancellationToken cancellationToken)
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
                innerException: ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                if (cacheKey is not null && _responseCache.TryGetValue(cacheKey, out var cached))
                {
                    return ApiGetResult<T>.NotModifiedWithValue((T?)cached);
                }
                return ApiGetResult<T>.NotModified();
            }

            if (!response.IsSuccessStatusCode)
            {
                throw await CreateRequestExceptionAsync(request, response, cancellationToken);
            }

            var value = await response.Content.ReadFromJsonAsync<T>(SerializerOptions, cancellationToken);

            if (cacheKey is not null && response.Headers.ETag is { Tag: { Length: > 0 } etag })
            {
                _etagCache[cacheKey] = etag;
                _responseCache[cacheKey] = value;
            }

            return ApiGetResult<T>.Ok(value);
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
        var problemDetail = TryReadProblemString(details, "detail");
        var retryable = response.StatusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            || (int)response.StatusCode >= 500;

        var retryAfterSeconds = TryReadRetryAfter(response);

        var userMessage = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                "Connexion non autorisee. Utilisez un compte habilite.",
            HttpStatusCode.BadRequest =>
                !string.IsNullOrWhiteSpace(problemDetail)
                    ? problemDetail.Trim()
                    : "Le filtre statistique n'a pas pu etre traite. Verifiez le perimetre choisi.",
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
            + $"code={technicalCode}; correlationId={correlationId}; retryAfter={retryAfterSeconds}s; body={details}");

        return new ApiRequestException(
            userMessage,
            technicalCode,
            correlationId,
            retryable,
            response.StatusCode,
            retryAfterSeconds);
    }

    private static int? TryReadRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter is not { } retryAfter)
        {
            return null;
        }

        if (retryAfter.Delta.HasValue)
        {
            return (int)Math.Ceiling(retryAfter.Delta.Value.TotalSeconds);
        }

        if (retryAfter.Date.HasValue)
        {
            var seconds = (retryAfter.Date.Value - DateTimeOffset.UtcNow).TotalSeconds;
            return seconds > 0 ? (int)Math.Ceiling(seconds) : 0;
        }

        return null;
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

public readonly struct ApiGetResult<T>
{
    public T? Value { get; }
    public bool IsNotModified { get; }

    private ApiGetResult(T? value, bool notModified)
    {
        Value = value;
        IsNotModified = notModified;
    }

    public static ApiGetResult<T> Ok(T? value) => new(value, notModified: false);

    public static ApiGetResult<T> NotModified() => new(default, notModified: true);

    // 304 with cached value: preserves IsNotModified flag so callers can skip re-processing,
    // but Value carries the previously received data so callers that only read Value get it.
    public static ApiGetResult<T> NotModifiedWithValue(T? value) => new(value, notModified: true);
}
