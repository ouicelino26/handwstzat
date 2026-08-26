using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Services.Analytics;

public sealed record TeamAnalyticsModel
{
    // Offense — API-provided rates take priority (source priority 1)
    public double? OverallShotRate { get; init; }
    public double? OpenPlayShotRate { get; init; }
    public double? PenaltyShotRate { get; init; }
    public double? GoalsPerMatch { get; init; }
    public double? AssistsPerMatch { get; init; }

    // Mastery / ball security
    public double? AssistTurnoverRatio { get; init; }
    public double? TurnoversPerMatch { get; init; }

    // Goalkeeping — API rate priority 1; aggregated from player list as priority 3 fallback
    public double? TeamGkSaveRate { get; init; }
    public double? TeamGkPenaltySaveRate { get; init; }
    public double? AggregatedGkSaveRate { get; init; }

    // Quality — AnalyticsQualityPolicy evaluated against shot / GK sample counts
    public QualityTierResult ShotRateQuality { get; init; } = new(QualityTier.NotApplicable, null, null);
    public QualityTierResult GkQuality { get; init; } = new(QualityTier.NotApplicable, null, null);
    public QualityTierResult PenaltyQuality { get; init; } = new(QualityTier.NotApplicable, null, null);

    // Metadata
    public int MatchesPlayed { get; init; }
    public int GoalsFor { get; init; }
    public int Assists { get; init; }
    public int Turnovers { get; init; }
    public int TotalShotAttempts { get; init; }
    public int PenaltyAttempts { get; init; }
}

public static class TeamAnalyticsBuilder
{
    public const int MinTeamShotSample = 30;
    public const int MinTeamPenaltySample = 10;
    public const int MinTeamGkShotSample = 30;

    // SUM(goals) / SUM(attempts) × 100 — never AVG(player rates)
    public static double? ComputeTeamShotRate(int goals, int attempts)
    {
        if (attempts <= 0) return null;
        return (double)goals / attempts * 100.0;
    }

    // assists / turnovers — N/A when turnovers = 0 (undefined ratio)
    public static double? ComputeTeamAssistTurnoverRatio(int assists, int turnovers)
    {
        if (turnovers <= 0) return null;
        return (double)assists / turnovers;
    }

    public static double? ComputeGoalsPerMatch(int goals, int matches)
    {
        if (matches <= 0) return null;
        return (double)goals / matches;
    }

    // CAT-17a — delegates to AnalyticsCalculationService for cross-page formula consistency
    public static double? ComputeGoalsSharePct(int playerGoals, int teamGoals) =>
        AnalyticsCalculationService.ComputeGoalsSharePct(playerGoals, teamGoals);

    // SUM(saves) / SUM(shotsFaced) × 100 — delegates to GoalkeeperAnalyticsBuilder (correct aggregation)
    public static double? ComputeAggregatedTeamGkSaveRate(IEnumerable<(int Saves, int ShotsFaced)> gkData) =>
        GoalkeeperAnalyticsBuilder.AggregateTeamSaveRate(gkData);

    public static TeamAnalyticsModel Build(TeamStatsDto teamStats, IReadOnlyList<PlayerGlobalStatsDto>? players)
    {
        players ??= [];

        var matches = teamStats.MatchesPlayed;
        var goals = teamStats.GoalsFor;
        var tech = teamStats.Technical;
        var overview = teamStats.Overview;

        var assists = overview?.AssistCount ?? 0;
        var turnovers = overview?.TurnoverCount ?? 0;
        var shotAttempts = tech?.ShotAttempts ?? 0;
        var penaltyAttempts = tech?.PenaltyAttempts ?? 0;
        var tirsSubis = tech?.TirsSubis ?? 0;
        var gkPenaltyFaced = (tech?.GoalkeeperPenaltyStops ?? 0) + (tech?.GoalkeeperPenaltyConcededGoals ?? 0);

        // GK player aggregation (priority 3 — fallback / verification only)
        var gkTuples = players
            .Where(p => p.IsGoalkeeper)
            .Select(p => (p.SaveCount, p.ShotsFaced));
        var aggregatedGkRate = ComputeAggregatedTeamGkSaveRate(gkTuples);

        // Quality policy applied to team-level samples
        var shotRateQuality = AnalyticsQualityPolicy.EvaluateTier(null, shotAttempts, MinTeamShotSample);
        var penaltyQuality = AnalyticsQualityPolicy.EvaluateTier(null, penaltyAttempts, MinTeamPenaltySample);
        var gkQuality = AnalyticsQualityPolicy.EvaluateTier(null, tirsSubis, MinTeamGkShotSample);

        return new TeamAnalyticsModel
        {
            // API-provided rates take priority when denominator > 0
            OverallShotRate = shotAttempts > 0 ? (double?)tech?.OverallShotSuccessRate : ComputeTeamShotRate(goals, shotAttempts),
            OpenPlayShotRate = shotAttempts > 0 ? (double?)tech?.OpenShotSuccessRate : null,
            PenaltyShotRate = penaltyAttempts > 0 ? (double?)tech?.PenaltySuccessRate : null,
            GoalsPerMatch = ComputeGoalsPerMatch(goals, matches),
            AssistsPerMatch = matches > 0 ? (double?)assists / matches : null,
            AssistTurnoverRatio = ComputeTeamAssistTurnoverRatio(assists, turnovers),
            TurnoversPerMatch = matches > 0 ? (double?)turnovers / matches : null,
            // GK: API priority when TirsSubis > 0; else fall back to player aggregation
            TeamGkSaveRate = tirsSubis > 0 ? (double?)tech?.GoalkeeperSaveRate : aggregatedGkRate,
            TeamGkPenaltySaveRate = gkPenaltyFaced > 0 ? (double?)tech?.GoalkeeperPenaltyStopRate : null,
            AggregatedGkSaveRate = aggregatedGkRate,
            ShotRateQuality = shotRateQuality,
            GkQuality = gkQuality,
            PenaltyQuality = penaltyQuality,
            MatchesPlayed = matches,
            GoalsFor = goals,
            Assists = assists,
            Turnovers = turnovers,
            TotalShotAttempts = shotAttempts,
            PenaltyAttempts = penaltyAttempts
        };
    }
}
