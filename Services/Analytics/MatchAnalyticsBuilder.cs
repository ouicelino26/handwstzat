using HandWStat.Models.Contracts;

namespace HandWStat.Services.Analytics;

public sealed record MatchTeamAnalytics(
    int? TeamId,
    string? TeamName,
    int Goals,
    int Attempts,
    int Assists,
    int Turnovers,
    int Interceptions,
    int Saves,
    int ShotsFaced,
    int Sanctions,
    int PenaltyGoals,
    int PenaltyAttempts)
{
    // SUM-based rates — never AVG(playerRate) (spec §6)
    // Attempts = officialGoals + event-based misses → always ≥ Goals (see BuildTeamAnalytics).
    public double? ShotRate => Attempts > 0 ? (double)Goals / Attempts : null;
    public double? SaveRate => ShotsFaced > 0 ? (double)Saves / ShotsFaced : null;
    public double? PenaltyRate => PenaltyAttempts > 0 ? (double)PenaltyGoals / PenaltyAttempts : null;
}

public sealed record MatchInsight(string Text, string? Tone = null);

public static class MatchAnalyticsBuilder
{
    public const double InsightShotRateMinDelta = 0.05;
    public const double InsightSaveRateMinDelta = 0.05;
    public const int InsightCountMinDelta = 3;
    public const int GkMinShotsFacedForInsight = 3;
    public const int GkMinShotsFacedForTopGk = 3;

    // Goals = officialGoals (canonical official score, spec §4).
    // Attempts = officialGoals + event-based misses (ShotAttempts - TotalGoals per player).
    // ShotRate = OfficialGoals / Attempts — not AVG(playerRate), not SUM(ShotAttempts).
    public static MatchTeamAnalytics BuildTeamAnalytics(
        int? teamId,
        string? teamName,
        int officialGoals,
        IEnumerable<PlayerGlobalStatsDto> teamPlayers)
    {
        var players = teamPlayers.ToList();
        // Non-goal shots from events (saves + misses + blocked).
        // Subtract event-based goals so official goals remain the single authoritative source.
        var eventMisses = players.Sum(p => Math.Max(0, p.ShotAttempts - p.TotalGoals));
        return new MatchTeamAnalytics(
            teamId,
            teamName,
            officialGoals,
            officialGoals + eventMisses,   // total shots: official goals + event misses
            players.Sum(p => p.AssistCount),
            players.Sum(p => p.TurnoverCount),
            players.Sum(p => p.InterceptionCount),
            players.Sum(p => p.SaveCount),
            players.Sum(p => p.ShotsFaced),
            players.Sum(p => p.SanctionCount),
            players.Sum(p => p.PenaltyGoalCount),
            players.Sum(p => p.PenaltyAttempts));
    }

    public static IReadOnlyList<MatchInsight> BuildInsights(
        MatchTeamAnalytics home,
        MatchTeamAnalytics away)
    {
        var insights = new List<MatchInsight>();

        // Shot rate: only when both teams attempted shots
        if (home.Attempts > 0 && away.Attempts > 0
            && home.ShotRate.HasValue && away.ShotRate.HasValue)
        {
            var delta = home.ShotRate.Value - away.ShotRate.Value;
            if (Math.Abs(delta) >= InsightShotRateMinDelta)
            {
                var better = delta > 0 ? home : away;
                var worse = delta > 0 ? away : home;
                insights.Add(new MatchInsight(
                    $"{better.TeamName} a eu une meilleure réussite au tir : {better.ShotRate!.Value:0%} contre {worse.ShotRate!.Value:0%}.",
                    "positive"));
            }
        }

        // Save rate: require minimum shots faced for statistical relevance
        if (home.ShotsFaced >= GkMinShotsFacedForInsight
            && away.ShotsFaced >= GkMinShotsFacedForInsight
            && home.SaveRate.HasValue && away.SaveRate.HasValue)
        {
            var delta = home.SaveRate.Value - away.SaveRate.Value;
            if (Math.Abs(delta) >= InsightSaveRateMinDelta)
            {
                var better = delta > 0 ? home : away;
                var worse = delta > 0 ? away : home;
                insights.Add(new MatchInsight(
                    $"Les gardiennes de {better.TeamName} ont mieux arrêté : {better.SaveRate!.Value:0%} contre {worse.SaveRate!.Value:0%}.",
                    "positive"));
            }
        }

        // Turnovers: meaningful gap only
        if (home.Turnovers + away.Turnovers > 0)
        {
            var delta = home.Turnovers - away.Turnovers;
            if (Math.Abs(delta) >= InsightCountMinDelta)
            {
                var more = delta > 0 ? home : away;
                var less = delta > 0 ? away : home;
                insights.Add(new MatchInsight(
                    $"{more.TeamName} a perdu {Math.Abs(delta)} ballons de plus ({more.Turnovers} contre {less.Turnovers}).",
                    "warning"));
            }
        }

        // Interceptions: meaningful gap only
        if (home.Interceptions + away.Interceptions > 0)
        {
            var delta = home.Interceptions - away.Interceptions;
            if (Math.Abs(delta) >= InsightCountMinDelta)
            {
                var more = delta > 0 ? home : away;
                var less = delta > 0 ? away : home;
                insights.Add(new MatchInsight(
                    $"{more.TeamName} a dominé défensivement : {more.Interceptions} interceptions contre {less.Interceptions}.",
                    null));
            }
        }

        return insights;
    }

    public static PlayerGlobalStatsDto? GetTopScorer(IEnumerable<PlayerGlobalStatsDto> players)
        => players
            .Where(p => !p.IsGoalkeeper && p.TotalGoals > 0)
            .OrderByDescending(p => p.TotalGoals)
            .ThenBy(p => p.FullName)
            .FirstOrDefault();

    public static PlayerGlobalStatsDto? GetTopCreator(IEnumerable<PlayerGlobalStatsDto> players)
        => players
            .Where(p => !p.IsGoalkeeper && p.AssistCount > 0)
            .OrderByDescending(p => p.AssistCount)
            .ThenBy(p => p.FullName)
            .FirstOrDefault();

    public static PlayerGlobalStatsDto? GetTopDefender(IEnumerable<PlayerGlobalStatsDto> players)
        => players
            .Where(p => !p.IsGoalkeeper && p.InterceptionCount > 0)
            .OrderByDescending(p => p.InterceptionCount)
            .ThenBy(p => p.FullName)
            .FirstOrDefault();

    public static PlayerGlobalStatsDto? GetTopGoalkeeper(
        IEnumerable<PlayerGlobalStatsDto> players,
        int minShotsFaced = GkMinShotsFacedForTopGk)
        => players
            .Where(p => p.IsGoalkeeper && p.ShotsFaced >= minShotsFaced)
            .OrderByDescending(p => p.GoalkeeperSaveRate)
            .ThenByDescending(p => p.SaveCount)
            .ThenBy(p => p.FullName)
            .FirstOrDefault();

    // Official score is authoritative — event goal count is for data quality only (spec §4, §30).
    public static bool IsEventScoreCoverageComplete(
        int officialTotalGoals,
        int eventTotalGoals,
        out int gap)
    {
        gap = officialTotalGoals - eventTotalGoals;
        return gap == 0;
    }
}
