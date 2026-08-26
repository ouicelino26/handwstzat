using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA7Tests
{
    // ── ComputeTeamShotRate ───────────────────────────────────────────────────

    [Fact]
    public void ComputeTeamShotRate_BasicFormula_Spec8of12()
    {
        // Spec: 8 goals / 12 attempts → 66.666...%
        var result = TeamAnalyticsBuilder.ComputeTeamShotRate(8, 12);
        Assert.NotNull(result);
        Assert.Equal(66.667, result!.Value, precision: 3);
    }

    [Fact]
    public void ComputeTeamShotRate_ZeroAttempts_ReturnsNull()
    {
        Assert.Null(TeamAnalyticsBuilder.ComputeTeamShotRate(0, 0));
    }

    [Fact]
    public void ComputeTeamShotRate_NegativeAttempts_ReturnsNull()
    {
        Assert.Null(TeamAnalyticsBuilder.ComputeTeamShotRate(5, -1));
    }

    [Fact]
    public void ComputeTeamShotRate_ZeroGoals_ReturnsZero()
    {
        var result = TeamAnalyticsBuilder.ComputeTeamShotRate(0, 10);
        Assert.Equal(0.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeTeamShotRate_AllGoals_Returns100()
    {
        var result = TeamAnalyticsBuilder.ComputeTeamShotRate(10, 10);
        Assert.Equal(100.0, result!.Value, precision: 5);
    }

    // ── ComputeAggregatedTeamGkSaveRate ───────────────────────────────────────

    [Fact]
    public void ComputeAggregatedTeamGkSaveRate_Spec34of110_NotAverage()
    {
        // Spec: GK1 4/10 + GK2 30/100 → SUM = 34/110 → 30.909%
        // NOT avg(40%, 30%) = 35%
        var gks = new[] { (Saves: 4, ShotsFaced: 10), (Saves: 30, ShotsFaced: 100) };
        var result = TeamAnalyticsBuilder.ComputeAggregatedTeamGkSaveRate(gks);
        Assert.NotNull(result);
        Assert.Equal(30.909, result!.Value, precision: 3);
        // Verify it is not the naive average (35%)
        Assert.True(result.Value < 32.0, $"Expected ≈30.9 (SUM/SUM), got {result.Value} — AVG would be 35.0");
    }

    [Fact]
    public void ComputeAggregatedTeamGkSaveRate_Empty_ReturnsNull()
    {
        Assert.Null(TeamAnalyticsBuilder.ComputeAggregatedTeamGkSaveRate([]));
    }

    [Fact]
    public void ComputeAggregatedTeamGkSaveRate_SingleGk_CorrectRate()
    {
        var gks = new[] { (Saves: 7, ShotsFaced: 10) };
        var result = TeamAnalyticsBuilder.ComputeAggregatedTeamGkSaveRate(gks);
        Assert.Equal(70.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeAggregatedTeamGkSaveRate_ZeroFaced_ReturnsNull()
    {
        var gks = new[] { (Saves: 0, ShotsFaced: 0) };
        Assert.Null(TeamAnalyticsBuilder.ComputeAggregatedTeamGkSaveRate(gks));
    }

    // ── ComputeGoalsSharePct (CAT-17a) ────────────────────────────────────────

    [Fact]
    public void ComputeGoalsSharePct_BasicFormula_Spec20of100()
    {
        // Spec: 20 player goals / 100 team goals → 20%
        var result = TeamAnalyticsBuilder.ComputeGoalsSharePct(20, 100);
        Assert.Equal(20.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeGoalsSharePct_ZeroTeamGoals_ReturnsNull()
    {
        Assert.Null(TeamAnalyticsBuilder.ComputeGoalsSharePct(5, 0));
    }

    [Fact]
    public void ComputeGoalsSharePct_ZeroPlayerGoals_ReturnsZero()
    {
        var result = TeamAnalyticsBuilder.ComputeGoalsSharePct(0, 50);
        Assert.Equal(0.0, result!.Value, precision: 5);
    }

    // ── ComputeTeamAssistTurnoverRatio ────────────────────────────────────────

    [Fact]
    public void ComputeTeamAssistTurnoverRatio_BasicFormula()
    {
        // 30 assists / 10 turnovers → 3.0
        var result = TeamAnalyticsBuilder.ComputeTeamAssistTurnoverRatio(30, 10);
        Assert.Equal(3.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeTeamAssistTurnoverRatio_ZeroTurnovers_ReturnsNull()
    {
        // Undefined ratio when turnovers = 0
        Assert.Null(TeamAnalyticsBuilder.ComputeTeamAssistTurnoverRatio(10, 0));
    }

    [Fact]
    public void ComputeTeamAssistTurnoverRatio_ZeroAssists_ReturnsZero()
    {
        var result = TeamAnalyticsBuilder.ComputeTeamAssistTurnoverRatio(0, 10);
        Assert.Equal(0.0, result!.Value, precision: 5);
    }

    // ── ComputeGoalsPerMatch ──────────────────────────────────────────────────

    [Fact]
    public void ComputeGoalsPerMatch_BasicFormula()
    {
        // 24 goals / 8 matches → 3.0
        var result = TeamAnalyticsBuilder.ComputeGoalsPerMatch(24, 8);
        Assert.Equal(3.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeGoalsPerMatch_ZeroMatches_ReturnsNull()
    {
        Assert.Null(TeamAnalyticsBuilder.ComputeGoalsPerMatch(10, 0));
    }

    // ── Quality tiers (MinTeamShotSample = 30) ────────────────────────────────

    [Fact]
    public void TeamShotQuality_ZeroAttempts_IsNotApplicable()
    {
        var q = AnalyticsQualityPolicy.EvaluateTier(null, 0, TeamAnalyticsBuilder.MinTeamShotSample);
        Assert.Equal(QualityTier.NotApplicable, q.Tier);
    }

    [Fact]
    public void TeamShotQuality_BelowMin_IsLow()
    {
        var q = AnalyticsQualityPolicy.EvaluateTier(null, 15, TeamAnalyticsBuilder.MinTeamShotSample);
        Assert.Equal(QualityTier.Low, q.Tier);
    }

    [Fact]
    public void TeamShotQuality_AtMin_IsHigh()
    {
        var q = AnalyticsQualityPolicy.EvaluateTier(null, 30, TeamAnalyticsBuilder.MinTeamShotSample);
        Assert.Equal(QualityTier.High, q.Tier);
    }

    [Fact]
    public void TeamShotQuality_AboveMin_IsHigh()
    {
        var q = AnalyticsQualityPolicy.EvaluateTier(null, 60, TeamAnalyticsBuilder.MinTeamShotSample);
        Assert.Equal(QualityTier.High, q.Tier);
    }

    // ── Build — TeamAnalyticsModel ────────────────────────────────────────────

    [Fact]
    public void Build_NoPlayers_ReturnsValidModel()
    {
        var stats = MakeTeamStats(matchesPlayed: 5, goalsFor: 20, shotAttempts: 40);
        var model = TeamAnalyticsBuilder.Build(stats, null);
        Assert.NotNull(model);
        Assert.Equal(5, model.MatchesPlayed);
        Assert.Equal(20, model.GoalsFor);
    }

    [Fact]
    public void Build_ApiGkRateUsed_WhenTirsSubisGt0()
    {
        // Priority 1: use API GoalkeeperSaveRate when TirsSubis > 0
        var stats = MakeTeamStats(matchesPlayed: 10, goalsFor: 30, shotAttempts: 60,
                                   tirsSubis: 50, gkSaveRate: 75.0);
        var model = TeamAnalyticsBuilder.Build(stats, []);
        Assert.Equal(75.0, model.TeamGkSaveRate);
    }

    [Fact]
    public void Build_AggregatedGkRate_UsedWhenNoApiData()
    {
        // TirsSubis = 0 → fall back to player-list aggregation
        var stats = MakeTeamStats(matchesPlayed: 10, goalsFor: 30, shotAttempts: 60, tirsSubis: 0);
        var players = new List<PlayerGlobalStatsDto> { MakeGk(saves: 7, shotsFaced: 10) };
        var model = TeamAnalyticsBuilder.Build(stats, players);
        // 7/10 × 100 = 70.0
        Assert.Equal(70.0, model.TeamGkSaveRate!.Value, precision: 5);
    }

    [Fact]
    public void Build_GoalsPerMatch_ComputedCorrectly()
    {
        var stats = MakeTeamStats(matchesPlayed: 8, goalsFor: 24, shotAttempts: 0);
        var model = TeamAnalyticsBuilder.Build(stats, []);
        Assert.Equal(3.0, model.GoalsPerMatch!.Value, precision: 5);
    }

    [Fact]
    public void Build_ShotRateQuality_Low_WhenBelowMinSample()
    {
        var stats = MakeTeamStats(matchesPlayed: 5, goalsFor: 5, shotAttempts: 10);
        var model = TeamAnalyticsBuilder.Build(stats, []);
        Assert.Equal(QualityTier.Low, model.ShotRateQuality.Tier);
    }

    [Fact]
    public void Build_ShotRateQuality_High_WhenAboveMinSample()
    {
        var stats = MakeTeamStats(matchesPlayed: 5, goalsFor: 20, shotAttempts: 40);
        var model = TeamAnalyticsBuilder.Build(stats, []);
        Assert.Equal(QualityTier.High, model.ShotRateQuality.Tier);
    }

    // ── Constants ────────────────────────────────────────────────────────────

    [Fact]
    public void MinTeamShotSample_Is30()
    {
        Assert.Equal(30, TeamAnalyticsBuilder.MinTeamShotSample);
    }

    [Fact]
    public void MinTeamPenaltySample_Is10()
    {
        Assert.Equal(10, TeamAnalyticsBuilder.MinTeamPenaltySample);
    }

    [Fact]
    public void MinTeamGkShotSample_Is30()
    {
        Assert.Equal(30, TeamAnalyticsBuilder.MinTeamGkShotSample);
    }

    // ── Cross-page: CAT-17a consistency ──────────────────────────────────────

    [Fact]
    public void GoalsSharePct_MatchesAnalyticsCalculationService()
    {
        var viaBuilder = TeamAnalyticsBuilder.ComputeGoalsSharePct(15, 75);
        var viaService = AnalyticsCalculationService.ComputeGoalsSharePct(15, 75);
        Assert.Equal(viaService, viaBuilder);
    }

    [Fact]
    public void GoalsSharePct_CrossPage_SameFormulaAcrossValues()
    {
        // Formula must be identical regardless of call path
        foreach (var goals in new[] { 0, 5, 10, 20, 100 })
        {
            var fromBuilder = TeamAnalyticsBuilder.ComputeGoalsSharePct(goals, 100);
            var fromService = AnalyticsCalculationService.ComputeGoalsSharePct(goals, 100);
            Assert.Equal(fromService, fromBuilder);
        }
    }

    // ── Cross-page: GK aggregation delegates to GoalkeeperAnalyticsBuilder ────

    [Fact]
    public void ComputeAggregatedTeamGkSaveRate_DelegatesToGoalkeeperAnalyticsBuilder()
    {
        var gks = new[] { (Saves: 25, ShotsFaced: 80), (Saves: 10, ShotsFaced: 30) };
        var viaTeam = TeamAnalyticsBuilder.ComputeAggregatedTeamGkSaveRate(gks);
        var viaGkBuilder = GoalkeeperAnalyticsBuilder.AggregateTeamSaveRate(gks);
        Assert.Equal(viaGkBuilder, viaTeam);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TeamStatsDto MakeTeamStats(
        int matchesPlayed = 0, int goalsFor = 0, int shotAttempts = 0,
        int tirsSubis = 0, double gkSaveRate = 0.0,
        int assists = 0, int turnovers = 0)
    {
        return new TeamStatsDto
        {
            MatchesPlayed = matchesPlayed,
            GoalsFor = goalsFor,
            Technical = new TechnicalStatsDto
            {
                ShotAttempts = shotAttempts,
                TirsSubis = tirsSubis,
                GoalkeeperSaveRate = gkSaveRate
            },
            Overview = new StatsOverviewDto
            {
                AssistCount = assists,
                TurnoverCount = turnovers
            }
        };
    }

    private static PlayerGlobalStatsDto MakeGk(int saves, int shotsFaced) =>
        new PlayerGlobalStatsDto
        {
            IsGoalkeeper = true,
            SaveCount = saves,
            ShotsFaced = shotsFaced
        };
}
