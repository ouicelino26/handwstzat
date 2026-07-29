using System.Net;
using System.Text;
using System.Text.Json;
using HandWStat.Configuration;
using HandWStat.Models.Analytics;
using HandWStat.Services;
using HandWStat.Services.Analytics;
using HandballManagerCore.DTO;

namespace HandWStat.Tests;

public sealed class V2AnalyticsGatewayTests
{
    [Fact]
    public async Task CompleteResponse_PreservesContractEvidenceAndExactQuery()
    {
        var handler = new RecordingHandler((_, _) => Task.FromResult(JsonResponse(LeagueAnalyticsTestData.CompleteResponse())));
        var gateway = CreateGateway(handler);
        var options = new StatsQueryOptionsDto
        {
            CompetitionId = 12,
            TeamId = 7,
            PlayerId = 999,
            PositionId = 3,
            MatchId = 51,
            From = new DateTime(2026, 1, 1, 8, 30, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 2, 1, 8, 30, 0, DateTimeKind.Utc),
            Year = 2026,
            Season = "2025-2026",
            Day = "J12",
            AttackId = 4,
            DefenseId = 5,
            Trigger = "Transition",
            ShootShade = "Aile"
        };

        var result = await gateway.GetPlayerAsync(
            42,
            options,
            ["goalkeeper", "overview", "offense", "defense"],
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.Success, result.Outcome);
        Assert.Equal("1.0", result.Response!.MetricVersion);
        Assert.Equal(6, result.Response.Offense!.TotalShotRate.Sample.Numerator);
        Assert.Equal(10, result.Response.Offense.TotalShotRate.Sample.Denominator);
        Assert.Equal(4, result.Response.Offense.TotalShotRate.MinimumSample);
        Assert.True(result.Response.Offense.TotalShotRate.Quality.SampleReliable);
        Assert.Equal(1, result.Response.Offense.TotalShotRate.Quality.QualityScore);

        var requestUri = Uri.UnescapeDataString(handler.LastRequest!.RequestUri!.ToString());
        Assert.Contains("api/v2/analytics/players/42", requestUri, StringComparison.Ordinal);
        Assert.Contains("include=defense,goalkeeper,offense,overview", requestUri, StringComparison.Ordinal);
        Assert.Contains("competitionId=12", requestUri, StringComparison.Ordinal);
        Assert.Contains("teamId=7", requestUri, StringComparison.Ordinal);
        Assert.Contains("matchId=51", requestUri, StringComparison.Ordinal);
        Assert.Contains("season=2025-2026", requestUri, StringComparison.Ordinal);
        Assert.Contains("day=J12", requestUri, StringComparison.Ordinal);
        Assert.Contains("attackId=4", requestUri, StringComparison.Ordinal);
        Assert.Contains("defenseId=5", requestUri, StringComparison.Ordinal);
        Assert.Contains("trigger=Transition", requestUri, StringComparison.Ordinal);
        Assert.Contains("shootShade=Aile", requestUri, StringComparison.Ordinal);
        Assert.DoesNotContain("playerId=", requestUri, StringComparison.Ordinal);
        Assert.DoesNotContain("positionId=", requestUri, StringComparison.Ordinal);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("token", handler.LastRequest.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task PartialResponse_IsValidWhenIncludedMatchesRequest()
    {
        var complete = LeagueAnalyticsTestData.CompleteResponse();
        var partial = complete with
        {
            Included = ["overview"],
            Offense = null,
            Defense = null,
            Goalkeeper = null
        };
        var gateway = CreateGateway(new RecordingHandler((_, _) => Task.FromResult(JsonResponse(partial))));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            ["overview"],
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.Success, result.Outcome);
        Assert.NotNull(result.Response!.Overview);
        Assert.Null(result.Response.Offense);
    }

    [Fact]
    public async Task NullRateValue_PreservesZeroSampleAndQualityReason()
    {
        var gateway = CreateGateway(new RecordingHandler(
            (_, _) => Task.FromResult(JsonResponse(LeagueAnalyticsTestData.ZeroDenominatorResponse()))));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        var rate = result.Response!.Offense!.TotalShotRate;
        Assert.Null(rate.Value);
        Assert.Equal(0, rate.Sample.Numerator);
        Assert.Equal(0, rate.Sample.Denominator);
        Assert.False(rate.Quality.SampleReliable);
        Assert.Equal("ZERO_OR_INVALID_DENOMINATOR", rate.Quality.Reason);
    }

    [Fact]
    public async Task CancellationToken_IsPropagatedToHttpHandler()
    {
        CancellationToken observed = default;
        var handler = new RecordingHandler((_, token) =>
        {
            observed = token;
            return Task.FromResult(JsonResponse(LeagueAnalyticsTestData.CompleteResponse()));
        });
        var gateway = CreateGateway(handler);
        using var cancellation = new CancellationTokenSource();

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            cancellation.Token);

        Assert.Equal(LeagueGatewayOutcome.Success, result.Outcome);
        Assert.True(observed.CanBeCanceled);
    }

    [Fact]
    public async Task RequestedCancellation_IsNotConvertedToFallback()
    {
        var handler = new RecordingHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return JsonResponse(LeagueAnalyticsTestData.CompleteResponse());
        });
        var gateway = CreateGateway(handler);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(25));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            cancellation.Token));
    }

    [Fact]
    public async Task Timeout_IsDistinguishedAndRetryable()
    {
        var gateway = CreateGateway(new RecordingHandler(
            (_, _) => throw new TaskCanceledException("simulated timeout")));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.Timeout, result.Outcome);
        Assert.Equal("API_TIMEOUT", result.Error!.TechnicalCode);
        Assert.True(result.Error.Retryable);
    }

    [Fact]
    public async Task NotFound_IsPlayerMissingAndNeverEndpointFallback()
    {
        var gateway = CreateGateway(new RecordingHandler(
            (_, _) => Task.FromResult(ProblemResponse(HttpStatusCode.NotFound, "PLAYER_NOT_FOUND"))));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.NotFound, result.Outcome);
        Assert.Equal(HttpStatusCode.NotFound, result.Error!.StatusCode);
    }

    [Fact]
    public async Task ServerError_IsDistinguishedAndRetryable()
    {
        var gateway = CreateGateway(new RecordingHandler(
            (_, _) => Task.FromResult(ProblemResponse(HttpStatusCode.InternalServerError, "LEAGUE_FAILURE"))));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.ServerError, result.Outcome);
        Assert.Equal("LEAGUE_FAILURE", result.Error!.TechnicalCode);
        Assert.True(result.Error.Retryable);
    }

    [Fact]
    public async Task InvalidJson_ReturnsContractError()
    {
        var gateway = CreateGateway(new RecordingHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{not-json", Encoding.UTF8, "application/json")
            })));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.ContractError, result.Outcome);
        Assert.Equal("LEAGUE_V2_JSON_INVALID", result.Error!.TechnicalCode);
    }

    [Fact]
    public async Task StructurallyInconsistentResponse_ReturnsContractError()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        response = response with
        {
            Offense = response.Offense! with { TotalGoals = 99 }
        };
        var gateway = CreateGateway(new RecordingHandler((_, _) => Task.FromResult(JsonResponse(response))));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.ContractError, result.Outcome);
        Assert.Equal("LEAGUE_V2_CONTRACT_INVALID", result.Error!.TechnicalCode);
    }

    [Fact]
    public async Task MissingRequiredJsonProperties_ReturnsIncompleteContractError()
    {
        var gateway = CreateGateway(new RecordingHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"playerId\":42,\"metricVersion\":\"1.0\"}",
                    Encoding.UTF8,
                    "application/json")
            })));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.ContractError, result.Outcome);
        Assert.Equal("LEAGUE_V2_CONTRACT_INCOMPLETE", result.Error!.TechnicalCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.NotImplemented)]
    public async Task EndpointUnavailable_IsTheOnlyFallbackOutcome(HttpStatusCode statusCode)
    {
        var gateway = CreateGateway(new RecordingHandler(
            (_, _) => Task.FromResult(ProblemResponse(statusCode, "ENDPOINT_UNAVAILABLE"))));

        var result = await gateway.GetPlayerAsync(
            42,
            new StatsQueryOptionsDto(),
            LeagueAnalyticsContract.AllSections,
            CancellationToken.None);

        Assert.Equal(LeagueGatewayOutcome.Unavailable, result.Outcome);
    }

    private static V2AnalyticsGateway CreateGateway(HttpMessageHandler handler)
    {
        return new V2AnalyticsGateway(
            new HttpClient(handler),
            new ApiSettings { BaseUrl = "https://example.test/" },
            new StubAuthService());
    }

    private static HttpResponseMessage JsonResponse(LeaguePlayerAnalyticsResponseDto response) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(response, LeagueAnalyticsTestData.JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

    private static HttpResponseMessage ProblemResponse(HttpStatusCode statusCode, string code) =>
        new(statusCode)
        {
            Content = new StringContent(
                $"{{\"title\":\"Safe title\",\"code\":\"{code}\",\"correlationId\":\"corr-league\"}}",
                Encoding.UTF8,
                "application/problem+json")
        };

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

        public RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        {
            _send = send;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return _send(request, cancellationToken);
        }
    }

    private sealed class StubAuthService : IApiAuthService
    {
        public ApiSession Session { get; } = new(true, "test", "Consultation", "token", null);

        public event Action? SessionChanged;

        public Task<ApiSession> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Session);

        public void Logout() => SessionChanged?.Invoke();

        public void ApplyAuthorization(HttpRequestMessage request)
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.AccessToken);
        }
    }
}
