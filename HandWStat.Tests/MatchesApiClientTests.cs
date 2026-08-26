using System.Net;
using HandWStat.Configuration;
using HandWStat.Models.Contracts;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

public sealed class MatchesApiClientTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static (MatchesApiClient client, MockMessageHandler handler) Create(HttpResponseMessage response)
    {
        var handler = new MockMessageHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        return (new MatchesApiClient(http, new ApiSettings { BaseUrl = "http://test/" }, new StubAuthService()), handler);
    }

    // ── GetMatchesAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetMatches_Success_ReturnsList()
    {
        var matches = new List<MatchListItemDto>
        {
            ApiTestHelpers.MakeMatch(1),
            ApiTestHelpers.MakeMatch(2)
        };
        var (client, _) = Create(ApiTestHelpers.JsonOkList(matches));

        var result = await client.GetMatchesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].MatchId);
        Assert.Equal(2, result[1].MatchId);
    }

    [Fact]
    public async Task GetMatches_EmptyList_ReturnsEmpty()
    {
        var (client, _) = Create(ApiTestHelpers.JsonOkList(new List<MatchListItemDto>()));

        var result = await client.GetMatchesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMatches_NullResponse_ThrowsApiRequestException()
    {
        // 200 + JSON null is an invalid API response — callers must not receive false empty list.
        var (client, _) = Create(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new System.Net.Http.StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchesAsync());
    }

    [Fact]
    public async Task GetMatches_WithTeamIdFilter_UrlContainsTeamId()
    {
        var (client, handler) = Create(ApiTestHelpers.JsonOkList(new List<MatchListItemDto>()));

        await client.GetMatchesAsync(teamId: 15);

        var req = Assert.Single(handler.Requests);
        Assert.Contains("teamId=15", req.RequestUri!.Query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMatches_WithSeasonFilter_UrlContainsSeason()
    {
        var (client, handler) = Create(ApiTestHelpers.JsonOkList(new List<MatchListItemDto>()));

        await client.GetMatchesAsync(season: "2025-2026");

        var req = Assert.Single(handler.Requests);
        Assert.Contains("season=", req.RequestUri!.Query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMatches_Http404_Throws()
    {
        var (client, _) = Create(ApiTestHelpers.NotFound());

        await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchesAsync());
    }

    [Fact]
    public async Task GetMatches_Http429_WithRetryAfter()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(retryAfterSeconds: 30, correlationId: "corr-match-429"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchesAsync());

        Assert.Equal(30, ex.RetryAfterSeconds);
        Assert.Equal("corr-match-429", ex.CorrelationId);
    }

    [Fact]
    public async Task GetMatches_Http503_Retryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchesAsync());

        Assert.True(ex.Retryable);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Fact]
    public async Task GetMatches_InvalidJson_Throws()
    {
        var (client, _) = Create(ApiTestHelpers.InvalidJson());

        await Assert.ThrowsAnyAsync<Exception>(() => client.GetMatchesAsync());
    }

    [Fact]
    public async Task GetMatches_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOkList(new List<MatchListItemDto>()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetMatchesAsync(cancellationToken: cts.Token));
    }

    // ── GetMatchAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMatch_Success_ReturnsMatchDto()
    {
        var match = ApiTestHelpers.MakeMatch(42);
        var (client, _) = Create(ApiTestHelpers.JsonOk(match));

        var result = await client.GetMatchAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.MatchId);
        Assert.Equal("Home", result.Team1Name);
    }

    [Fact]
    public async Task GetMatch_UrlContainsMatchId()
    {
        var (client, handler) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeMatch(77)));

        await client.GetMatchAsync(77);

        var req = Assert.Single(handler.Requests);
        Assert.Contains("Matches/77", req.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMatch_NullResponse_ReturnsNull()
    {
        var (client, _) = Create(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new System.Net.Http.StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.GetMatchAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMatch_304_ReturnsNull()
    {
        var (client, _) = Create(ApiTestHelpers.NotModified());

        var result = await client.GetMatchAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMatch_Http404_Throws()
    {
        var (client, _) = Create(ApiTestHelpers.NotFound("corr-getmatch-404"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchAsync(999));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("corr-getmatch-404", ex.CorrelationId);
    }

    [Fact]
    public async Task GetMatch_Http429_RetryAfterSurfaced()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(retryAfterSeconds: 15));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchAsync(1));

        Assert.Equal(15, ex.RetryAfterSeconds);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetMatch_Http503_Retryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchAsync(1));

        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetMatch_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeMatch(1)));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetMatchAsync(1, cts.Token));
    }

    // ── GetMatchSummaryAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetMatchSummary_Success_ReturnsSummary()
    {
        var summary = ApiTestHelpers.MakeMatchSummary(10);
        var (client, _) = Create(ApiTestHelpers.JsonOk(summary));

        var result = await client.GetMatchSummaryAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.MatchId);
        Assert.Equal("Home Team", result.Team1Name);
        Assert.Equal(28, result.Team1Score);
    }

    [Fact]
    public async Task GetMatchSummary_PartialData_MissingEventsAreZero()
    {
        var summary = new MatchSummaryDto { MatchId = 5, Team1Name = "A", Team2Name = "B" };
        var (client, _) = Create(ApiTestHelpers.JsonOk(summary));

        var result = await client.GetMatchSummaryAsync(5);

        Assert.NotNull(result);
        Assert.Equal(0, result.EventCount);
        Assert.Equal(0, result.GoalCount);
    }

    [Fact]
    public async Task GetMatchSummary_EmptyTopScorers_ReturnsSummaryWithEmptyList()
    {
        var summary = ApiTestHelpers.MakeMatchSummary(3);
        summary.TopScorers.Clear();
        var (client, _) = Create(ApiTestHelpers.JsonOk(summary));

        var result = await client.GetMatchSummaryAsync(3);

        Assert.NotNull(result);
        Assert.Empty(result.TopScorers);
    }

    [Fact]
    public async Task GetMatchSummary_304_ReturnsNull()
    {
        var (client, _) = Create(ApiTestHelpers.NotModified());

        var result = await client.GetMatchSummaryAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMatchSummary_Http404_Throws()
    {
        var (client, _) = Create(ApiTestHelpers.NotFound("corr-summary-404"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchSummaryAsync(404));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("corr-summary-404", ex.CorrelationId);
    }

    [Fact]
    public async Task GetMatchSummary_Http429_RetryAfterSurfaced()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(retryAfterSeconds: 25, correlationId: "corr-429-summary"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchSummaryAsync(1));

        Assert.Equal(25, ex.RetryAfterSeconds);
        Assert.Equal("corr-429-summary", ex.CorrelationId);
    }

    [Fact]
    public async Task GetMatchSummary_Http503_Retryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetMatchSummaryAsync(1));

        Assert.True(ex.Retryable);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Fact]
    public async Task GetMatchSummary_InvalidJson_Throws()
    {
        var (client, _) = Create(ApiTestHelpers.InvalidJson());

        await Assert.ThrowsAnyAsync<Exception>(() => client.GetMatchSummaryAsync(1));
    }

    [Fact]
    public async Task GetMatchSummary_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeMatchSummary()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetMatchSummaryAsync(1, cts.Token));
    }
}
