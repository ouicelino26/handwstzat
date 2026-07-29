using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;
using HandballManagerCore.DTO;

namespace HandWStat.Tests;

public sealed class LeagueAnalyticsFallbackTests
{
    [Fact]
    public void ValidV2_MapsAllMetricsInRequiredOrderAndPreservesEvidence()
    {
        var response = LeagueAnalyticsTestData.CompleteResponse();
        var result = Service().Resolve(
            42,
            LeagueGatewayResult.Success(response),
            LeagueAnalyticsTestData.V1Snapshot(),
            LeagueAnalyticsTestData.Scope());

        Assert.Equal(AnalyticsSourceStatus.V2Complete, result.Source);
        Assert.Equal("1.0", result.Analytics!.MetricVersion);
        Assert.Equal(
            [
                "TOTAL_GOALS",
                "OPEN_PLAY_GOALS",
                "PENALTY_GOALS",
                "ASSISTS",
                "PENALTIES_WON",
                "SANCTIONS_DRAWN",
                "TOTAL_TURNOVERS",
                "BAD_PASSES",
                "FAILED_PIVOT_PASSES",
                "TOTAL_SHOT_RATE",
                "OPEN_PLAY_SHOT_RATE",
                "PENALTY_SHOT_RATE"
            ],
            result.Analytics.Offense.Select(metric => metric.MetricCode));
        Assert.Equal(
            [
                "INTERCEPTIONS",
                "BLOCKS",
                "OFFENSIVE_FOULS_DRAWN",
                "NEUTRALIZATIONS",
                "PENALTIES_CONCEDED",
                "SANCTIONS_CONCEDED",
                "WARNINGS_CONCEDED",
                "TWO_MINUTE_SUSPENSIONS_CONCEDED",
                "DISQUALIFICATIONS_CONCEDED"
            ],
            result.Analytics.Defense.Select(metric => metric.MetricCode));
        Assert.Equal(13, result.Analytics.Goalkeeper.Count);

        var rate = Metric(result.Analytics.Offense, "TOTAL_SHOT_RATE").Rate!;
        Assert.Equal(60, rate.Value);
        Assert.Equal(6, rate.Numerator);
        Assert.Equal(10, rate.Denominator);
        Assert.Equal(4, rate.MinimumSample);
        Assert.True(rate.SampleReliable);
        Assert.True(rate.QualityKnown);
        Assert.Equal(1, rate.QualityScore);
        Assert.Equal("1.0", rate.MetricVersion);
        Assert.Equal("API v2 complète", rate.SourceLabel);
    }

    [Fact]
    public void EndpointUnavailable_UsesOnlyCompatibleV1MetricsWithExplicitProvenance()
    {
        var unavailable = LeagueGatewayResult.Failure(
            LeagueGatewayOutcome.Unavailable,
            new LeagueAnalyticsError(
                "Endpoint absent",
                "HTTP_501",
                "corr-1",
                true,
                System.Net.HttpStatusCode.NotImplemented));

        var result = Service().Resolve(
            42,
            unavailable,
            LeagueAnalyticsTestData.V1Snapshot(),
            LeagueAnalyticsTestData.Scope());

        Assert.True(result.IsSuccess);
        Assert.Equal(AnalyticsSourceStatus.V1Partial, result.Source);
        Assert.Null(result.Analytics!.MetricVersion);

        AssertCount(result.Analytics.Offense, "TOTAL_GOALS", 6, AnalyticsSourceStatus.V1Compatible);
        AssertCount(result.Analytics.Offense, "OPEN_PLAY_GOALS", 5, AnalyticsSourceStatus.V1Compatible);
        AssertCount(result.Analytics.Offense, "PENALTY_GOALS", 1, AnalyticsSourceStatus.V1Compatible);
        AssertCount(result.Analytics.Offense, "ASSISTS", 3, AnalyticsSourceStatus.V1Compatible);
        AssertCount(result.Analytics.Offense, "TOTAL_TURNOVERS", 10, AnalyticsSourceStatus.V1Compatible);
        AssertCount(result.Analytics.Offense, "BAD_PASSES", 2, AnalyticsSourceStatus.V1Compatible);

        Assert.Null(Metric(result.Analytics.Offense, "PENALTIES_WON").CountValue);
        Assert.Null(Metric(result.Analytics.Offense, "SANCTIONS_DRAWN").CountValue);
        var pivot = Metric(result.Analytics.Offense, "FAILED_PIVOT_PASSES");
        Assert.Null(pivot.CountValue);
        Assert.Equal("DATA_MISSING", pivot.Availability);
        Assert.NotEqual(2, pivot.CountValue);

        var totalShotRate = Metric(result.Analytics.Offense, "TOTAL_SHOT_RATE").Rate!;
        Assert.Equal(60, totalShotRate.Value);
        Assert.Equal(6, totalShotRate.Numerator);
        Assert.Equal(10, totalShotRate.Denominator);
        Assert.Null(totalShotRate.MetricVersion);
        Assert.False(totalShotRate.QualityKnown);
        Assert.Null(totalShotRate.MinimumSample);
    }

    [Fact]
    public void V1Fallback_PreservesTurnoverSanctionAndGoalkeeperTaxonomies()
    {
        var view = LeaguePlayerAnalyticsMapper.FromV1(
            42,
            LeagueAnalyticsTestData.V1Snapshot(),
            LeagueAnalyticsTestData.Scope());

        var turnover = Metric(view.Offense, "TOTAL_TURNOVERS");
        Assert.Equal(10, turnover.CountValue);
        Assert.Equal(
            [2, 3, 3, 2, null],
            turnover.Breakdown.Select(item => item.Value).ToArray());

        var sanctions = Metric(view.Defense, "SANCTIONS_CONCEDED");
        Assert.Equal(7, sanctions.CountValue);
        Assert.Equal(7, sanctions.Breakdown.Sum(item => item.Value));
        AssertCount(view.Defense, "PENALTIES_CONCEDED", 2, AnalyticsSourceStatus.V1Compatible);

        AssertCount(view.Goalkeeper, "TOTAL_SAVES", 12, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "OPEN_PLAY_SAVES", 10, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "PENALTY_SAVES", 2, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "TOTAL_SHOTS_FACED", 24, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "OPEN_PLAY_SHOTS_FACED", 20, AnalyticsSourceStatus.V1Partial);
        AssertCount(view.Goalkeeper, "PENALTY_SHOTS_FACED", 4, AnalyticsSourceStatus.V1Partial);
        AssertCount(view.Goalkeeper, "GOALKEEPER_ASSISTS", 3, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "GOALKEEPER_GOALS", 6, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "GOALKEEPER_TURNOVERS", 10, AnalyticsSourceStatus.V1Compatible);
        AssertCount(view.Goalkeeper, "GOALKEEPER_MISSED_SHOTS", 4, AnalyticsSourceStatus.V1Compatible);

        Assert.Equal(50, Metric(view.Goalkeeper, "TOTAL_SAVE_RATE").Rate!.Value);
        Assert.Equal(50, Metric(view.Goalkeeper, "OPEN_PLAY_SAVE_RATE").Rate!.Value);
        Assert.Equal(50, Metric(view.Goalkeeper, "PENALTY_SAVE_RATE").Rate!.Value);
    }

    [Fact]
    public void V1Fallback_ZeroAttemptsStayNullAndNeverBecomeZeroPercent()
    {
        var snapshot = LeagueAnalyticsTestData.V1Snapshot();
        snapshot.Global.TotalGoals = 0;
        snapshot.Global.GoalCount = 0;
        snapshot.Global.PenaltyGoalCount = 0;
        snapshot.Global.ShotAttempts = 0;
        snapshot.Global.OpenShotAttempts = 0;
        snapshot.Global.PenaltyAttempts = 0;
        snapshot.Global.SaveCount = 0;
        snapshot.Global.ShotsFaced = 0;
        snapshot.Goalkeeper.Arrets = 0;
        snapshot.Goalkeeper.ArretsPenalty = 0;
        snapshot.Goalkeeper.ButsPris = 0;
        snapshot.Goalkeeper.ButsPenalty = 0;

        var view = LeaguePlayerAnalyticsMapper.FromV1(42, snapshot, LeagueAnalyticsTestData.Scope());

        foreach (var rate in view.Offense.Concat(view.Goalkeeper).Where(metric => metric.Kind == LeagueMetricDisplayKind.Rate))
        {
            Assert.Null(rate.Rate!.Value);
            Assert.Equal(0, rate.Rate.Denominator);
        }
    }

    [Theory]
    [InlineData(LeagueGatewayOutcome.ContractError, AnalyticsSourceStatus.ContractError)]
    [InlineData(LeagueGatewayOutcome.NotFound, AnalyticsSourceStatus.Unavailable)]
    [InlineData(LeagueGatewayOutcome.Timeout, AnalyticsSourceStatus.Unavailable)]
    [InlineData(LeagueGatewayOutcome.ServerError, AnalyticsSourceStatus.Unavailable)]
    public void ReceivedV2FailureOtherThanEndpointUnavailable_NeverFallsBack(
        LeagueGatewayOutcome outcome,
        AnalyticsSourceStatus expectedSource)
    {
        var failure = LeagueGatewayResult.Failure(
            outcome,
            new LeagueAnalyticsError("Erreur sûre", "LEAGUE_ERROR", null, false, null));

        var result = Service().Resolve(
            42,
            failure,
            LeagueAnalyticsTestData.V1Snapshot(),
            LeagueAnalyticsTestData.Scope());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Analytics);
        Assert.Equal(expectedSource, result.Source);
        Assert.Equal("LEAGUE_ERROR", result.Error!.TechnicalCode);
    }

    private static LeaguePlayerAnalyticsService Service() => new(new StubGateway());

    private static LeagueMetricDisplayModel Metric(
        IEnumerable<LeagueMetricDisplayModel> metrics,
        string code) =>
        Assert.Single(metrics, metric => metric.MetricCode == code);

    private static void AssertCount(
        IEnumerable<LeagueMetricDisplayModel> metrics,
        string code,
        int expected,
        AnalyticsSourceStatus source)
    {
        var metric = Metric(metrics, code);
        Assert.Equal(expected, metric.CountValue);
        Assert.Equal(source, metric.Source);
    }

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
