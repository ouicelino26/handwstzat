using System.Net;
using HandWStat.Configuration;
using HandWStat.Models.Contracts;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

public sealed class StatsApiClientTeamTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static (StatsApiClient client, MockMessageHandler handler) Create(HttpResponseMessage response)
    {
        var handler = new MockMessageHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        return (new StatsApiClient(http, new ApiSettings { BaseUrl = "http://test/" }, new StubAuthService()), handler);
    }

    // ── GetTeamStatsAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetTeamStats_Success_ReturnsTeamDto()
    {
        var dto = ApiTestHelpers.MakeTeamStats(teamId: 7);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetTeamStatsAsync(7);

        Assert.NotNull(result);
        Assert.Equal(7, result.TeamId);
        Assert.Equal("Team 7", result.TeamName);
        Assert.Equal(10, result.MatchesPlayed);
    }

    [Fact]
    public async Task GetTeamStats_Success_OverviewPopulated()
    {
        var dto = ApiTestHelpers.MakeTeamStats();
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetTeamStatsAsync(1);

        Assert.NotNull(result?.Overview);
        Assert.Equal(10, result.Overview.MatchCount);
        Assert.Equal(14, result.Overview.PlayerCount);
    }

    [Fact]
    public async Task GetTeamStats_NullResponse_ReturnsNull()
    {
        var (client, _) = Create(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new System.Net.Http.StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.GetTeamStatsAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTeamStats_PartialData_NullMetricsAreZeroed()
    {
        var dto = new TeamStatsDto { TeamId = 3, TeamName = "Partial FC", MatchesPlayed = 0 };
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetTeamStatsAsync(3);

        Assert.NotNull(result);
        Assert.Equal(0, result.GoalsFor);
        Assert.Equal(0, result.MatchesPlayed);
    }

    [Fact]
    public async Task GetTeamStats_UsesGetMethod()
    {
        var (client, handler) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeTeamStats()));

        await client.GetTeamStatsAsync(5);

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, req.Method);
    }

    [Fact]
    public async Task GetTeamStats_UrlContainsTeamId()
    {
        var (client, handler) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeTeamStats(42)));

        await client.GetTeamStatsAsync(42);

        var req = Assert.Single(handler.Requests);
        Assert.Contains("teams/42", req.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTeamStats_304_ReturnsNull()
    {
        var (client, _) = Create(ApiTestHelpers.NotModified());

        var result = await client.GetTeamStatsAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTeamStats_Http404_ThrowsApiRequestException()
    {
        var (client, _) = Create(ApiTestHelpers.NotFound("corr-team-404"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamStatsAsync(99));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("corr-team-404", ex.CorrelationId);
    }

    [Fact]
    public async Task GetTeamStats_Http429_RetryAfterSurfaced()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(retryAfterSeconds: 20));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamStatsAsync(1));

        Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
        Assert.Equal(20, ex.RetryAfterSeconds);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetTeamStats_Http503_ThrowsRetryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamStatsAsync(1));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetTeamStats_ContractError_TechnicalCodeFromBody()
    {
        var body = ApiTestHelpers.Serialize(new { code = "CONTRACT_ERROR", correlationId = "corr-team-contract" });
        var (client, _) = Create(ApiTestHelpers.StatusOnly(HttpStatusCode.UnprocessableEntity, body, "corr-team-contract"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamStatsAsync(1));

        Assert.Equal("CONTRACT_ERROR", ex.TechnicalCode);
    }

    [Fact]
    public async Task GetTeamStats_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeTeamStats()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetTeamStatsAsync(1, cancellationToken: cts.Token));
    }

    // ── GetTeamPlayersAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetTeamPlayers_Success_ReturnsList()
    {
        var players = new List<PlayerGlobalStatsDto>
        {
            new() { PlayerId = 1, FullName = "Alice" },
            new() { PlayerId = 2, FullName = "Bob" }
        };
        var (client, _) = Create(ApiTestHelpers.JsonOkList(players));

        var result = await client.GetTeamPlayersAsync(3);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTeamPlayers_EmptyList_ReturnsEmpty()
    {
        var (client, _) = Create(ApiTestHelpers.JsonOkList(new List<PlayerGlobalStatsDto>()));

        var result = await client.GetTeamPlayersAsync(3);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTeamPlayers_NullResponse_ThrowsApiRequestException()
    {
        // 200 + JSON null is an invalid API response — callers must not receive false empty list.
        var (client, _) = Create(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new System.Net.Http.StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamPlayersAsync(3));
    }

    [Fact]
    public async Task GetTeamPlayers_Http429_ThrowsWithRetryAfter()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(60));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamPlayersAsync(1));

        Assert.Equal(60, ex.RetryAfterSeconds);
    }

    [Fact]
    public async Task GetTeamPlayers_Http503_ThrowsRetryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetTeamPlayersAsync(1));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetTeamPlayers_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOkList(new List<PlayerGlobalStatsDto>()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetTeamPlayersAsync(1, cancellationToken: cts.Token));
    }
}
