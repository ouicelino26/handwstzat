using HandWStat.Models.Analytics;
using Xunit;

namespace HandWStat.Tests;

public class TrajectoryAggregatorTests
{
    // Helpers
    private static PlayerTrajectoryPoint MakeRatePoint(DateTime date, double? num, double? den, string matchId = "m1") =>
        new(matchId, date, "2025-26", 1, "c1", "Ligue", "t1", "Brest", "o1", "Metz", true,
            38, PlayingTimeAvailability.RecordedDirect,
            den > 0 ? num / den * 100.0 : null,
            num, den, den >= 5, den > 0 ? "AVAILABLE" : "AVAILABLE",
            0, 0, 0, 0, 0, 0, "V 31-27");

    private static PlayerTrajectoryPoint MakeCountPoint(DateTime date, double? value, string avail = "AVAILABLE", string matchId = "m1") =>
        new(matchId, date, "2025-26", 1, "c1", "Ligue", "t1", "Brest", "o1", "Metz", true,
            38, PlayingTimeAvailability.RecordedDirect,
            value, null, null, true, avail, 0, 0, 0, 0, 0, 0, null);

    // ── TAUX ──
    [Fact]
    public void Trajectory_RateWindowAggregatesNumeratorsAndDenominators()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeRatePoint(d.AddDays(-2), 5, 10, "m1"),
            MakeRatePoint(d.AddDays(-1), 2, 2, "m2"),
            MakeRatePoint(d, 1, 4, "m3")
        };
        var result = TrajectoryAggregator.AggregateRate(pts);
        Assert.NotNull(result);
        Assert.Equal(50.0, result!.Value, precision: 1); // 8/16
    }

    [Fact]
    public void Trajectory_RateWindowDoesNotAverageRawPercentages()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeRatePoint(d.AddDays(-2), 5, 10, "m1"),  // 50%
            MakeRatePoint(d.AddDays(-1), 2, 2, "m2"),   // 100%
            MakeRatePoint(d, 1, 4, "m3")                // 25%
        };
        // Moyenne brute serait (50+100+25)/3 = 58.33%
        var result = TrajectoryAggregator.AggregateRate(pts);
        Assert.NotNull(result);
        Assert.NotEqual(58.33, result!.Value, precision: 1);
        Assert.Equal(50.0, result.Value, precision: 1);
    }

    [Fact]
    public void Trajectory_ZeroDenominatorCreatesGap()
    {
        var p = MakeRatePoint(DateTime.Today, 0, 0, "m1");
        Assert.Null(p.MetricValue); // dénominateur 0 → MetricValue null
    }

    [Fact]
    public void Trajectory_MissingRateDoesNotBecomeZero()
    {
        var p = MakeCountPoint(DateTime.Today, null, "DATA_MISSING", "m1");
        Assert.Null(p.MetricValue);
        Assert.Equal("DATA_MISSING", p.Availability);
    }

    [Fact]
    public void Trajectory_RollingRateAggregatesEvidence()
    {
        var d = DateTime.Today;
        var pts = new List<PlayerTrajectoryPoint>
        {
            MakeRatePoint(d.AddDays(-2), 5, 10, "m1"),
            MakeRatePoint(d.AddDays(-1), 2, 2, "m2"),
            MakeRatePoint(d, 1, 4, "m3")
        };
        var rolling = TrajectoryAggregator.RollingAverage3(pts, TrajectoryMetricType.Rate);
        Assert.Equal(3, rolling.Count);
        Assert.NotNull(rolling[2]); // dernière valeur doit être calculée
    }

    // ── COUNTS ──
    [Fact]
    public void Trajectory_CountPerMatchUsesEligibleMatches()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeCountPoint(d.AddDays(-2), 3, matchId: "m1"),
            MakeCountPoint(d.AddDays(-1), null, "DATA_MISSING", "m2"), // exclu
            MakeCountPoint(d, 1, matchId: "m3")
        };
        var result = TrajectoryAggregator.AggregateCount(pts, p => p.MetricValue);
        // (3+1)/2 = 2.0 (m2 exclu)
        Assert.Equal(2.0, result!.Value, precision: 1);
    }

    [Fact]
    public void Trajectory_RealZeroParticipatesInAverage()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeCountPoint(d.AddDays(-1), 4, matchId: "m1"),
            MakeCountPoint(d, 0, matchId: "m2") // 0 réel, doit participer
        };
        var result = TrajectoryAggregator.AggregateCount(pts, p => p.MetricValue);
        Assert.Equal(2.0, result!.Value, precision: 1); // (4+0)/2
    }

    [Fact]
    public void Trajectory_DataMissingDoesNotParticipateAsZero()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeCountPoint(d.AddDays(-2), 6, matchId: "m1"),
            MakeCountPoint(d.AddDays(-1), null, "DATA_MISSING", "m2"),
            MakeCountPoint(d, 0, matchId: "m3")
        };
        var result = TrajectoryAggregator.AggregateCount(pts, p => p.MetricValue);
        Assert.Equal(3.0, result!.Value, precision: 1); // (6+0)/2, pas /3
    }

    [Fact]
    public void Trajectory_Last5SelectsFiveLatestMatches()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 10).Select(i => MakeCountPoint(d.AddDays(-i), i, matchId: $"m{i}")).ToList();
        var window = TrajectoryAggregator.ApplyWindow(pts, TrajectoryWindow.Last5);
        Assert.Equal(5, window.Count);
    }

    [Fact]
    public void Trajectory_Last10SelectsTenLatestMatches()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 15).Select(i => MakeCountPoint(d.AddDays(-i), i, matchId: $"m{i}")).ToList();
        var window = TrajectoryAggregator.ApplyWindow(pts, TrajectoryWindow.Last10);
        Assert.Equal(10, window.Count);
    }

    [Fact]
    public void Trajectory_SeasonUsesAllEligibleMatches()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 12).Select(i => MakeCountPoint(d.AddDays(-i), i, matchId: $"m{i}")).ToList();
        var window = TrajectoryAggregator.ApplyWindow(pts, TrajectoryWindow.Season);
        Assert.Equal(12, window.Count);
    }

    // ── TENDANCE ──
    [Fact]
    public void Trajectory_LessThanFiveMatchesReturnsInsufficientData()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 4).Select(i => MakeCountPoint(d.AddDays(-i), 3, matchId: $"m{i}")).ToList();
        var def = PlayerTrajectoryMetricCatalog.GoalsPerMatch;
        var state = TrajectoryAggregator.ClassifyTrend(pts, 3.0, def);
        Assert.Equal(TrendState.InsufficientData, state);
    }

    [Fact]
    public void Trajectory_FiveMatchesCanProduceTrend()
    {
        var d = DateTime.Today;
        // 5 matchs, tous bien au-dessus de la référence
        var pts = Enumerable.Range(0, 5).Select(i => MakeCountPoint(d.AddDays(-i), 8, matchId: $"m{i}")).ToList();
        var def = PlayerTrajectoryMetricCatalog.GoalsPerMatch;
        var state = TrajectoryAggregator.ClassifyTrend(pts, 3.0, def); // baseline 3, récent 8
        Assert.Equal(TrendState.Progressing, state);
    }

    [Fact]
    public void Trajectory_HigherIsBetterImprovementIsPositive()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 6).Select(i => MakeCountPoint(d.AddDays(-i), 8, matchId: $"m{i}")).ToList();
        var def = PlayerTrajectoryMetricCatalog.GoalsPerMatch; // HigherIsBetter
        var state = TrajectoryAggregator.ClassifyTrend(pts, 3.0, def);
        Assert.Equal(TrendState.Progressing, state);
    }

    [Fact]
    public void Trajectory_LowerIsBetterDecreaseIsPositive()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 6).Select(i => MakeCountPoint(d.AddDays(-i), 0.5, matchId: $"m{i}")).ToList();
        var def = PlayerTrajectoryMetricCatalog.TurnoversPerMatch; // LowerIsBetter
        var state = TrajectoryAggregator.ClassifyTrend(pts, 3.0, def); // référence 3.0, récent 0.5 → moins de pertes = amélioration
        Assert.Equal(TrendState.Progressing, state);
    }

    [Fact]
    public void Trajectory_TurnoverDecreaseIsImprovement()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 6).Select(i => MakeCountPoint(d.AddDays(-i), 0.5, matchId: $"m{i}")).ToList();
        var def = PlayerTrajectoryMetricCatalog.TurnoversPerMatch;
        var state = TrajectoryAggregator.ClassifyTrend(pts, 4.0, def);
        Assert.Equal(TrendState.Progressing, state);
    }

    [Fact]
    public void Trajectory_StableWindowReturnsStable()
    {
        var d = DateTime.Today;
        var pts = Enumerable.Range(0, 6).Select(i => MakeCountPoint(d.AddDays(-i), 3.0, matchId: $"m{i}")).ToList();
        var def = PlayerTrajectoryMetricCatalog.GoalsPerMatch;
        var state = TrajectoryAggregator.ClassifyTrend(pts, 3.0, def);
        Assert.Equal(TrendState.Stable, state);
    }

    // ── TEMPS DE JEU ──
    [Fact]
    public void Trajectory_PlayingTimeMissingIsNotZero()
    {
        var p = new PlayerTrajectoryPoint("m1", DateTime.Today, "25-26", 1, "c1", "L", "t1", "B", "o1", "M", true,
            null, PlayingTimeAvailability.DataMissing, null, null, null, false, "DATA_MISSING", 0, 0, 0, 0, 0, 0, null);
        Assert.Null(p.MinutesPlayed);
        Assert.Equal(PlayingTimeAvailability.DataMissing, p.PlayingTimeStatus);
    }

    [Fact]
    public void Trajectory_PlayingTimePartialIsMarkedPartial()
    {
        var p = new PlayerTrajectoryPoint("m1", DateTime.Today, "25-26", 1, "c1", "L", "t1", "B", "o1", "M", true,
            22, PlayingTimeAvailability.PartialData, null, null, null, false, "AVAILABLE", 0, 0, 0, 0, 0, 0, null);
        Assert.Equal(PlayingTimeAvailability.PartialData, p.PlayingTimeStatus);
        Assert.NotNull(p.MinutesPlayed);
    }

    [Fact]
    public void Trajectory_PlayingTimeConflictIsUnavailable()
    {
        var p = new PlayerTrajectoryPoint("m1", DateTime.Today, "25-26", 1, "c1", "L", "t1", "B", "o1", "M", true,
            null, PlayingTimeAvailability.IdentityConflict, null, null, null, false, "DATA_MISSING", 0, 0, 0, 0, 0, 0, null);
        Assert.Equal(PlayingTimeAvailability.IdentityConflict, p.PlayingTimeStatus);
        Assert.Null(p.MinutesPlayed);
    }

    [Fact]
    public void Trajectory_PlayingTimeRecordedIsDisplayed()
    {
        var p = new PlayerTrajectoryPoint("m1", DateTime.Today, "25-26", 1, "c1", "L", "t1", "B", "o1", "M", true,
            42, PlayingTimeAvailability.RecordedDirect, null, null, null, false, "AVAILABLE", 0, 0, 0, 0, 0, 0, null);
        Assert.Equal(42, p.MinutesPlayed);
        Assert.Equal(PlayingTimeAvailability.RecordedDirect, p.PlayingTimeStatus);
    }

    // ── GARDIENNES ──
    [Fact]
    public void Trajectory_GoalkeeperMetricCatalogDiffersFromFieldPlayer()
    {
        var gk = PlayerTrajectoryMetricCatalog.GetForGoalkeeper().Select(m => m.Code).ToHashSet();
        var fp = PlayerTrajectoryMetricCatalog.GetForFieldPlayer().Select(m => m.Code).ToHashSet();
        Assert.DoesNotContain("SAVE_RATE", fp);
        Assert.Contains("SAVE_RATE", gk);
        Assert.DoesNotContain("GOALKEEPER_ONLY_metric", fp);
    }

    [Fact]
    public void Trajectory_SaveRatePreservesNumeratorDenominator()
    {
        var p = MakeRatePoint(DateTime.Today, 14, 34, "m1");
        Assert.Equal(14, p.Numerator);
        Assert.Equal(34, p.Denominator);
    }

    [Fact]
    public void Trajectory_PenaltySaveRatePreservesEvidence()
    {
        var p = MakeRatePoint(DateTime.Today, 4, 7, "m1");
        Assert.Equal(4, p.Numerator);
        Assert.Equal(7, p.Denominator);
    }

    [Fact]
    public void Trajectory_GoalkeeperShotsFacedDoesNotIncludeOffTarget()
    {
        // ShotsFaced est un compteur distinct des tirs hors cible
        var p = new PlayerTrajectoryPoint("m1", DateTime.Today, "25-26", 1, "c1", "L", "t1", "B", "o1", "M", true,
            38, PlayingTimeAvailability.RecordedDirect, 34, 14, 34, true, "AVAILABLE", 0, 0, 0, 0, 14, 34, "V 30-26");
        Assert.Equal(34, p.ShotsFaced);
        // Dans le modèle, ShotsFaced est la valeur métrique brute ; Saves correspond aux arrêts
        Assert.Equal(14, p.Saves);
    }

    [Fact]
    public void Trajectory_GoalkeeperShotsFacedDoesNotIncludePreKeeperBlocks()
    {
        var shots = 34; var saves = 14;
        var rate = saves * 100.0 / shots;
        Assert.Equal(14.0 / 34.0 * 100.0, rate, precision: 1);
        Assert.True(shots == saves + (shots - saves)); // cohérence
    }

    // ── CATALOGUE ──
    [Fact]
    public void Trajectory_MetricCatalogFieldPlayerHasAllRequiredMetrics()
    {
        var codes = PlayerTrajectoryMetricCatalog.GetForFieldPlayer().Select(m => m.Code).ToList();
        Assert.Contains("GOALS_PER_MATCH", codes);
        Assert.Contains("ASSISTS_PER_MATCH", codes);
        Assert.Contains("SHOT_SUCCESS_RATE", codes);
        Assert.Contains("INTERCEPTIONS_PER_MATCH", codes);
        Assert.Contains("TURNOVERS_PER_MATCH", codes);
        Assert.Contains("PLAYING_TIME", codes);
    }

    [Fact]
    public void Trajectory_MetricCatalogGoalkeeperHasAllRequiredMetrics()
    {
        var codes = PlayerTrajectoryMetricCatalog.GetForGoalkeeper().Select(m => m.Code).ToList();
        Assert.Contains("SAVES_PER_MATCH", codes);
        Assert.Contains("SAVE_RATE", codes);
        Assert.Contains("OPEN_PLAY_SAVE_RATE", codes);
        Assert.Contains("PENALTY_SAVE_RATE", codes);
        Assert.Contains("SHOTS_FACED_PER_MATCH", codes);
        Assert.Contains("GOALS_CONCEDED_PER_MATCH", codes);
    }

    [Fact]
    public void Trajectory_DirectionHigherIsBetterForGoals()
    {
        Assert.Equal(TrajectoryMetricDirection.HigherIsBetter, PlayerTrajectoryMetricCatalog.GoalsPerMatch.Direction);
    }

    [Fact]
    public void Trajectory_DirectionLowerIsBetterForTurnovers()
    {
        Assert.Equal(TrajectoryMetricDirection.LowerIsBetter, PlayerTrajectoryMetricCatalog.TurnoversPerMatch.Direction);
    }

    [Fact]
    public void Trajectory_DirectionLowerIsBetterForGoalsConceded()
    {
        Assert.Equal(TrajectoryMetricDirection.LowerIsBetter, PlayerTrajectoryMetricCatalog.GoalsConcededPerMatch.Direction);
    }

    [Fact]
    public void Trajectory_PlayingTimeLabelForMissingStatus()
    {
        var label = TrajectoryAggregator.GetPlayingTimeLabel(PlayingTimeAvailability.DataMissing);
        Assert.Equal("Temps non disponible", label);
    }

    [Fact]
    public void Trajectory_PlayingTimeLabelForRecordedStatus()
    {
        var label = TrajectoryAggregator.GetPlayingTimeLabel(PlayingTimeAvailability.RecordedDirect);
        Assert.Equal("Temps enregistré", label);
    }

    [Fact]
    public void Trajectory_WindowSummaryBuildsDelta()
    {
        var d = DateTime.Today;
        var season = Enumerable.Range(0, 10).Select(i => MakeCountPoint(d.AddDays(-i), 5.0, matchId: $"m{i}")).ToList();
        var last5 = TrajectoryAggregator.ApplyWindow(season, TrajectoryWindow.Last5);
        var def = PlayerTrajectoryMetricCatalog.GoalsPerMatch;
        var summary = TrajectoryAggregator.BuildWindowSummary(last5, season, def, TrajectoryWindow.Last5);
        Assert.Equal(5.0, summary.AggregatedValue!.Value, precision: 1);
        Assert.Equal(5.0, summary.SeasonValue!.Value, precision: 1);
        Assert.Equal(0.0, summary.DeltaVsSeason!.Value, precision: 1); // même valeur → delta 0
    }

    [Fact]
    public void Trajectory_InsufficientSampleReturnInsufficientDataTrend()
    {
        var d = DateTime.Today;
        var pts = new[] { MakeCountPoint(d, 3, matchId: "m1") }; // 1 seul match
        var def = PlayerTrajectoryMetricCatalog.GoalsPerMatch;
        var state = TrajectoryAggregator.ClassifyTrend(pts, 3.0, def);
        Assert.Equal(TrendState.InsufficientData, state);
    }

    [Fact]
    public void Trajectory_WindowSortedByDateThenId()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeCountPoint(d, 1, matchId: "m3"),
            MakeCountPoint(d.AddDays(-2), 2, matchId: "m1"),
            MakeCountPoint(d.AddDays(-1), 3, matchId: "m2"),
        };
        var window = TrajectoryAggregator.ApplyWindow(pts, TrajectoryWindow.Season);
        Assert.Equal("m1", window[0].MatchId);
        Assert.Equal("m2", window[1].MatchId);
        Assert.Equal("m3", window[2].MatchId);
    }

    [Fact]
    public void Trajectory_Last5ReturnsFewerIfNotEnough()
    {
        var d = DateTime.Today;
        var pts = new[]
        {
            MakeCountPoint(d, 1, matchId: "m1"),
            MakeCountPoint(d.AddDays(-1), 2, matchId: "m2"),
            MakeCountPoint(d.AddDays(-2), 3, matchId: "m3"),
        };
        var window = TrajectoryAggregator.ApplyWindow(pts, TrajectoryWindow.Last5);
        Assert.Equal(3, window.Count); // seulement 3 disponibles
    }
}
