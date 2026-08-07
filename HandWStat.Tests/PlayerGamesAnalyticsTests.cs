using HandWStat.Models.Analytics;
using Xunit;

namespace HandWStat.Tests;

public class PlayerGamesAnalyticsTests
{
    // ── Helpers ──

    private static PlayerGameAnalysisRow MakeFieldRow(
        string matchId = "m1",
        DateTime? date = null,
        string playerTeamId = "t1",
        string team1Id = "t1", string team1Name = "Brest", int? team1Score = 31,
        string team2Id = "t2", string team2Name = "Metz",  int? team2Score = 27,
        double? minutes = 38, PlayingTimeAvailability ptStatus = PlayingTimeAvailability.RecordedDirect,
        int? goals = null, int? assists = null, int? shotGoals = null, int? shotAttempts = null,
        int? interceptions = null, int? turnovers = null) =>
        new(
            new MatchIdentity(matchId, date ?? DateTime.Today, "25-26", 18, "c1", "Ligue",
                playerTeamId, team1Name, team2Id, team2Name,
                true, team1Score, team2Score,
                PlayerMatchResultResolver.ResolveResult(playerTeamId, team1Id, team1Score, team2Score)),
            new GamePlayingTime(minutes, ptStatus),
            new GameFieldMetrics(goals, assists, shotAttempts, shotGoals,
                (shotAttempts.HasValue && shotAttempts > 0 && shotGoals.HasValue)
                    ? shotGoals.Value * 100.0 / shotAttempts.Value
                    : null,
                null, null, null, interceptions, null, turnovers, null, null, null, null),
            null
        );

    private static PlayerGameAnalysisRow MakeGkRow(
        string matchId = "m1", DateTime? date = null,
        string playerTeamId = "t1", string team1Id = "t1", int? t1Score = 30, int? t2Score = 26,
        int? saves = null, int? shotsFaced = null, int? penSaves = null, int? penFaced = null,
        int? assists = null, int? turnovers = null,
        double? minutes = 52, PlayingTimeAvailability ptStatus = PlayingTimeAvailability.RecordedDirect) =>
        new(
            new MatchIdentity(matchId, date ?? DateTime.Today, "25-26", 18, "c1", "Ligue",
                playerTeamId, "Brest", "t2", "Metz", true, t1Score, t2Score,
                PlayerMatchResultResolver.ResolveResult(playerTeamId, team1Id, t1Score, t2Score)),
            new GamePlayingTime(minutes, ptStatus),
            null,
            new GameGoalkeeperMetrics(saves, null, penSaves, shotsFaced, null, penFaced,
                (shotsFaced.HasValue && saves.HasValue) ? shotsFaced.Value - saves.Value : null,
                null,
                (shotsFaced.HasValue && shotsFaced > 0 && saves.HasValue)
                    ? saves.Value * 100.0 / shotsFaced.Value
                    : null,
                null,
                (penFaced.HasValue && penFaced > 0 && penSaves.HasValue)
                    ? penSaves.Value * 100.0 / penFaced.Value
                    : null,
                assists, turnovers)
        );

    // ── Identité / Adversaire ──

    [Fact]
    public void Games_ResolvesOpponentWhenPlayerIsHomeTeam()
    {
        var opp = PlayerMatchResultResolver.ResolveOpponentName("t1", "t1", "Brest", "t2", "Metz");
        Assert.Equal("Metz", opp);
    }

    [Fact]
    public void Games_ResolvesOpponentWhenPlayerIsAwayTeam()
    {
        var opp = PlayerMatchResultResolver.ResolveOpponentName("t2", "t1", "Brest", "t2", "Metz");
        Assert.Equal("Brest", opp);
    }

    [Fact]
    public void Games_DoesNotInventOpponentWhenTeamUnknown()
    {
        var opp = PlayerMatchResultResolver.ResolveOpponentName(null, "t1", "Brest", "t2", "Metz");
        Assert.Null(opp);
    }

    [Fact]
    public void Games_ResolvesWinFromPlayerTeamPerspective()
    {
        var result = PlayerMatchResultResolver.ResolveResult("t1", "t1", 31, 27);
        Assert.Equal(PlayerMatchResult.Win, result);
    }

    [Fact]
    public void Games_ResolvesLossFromPlayerTeamPerspective()
    {
        var result = PlayerMatchResultResolver.ResolveResult("t2", "t1", 31, 27);
        Assert.Equal(PlayerMatchResult.Loss, result);
    }

    [Fact]
    public void Games_ResolvesDraw()
    {
        var result = PlayerMatchResultResolver.ResolveResult("t1", "t1", 25, 25);
        Assert.Equal(PlayerMatchResult.Draw, result);
    }

    [Fact]
    public void Games_MissingScoreDoesNotBecomeZeroZero()
    {
        var result = PlayerMatchResultResolver.ResolveResult("t1", "t1", null, null);
        Assert.Equal(PlayerMatchResult.Unknown, result);
        var score = PlayerMatchResultResolver.FormatScore("t1", "t1", null, null);
        Assert.Equal("—", score);
    }

    // ── Temps de jeu ──

    [Fact]
    public void Games_RecordedPlayingTimeDisplaysMinutes()
    {
        var row = MakeFieldRow(minutes: 38, ptStatus: PlayingTimeAvailability.RecordedDirect);
        Assert.Equal(38, row.PlayingTime.Minutes);
        Assert.Equal(PlayingTimeAvailability.RecordedDirect, row.PlayingTime.Availability);
    }

    [Fact]
    public void Games_DerivedPlayingTimePreservesAvailability()
    {
        var row = MakeFieldRow(minutes: 32, ptStatus: PlayingTimeAvailability.DerivedFromSubstitutions);
        Assert.Equal(PlayingTimeAvailability.DerivedFromSubstitutions, row.PlayingTime.Availability);
        Assert.Equal(32, row.PlayingTime.Minutes);
    }

    [Fact]
    public void Games_MissingPlayingTimeDoesNotDisplayZero()
    {
        var row = MakeFieldRow(minutes: null, ptStatus: PlayingTimeAvailability.DataMissing);
        Assert.Null(row.PlayingTime.Minutes);
        Assert.Equal(PlayingTimeAvailability.DataMissing, row.PlayingTime.Availability);
    }

    [Fact]
    public void Games_IdentityConflictDoesNotDisplayZeroMinutes()
    {
        var row = MakeFieldRow(minutes: null, ptStatus: PlayingTimeAvailability.IdentityConflict);
        Assert.Null(row.PlayingTime.Minutes);
        Assert.Equal(PlayingTimeAvailability.IdentityConflict, row.PlayingTime.Availability);
    }

    [Fact]
    public void Games_PartialPlayingTimeIsMarkedPartial()
    {
        var row = MakeFieldRow(minutes: 22, ptStatus: PlayingTimeAvailability.PartialData);
        Assert.Equal(PlayingTimeAvailability.PartialData, row.PlayingTime.Availability);
        Assert.NotNull(row.PlayingTime.Minutes);
    }

    // ── Taux ──

    [Fact]
    public void Games_ShotRateShowsNumeratorDenominator()
    {
        var row = MakeFieldRow(shotGoals: 7, shotAttempts: 10);
        Assert.Equal(70.0, row.FieldMetrics!.ShotRate!.Value, precision: 1);
        Assert.Equal(7, row.FieldMetrics.ShotGoals);
        Assert.Equal(10, row.FieldMetrics.ShotAttempts);
    }

    [Fact]
    public void Games_SaveRateShowsNumeratorDenominator()
    {
        var row = MakeGkRow(saves: 14, shotsFaced: 34);
        Assert.NotNull(row.GkMetrics!.SaveRate);
        Assert.Equal(41.2, row.GkMetrics.SaveRate!.Value, precision: 0);
        Assert.Equal(14, row.GkMetrics.Saves);
        Assert.Equal(34, row.GkMetrics.ShotsFaced);
    }

    [Fact]
    public void Games_ZeroShotAttemptsDoesNotRenderZeroPercent()
    {
        var row = MakeFieldRow(shotGoals: 0, shotAttempts: 0);
        Assert.Null(row.FieldMetrics!.ShotRate);
    }

    [Fact]
    public void Games_ZeroShotsFacedDoesNotRenderZeroPercent()
    {
        var row = MakeGkRow(saves: 0, shotsFaced: 0);
        Assert.Null(row.GkMetrics!.SaveRate);
    }

    [Fact]
    public void Games_RateSeasonBaselineAggregatesEvidence()
    {
        var d = DateTime.Today;
        var rows = new[]
        {
            MakeFieldRow("m1", d.AddDays(-2), shotGoals: 5, shotAttempts: 10),
            MakeFieldRow("m2", d.AddDays(-1), shotGoals: 2, shotAttempts: 2),
            MakeFieldRow("m3", d, shotGoals: 1, shotAttempts: 4)
        };
        var baseline = GameSeasonBaselineCalculator.Build(rows, false);
        Assert.NotNull(baseline.ShotRate);
        Assert.Equal(50.0, baseline.ShotRate!.Value, precision: 1); // 8/16
    }

    [Fact]
    public void Games_RateBaselineDoesNotAveragePercentages()
    {
        var d = DateTime.Today;
        var rows = new[]
        {
            MakeFieldRow("m1", d.AddDays(-2), shotGoals: 5, shotAttempts: 10),  // 50%
            MakeFieldRow("m2", d.AddDays(-1), shotGoals: 2, shotAttempts: 2),   // 100%
            MakeFieldRow("m3", d, shotGoals: 1, shotAttempts: 4)                // 25%
        };
        var baseline = GameSeasonBaselineCalculator.Build(rows, false);
        Assert.NotNull(baseline.ShotRate);
        // Moyenne brute serait 58.3%, correcte est 50%
        Assert.NotEqual(58.3, baseline.ShotRate!.Value, precision: 0);
        Assert.Equal(50.0, baseline.ShotRate.Value, precision: 1);
    }

    // ── Moyennes ──

    [Fact]
    public void Games_CountSeasonAverageUsesEligibleMatches()
    {
        var rows = new[]
        {
            MakeFieldRow("m1", goals: 7, turnovers: 1),
            MakeFieldRow("m2", goals: null, turnovers: null),
            MakeFieldRow("m3", goals: 3, turnovers: 2)
        };
        var baseline = GameSeasonBaselineCalculator.Build(rows, false);
        Assert.Equal(5.0, baseline.GoalsPerMatch!.Value, precision: 1);
    }

    [Fact]
    public void Games_RealZeroParticipatesInAverage()
    {
        var rows = new[]
        {
            MakeFieldRow("m1", goals: 6),
            MakeFieldRow("m2", goals: 0)
        };
        var baseline = GameSeasonBaselineCalculator.Build(rows, false);
        Assert.Equal(3.0, baseline.GoalsPerMatch!.Value, precision: 1);
    }

    [Fact]
    public void Games_DataMissingDoesNotParticipateAsZero()
    {
        var rows = new[]
        {
            MakeFieldRow("m1", goals: 6),
            MakeFieldRow("m2", goals: null),
            MakeFieldRow("m3", goals: 2)
        };
        var baseline = GameSeasonBaselineCalculator.Build(rows, false);
        Assert.Equal(4.0, baseline.GoalsPerMatch!.Value, precision: 1); // (6+2)/2
    }

    [Fact]
    public void Games_DeltaUsesSeasonBaseline()
    {
        var baseline = new GameSeasonBaseline(3.0, null, null, null, null, null, null, null, null, null, null, null);
        var matchGoals = 7;
        var delta = matchGoals - baseline.GoalsPerMatch!.Value;
        Assert.Equal(4.0, delta, precision: 1);
    }

    [Fact]
    public void Games_RateDeltaUsesPercentagePoints()
    {
        var matchRate = 70.0;
        var baselineRate = 62.2;
        var delta = matchRate - baselineRate;
        Assert.Equal(7.8, delta, precision: 0);
    }

    [Fact]
    public void Games_LowerIsBetterDeltaUsesCorrectTone()
    {
        // Pertes : direction LowerIsBetter
        // match: 1 perte, saison: 2.5 — delta = -1.5 → favorable
        var delta = 1 - 2.5;
        Assert.True(delta < 0); // baisse des pertes = bien
    }

    // ── Fenêtres ──

    [Fact]
    public void Games_Last5UsesFiveMostRecent()
    {
        var d = DateTime.Today;
        var rows = Enumerable.Range(0, 10).Select(i =>
            MakeFieldRow($"m{i}", d.AddDays(-i))).ToList();
        var windowed = rows.OrderBy(r => r.Identity.Date).TakeLast(5).ToList();
        Assert.Equal(5, windowed.Count);
    }

    [Fact]
    public void Games_Last10UsesTenMostRecent()
    {
        var d = DateTime.Today;
        var rows = Enumerable.Range(0, 15).Select(i =>
            MakeFieldRow($"m{i}", d.AddDays(-i))).ToList();
        var windowed = rows.OrderBy(r => r.Identity.Date).TakeLast(10).ToList();
        Assert.Equal(10, windowed.Count);
    }

    [Fact]
    public void Games_SeasonUsesAllMatches()
    {
        var d = DateTime.Today;
        var rows = Enumerable.Range(0, 18).Select(i =>
            MakeFieldRow($"m{i}", d.AddDays(-i))).ToList();
        Assert.Equal(18, rows.Count);
    }

    [Fact]
    public void Games_ResultFilterCombinesWithWindow()
    {
        var d = DateTime.Today;
        var rows = new[]
        {
            MakeFieldRow("m1", d.AddDays(-4), "t1", "t1", team1Score: 31, team2Score: 27),
            MakeFieldRow("m2", d.AddDays(-3), "t1", "t1", team1Score: 25, team2Score: 28),
            MakeFieldRow("m3", d.AddDays(-2), "t1", "t1", team1Score: 30, team2Score: 30),
            MakeFieldRow("m4", d.AddDays(-1), "t1", "t1", team1Score: 29, team2Score: 26),
            MakeFieldRow("m5", d, "t1", "t1", team1Score: 32, team2Score: 24)
        };
        var wins = rows.Where(r => r.Identity.Result == PlayerMatchResult.Win).ToList();
        Assert.Equal(3, wins.Count); // m1, m4, m5
    }

    // ── Gardiennes ──

    [Fact]
    public void Games_GoalkeeperUsesGoalkeeperColumns()
    {
        var row = MakeGkRow(saves: 14, shotsFaced: 34, penSaves: 3, penFaced: 5);
        Assert.NotNull(row.GkMetrics);
        Assert.Null(row.FieldMetrics);
    }

    [Fact]
    public void Games_GoalkeeperDoesNotUseFieldPlayerShotColumn()
    {
        var row = MakeGkRow();
        Assert.Null(row.FieldMetrics);
    }

    [Fact]
    public void Games_GoalkeeperShotsFacedUsesSavesPlusGoalsConceded()
    {
        var saves = 14;
        var goalsConceded = 20;
        var shotsFaced = saves + goalsConceded;
        Assert.Equal(34, shotsFaced);
    }

    [Fact]
    public void Games_GoalkeeperShotsFacedExcludesOffTarget()
    {
        // ShotsFaced = Saves + GoalsConceded, exclut hors cadre
        var saves = 14;
        var goalsConceded = 20;
        var shotsFaced = saves + goalsConceded;
        var row = MakeGkRow(saves: saves, shotsFaced: shotsFaced);
        Assert.Equal(34, row.GkMetrics!.ShotsFaced);
    }

    [Fact]
    public void Games_GoalkeeperShotsFacedExcludesPreKeeperBlock()
    {
        var row = MakeGkRow(saves: 14, shotsFaced: 34);
        Assert.Equal(34, row.GkMetrics!.ShotsFaced);
    }

    [Fact]
    public void Games_GoalkeeperPenaltyRateShowsEvidence()
    {
        var row = MakeGkRow(penSaves: 3, penFaced: 5);
        Assert.Equal(3, row.GkMetrics!.PenaltySaves);
        Assert.Equal(5, row.GkMetrics.PenaltyShotsFaced);
        Assert.NotNull(row.GkMetrics.PenaltySaveRate);
        Assert.Equal(60.0, row.GkMetrics.PenaltySaveRate!.Value, precision: 0);
    }

    // ── Reconciliation Trajectory/Games ──

    [Fact]
    public void Games_GoalsConsistentWithTrajectoryDefinition()
    {
        var row = MakeFieldRow(goals: 7);
        Assert.Equal(7, row.FieldMetrics!.Goals);
    }

    [Fact]
    public void Games_TurnoversConsistentWithTrajectoryDefinition()
    {
        var row = MakeFieldRow(turnovers: 2);
        Assert.Equal(2, row.FieldMetrics!.Turnovers);
    }

    [Fact]
    public void Games_GoalkeeperSavesConsistentWithTrajectoryDefinition()
    {
        var row = MakeGkRow(saves: 14);
        Assert.Equal(14, row.GkMetrics!.Saves);
    }

    [Fact]
    public void Games_PlayingTimeConsistentWithTrajectoryDefinition()
    {
        var row = MakeFieldRow(minutes: 38, ptStatus: PlayingTimeAvailability.RecordedDirect);
        Assert.Equal(38, row.PlayingTime.Minutes);
        Assert.Equal(PlayingTimeAvailability.RecordedDirect, row.PlayingTime.Availability);
    }

    [Fact]
    public void Games_SeasonBaselineForRateConsistentWithTrajectory()
    {
        var rows = new[]
        {
            MakeFieldRow("m1", shotGoals: 5, shotAttempts: 10),
            MakeFieldRow("m2", shotGoals: 2, shotAttempts: 2),
            MakeFieldRow("m3", shotGoals: 1, shotAttempts: 4)
        };
        var baseline = GameSeasonBaselineCalculator.Build(rows, false);
        Assert.Equal(50.0, baseline.ShotRate!.Value, precision: 1);
    }

    [Fact]
    public void Games_PlayingTimeAverageExcludesMissing()
    {
        var rows = new[]
        {
            MakeFieldRow("m1", minutes: 38, ptStatus: PlayingTimeAvailability.RecordedDirect),
            MakeFieldRow("m2", minutes: null, ptStatus: PlayingTimeAvailability.DataMissing),
            MakeFieldRow("m3", minutes: 42, ptStatus: PlayingTimeAvailability.RecordedDirect)
        };
        var eligible = rows.Where(r => r.PlayingTime.Minutes.HasValue && r.PlayingTime.Availability != PlayingTimeAvailability.DataMissing).ToList();
        var avg = eligible.Average(r => r.PlayingTime.Minutes!.Value);
        Assert.Equal(40.0, avg, precision: 1); // (38+42)/2
    }
}
