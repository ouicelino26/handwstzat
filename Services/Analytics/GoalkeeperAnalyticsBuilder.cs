namespace HandWStat.Services.Analytics;

using HandWStat.Models.Analytics;

// ──────────────────────────────────────────────────────────────────────────────
// GoalkeeperSaveViewModel — canonical GK analytics model
// Single source for Players / Compare / PositionProfiles / Teams.
// All rates and per-60 values carry resolved quality tiers.
// ──────────────────────────────────────────────────────────────────────────────

public sealed record GoalkeeperSaveViewModel
{
    // ── Counts ───────────────────────────────────────────────────────────────
    public int TotalSaves { get; init; }
    public int TotalShotsFaced { get; init; }
    public int TotalGoalsConceded { get; init; }
    public int OpenPlaySaves { get; init; }
    public int OpenPlayShotsFaced { get; init; }
    public int PenaltySaves { get; init; }
    public int PenaltyShotsFaced { get; init; }

    // ── Rates (null = N/A) ────────────────────────────────────────────────────
    public double? TotalSaveRate { get; init; }
    public double? OpenPlaySaveRate { get; init; }
    public double? PenaltySaveRate { get; init; }

    // ── Per-60 ────────────────────────────────────────────────────────────────
    public double? SavesPer60 { get; init; }
    public double? ShotsFacedPer60 { get; init; }
    public double? GoalsConcededPer60 { get; init; }

    // ── Context ───────────────────────────────────────────────────────────────
    public double PlayingTimeMinutes { get; init; }
    public int MatchesPlayed { get; init; }

    // ── Quality tiers ─────────────────────────────────────────────────────────
    public QualityTierResult TotalSaveRateQuality    { get; init; } = new(QualityTier.NotApplicable, null, null);
    public QualityTierResult OpenPlaySaveRateQuality { get; init; } = new(QualityTier.NotApplicable, null, null);
    public QualityTierResult PenaltySaveRateQuality  { get; init; } = new(QualityTier.NotApplicable, null, null);
    public QualityTierResult Per60Quality            { get; init; } = new(QualityTier.NotApplicable, null, null);

    // ── Data consistency ──────────────────────────────────────────────────────
    public bool HasDataInconsistency { get; init; }
    public string? DataInconsistencyReason { get; init; }
}

// ──────────────────────────────────────────────────────────────────────────────
// GoalkeeperAnalyticsBuilder — constructs GoalkeeperSaveViewModel from raw inputs
//
// Source priority for rates:
//   1. API value (apiTotalSaveRate, etc.)   — preferred when v2 API available
//   2. Computed formula (ComputeTotalSaveRate, etc.)
//   3. Legacy DTO value (legacyTotalSaveRate, etc.)
//   → null if no source available
//
// Team aggregation: sum(saves) / sum(shotsFaced), NOT average(individual rates)
// ──────────────────────────────────────────────────────────────────────────────

public static class GoalkeeperAnalyticsBuilder
{
    public static GoalkeeperSaveViewModel Build(
        int totalSaves,
        int totalShotsFaced,
        int openPlaySaves,
        int openPlayShotsFaced,
        int penaltySaves,
        int penaltyShotsFaced,
        int goalsConceded,
        double playingTimeMinutes,
        int matchesPlayed,
        double? apiTotalSaveRate = null,
        double? apiOpenPlaySaveRate = null,
        double? apiPenaltySaveRate = null,
        double? apiSavesPer60 = null,
        double? legacyTotalSaveRate = null,
        double? legacyPenaltySaveRate = null)
    {
        var inconsistent = totalSaves > totalShotsFaced;

        var totalSaveRate = apiTotalSaveRate
            ?? AnalyticsCalculationService.ComputeTotalSaveRate(totalSaves, totalShotsFaced)
            ?? legacyTotalSaveRate;

        var openPlaySaveRate = apiOpenPlaySaveRate
            ?? AnalyticsCalculationService.ComputeOpenPlaySaveRate(openPlaySaves, openPlayShotsFaced);

        var penaltySaveRate = apiPenaltySaveRate
            ?? AnalyticsCalculationService.ComputePenaltySaveRate(penaltySaves, penaltyShotsFaced)
            ?? legacyPenaltySaveRate;

        var savesPer60 = apiSavesPer60.HasValue
            ? AnalyticsCalculationService.NormalizeApiPer60(apiSavesPer60.Value, playingTimeMinutes)
            : null;
        var shotsFacedPer60     = AnalyticsCalculationService.ComputeShotsFacedPer60(totalShotsFaced, playingTimeMinutes);
        var goalsConcededPer60  = AnalyticsCalculationService.ComputeGoalsConcededPer60(goalsConceded, playingTimeMinutes);

        var totalMinSample  = AnalyticsV3Catalog.Get("CAT-21")?.MinimumSampleCount ?? 20;
        var openMinSample   = AnalyticsV3Catalog.Get("CAT-13")?.MinimumSampleCount ?? 20;
        var penMinSample    = AnalyticsV3Catalog.Get("CAT-14")?.MinimumSampleCount ?? 5;
        var minPlayingTime  = AnalyticsV3Catalog.Get("CAT-22")?.MinimumPlayingTimeMinutes ?? 150.0;

        return new GoalkeeperSaveViewModel
        {
            TotalSaves              = totalSaves,
            TotalShotsFaced         = totalShotsFaced,
            TotalGoalsConceded      = goalsConceded,
            OpenPlaySaves           = openPlaySaves,
            OpenPlayShotsFaced      = openPlayShotsFaced,
            PenaltySaves            = penaltySaves,
            PenaltyShotsFaced       = penaltyShotsFaced,
            TotalSaveRate           = totalSaveRate,
            OpenPlaySaveRate        = openPlaySaveRate,
            PenaltySaveRate         = penaltySaveRate,
            SavesPer60              = savesPer60,
            ShotsFacedPer60         = shotsFacedPer60,
            GoalsConcededPer60      = goalsConcededPer60,
            PlayingTimeMinutes      = playingTimeMinutes,
            MatchesPlayed           = matchesPlayed,
            TotalSaveRateQuality    = AnalyticsQualityPolicy.EvaluateTier(null, totalShotsFaced, totalMinSample),
            OpenPlaySaveRateQuality = AnalyticsQualityPolicy.EvaluateTier(null, openPlayShotsFaced, openMinSample),
            PenaltySaveRateQuality  = AnalyticsQualityPolicy.EvaluateTier(null, penaltyShotsFaced, penMinSample),
            Per60Quality            = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(playingTimeMinutes, minPlayingTime),
            HasDataInconsistency    = inconsistent,
            DataInconsistencyReason = inconsistent
                ? $"Arrêts ({totalSaves}) > Tirs subis ({totalShotsFaced}) — données API incohérentes"
                : null,
        };
    }

    /// <summary>
    /// Aggregates multiple GK save rates using weighted sum — NOT average of individual percentages.
    /// sum(saves) / sum(shotsFaced) × 100. Returns null if total shots faced is zero.
    /// </summary>
    public static double? AggregateTeamSaveRate(IEnumerable<(int Saves, int ShotsFaced)> goalkeepers)
    {
        int totalSaves = 0, totalFaced = 0;
        foreach (var (s, f) in goalkeepers)
        {
            totalSaves += s;
            totalFaced += f;
        }
        return AnalyticsCalculationService.ComputeTotalSaveRate(totalSaves, totalFaced);
    }
}
