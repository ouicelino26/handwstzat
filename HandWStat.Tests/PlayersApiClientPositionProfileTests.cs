using System.Net;
using HandWStat.Configuration;
using HandWStat.Models.Contracts;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

public sealed class PlayersApiClientPositionProfileTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static (PlayersApiClient client, MockMessageHandler handler) Create(HttpResponseMessage response)
    {
        var handler = new MockMessageHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        return (new PlayersApiClient(http, new ApiSettings { BaseUrl = "http://test/" }, new StubAuthService()), handler);
    }

    // ── GetPlayerPositionProfileAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetPositionProfile_ValidPlayer_ReturnsProfileWithSelectedPlayer()
    {
        var dto = ApiTestHelpers.MakePositionProfile(42);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(42);

        Assert.NotNull(result);
        Assert.NotNull(result.SelectedPlayer);
        Assert.Equal(42, result.SelectedPlayer.PlayerId);
        Assert.Equal(24, result.CohortPlayerCount);
    }

    [Fact]
    public async Task GetPositionProfile_PlayerWithoutProfile_SelectedPlayerIsNull()
    {
        var dto = new PositionProfileResponseDto
        {
            PositionId = 3,
            PositionCode = "ARG",
            CohortPlayerCount = 18,
            SelectedPlayer = null,
            MedianProfile = null,
            Players = []
        };
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(99);

        Assert.NotNull(result);
        Assert.Null(result.SelectedPlayer);
    }

    [Fact]
    public async Task GetPositionProfile_EmptyCohort_CohortCountIsZero()
    {
        var dto = new PositionProfileResponseDto
        {
            PositionId = 1,
            CohortPlayerCount = 0,
            SelectedPlayer = null,
            Players = []
        };
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(1);

        Assert.NotNull(result);
        Assert.Equal(0, result.CohortPlayerCount);
    }

    [Fact]
    public async Task GetPositionProfile_SmallSample_ProfileStillReturned()
    {
        var dto = ApiTestHelpers.MakePositionProfile(5);
        dto.CohortPlayerCount = 3;
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(5);

        Assert.NotNull(result);
        Assert.Equal(3, result.CohortPlayerCount);
    }

    [Fact]
    public async Task GetPositionProfile_AxisHigherIsBetterTrue_ValueRetained()
    {
        var dto = ApiTestHelpers.MakePositionProfile(10);
        var higherBetter = dto.SelectedPlayer!.Axes.First(a => a.HigherIsBetter);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(10);

        var axis = result!.SelectedPlayer!.Axes.First(a => a.Key == higherBetter.Key);
        Assert.True(axis.HigherIsBetter);
        Assert.Equal(higherBetter.Value, axis.Value);
        Assert.Equal(higherBetter.Percentile, axis.Percentile);
    }

    [Fact]
    public async Task GetPositionProfile_AxisHigherIsBetterFalse_ValueRetained()
    {
        var dto = ApiTestHelpers.MakePositionProfile(10);
        var lowerBetter = dto.SelectedPlayer!.Axes.First(a => !a.HigherIsBetter);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(10);

        var axis = result!.SelectedPlayer!.Axes.First(a => a.Key == lowerBetter.Key);
        Assert.False(axis.HigherIsBetter);
        Assert.Equal(lowerBetter.Value, axis.Value);
    }

    [Fact]
    public async Task GetPositionProfile_GoalkeeperProfile_FlagSet()
    {
        var dto = ApiTestHelpers.MakePositionProfile(1);
        dto.IsGoalkeeperProfile = true;
        dto.PositionCode = "GB";
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var result = await client.GetPlayerPositionProfileAsync(1);

        Assert.NotNull(result);
        Assert.True(result.IsGoalkeeperProfile);
        Assert.Equal("GB", result.PositionCode);
    }

    [Fact]
    public async Task GetPositionProfile_UrlContainsPlayerIdAndEndpoint()
    {
        var dto = ApiTestHelpers.MakePositionProfile(33);
        var (client, handler) = Create(ApiTestHelpers.JsonOk(dto));

        await client.GetPlayerPositionProfileAsync(33);

        var req = Assert.Single(handler.Requests);
        Assert.Contains("Players/33/position-profile", req.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPositionProfile_304_ReturnsNull()
    {
        var (client, _) = Create(ApiTestHelpers.NotModified());

        var result = await client.GetPlayerPositionProfileAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPositionProfile_Http404_Throws()
    {
        var (client, _) = Create(ApiTestHelpers.NotFound("corr-profile-404"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetPlayerPositionProfileAsync(404));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("corr-profile-404", ex.CorrelationId);
    }

    [Fact]
    public async Task GetPositionProfile_Http429_RetryAfterSurfaced()
    {
        var (client, _) = Create(ApiTestHelpers.TooManyRequests(retryAfterSeconds: 35));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetPlayerPositionProfileAsync(1));

        Assert.Equal(35, ex.RetryAfterSeconds);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetPositionProfile_Http503_Retryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetPlayerPositionProfileAsync(1));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task GetPositionProfile_ContractError_TechnicalCodePreserved()
    {
        var body = ApiTestHelpers.Serialize(new { code = "CONTRACT_ERROR", correlationId = "corr-profile-contract" });
        var (client, _) = Create(ApiTestHelpers.StatusOnly(HttpStatusCode.UnprocessableEntity, body, "corr-profile-contract"));

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() => client.GetPlayerPositionProfileAsync(1));

        Assert.Equal("CONTRACT_ERROR", ex.TechnicalCode);
        Assert.Equal("corr-profile-contract", ex.CorrelationId);
    }

    [Fact]
    public async Task GetPositionProfile_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakePositionProfile()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetPlayerPositionProfileAsync(1, cancellationToken: cts.Token));
    }

    // ── ComparePositionProfilesAsync (POST) ───────────────────────────────────

    [Fact]
    public async Task ComparePositionProfiles_Success_ReturnsProfile()
    {
        var dto = ApiTestHelpers.MakePositionProfile(10);
        var (client, _) = Create(ApiTestHelpers.JsonOk(dto));

        var request = new PositionProfileCompareRequestDto { PlayerIds = [10, 20] };
        var result = await client.ComparePositionProfilesAsync(request);

        Assert.NotNull(result);
        Assert.NotNull(result.SelectedPlayer);
    }

    [Fact]
    public async Task ComparePositionProfiles_UsesPostMethod()
    {
        var dto = ApiTestHelpers.MakePositionProfile(1);
        var (client, handler) = Create(ApiTestHelpers.JsonOk(dto));

        await client.ComparePositionProfilesAsync(new PositionProfileCompareRequestDto { PlayerIds = [1] });

        var req = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, req.Method);
    }

    [Fact]
    public async Task ComparePositionProfiles_DoesNotSendIfNoneMatch()
    {
        var dto = ApiTestHelpers.MakePositionProfile(1);
        var (client, handler) = Create(ApiTestHelpers.JsonOk(dto));

        await client.ComparePositionProfilesAsync(new PositionProfileCompareRequestDto { PlayerIds = [1] });

        var req = Assert.Single(handler.Requests);
        Assert.False(req.Headers.IfNoneMatch.Any());
    }

    [Fact]
    public async Task ComparePositionProfiles_Http503_Retryable()
    {
        var (client, _) = Create(ApiTestHelpers.ServiceUnavailable());

        var ex = await Assert.ThrowsAsync<ApiRequestException>(() =>
            client.ComparePositionProfilesAsync(new PositionProfileCompareRequestDto { PlayerIds = [1, 2] }));

        Assert.True(ex.Retryable);
    }

    [Fact]
    public async Task ComparePositionProfiles_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var (client, _) = Create(ApiTestHelpers.JsonOk(ApiTestHelpers.MakePositionProfile()));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.ComparePositionProfilesAsync(
                new PositionProfileCompareRequestDto { PlayerIds = [1] },
                cts.Token));
    }
}
