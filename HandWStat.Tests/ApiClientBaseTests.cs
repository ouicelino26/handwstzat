using System.Net;
using System.Text;
using HandWStat.Configuration;
using HandWStat.Services;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

public sealed class ApiClientBaseTests
{
    [Fact]
    public async Task FailedProblemDetails_ReturnsSafeTypedError()
    {
        const string technicalBody = "{\"title\":\"SQL stack trace\",\"code\":\"ANALYTICS_FAILURE\",\"correlationId\":\"corr-123\"}";
        using var httpClient = new HttpClient(new StubHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(technicalBody, Encoding.UTF8, "application/problem+json")
        }));
        var client = new TestApiClient(httpClient, new ApiSettings { BaseUrl = "https://example.test/" }, new StubAuthService());

        var error = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetValueAsync());

        Assert.Equal("ANALYTICS_FAILURE", error.TechnicalCode);
        Assert.Equal("corr-123", error.CorrelationId);
        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);
        Assert.True(error.Retryable);
        Assert.DoesNotContain("SQL stack trace", error.UserMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TooManyRequests_SurfacesRetryAfterSecondsFromDeltaHeader()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.Add("Retry-After", "42");
        using var httpClient = new HttpClient(new StubHandler(response));
        var client = new TestApiClient(httpClient, new ApiSettings { BaseUrl = "https://example.test/" }, new StubAuthService());

        var error = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetValueAsync());

        Assert.Equal(42, error.RetryAfterSeconds);
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
    }

    [Fact]
    public async Task CorrelationId_IsExtractedFromXCorrelationIdHeaderWhenBodyIsNotProblemDetails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        response.Headers.Add("X-Correlation-ID", "header-corr-456");
        using var httpClient = new HttpClient(new StubHandler(response));
        var client = new TestApiClient(httpClient, new ApiSettings { BaseUrl = "https://example.test/" }, new StubAuthService());

        var error = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetValueAsync());

        Assert.Equal("header-corr-456", error.CorrelationId);
    }

    private sealed class TestApiClient : ApiClientBase
    {
        public TestApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
            : base(httpClient, settings, authService)
        {
        }

        public Task<object?> GetValueAsync() => GetAsync<object>("api/test");
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_response);
    }

    private sealed class StubAuthService : IApiAuthService
    {
        public ApiSession Session { get; } = new(true, "test", "User", "token", null);

        public event Action? SessionChanged;

        public Task<ApiSession> LoginAsync(string username, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(Session);

        public void Logout() => SessionChanged?.Invoke();

        public void ApplyAuthorization(HttpRequestMessage request)
        {
        }
    }
}
