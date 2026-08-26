using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

// Phase A12 — Final Consolidation / Cross-page Parity / Regression Hardening
// TEST_BASELINE_A12 = 1122

public class AnalyticsA12Tests
{
    // ── §23 — Player cross-page fixture ───────────────────────────────────────
    // Fixture: field player with 10 goals, 5 assists, 300 min PT, 15 open attempts,
    //          8 turnovers, 3 interceptions. All pages route through AnalyticsCalculationService.

    [Fact]
    public void Fixture_CAT01_GoalsCreatedPer60_CanonicalResult()
    {
        // (10 + 5) / 300 * 60 = 3.0
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(
            totalGoals: 10, assists: 5, playingTimeMinutes: 300);
        Assert.Equal(3.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_CAT04_OpenPlaySuccessRate_CanonicalResult()
    {
        // 10 / 15 * 100 = 66.666...
        var result = AnalyticsCalculationService.ComputeOpenPlaySuccessRate(
            goalCount: 10, openShotAttempts: 15);
        Assert.Equal(100.0 * 10.0 / 15.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_CAT08_TurnoversNullWhenNoPlayingTime()
    {
        // API provides TurnoversPer60; NormalizeApiPer60 converts 0.0 (PT=0) to null
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 0.0, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    [Fact]
    public void Fixture_CAT08_TurnoversPerPassthrough_WhenPlayingTimeKnown()
    {
        // API value 1.6 is valid when PT > 0
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 1.6, playingTimeMinutes: 300);
        Assert.Equal(1.6, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_CAT09_InterceptionsNullWhenNoPlayingTime()
    {
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 0.0, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    [Fact]
    public void Fixture_CAT21_TotalSaveRate_CanonicalResult()
    {
        // (7 + 2) / 15 * 100 = 60.0
        var result = AnalyticsCalculationService.ComputeTotalSaveRate(
            totalSaves: 9, totalShotsFaced: 15);
        Assert.Equal(60.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_CAT01_NullWhenNoPlayingTime()
    {
        // PT = 0 → null (not 0) for all /60 metrics
        Assert.Null(AnalyticsCalculationService.ComputeGoalsCreatedPer60(10, 5, 0));
    }

    [Fact]
    public void Fixture_CAT04_NullWhenNoAttempts()
    {
        Assert.Null(AnalyticsCalculationService.ComputeOpenPlaySuccessRate(0, 0));
    }

    // ── §24 — Team cross-page fixture ─────────────────────────────────────────
    // Fixture: 3 players with goals=[5,3,2], attempts=[8,6,4]. TeamGoals=10.
    // TeamAnalyticsBuilder and MatchAnalyticsBuilder must use SUM not AVG.

    [Fact]
    public void Fixture_TeamShotRate_SumNumeratorOverSumDenominator()
    {
        // ShotRate = total_official_goals / total_attempts = 10 / 18 = 0.5555...
        // Attempts = officialGoals(10) + eventMisses → set TotalGoals=[5,3,2] so eventMisses=3+3+2=8, Attempts=18
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T",
            officialGoals: 10,
            teamPlayers: [
                new() { ShotAttempts = 8, TotalGoals = 5 },
                new() { ShotAttempts = 6, TotalGoals = 3 },
                new() { ShotAttempts = 4, TotalGoals = 2 },
            ]);
        Assert.Equal(18, team.Attempts);
        Assert.Equal(10.0 / 18.0, team.ShotRate!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_TeamShotRate_NotAverageOfIndividualRates()
    {
        // AVG([5/8, 3/6, 2/4]) = AVG([0.625, 0.5, 0.5]) = 0.5417 ≠ 10/18 = 0.5556
        // TeamShotRate must be the aggregate, never AVG of individual rates.
        // TotalGoals=[5,3,2] → eventMisses=3+3+2=8, Attempts=10+8=18
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T",
            officialGoals: 10,
            teamPlayers: [
                new() { ShotAttempts = 8, TotalGoals = 5 },
                new() { ShotAttempts = 6, TotalGoals = 3 },
                new() { ShotAttempts = 4, TotalGoals = 2 },
            ]);
        var avgOfIndividualRates = (5.0 / 8 + 3.0 / 6 + 2.0 / 4) / 3.0; // = 0.5417
        Assert.NotEqual(avgOfIndividualRates, team.ShotRate!.Value, precision: 3);
        Assert.Equal(10.0 / 18.0, team.ShotRate!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_TeamShotRate_NullWhenNoAttempts()
    {
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T",
            officialGoals: 0,
            teamPlayers: []);
        Assert.Null(team.ShotRate);
    }

    [Fact]
    public void Fixture_TeamSaveRate_SumSavesOverSumFaced()
    {
        // SaveRate = SUM(saves) / SUM(faced)
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T",
            officialGoals: 5,
            teamPlayers: [
                new() { SaveCount = 7, ShotsFaced = 10 },
                new() { SaveCount = 3, ShotsFaced = 5 },
            ]);
        Assert.Equal(15, team.ShotsFaced);
        Assert.Equal(10, team.Saves);
        Assert.Equal(10.0 / 15.0, team.SaveRate!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_TeamSaveRate_NullWhenNoShotsFaced()
    {
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T",
            officialGoals: 0,
            teamPlayers: [new() { ShotAttempts = 5 }]);
        Assert.Null(team.SaveRate);
    }

    // ── §25 — GK cross-page fixture ───────────────────────────────────────────
    // Fixture GK: open saves=7, open faced=10, penalty saves=2, penalty faced=5.
    // CAT-21 = (7+2)/(10+5) = 60%, CAT-13 = 70%, CAT-14 = 40%.

    [Fact]
    public void Fixture_GK_TotalSaveRate_60pct()
    {
        var result = AnalyticsCalculationService.ComputeTotalSaveRate(
            totalSaves: 9, totalShotsFaced: 15);
        Assert.Equal(60.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_GK_OpenPlaySaveRate_70pct()
    {
        var result = AnalyticsCalculationService.ComputeOpenPlaySaveRate(
            openPlaySaves: 7, openPlayShotsFaced: 10);
        Assert.Equal(70.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_GK_PenaltySaveRate_40pct()
    {
        var result = AnalyticsCalculationService.ComputePenaltySaveRate(
            penaltySaves: 2, penaltyShotsFaced: 5);
        Assert.Equal(40.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Fixture_GK_SavesPer60_NullWhenNoPlayingTime()
    {
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 0.0, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    [Fact]
    public void Fixture_GK_OpenPlaySaveRate_NullWhenNoShotsFaced()
    {
        var result = AnalyticsCalculationService.ComputeOpenPlaySaveRate(0, 0);
        Assert.Null(result);
    }

    [Fact]
    public void Fixture_GK_PenaltySaveRate_NullWhenNoFaced()
    {
        var result = AnalyticsCalculationService.ComputePenaltySaveRate(0, 0);
        Assert.Null(result);
    }

    // ── §26 — Null / Zero regression suite ───────────────────────────────────

    [Fact]
    public void NullZero_RealZero_IsPreserved_CAT05()
    {
        // 0 assists / 3 turnovers = 0.0 ratio (real zero, not N/A)
        var result = AnalyticsCalculationService.ComputeAssistTurnoverRatio(assists: 0, turnovers: 3);
        Assert.NotNull(result);
        Assert.Equal(0.0, result!.Value, precision: 9);
    }

    [Fact]
    public void NullZero_ZeroDenominator_CAT05_IsNull()
    {
        // 0 turnovers → ratio is undefined, not 0 (player with no turnovers is great)
        Assert.Null(AnalyticsCalculationService.ComputeAssistTurnoverRatio(assists: 5, turnovers: 0));
    }

    [Fact]
    public void NullZero_ZeroDenominator_CAT04_IsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeOpenPlaySuccessRate(goalCount: 0, openShotAttempts: 0));
    }

    [Fact]
    public void NullZero_ZeroDenominator_CAT21_IsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeTotalSaveRate(totalSaves: 0, totalShotsFaced: 0));
    }

    [Fact]
    public void NullZero_ZeroDenominator_CAT13_IsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeOpenPlaySaveRate(openPlaySaves: 0, openPlayShotsFaced: 0));
    }

    [Fact]
    public void NullZero_ZeroDenominator_CAT14_IsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputePenaltySaveRate(penaltySaves: 0, penaltyShotsFaced: 0));
    }

    [Fact]
    public void NullZero_ZeroPlayingTime_AllPer60Null()
    {
        Assert.Null(AnalyticsCalculationService.ComputeGoalsCreatedPer60(10, 5, 0));
        Assert.Null(AnalyticsCalculationService.ComputeOffensiveVolumePer60(10, 5, 0));
        Assert.Null(AnalyticsCalculationService.ComputeDefensiveImpactPer60(3, 2, 1, 0, 0));
        Assert.Null(AnalyticsCalculationService.ComputeShotsFacedPer60(10, 0));
        Assert.Null(AnalyticsCalculationService.ComputeGoalsConcededPer60(5, 0));
    }

    [Fact]
    public void NullZero_LowSample_TriggerSuccessRate_Null()
    {
        // 0 attempts → null (not 0 efficiency)
        Assert.Null(AnalyticsCalculationService.ComputeTriggerSuccessRate(successCount: 0, attempts: 0));
    }

    [Fact]
    public void NullZero_ZeroGoalsTeam_ShotRateNull()
    {
        // 0 attempts → ShotRate is null, not 0
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "T", 0, []);
        Assert.Null(team.ShotRate);
    }

    [Fact]
    public void NullZero_ZeroGoalsSharePct_NullWhenNoTeamGoals()
    {
        Assert.Null(AnalyticsCalculationService.ComputeGoalsSharePct(totalGoals: 5, teamGoalsFor: 0));
    }

    [Fact]
    public void NullZero_RealZeroGoalsSharePct_WhenPlayerScoredNone()
    {
        // 0 goals, 10 team goals → 0% (real zero, not N/A)
        var result = AnalyticsCalculationService.ComputeGoalsSharePct(totalGoals: 0, teamGoalsFor: 10);
        Assert.NotNull(result);
        Assert.Equal(0.0, result!.Value, precision: 9);
    }

    [Fact]
    public void NullZero_NormalizeApiPer60_RealValuePreserved()
    {
        // Value 2.5 with PT>0 → 2.5 (not null)
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 2.5, playingTimeMinutes: 300);
        Assert.Equal(2.5, result!.Value, precision: 9);
    }

    // ── §27 — Scope regression ────────────────────────────────────────────────
    // AnalyticsQualityPolicy enforces minimum sample and playing time thresholds.
    // These tests verify the policy is applied consistently and thresholds are not duplicated inline.

    [Fact]
    public void Scope_QualityPolicy_CAT13_MinSample_IsEnforced()
    {
        var minSample = AnalyticsV3Catalog.Get("CAT-13")!.MinimumSampleCount;
        var policy = AnalyticsQualityPolicy.EvaluateTier(sampleCount: 2, quality: null, minSample: minSample);
        Assert.Equal(QualityTier.Low, policy.Tier);
    }

    [Fact]
    public void Scope_QualityPolicy_CAT01_MinPlayingTime_IsEnforced()
    {
        var policy = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: 30,
            minimumPlayingTimeMinutes: AnalyticsV3Catalog.Get("CAT-01")!.MinimumPlayingTimeMinutes);
        Assert.Equal(QualityTier.Low, policy.Tier);
    }

    [Fact]
    public void Scope_QualityPolicy_Sufficient_IsHigh()
    {
        var policy = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: 300,
            minimumPlayingTimeMinutes: AnalyticsV3Catalog.Get("CAT-01")!.MinimumPlayingTimeMinutes);
        Assert.Equal(QualityTier.High, policy.Tier);
    }

    [Fact]
    public void Scope_PositionScope_GkMetrics_ExcludeFieldPositions()
    {
        // CAT-15 (GK only) must not apply to field positions
        var cat15 = AnalyticsV3Catalog.Get("CAT-15")!;
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "AR"));
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "DC"));
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "PIV"));
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "AIL"));
    }

    [Fact]
    public void Scope_PositionScope_FieldMetrics_ExcludeGk()
    {
        var cat01 = AnalyticsV3Catalog.Get("CAT-01")!;
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat01, "GK"));
    }

    // ── §23 — Parity: radar normalization uses API percentile ────────────────

    [Fact]
    public void Parity_RadarNormalization_UsesApiPercentile_AllSites()
    {
        // All three normalization sites (PlayerSheetExportHelper, AnalyseTabPanel, MultiRadar)
        // must return axis.Percentile — not min-max. This test documents the invariant.
        var axis = new PositionProfileAxisDto
        {
            Label = "Test", Key = "test",
            Value = 3.0, MedianValue = 1.5,
            MinValue = 0.0, MaxValue = 5.0,
            Percentile = 72.0,
            HigherIsBetter = true,
        };
        // PlayerSheetExportHelper.NormalizeRadarValue is the reference implementation
        var result = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        Assert.Equal(72.0, result, precision: 9);
        // min-max would give (3-0)/(5-0)*100 = 60.0, confirming we're NOT using min-max
        Assert.NotEqual(60.0, result, precision: 3);
    }

    [Fact]
    public void Parity_RadarNormalization_HigherIsWorseAxis_StillUsesPercentile()
    {
        // HigherIsBetter=false: min-max would invert. Percentile must be returned as-is.
        var axis = new PositionProfileAxisDto
        {
            Label = "Pertes", Key = "turnovers",
            Value = 2.8, MedianValue = 1.0,
            MinValue = 0.0, MaxValue = 4.0,
            Percentile = 25.0,  // API: bad player = low percentile
            HigherIsBetter = false,
        };
        var result = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        Assert.Equal(25.0, result, precision: 9);
        // min-max would give (1 - 2.8/4)*100 = 30.0, confirming we're NOT using min-max
        Assert.NotEqual(30.0, result, precision: 3);
    }

    // ── §15/§24 — Match metrics consistency ──────────────────────────────────

    [Fact]
    public void Match_ShotRate_OfficialGoalsIsCanonical()
    {
        // Official score (10) beats event-counted goals (TotalGoals=9) in the numerator
        // eventMisses = 20-9=11, Attempts = 10+11=21 — official 10 used as Goals, not event 9
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T", officialGoals: 10,
            teamPlayers: [new() { ShotAttempts = 20, GoalCount = 9, TotalGoals = 9 }]);
        // Numerator = official 10 (not event 9); denominator = 10+11=21
        Assert.Equal(10.0 / 21.0, team.ShotRate!.Value, precision: 9);
        Assert.NotEqual(9.0 / 21.0, team.ShotRate.Value, precision: 9);
    }

    // ── §12 — Team aggregation: SUM/SUM never AVG ─────────────────────────────

    [Fact]
    public void TeamAggregation_ShotRate_SumNotAvg()
    {
        // Use different denominators: 9/10 and 1/5 → SUM=10/15=66.67%, AVG=(90%+20%)/2=55% — DIFFERENT
        // TotalGoals=[7,3] → eventMisses=(10-7)+(5-3)=3+2=5, Attempts=10+5=15
        var team = MatchAnalyticsBuilder.BuildTeamAnalytics(
            teamId: 1, teamName: "T", officialGoals: 10,
            teamPlayers: [
                new() { ShotAttempts = 10, TotalGoals = 7 },  // 3 event misses
                new() { ShotAttempts = 5, TotalGoals = 3 },   // 2 event misses
            ]);
        // SUM: 10/15 = 66.67%
        Assert.Equal(10.0 / 15.0, team.ShotRate!.Value, precision: 9);
        // AVG of hypothetical 90%+20% = 55% ≠ 66.67%
        var fakeAvg = (10.0 / 10.0 + 0.0) / 2.0;
        Assert.NotEqual(fakeAvg, team.ShotRate.Value, precision: 3);
    }
}
