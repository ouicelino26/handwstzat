using HandWStat.Models.Contracts;

namespace HandWStat.Models.Matches;

/// <summary>
/// Builds per-team comparison metrics for the match summary.
///
/// Semantic constraints (never aggregate across semantic boundaries):
/// - PenaltiesConceded != SanctionConceded — always kept as separate metric
/// - BlockedShot != Save
/// - Neutralization != Block
/// - OffensiveFoulDrawn != Interception
/// - FailedPivotPass != BadPass
/// - TotalSanctions = Warnings + TwoMinutes + Disqualifications (PenaltiesConceded EXCLUDED)
///
/// Rate rules:
/// - Always SUM(goals)/SUM(attempts), never average of percentages
/// - ZeroDenominator -> Availability = ZeroDenominator, Value = null
/// - DataMissing -> Availability = DataMissing, Value = null
/// </summary>
public static class MatchComparisonBuilder
{
    private static readonly string[] AllPrimaryMetricCodes =
    [
        "GOALS",
        "ASSISTS",
        "SHOT_RATE",
        "7M_PENALTIES_WON",
        "INTERCEPTIONS",
        "SAVES",
        "SAVE_RATE",
        "PENALTIES_DRAWN",
        "TURNOVERS",
        "BAD_PASSES",
        "TOTAL_SANCTIONS",
        "PENALTIES_CONCEDED"
    ];

    public static IReadOnlyList<string> GetAllPrimaryMetricCodes() => AllPrimaryMetricCodes;

    public static IReadOnlyList<MatchComparisonSection> Build(
        MatchSummaryDto summary,
        IReadOnlyList<PlayerGlobalStatsDto> allPlayers,
        string? team1Name,
        string? team2Name)
    {
        var team1Players = FilterByTeam(allPlayers, team1Name);
        var team2Players = FilterByTeam(allPlayers, team2Name);

        return
        [
            BuildAttack(summary, team1Players, team2Players, team1Name, team2Name),
            BuildDefense(summary, team1Players, team2Players, team1Name, team2Name),
            BuildMastery(summary, team1Players, team2Players, team1Name, team2Name)
        ];
    }

    private static IReadOnlyList<PlayerGlobalStatsDto> FilterByTeam(
        IReadOnlyList<PlayerGlobalStatsDto> players,
        string? teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName) || players.Count == 0)
        {
            return [];
        }

        return players
            .Where(p => string.Equals(p.TeamName?.Trim(), teamName.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static MatchComparisonSection BuildAttack(
        MatchSummaryDto summary,
        IReadOnlyList<PlayerGlobalStatsDto> team1Players,
        IReadOnlyList<PlayerGlobalStatsDto> team2Players,
        string? team1Name,
        string? team2Name)
    {
        var metrics = new List<MatchComparisonMetric>();

        // Goals — from match scores (ground truth)
        metrics.Add(BuildGoalsMetric(summary));

        // Assists
        metrics.Add(BuildSumMetric(
            "ASSISTS", "Passes décisives", ComparisonFamily.Attack, MetricDirection.HigherIsBetter,
            team1Players, team2Players, p => p.AssistCount));

        // Shot rate = SUM(goals)/SUM(attempts), never average of percentages
        metrics.Add(BuildShotRateMetric(team1Players, team2Players));

        // 7m Penalties won
        metrics.Add(BuildSumMetric(
            "7M_PENALTIES_WON", "7m obtenus", ComparisonFamily.Attack, MetricDirection.HigherIsBetter,
            team1Players, team2Players, p => p.PenaltyGoalCount + p.PenaltyAttempts - p.PenaltyAttempts,
            numeratorSelector: p => p.PenaltyAttempts));

        return new MatchComparisonSection(ComparisonFamily.Attack, "Attaque", metrics);
    }

    private static MatchComparisonSection BuildDefense(
        MatchSummaryDto summary,
        IReadOnlyList<PlayerGlobalStatsDto> team1Players,
        IReadOnlyList<PlayerGlobalStatsDto> team2Players,
        string? team1Name,
        string? team2Name)
    {
        var metrics = new List<MatchComparisonMetric>();

        // Interceptions — HigherIsBetter
        metrics.Add(BuildSumMetric(
            "INTERCEPTIONS", "Interceptions", ComparisonFamily.Defense, MetricDirection.HigherIsBetter,
            team1Players, team2Players, p => p.InterceptionCount));

        // Saves / SaveRate = Saves/(Saves+GoalsConceded)
        // Do NOT include off-target shots or pre-keeper blocks
        metrics.Add(BuildSaveRateMetric(team1Players, team2Players));

        // Penalties Drawn (defensive achievement) — HigherIsBetter
        metrics.Add(BuildSumMetric(
            "PENALTIES_DRAWN", "7m provoqués (def.)", ComparisonFamily.Defense, MetricDirection.HigherIsBetter,
            team1Players, team2Players, p => p.ShotsFaced > 0 ? 0 : 0, // DataMissing — no field in DTO
            availability: MetricAvailability.DataMissing));

        return new MatchComparisonSection(ComparisonFamily.Defense, "Défense", metrics);
    }

    private static MatchComparisonSection BuildMastery(
        MatchSummaryDto summary,
        IReadOnlyList<PlayerGlobalStatsDto> team1Players,
        IReadOnlyList<PlayerGlobalStatsDto> team2Players,
        string? team1Name,
        string? team2Name)
    {
        var metrics = new List<MatchComparisonMetric>();

        // Turnovers — LowerIsBetter
        metrics.Add(BuildSumMetric(
            "TURNOVERS", "Pertes de balle", ComparisonFamily.Mastery, MetricDirection.LowerIsBetter,
            team1Players, team2Players, p => p.TurnoverCount));

        // BadPasses — LowerIsBetter
        // Note: NOT FailedPivotPass — PlayerGlobalStatsDto.TurnoverCount captures technical losses
        // DataMissing: PlayerGlobalStatsDto has TurnoverCount but no separate BadPass field
        metrics.Add(BuildSumMetric(
            "BAD_PASSES", "Mauvaises passes", ComparisonFamily.Mastery, MetricDirection.LowerIsBetter,
            team1Players, team2Players, _ => 0,
            availability: MetricAvailability.DataMissing));

        // TotalSanctions = Warnings + TwoMinutes + Disqualifications (PenaltiesConceded EXCLUDED)
        // PlayerGlobalStatsDto.SanctionCount = Warnings + TwoMinutes + Disqualifications
        metrics.Add(BuildSumMetric(
            "TOTAL_SANCTIONS", "Sanctions", ComparisonFamily.Mastery, MetricDirection.LowerIsBetter,
            team1Players, team2Players, p => p.SanctionCount));

        // PenaltiesConceded — separate metric, LowerIsBetter
        // Not available as separate field in PlayerGlobalStatsDto — DataMissing
        metrics.Add(BuildSumMetric(
            "PENALTIES_CONCEDED", "7m concédés", ComparisonFamily.Mastery, MetricDirection.LowerIsBetter,
            team1Players, team2Players, _ => 0,
            availability: MetricAvailability.DataMissing));

        return new MatchComparisonSection(ComparisonFamily.Mastery, "Maîtrise", metrics);
    }

    private static MatchComparisonMetric BuildGoalsMetric(MatchSummaryDto summary)
    {
        var t1Goals = summary.Team1Score;
        var t2Goals = summary.Team2Score;

        if (t1Goals == null && t2Goals == null)
        {
            return new MatchComparisonMetric(
                "GOALS", "Buts", ComparisonFamily.Attack, MetricDirection.HigherIsBetter,
                null, null, null, null, null, null, MetricAvailability.DataMissing);
        }

        return new MatchComparisonMetric(
            "GOALS", "Buts", ComparisonFamily.Attack, MetricDirection.HigherIsBetter,
            t1Goals.HasValue ? (double)t1Goals.Value : null,
            t2Goals.HasValue ? (double)t2Goals.Value : null,
            t1Goals, null, t2Goals, null,
            MetricAvailability.Available);
    }

    private static MatchComparisonMetric BuildShotRateMetric(
        IReadOnlyList<PlayerGlobalStatsDto> team1Players,
        IReadOnlyList<PlayerGlobalStatsDto> team2Players)
    {
        // ShotRate = SUM(goals)/SUM(attempts) — never average of percentages
        var t1Goals = team1Players.Sum(p => p.TotalGoals);
        var t1Attempts = team1Players.Sum(p => p.ShotAttempts);
        var t2Goals = team2Players.Sum(p => p.TotalGoals);
        var t2Attempts = team2Players.Sum(p => p.ShotAttempts);

        if (team1Players.Count == 0 && team2Players.Count == 0)
        {
            return new MatchComparisonMetric(
                "SHOT_RATE", "Taux de tir", ComparisonFamily.Attack, MetricDirection.HigherIsBetter,
                null, null, null, null, null, null, MetricAvailability.DataMissing, IsRate: true);
        }

        // ZeroDenominator check for each team
        double? t1Rate = t1Attempts > 0 ? (double)t1Goals / t1Attempts * 100 : null;
        double? t2Rate = t2Attempts > 0 ? (double)t2Goals / t2Attempts * 100 : null;

        var availability = MetricAvailability.Available;
        if (t1Attempts == 0 && t2Attempts == 0)
        {
            availability = MetricAvailability.ZeroDenominator;
        }
        else if (t1Attempts == 0 || t2Attempts == 0)
        {
            availability = MetricAvailability.Partial;
        }

        return new MatchComparisonMetric(
            "SHOT_RATE", "Taux de tir", ComparisonFamily.Attack, MetricDirection.HigherIsBetter,
            t1Rate, t2Rate,
            t1Goals, t1Attempts, t2Goals, t2Attempts,
            availability, IsRate: true);
    }

    private static MatchComparisonMetric BuildSaveRateMetric(
        IReadOnlyList<PlayerGlobalStatsDto> team1Players,
        IReadOnlyList<PlayerGlobalStatsDto> team2Players)
    {
        // SaveRate = Saves / (Saves + GoalsConceded)
        // ShotsFaced = shots that reached the goalkeeper = Saves + goals conceded
        // Do NOT include off-target shots or pre-keeper blocks
        var t1Saves = team1Players.Sum(p => p.SaveCount);
        var t1ShotsFaced = team1Players.Sum(p => p.ShotsFaced);
        var t2Saves = team2Players.Sum(p => p.SaveCount);
        var t2ShotsFaced = team2Players.Sum(p => p.ShotsFaced);

        if (team1Players.Count == 0 && team2Players.Count == 0)
        {
            return new MatchComparisonMetric(
                "SAVE_RATE", "Taux d'arrêt", ComparisonFamily.Defense, MetricDirection.HigherIsBetter,
                null, null, null, null, null, null, MetricAvailability.DataMissing, IsRate: true);
        }

        // ZERO_DENOMINATOR_RENDERED_AS_ZERO_PERCENT=NO
        double? t1Rate = t1ShotsFaced > 0 ? (double)t1Saves / t1ShotsFaced * 100 : null;
        double? t2Rate = t2ShotsFaced > 0 ? (double)t2Saves / t2ShotsFaced * 100 : null;

        var availability = MetricAvailability.Available;
        if (t1ShotsFaced == 0 && t2ShotsFaced == 0)
        {
            availability = MetricAvailability.ZeroDenominator;
        }
        else if (t1ShotsFaced == 0 || t2ShotsFaced == 0)
        {
            availability = MetricAvailability.Partial;
        }

        return new MatchComparisonMetric(
            "SAVE_RATE", "Taux d'arrêt", ComparisonFamily.Defense, MetricDirection.HigherIsBetter,
            t1Rate, t2Rate,
            t1Saves, t1ShotsFaced, t2Saves, t2ShotsFaced,
            availability, IsRate: true);
    }

    private static MatchComparisonMetric BuildSumMetric(
        string code,
        string label,
        ComparisonFamily family,
        MetricDirection direction,
        IReadOnlyList<PlayerGlobalStatsDto> team1Players,
        IReadOnlyList<PlayerGlobalStatsDto> team2Players,
        Func<PlayerGlobalStatsDto, int> valueSelector,
        Func<PlayerGlobalStatsDto, int>? numeratorSelector = null,
        MetricAvailability availability = MetricAvailability.Available)
    {
        if (availability == MetricAvailability.DataMissing)
        {
            return new MatchComparisonMetric(
                code, label, family, direction,
                null, null, null, null, null, null,
                MetricAvailability.DataMissing);
        }

        if (team1Players.Count == 0 && team2Players.Count == 0)
        {
            return new MatchComparisonMetric(
                code, label, family, direction,
                null, null, null, null, null, null,
                MetricAvailability.DataMissing);
        }

        var t1Value = team1Players.Count > 0 ? (double)team1Players.Sum(valueSelector) : (double?)null;
        var t2Value = team2Players.Count > 0 ? (double)team2Players.Sum(valueSelector) : (double?)null;

        var actualAvailability = (t1Value == null || t2Value == null)
            ? MetricAvailability.Partial
            : MetricAvailability.Available;

        return new MatchComparisonMetric(
            code, label, family, direction,
            t1Value, t2Value,
            null, null, null, null,
            actualAvailability);
    }
}
