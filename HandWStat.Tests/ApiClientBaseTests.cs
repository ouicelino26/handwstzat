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
