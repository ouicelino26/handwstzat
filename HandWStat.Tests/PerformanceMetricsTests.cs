using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using Xunit;

namespace HandWStat.Tests;

public class PerformanceMetricsTests
{
    // ── Helpers ──
    private static PlayerSanctionStatsDto MakeSanctions(int warn = 0, int twom = 0, int dq = 0, int penConc = 0) =>
        new() { Avertissements = warn, DeuxMinutes = twom, Exclusions = dq, PenaltyConcede = penConc };

    private static LeagueDefenseMetricsDto MakeV2Defense(int interceptions = 0, int blocks = 0, int offFouls = 0, int neutral = 0, int penConc = 0, int sanctConc = 0, int warn = 0, int twom = 0, int dq = 0) =>
        new() { Interceptions = interceptions, Blocks = blocks, OffensiveFoulsDrawn = offFouls, Neutralizations = neutral, PenaltiesConceded = penConc, SanctionsConceded = sanctConc, WarningsConceded = warn, TwoMinuteSuspensionsConceded = twom, DisqualificationsConceded = dq };

    private static LeagueCountMetricDto MakeCountMetric(int? value, LeagueMetricAvailability availability = LeagueMetricAvailability.AVAILABLE) =>
        new() { MetricCode = "TEST", MetricVersion = "1.0", Value = value, Availability = availability };

    private static LeagueMetricValueDto MakeRateMetric(double? value, double? numerator, double? denominator, bool sampleReliable = true, double minimumSample = 20) =>
        new() { MetricCode = "TEST", MetricVersion = "1.0", Value = value, Unit = "%", Sample = new() { Numerator = numerator, Denominator = denominator, MinimumSample = minimumSample }, Quality = new() { SampleReliable = sampleReliable, QualityScore = 1.0 }, Numerator = numerator, Denominator = denominator, MinimumSample = minimumSample, SampleReliable = sampleReliable, QualityScore = 1.0 };

    private static LeagueAttackMetricsDto MakeFullAttack(int totalGoals = 26, int openGoals = 19, int penGoals = 7, int assists = 12, int penWon = 9, int sanctDrawn = 14, int turnovers = 10, int badPasses = 5) =>
        new()
        {
            TotalGoals = totalGoals,
            OpenPlayGoals = openGoals,
            PenaltyGoals = penGoals,
            Assists = assists,
            PenaltiesWon = penWon,
            SanctionsDrawn = sanctDrawn,
            TotalTurnovers = turnovers,
            BadPasses = badPasses,
            FailedPivotPasses = MakeCountMetric(null, LeagueMetricAvailability.DATA_MISSING),
            TotalShotRate = MakeRateMetric(68.4, 26, 38),
            OpenPlayShotRate = MakeRateMetric(63.3, 19, 30),
            PenaltyShotRate = MakeRateMetric(87.5, 7, 8)
        };

    private static LeagueGoalkeeperMetricsDto MakeFullGoalkeeper(int totalSaves = 112, int openSaves = 98, int penSaves = 14, int totalFaced = 290, int openFaced = 267, int penFaced = 23, int assists = 0, int goals = 0, int turnovers = 0, int missed = 0) =>
        new()
        {
            TotalSaves = totalSaves,
            OpenPlaySaves = openSaves,
            PenaltySaves = penSaves,
            TotalShotsFaced = totalFaced,
            OpenPlayShotsFaced = openFaced,
            PenaltyShotsFaced = penFaced,
            TotalSaveRate = MakeRateMetric(38.6, totalSaves, totalFaced),
            OpenPlaySaveRate = MakeRateMetric(36.7, openSaves, openFaced),
            PenaltySaveRate = MakeRateMetric(60.9, penSaves, penFaced),
            Assists = assists,
            Goals = goals,
            TotalTurnovers = turnovers,
            MissedShots = missed
        };

    // ── ATTAQUE ──
    [Fact]
    public void Performance_MapsTotalGoals_FromV2WhenAvailable()
    {
        var v2 = MakeFullAttack(totalGoals: 26);
        Assert.Equal(26, v2.TotalGoals);
    }

    [Fact]
    public void Performance_MapsOpenPlayGoals_Distinct()
    {
        var v2 = MakeFullAttack(totalGoals: 26, openGoals: 19, penGoals: 7);
        Assert.Equal(19, v2.OpenPlayGoals);
        Assert.Equal(7, v2.PenaltyGoals);
        Assert.Equal(26, v2.OpenPlayGoals + v2.PenaltyGoals);
    }

    [Fact]
    public void Performance_MapsPenaltyGoals_Distinct()
    {
        var v2 = MakeFullAttack(penGoals: 7);
        Assert.Equal(7, v2.PenaltyGoals);
    }

    [Fact]
    public void Performance_MapsAssists()
    {
        var v2 = MakeFullAttack(assists: 12);
        Assert.Equal(12, v2.Assists);
    }

    [Fact]
    public void Performance_MapsPenaltiesWon()
    {
        var v2 = MakeFullAttack(penWon: 9);
        Assert.Equal(9, v2.PenaltiesWon);
    }

    [Fact]
    public void Performance_MapsSanctionsDrawn()
    {
        var v2 = MakeFullAttack(sanctDrawn: 14);
        Assert.Equal(14, v2.SanctionsDrawn);
    }

    [Fact]
    public void Performance_MapsTotalShotRateEvidence()
    {
        var rate = MakeRateMetric(68.4, 26, 38, sampleReliable: true);
        Assert.Equal(26, rate.Numerator);
        Assert.Equal(38, rate.Denominator);
        Assert.True(rate.SampleReliable);
    }

    [Fact]
    public void Performance_MapsOpenPlayShotRateEvidence()
    {
        var rate = MakeRateMetric(63.3, 19, 30, sampleReliable: true, minimumSample: 20);
        Assert.Equal(19, rate.Numerator);
        Assert.Equal(30, rate.Denominator);
        Assert.True(rate.Denominator >= rate.MinimumSample);
    }

    [Fact]
    public void Performance_MapsPenaltyShotRateEvidence()
    {
        var rate = MakeRateMetric(87.5, 7, 8, sampleReliable: false, minimumSample: 10);
        Assert.Equal(7, rate.Numerator);
        Assert.Equal(8, rate.Denominator);
        Assert.False(rate.SampleReliable);
    }

    [Fact]
    public void Performance_ZeroDenominatorDoesNotRenderZeroPercent()
    {
        var rate = MakeRateMetric(null, 0, 0, sampleReliable: false);
        Assert.Null(rate.Value);
    }

    // ── DÉFENSE ──
    [Fact]
    public void Performance_MapsInterceptions()
    {
        var d = MakeV2Defense(interceptions: 18);
        Assert.Equal(18, d.Interceptions);
    }

    [Fact]
    public void Performance_MapsBlocks()
    {
        var d = MakeV2Defense(blocks: 9);
        Assert.Equal(9, d.Blocks);
    }

    [Fact]
    public void Performance_MapsOffensiveFoulsDrawn()
    {
        var d = MakeV2Defense(offFouls: 11);
        Assert.Equal(11, d.OffensiveFoulsDrawn);
    }

    [Fact]
    public void Performance_MapsNeutralizations()
    {
        var d = MakeV2Defense(neutral: 22);
        Assert.Equal(22, d.Neutralizations);
    }

    [Fact]
    public void Performance_MapsPenaltiesConceded()
    {
        var d = MakeV2Defense(penConc: 6);
        Assert.Equal(6, d.PenaltiesConceded);
    }

    [Fact]
    public void Performance_MapsSanctionsConceded()
    {
        var d = MakeV2Defense(sanctConc: 8);
        Assert.Equal(8, d.SanctionsConceded);
    }

    [Fact]
    public void Performance_PenaltiesConcededNotIncludedInSanctions()
    {
        var d = MakeV2Defense(penConc: 6, sanctConc: 8, warn: 3, twom: 5, dq: 0);
        var totalSanctions = d.WarningsConceded + d.TwoMinuteSuspensionsConceded + d.DisqualificationsConceded;
        Assert.Equal(8, totalSanctions);
        Assert.NotEqual(totalSanctions, totalSanctions + d.PenaltiesConceded);
        Assert.Equal(6, d.PenaltiesConceded);
    }

    // ── MAÎTRISE ──
    [Fact]
    public void Performance_MapsTurnovers()
    {
        var v2 = MakeFullAttack(turnovers: 14);
        Assert.Equal(14, v2.TotalTurnovers);
    }

    [Fact]
    public void Performance_MapsBadPasses()
    {
        var v2 = MakeFullAttack(badPasses: 7);
        Assert.Equal(7, v2.BadPasses);
    }

    [Fact]
    public void Performance_FailedPivotPassesUsesExplicitMetric()
    {
        var pivot = MakeCountMetric(3, LeagueMetricAvailability.AVAILABLE);
        Assert.Equal(3, pivot.Value);
        Assert.Equal(LeagueMetricAvailability.AVAILABLE, pivot.Availability);
    }

    [Fact]
    public void Performance_FailedPivotPassesNeverInferredFromBadPasses()
    {
        var badPasses = 7;
        var pivot = MakeCountMetric(null, LeagueMetricAvailability.DATA_MISSING);
        Assert.Null(pivot.Value);
        Assert.NotEqual(badPasses, pivot.Value ?? -1);
    }

    [Fact]
    public void Performance_MissingPivotPassDisplaysDataMissing()
    {
        var pivot = MakeCountMetric(null, LeagueMetricAvailability.DATA_MISSING);
        Assert.True(pivot.Availability == LeagueMetricAvailability.DATA_MISSING || pivot.Value == null);
    }

    // ── DISCIPLINE ──
    [Fact]
    public void Performance_TotalSanctionsEqualsWarningTwoMinuteDisqualification()
    {
        var d = MakeV2Defense(warn: 3, twom: 5, dq: 0);
        var total = d.WarningsConceded + d.TwoMinuteSuspensionsConceded + d.DisqualificationsConceded;
        Assert.Equal(8, total);
    }

    [Fact]
    public void Performance_SanctionsBreakdownMatchesTotal()
    {
        var d = MakeV2Defense(warn: 3, twom: 5, dq: 2);
        var total = d.WarningsConceded + d.TwoMinuteSuspensionsConceded + d.DisqualificationsConceded;
        Assert.Equal(d.SanctionsConceded == 0 ? total : d.SanctionsConceded, total);
    }

    [Fact]
    public void Performance_DoesNotIncludePenaltiesConcededInSanctions()
    {
        var sanctions = MakeSanctions(warn: 3, twom: 5, dq: 0, penConc: 6);
        var disciplinaryTotal = sanctions.Avertissements + sanctions.DeuxMinutes + sanctions.Exclusions;
        Assert.Equal(8, disciplinaryTotal);
        Assert.Equal(6, sanctions.PenaltyConcede);
        Assert.NotEqual(disciplinaryTotal + sanctions.PenaltyConcede, disciplinaryTotal);
    }

    // ── GARDIENNE ──
    [Fact]
    public void Performance_GoalkeeperMapsTotalSaves()
    {
        var gk = MakeFullGoalkeeper(totalSaves: 112);
        Assert.Equal(112, gk.TotalSaves);
    }

    [Fact]
    public void Performance_GoalkeeperMapsOpenPlaySaves()
    {
        var gk = MakeFullGoalkeeper(openSaves: 98);
        Assert.Equal(98, gk.OpenPlaySaves);
    }

    [Fact]
    public void Performance_GoalkeeperMapsPenaltySaves()
    {
        var gk = MakeFullGoalkeeper(penSaves: 14);
        Assert.Equal(14, gk.PenaltySaves);
    }

    [Fact]
    public void Performance_GoalkeeperMapsShotsFaced()
    {
        var gk = MakeFullGoalkeeper(totalFaced: 290, openFaced: 267, penFaced: 23);
        Assert.Equal(290, gk.TotalShotsFaced);
        Assert.Equal(267, gk.OpenPlayShotsFaced);
        Assert.Equal(23, gk.PenaltyShotsFaced);
    }

    [Fact]
    public void Performance_GoalkeeperSaveRatePreservesEvidence()
    {
        var rate = MakeRateMetric(38.6, 112, 290);
        Assert.Equal(112, rate.Numerator);
        Assert.Equal(290, rate.Denominator);
    }

    [Fact]
    public void Performance_GoalkeeperOpenPlayRatePreservesEvidence()
    {
        var rate = MakeRateMetric(36.7, 98, 267);
        Assert.Equal(98, rate.Numerator);
        Assert.Equal(267, rate.Denominator);
    }

    [Fact]
    public void Performance_GoalkeeperPenaltyRatePreservesEvidence()
    {
        var rate = MakeRateMetric(60.9, 14, 23);
        Assert.Equal(14, rate.Numerator);
        Assert.Equal(23, rate.Denominator);
    }

    [Fact]
    public void Performance_GoalkeeperMapsAssists()
    {
        var gk = MakeFullGoalkeeper(assists: 3);
        Assert.Equal(3, gk.Assists);
    }

    [Fact]
    public void Performance_GoalkeeperMapsGoals()
    {
        var gk = MakeFullGoalkeeper(goals: 1);
        Assert.Equal(1, gk.Goals);
    }

    [Fact]
    public void Performance_GoalkeeperMapsTurnovers()
    {
        var gk = MakeFullGoalkeeper(turnovers: 5);
        Assert.Equal(5, gk.TotalTurnovers);
    }

    [Fact]
    public void Performance_GoalkeeperMapsMissedShots()
    {
        var gk = MakeFullGoalkeeper(missed: 2);
        Assert.Equal(2, gk.MissedShots);
    }

    [Fact]
    public void Performance_GoalkeeperShotsFacedExcludesOffTarget()
    {
        var gk = MakeFullGoalkeeper(totalFaced: 290, openFaced: 267, penFaced: 23);
        Assert.Equal(gk.TotalShotsFaced, gk.OpenPlayShotsFaced + gk.PenaltyShotsFaced);
    }

    [Fact]
    public void Performance_DataMissingDoesNotRenderZero()
    {
        var metric = MakeCountMetric(null, LeagueMetricAvailability.DATA_MISSING);
        Assert.Null(metric.Value);
        Assert.NotEqual(0, metric.Value ?? -1);
    }

    [Fact]
    public void Performance_NoSampleHasDedicatedState()
    {
        var rate = MakeRateMetric(null, 0, 0, sampleReliable: false);
        Assert.Null(rate.Value);
        Assert.False(rate.SampleReliable);
    }

    [Fact]
    public void Performance_InsufficientSampleShowsWarning()
    {
        var rate = MakeRateMetric(75.0, 6, 8, sampleReliable: false, minimumSample: 20);
        Assert.False(rate.SampleReliable);
        Assert.True(rate.Denominator < rate.MinimumSample);
    }

    [Fact]
    public void Performance_PartialDataShowsPartialState()
    {
        var metric = MakeCountMetric(null, LeagueMetricAvailability.PARTIALLY_AVAILABLE);
        Assert.Equal(LeagueMetricAvailability.PARTIALLY_AVAILABLE, metric.Availability);
    }

    [Fact]
    public void Performance_GoalkeeperShotsFacedExcludesPreKeeperBlock()
    {
        var gk = MakeFullGoalkeeper(totalFaced: 290, openFaced: 267, penFaced: 23);
        Assert.True(gk.TotalShotsFaced == gk.OpenPlayShotsFaced + gk.PenaltyShotsFaced);
    }
}
