namespace HandWStat.Models.Analytics;

using HandWStat.Models.Contracts;

// ──────────────────────────────────────────────────────────────────────────────
// Contextual analytics — dimension model and helpers for CAT-19
// ──────────────────────────────────────────────────────────────────────────────

public enum ContextDimension
{
    ScoreState,
    AttackSituation,
    DefenseStructure,
    AttackSystem,
}

public static class ContextAnalyticsHelper
{
    // ── Position-aware default dimension ─────────────────────────────────────

    public static ContextDimension GetDefaultDimension(AnalyticsPosition position) => position switch
    {
        AnalyticsPosition.DC  => ContextDimension.AttackSystem,
        AnalyticsPosition.AR  => ContextDimension.DefenseStructure,
        AnalyticsPosition.PIV => ContextDimension.DefenseStructure,
        AnalyticsPosition.AIL => ContextDimension.AttackSituation,
        _                     => ContextDimension.ScoreState,
    };

    // ── Dimension metadata ────────────────────────────────────────────────────

    public static string GetDimensionLabel(ContextDimension dimension) => dimension switch
    {
        ContextDimension.ScoreState       => "État du score",
        ContextDimension.AttackSituation  => "Type d'attaque",
        ContextDimension.DefenseStructure => "Défense adverse",
        ContextDimension.AttackSystem     => "Système offensif",
        _                                 => dimension.ToString(),
    };

    // ── Row retrieval ─────────────────────────────────────────────────────────

    public static IReadOnlyList<EventContextSplitDto> GetRows(
        EventContextBreakdownDto breakdown,
        ContextDimension dimension) => dimension switch
    {
        ContextDimension.ScoreState       => breakdown.ScoreStates,
        ContextDimension.AttackSituation  => breakdown.AttackSituations,
        ContextDimension.DefenseStructure => breakdown.DefenseStructures,
        ContextDimension.AttackSystem     => breakdown.AttackSystems,
        _                                 => [],
    };

    public static IReadOnlyList<ContextDimension> GetAvailableDimensions(EventContextBreakdownDto breakdown) =>
        Enum.GetValues<ContextDimension>()
            .Where(d => GetRows(breakdown, d).Any(r => r.Events > 0))
            .ToList();

    // ── FR label for context codes ────────────────────────────────────────────
    // Uses ContextLabel from API when present; maps known English codes as fallback.

    public static string GetContextFrLabel(string? contextCode, string? contextLabel)
    {
        if (!string.IsNullOrWhiteSpace(contextLabel)) return contextLabel;
        return contextCode?.ToUpperInvariant() switch
        {
            "WINNING" or "AHEAD"   => "En avance",
            "TIED"    or "EQUAL"   => "Égalité",
            "TRAILING" or "BEHIND" => "En retard",
            _ => contextCode ?? "—",
        };
    }

    // ── Sample count (denominator for quality evaluation) ────────────────────
    // For GK: total shots faced. For field: ShotAttempts (denominator of ShotSuccessRate).

    public static int GetSampleCount(EventContextSplitDto row, bool isGoalkeeper)
    {
        if (isGoalkeeper) return GetGkShotsFaced(row);
        return row.ShotAttempts > 0 ? row.ShotAttempts : row.Events;
    }

    // ── GK shots faced (computable from the four GK count fields) ────────────
    // TirsSubis = GoalkeeperSaves + GoalkeeperPenaltySaves
    //           + GoalkeeperConcededGoals + GoalkeeperPenaltyConcededGoals

    public static int GetGkShotsFaced(EventContextSplitDto row) =>
        row.GoalkeeperSaves + row.GoalkeeperPenaltySaves
        + row.GoalkeeperConcededGoals + row.GoalkeeperPenaltyConcededGoals;

    // ── Scope options builder (testable extract of AnalyseTabPanel logic) ─────
    // PlayerId is mandatory — context results without a playerId scope are league-wide,
    // which must never appear in a player sheet.

    public static StatsQueryOptionsDto BuildContextOptions(
        int playerId,
        int? competitionId = null,
        int? teamId = null,
        string? season = null,
        string? day = null) =>
        new()
        {
            PlayerId      = playerId,
            CompetitionId = competitionId,
            TeamId        = teamId,
            Season        = season,
            Day           = day,
        };
}
