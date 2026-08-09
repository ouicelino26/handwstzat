using System.Net;
using HandWStat.Configuration;
using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

public sealed class StatsApiClientCompareTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static (StatsApiClient client, MockMessageHandler handler) Create(HttpResponseMessage response)
    {
        var handler = new MockMessageHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        return (new StatsApiClient(http, new ApiSettings { BaseUrl = "http://test/" }, new StubAuthService()), handler);
    }

    private static ComparePlayersRequestDto Request(params int[] ids) => new() { PlayerIds = ids.ToList() };

    // ── SUCCESS cases ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TwoPlayers_ReturnsParallelListsWithBothIds()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(10, 20);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.ComparePlayersAsync(Request(10, 20));

        Assert.NotNull(result);
        Assert.Equal(2, result.Players.Count);
        Assert.Equal(2, result.Offense.Count);
        Assert.Equal(2, result.Defense.Count);
        Assert.Equal(2, result.Goalkeeper.Count);
        Assert.Equal(10, result.Players[0].PlayerId);
        Assert.Equal(20, result.Players[1].PlayerId);
    }

    [Fact]
    public async Task ThreePlayers_ReturnsAllThreeInParallelLists()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(1, 2, 3);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.ComparePlayersAsync(Request(1, 2, 3));

        Assert.NotNull(result);
        Assert.Equal(3, result.Players.Count);
        Assert.Equal(3, result.Technical.Count);
        Assert.Equal(3, result.Passing.Count);
        Assert.Equal(3, result.Sanctions.Count);
    }

    [Fact]
    public async Task SixPlayers_ReturnsAllSixWhenContractAllows()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(1, 2, 3, 4, 5, 6);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.ComparePlayersAsync(Request(1, 2, 3, 4, 5, 6));

        Assert.NotNull(result);
        Assert.Equal(6, result.Players.Count);
    }

    [Fact]
    public async Task EmptyPlayerIdList_ReturnsEmptyResponse()
    {
        var dto = new ComparePlayersResponseDto();
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.ComparePlayersAsync(Request());

        Assert.NotNull(result);
        Assert.Empty(result.Players);
    }

    [Fact]
    public async Task GoalkeeperPlayer_GoalkeeperListPopulated()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(99);
        dto.Players[0].IsGoalkeeper = true;
        dto.Goalkeeper[0].TauxArret = 0.65;
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.ComparePlayersAsync(Request(99));

        Assert.NotNull(result);
        Assert.True(result.Players[0].IsGoalkeeper);
        Assert.Equal(0.65, result.Goalkeeper[0].TauxArret);
    }

    [Fact]
    public async Task MissingMetrics_SomeListsEmptyOthersPopulated()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(5, 6);
        dto.Offense.Clear();
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.ComparePlayersAsync(Request(5, 6));

        Assert.NotNull(result);
        Assert.Equal(2, result.Players.Count);
        Assert.Empty(result.Offense);
    }

    [Fact]
    public async Task NullResponse_ReturnsNull()
    {
        var (client, _) = Create(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new System.Net.Http.StringContent("null", System.Text.Encoding.UTF8, "application/json")
        });

        var result = await client.ComparePlayersAsync(Request(1));

        Assert.Null(result);
    }

    [Fact]
    public async Task PostRequest_DoesNotSendIfNoneMatchHeader()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(7, 8);
        var (client, handler) = Create(ApiTestHelpers.JsonOk(dto));

        await client.ComparePlayersAsync(Request(7, 8));

        var req = Assert.Single(handler.Requests);
        Assert.False(req.Headers.IfNoneMatch.Any());
    }

    [Fact]
    public async Task PostRequest_UsesPostMethod()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(1);
        var (client, handler) = Create(ApiTestHelpers.JsonOk(dto));

        await client.ComparePlayersAsync(Request(1));

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, req.Method);
    }

    [Fact]
    public async Task PostRequest_UrlIsCorrect()
    {
        var dto = ApiTestHelpers.MakeCompareResponse(1);
        var (client, handler) = Create(ApiTestHelpers.JsonOk(dto));

        await client.ComparePlayersAsync(Request(1));

        var req = Assert.Single(handler.Requests);
        Assert.Contains("Stats/compare/players", req.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    // ── ERROR cases ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Http400_ThrowsApiRequestException_NotRetryable()
    {
        var (client, _) = Create(ApiTestHelpers.StatusOnly(HttpStatusCode.BadRequest));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1)));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.False(ex.Retryable);
    }

    [Fact]
    public async Task Http401_ThrowsApiRequestException_WithAuthMessage()
    {
        var (client, _) = Create(ApiTestHelpers.StatusOnly(HttpStatusCode.Unauthorized));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1)));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.False(ex.Retryable);
    }

    [Fact]
    public async Task Http403_ThrowsApiRequestException_WithForbiddenMessage()
    {
        var (client, _) = Create(ApiTestHelpers.StatusOnly(HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1)));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.False(ex.Retryable);
    }

    [Fact]
    public async Task Http404_ThrowsApiRequestException()
    {
        var (client, _) = Create(ApiTestHelpers.NotFound("corr-compare-404"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1)));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("corr-compare-404", ex.CorrelationId);
    }

    [Fact]
    public async Task Http429_ThrowsApiRequestException_WithRetryAfter()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(retryAfterSeconds: 45, correlationId: "corr-429"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1)));

        Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
        Assert.Equal(45, ex.RetryAfterSeconds);
        Assert.True(ex.Retryable);
        Assert.Equal("corr-429", ex.CorrelationId);
    }

    [Fact]
    public async Task Http503_ThrowsApiRequestException_Retryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable("corr-503"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1)));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task Http503_DoesNotSilentlyFallback_ThrowsDirectly()
    {
        var (client, handler) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1, 2)));

        Assert.Single(handler.Requests);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Fact]
    public async Task InvalidJson_ThrowsContractError()
    {
        var (client, _) = Create(ApiTestHelpers.InvalidJson());

        await Assert.ThrowsAnyAsync<Exception>(() => client.ComparePlayersAsync(Request(1)));
    }

    [Fact]
    public async Task ContractError_TechnicalCodePreservedFromBody()
    {
        var body = ApiTestHelpers.Serialize(new { code = "CONTRACT_ERROR", correlationId = "corr-contract" });
        var (client, _) = Create(ApiTestHelpers.StatusOnly(HttpStatusCode.UnprocessableEntity, body, "corr-contract"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.ComparePlayersAsync(Request(1, 2)));

        Assert.Equal("CONTRACT_ERROR", ex.TechnicalCode);
        Assert.Equal("corr-contract", ex.CorrelationId);
    }

    [Fact]
    public async Task Cancellation_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handler = new MockMessageHandler(ApiTestHelpers.JsonOk(ApiTestHelpers.MakeCompareResponse(1)));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var client = new StatsApiClient(http, new ApiSettings { BaseUrl = "http://test/" }, new StubAuthService());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.ComparePlayersAsync(Request(1), cts.Token));
    }
}
