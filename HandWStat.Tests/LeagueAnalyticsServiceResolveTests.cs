using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests;

public sealed class LeagueAnalyticsServiceResolveTests
{
    [Fact]
    public void NotModified304_ReturnsV2SourceWithoutAnalyticsObject()
    {
        // 304 Not Modified → Success with null response → caller reuses cached view
        var result = Service().Resolve(
            42,
            LeagueGatewayResult.Success(null),
            LeagueAnalyticsTestData.V1Snapshot(),
            LeagueAnalyticsTestData.Scope());

        Assert.Equal(AnalyticsSourceStatus.V2Complete, result.Source);
        Assert.Null(result.Analytics);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void FailedPivotPasses_V2DataMissing_IsNeverSubstitutedWithZero()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, LeagueAnalyticsTestData.Scope());

        var pivot = view.Offense.Single(m => m.MetricCode == "FAILED_PIVOT_PASSES");
        Assert.Null(pivot.CountValue);
        Assert.True(pivot.IsUnavailable);
        Assert.NotEqual(0, pivot.CountValue);
        Assert.Equal("DATA_MISSING", pivot.Availability);
        Assert.NotNull(pivot.UnavailableMessage);
        Assert.Contains("fichiers actuels", pivot.UnavailableMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void FailedPivotPasses_V1Snapshot_IsNeverSubstitutedWithBadPasses()
    {
        var snapshot = LeagueAnalyticsTestData.V1Snapshot();
        var view = LeaguePlayerAnalyticsMapper.FromV1(42, snapshot, LeagueAnalyticsTestData.Scope());

        var pivot = view.Offense.Single(m => m.MetricCode == "FAILED_PIVOT_PASSES");
        var badPasses = view.Offense.Single(m => m.MetricCode == "BAD_PASSES");

        Assert.Null(pivot.CountValue);
        Assert.NotEqual(badPasses.CountValue, pivot.CountValue);
        Assert.Equal("DATA_MISSING", pivot.Availability);
    }

    [Fact]
    public void FieldPlayer_V2_GoalkeeperMetricsPresent_ButIsGoalkeeperFalse()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse(isGoalkeeper: false);
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, LeagueAnalyticsTestData.Scope());

        Assert.False(view.IsGoalkeeper);
        // Goalkeeper metrics array is still populated — display logic decides whether to render
        Assert.Equal(13, view.Goalkeeper.Count);
    }

    [Fact]
    public void V2Scope_MatchCountFromOverview_NotFromScopeParameter()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var scope = LeagueAnalyticsTestData.Scope();
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, scope);

        // MatchesPlayed = 8 in CompleteResponse overview
        Assert.Equal(8, view.Scope.MatchCount);
    }

    [Fact]
    public void V1Scope_MatchCountFromScopeParameter_Unchanged()
    {
        var snapshot = LeagueAnalyticsTestData.V1Snapshot();
        var scope = LeagueAnalyticsTestData.Scope();
        var view = LeaguePlayerAnalyticsMapper.FromV1(42, snapshot, scope);

        // V1 does not override MatchCount from overview
        Assert.Equal(scope.MatchCount, view.Scope.MatchCount);
    }

    [Fact]
    public void AllOffenseMetrics_V2Complete_HaveCorrectSourceStatus()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, LeagueAnalyticsTestData.Scope());

        foreach (var metric in view.Offense.Where(m => m.MetricCode != "FAILED_PIVOT_PASSES"))
        {
            Assert.Equal(AnalyticsSourceStatus.V2Complete, metric.Source);
        }

        var pivot = view.Offense.Single(m => m.MetricCode == "FAILED_PIVOT_PASSES");
        Assert.Equal(AnalyticsSourceStatus.Unavailable, pivot.Source);
    }

    [Fact]
    public void AllDefenseMetrics_V2Complete_HaveCorrectSourceStatus()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, LeagueAnalyticsTestData.Scope());

        foreach (var metric in view.Defense)
        {
            Assert.Equal(AnalyticsSourceStatus.V2Complete, metric.Source);
        }
    }

    [Fact]
    public void V2MetricVersion_IsPreservedInRateDisplayModel()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, LeagueAnalyticsTestData.Scope());

        var totalShotRate = view.Offense.Single(m => m.MetricCode == "TOTAL_SHOT_RATE");
        Assert.Equal("1.0", totalShotRate.Rate!.MetricVersion);
    }

    [Fact]
    public void V2Response_PlayerId_MatchesViewModelPlayerId()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var view = LeaguePlayerAnalyticsMapper.FromV2(response, LeagueAnalyticsTestData.Scope());

        Assert.Equal(42, view.PlayerId);
    }

    private static LeaguePlayerAnalyticsService Service() => new(new StubGateway());

    private sealed class StubGateway : ILeagueAnalyticsGateway
    {
        public Task<LeagueGatewayResult> GetPlayerAsync(
            int playerId,
            StatsQueryOptionsDto options,
            IReadOnlyCollection<string> include,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
