using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

// Phase A10 — MatchAnalyticsBuilder unit tests
// TEST_BASELINE_A10 = 1052

public class AnalyticsA10Tests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PlayerGlobalStatsDto MakePlayer(
        int? teamId = 1, string fullName = "Player",
        bool isGoalkeeper = false,
        int goals = 0, int penaltyGoals = 0, int totalGoals = 0,
        int assists = 0, int turnovers = 0, int interceptions = 0,
        int saves = 0, int shotsFaced = 0, int sanctions = 0,
        int shotAttempts = 0, int penaltyAttempts = 0)
        => new PlayerGlobalStatsDto
        {
            TeamId = teamId,
            FullName = fullName,
            IsGoalkeeper = isGoalkeeper,
            GoalCount = goals,
            PenaltyGoalCount = penaltyGoals,
            TotalGoals = totalGoals,
            AssistCount = assists,
            TurnoverCount = turnovers,
            InterceptionCount = interceptions,
            SaveCount = saves,
            ShotsFaced = shotsFaced,
            SanctionCount = sanctions,
            ShotAttempts = shotAttempts,
            PenaltyAttempts = penaltyAttempts,
            GoalkeeperSaveRate = shotsFaced > 0 ? (double)saves / shotsFaced : 0.0,
        };

    // ── BuildTeamAnalytics — SUM formula ──────────────────────────────────────

    [Fact]
    public void BuildTeamAnalytics_Goals_UsesOfficialScore_NotEventSum()
    {
        // Event goals = 2 (player counts), official = 5 — official wins
        var players = new[] { MakePlayer(goals: 1, shotAttempts: 3), MakePlayer(goals: 1, shotAttempts: 3) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", officialGoals: 5, teamPlayers: players);
        Assert.Equal(5, result.Goals);
    }

    [Fact]
    public void BuildTeamAnalytics_Attempts_IsSumAcrossAllPlayers()
    {
        var players = new[] { MakePlayer(shotAttempts: 4), MakePlayer(shotAttempts: 6) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", 0, players);
        Assert.Equal(10, result.Attempts);
    }

    [Fact]
    public void BuildTeamAnalytics_Assists_IsSumAcrossAllPlayers()
    {
        var players = new[] { MakePlayer(assists: 2), MakePlayer(assists: 3) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", 0, players);
        Assert.Equal(5, result.Assists);
    }

    [Fact]
    public void BuildTeamAnalytics_EmptyRoster_AllCountsZero()
    {
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", officialGoals: 0, teamPlayers: []);
        Assert.Equal(0, result.Attempts);
        Assert.Equal(0, result.Assists);
        Assert.Equal(0, result.Turnovers);
        Assert.Equal(0, result.Interceptions);
        Assert.Equal(0, result.Saves);
    }

    [Fact]
    public void BuildTeamAnalytics_ShotRate_GoalsOverAttempts()
    {
        // official 3 goals + 7 event misses (no event goals) → 10 total shots → 30%
        var players = new[] { MakePlayer(shotAttempts: 4, totalGoals: 0), MakePlayer(shotAttempts: 3, totalGoals: 0) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", officialGoals: 3, teamPlayers: players);
        Assert.NotNull(result.ShotRate);
        Assert.Equal(0.3, result.ShotRate!.Value, precision: 9);
    }

    [Fact]
    public void BuildTeamAnalytics_ShotRate_NullWhenAttemptsZero()
    {
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", 0, []);
        Assert.Null(result.ShotRate);
    }

    [Fact]
    public void BuildTeamAnalytics_SaveRate_SavesOverShotsFaced()
    {
        var players = new[] { MakePlayer(isGoalkeeper: true, saves: 7, shotsFaced: 10) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", 0, players);
        Assert.NotNull(result.SaveRate);
        Assert.Equal(0.7, result.SaveRate!.Value, precision: 9);
    }

    [Fact]
    public void BuildTeamAnalytics_SaveRate_NullWhenShotsFacedZero()
    {
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", 0, []);
        Assert.Null(result.SaveRate);
    }

    [Fact]
    public void BuildTeamAnalytics_PenaltyRate_PenaltyGoalsOverPenaltyAttempts()
    {
        var players = new[] { MakePlayer(penaltyGoals: 3, penaltyAttempts: 4) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Team", 0, players);
        Assert.NotNull(result.PenaltyRate);
        Assert.Equal(0.75, result.PenaltyRate!.Value, precision: 9);
    }

    [Fact]
    public void BuildTeamAnalytics_HomeAwayIsolation_SumsOnlyCorrectTeam()
    {
        // team1 players (teamId=1) and team2 players (teamId=2) mixed — build only team1
        var team1 = new[] { MakePlayer(teamId: 1, shotAttempts: 5) };
        var team2 = new[] { MakePlayer(teamId: 2, shotAttempts: 99) };
        var result = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "Home", 0, team1);
        Assert.Equal(5, result.Attempts);
    }

    // ── BuildInsights — thresholds ────────────────────────────────────────────

    private static MatchTeamAnalytics MakeAnalytics(
        int goals = 0, int attempts = 0, int assists = 0, int turnovers = 0,
        int interceptions = 0, int saves = 0, int shotsFaced = 0,
        string teamName = "Team")
        => new MatchTeamAnalytics(1, teamName, goals, attempts, assists,
            turnovers, interceptions, saves, shotsFaced, 0, 0, 0);

    [Fact]
    public void BuildInsights_ShotRateDeltaAboveThreshold_ProducesInsight()
    {
        // home: 7/10 = 70%, away: 4/10 = 40% → delta = 30% >= 5%
        var home = MakeAnalytics(goals: 7, attempts: 10, teamName: "Home");
        var away = MakeAnalytics(goals: 4, attempts: 10, teamName: "Away");
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.Contains(insights, i => i.Text.Contains("Home") && i.Text.Contains("réussite au tir"));
    }

    [Fact]
    public void BuildInsights_ShotRateDeltaBelowThreshold_NoInsight()
    {
        // home: 5/10 = 50%, away: 5/10 = 50% → delta = 0% < 5%
        var home = MakeAnalytics(goals: 5, attempts: 10);
        var away = MakeAnalytics(goals: 5, attempts: 10);
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.DoesNotContain(insights, i => i.Text.Contains("réussite au tir"));
    }

    [Fact]
    public void BuildInsights_SaveRateDeltaAboveThreshold_ProducesInsight()
    {
        // home: 8/10 = 80%, away: 5/10 = 50% → delta = 30% >= 5%
        var home = MakeAnalytics(saves: 8, shotsFaced: 10, teamName: "Home");
        var away = MakeAnalytics(saves: 5, shotsFaced: 10, teamName: "Away");
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.Contains(insights, i => i.Text.Contains("Home") && i.Text.Contains("arrêté"));
    }

    [Fact]
    public void BuildInsights_SaveRate_NotEmittedWhenBelowMinShotsFaced()
    {
        // shotsFaced = 2 < GkMinShotsFacedForInsight (3) — no save insight regardless of delta
        var home = MakeAnalytics(saves: 2, shotsFaced: 2);
        var away = MakeAnalytics(saves: 0, shotsFaced: 2);
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.DoesNotContain(insights, i => i.Text.Contains("arrêté"));
    }

    [Fact]
    public void BuildInsights_TurnoversGapAboveThreshold_ProducesInsight()
    {
        // home 8, away 4 → delta = 4 >= 3
        var home = MakeAnalytics(turnovers: 8, teamName: "Home");
        var away = MakeAnalytics(turnovers: 4, teamName: "Away");
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.Contains(insights, i => i.Text.Contains("Home") && i.Text.Contains("ballons"));
    }

    [Fact]
    public void BuildInsights_TurnoversGapBelowThreshold_NoInsight()
    {
        // home 5, away 4 → delta = 1 < 3
        var home = MakeAnalytics(turnovers: 5);
        var away = MakeAnalytics(turnovers: 4);
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.DoesNotContain(insights, i => i.Text.Contains("ballons"));
    }

    [Fact]
    public void BuildInsights_InterceptionsGapAboveThreshold_ProducesInsight()
    {
        // home 7, away 2 → delta = 5 >= 3
        var home = MakeAnalytics(interceptions: 7, teamName: "Home");
        var away = MakeAnalytics(interceptions: 2, teamName: "Away");
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.Contains(insights, i => i.Text.Contains("Home") && i.Text.Contains("interceptions"));
    }

    [Fact]
    public void BuildInsights_AllZeros_NoInsights()
    {
        var home = MakeAnalytics();
        var away = MakeAnalytics();
        var insights = MatchAnalyticsBuilder.BuildInsights(home, away);
        Assert.Empty(insights);
    }

    // ── GetTopScorer / GetTopCreator / GetTopDefender ─────────────────────────

    [Fact]
    public void GetTopScorer_ReturnsHighestGoalScorer_ExcludesGk()
    {
        var players = new[]
        {
            MakePlayer(fullName: "A", totalGoals: 5),
            MakePlayer(fullName: "B", totalGoals: 8),
            MakePlayer(fullName: "GK", isGoalkeeper: true, totalGoals: 10),
        };
        var result = MatchAnalyticsBuilder.GetTopScorer(players);
        Assert.NotNull(result);
        Assert.Equal("B", result!.FullName);
    }

    [Fact]
    public void GetTopScorer_ReturnsNull_WhenNoGoals()
    {
        var players = new[] { MakePlayer(totalGoals: 0) };
        var result = MatchAnalyticsBuilder.GetTopScorer(players);
        Assert.Null(result);
    }

    [Fact]
    public void GetTopCreator_ReturnsHighestAssistCount()
    {
        var players = new[]
        {
            MakePlayer(fullName: "A", assists: 2),
            MakePlayer(fullName: "B", assists: 5),
        };
        var result = MatchAnalyticsBuilder.GetTopCreator(players);
        Assert.Equal("B", result!.FullName);
    }

    [Fact]
    public void GetTopDefender_ReturnsHighestInterceptionCount()
    {
        var players = new[]
        {
            MakePlayer(fullName: "A", interceptions: 1),
            MakePlayer(fullName: "B", interceptions: 4),
        };
        var result = MatchAnalyticsBuilder.GetTopDefender(players);
        Assert.Equal("B", result!.FullName);
    }

    // ── GetTopGoalkeeper ──────────────────────────────────────────────────────

    [Fact]
    public void GetTopGoalkeeper_ReturnsGkWithHighestSaveRate_AboveMinFaced()
    {
        var players = new[]
        {
            MakePlayer(fullName: "GK1", isGoalkeeper: true, saves: 5, shotsFaced: 10),
            MakePlayer(fullName: "GK2", isGoalkeeper: true, saves: 8, shotsFaced: 10),
        };
        players[0].GoalkeeperSaveRate = 0.5;
        players[1].GoalkeeperSaveRate = 0.8;
        var result = MatchAnalyticsBuilder.GetTopGoalkeeper(players, minShotsFaced: 3);
        Assert.Equal("GK2", result!.FullName);
    }

    [Fact]
    public void GetTopGoalkeeper_ExcludesGkBelowMinShotsFaced()
    {
        var players = new[]
        {
            MakePlayer(fullName: "GKFew", isGoalkeeper: true, saves: 2, shotsFaced: 2),
        };
        var result = MatchAnalyticsBuilder.GetTopGoalkeeper(players, minShotsFaced: 3);
        Assert.Null(result);
    }

    [Fact]
    public void GetTopGoalkeeper_ExcludesFieldPlayers()
    {
        var players = new[]
        {
            MakePlayer(fullName: "Field", isGoalkeeper: false, saves: 5, shotsFaced: 10),
        };
        var result = MatchAnalyticsBuilder.GetTopGoalkeeper(players, minShotsFaced: 3);
        Assert.Null(result);
    }

    // ── IsEventScoreCoverageComplete ──────────────────────────────────────────

    [Fact]
    public void IsEventScoreCoverageComplete_ExactMatch_ReturnsTrue_GapZero()
    {
        var result = MatchAnalyticsBuilder.IsEventScoreCoverageComplete(15, 15, out int gap);
        Assert.True(result);
        Assert.Equal(0, gap);
    }

    [Fact]
    public void IsEventScoreCoverageComplete_EventsLessThanOfficial_ReturnsFalse_PositiveGap()
    {
        var result = MatchAnalyticsBuilder.IsEventScoreCoverageComplete(20, 17, out int gap);
        Assert.False(result);
        Assert.Equal(3, gap);
    }

    [Fact]
    public void IsEventScoreCoverageComplete_EventsMoreThanOfficial_ReturnsFalse_NegativeGap()
    {
        var result = MatchAnalyticsBuilder.IsEventScoreCoverageComplete(15, 17, out int gap);
        Assert.False(result);
        Assert.Equal(-2, gap);
    }
}
