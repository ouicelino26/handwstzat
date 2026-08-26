using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

// ──────────────────────────────────────────────────────────────────────────────
// Quality tier — maps the backend MetricQuality contract to a display tier
// ──────────────────────────────────────────────────────────────────────────────

public enum QualityTier
{
    High,          // Reliable sample — display normally, eligible for rankings
    Medium,        // Sample reliable but score below threshold — show with light annotation
    Low,           // Sample not reliable — show with warning, exclude from rankings
    NotApplicable  // Denominator = 0, no playing time, or zero sample — display "—"
}

// ──────────────────────────────────────────────────────────────────────────────
// Display context — determines visibility and winner rules
// ──────────────────────────────────────────────────────────────────────────────

public enum AnalyticsDisplayContext
{
    IndividualAnalysis, // Player/team detail page
    Rankings,           // Any sorted list / leaderboard
    Compare,            // Side-by-side player comparison
    ContextSplits       // Contextual breakdown table rows
}

// ──────────────────────────────────────────────────────────────────────────────
// Quality tier result — returned by EvaluateTier
// ──────────────────────────────────────────────────────────────────────────────

public sealed record QualityTierResult(
    QualityTier Tier,
    string? Reason,
    int? SampleCount);

// ──────────────────────────────────────────────────────────────────────────────
// Analytics quality policy — single source of truth for all quality decisions
// ──────────────────────────────────────────────────────────────────────────────

public static class AnalyticsQualityPolicy
{
    // MetricQuality.QualityScore threshold: score >= this → High, below → Medium
    private const double HighScoreThreshold = 0.5;

    // ── Core evaluation ──────────────────────────────────────────────────────

    /// <summary>
    /// Evaluates the quality tier for a metric.
    /// </summary>
    /// <param name="quality">MetricQuality from the API v2 (LeagueMetricQualityDto), or null for v1 metrics.</param>
    /// <param name="sampleCount">Actual sample count (shots, matches, etc.).</param>
    /// <param name="minSample">Required minimum sample for the metric (from AnalyticsV3Catalog).</param>
    /// <param name="denominator">Optional denominator — if 0, returns NotApplicable immediately.</param>
    public static QualityTierResult EvaluateTier(
        LeagueMetricQualityDto? quality,
        int? sampleCount,
        int minSample,
        double? denominator = null)
    {
        if (denominator.HasValue && denominator.Value == 0.0)
            return new(QualityTier.NotApplicable, QualityReasons.ZeroDenominator, sampleCount);

        if (sampleCount.HasValue && sampleCount.Value == 0)
            return new(QualityTier.NotApplicable, null, 0);

        if (quality is not null)
        {
            if (!quality.SampleReliable)
                return new(QualityTier.Low, quality.Reason ?? QualityReasons.BelowMinimumSample, sampleCount);

            return quality.QualityScore >= HighScoreThreshold
                ? new(QualityTier.High, null, sampleCount)
                : new(QualityTier.Medium, quality.Reason, sampleCount);
        }

        // Fallback when no MetricQuality from API v2
        if (sampleCount.HasValue && sampleCount.Value < minSample)
            return new(QualityTier.Low, QualityReasons.BelowMinimumSample, sampleCount);

        return new(QualityTier.High, null, sampleCount);
    }

    /// <summary>
    /// Evaluates playing-time quality for /60 metrics.
    /// PlayingTimeMinutes &lt;= 0 → NotApplicable (never treated as 0 production).
    /// </summary>
    public static QualityTierResult EvaluatePlayingTimeTier(
        double playingTimeMinutes,
        double minimumPlayingTimeMinutes)
    {
        if (playingTimeMinutes <= 0)
            return new(QualityTier.NotApplicable, QualityReasons.ZeroDenominator, null);

        if (playingTimeMinutes < minimumPlayingTimeMinutes)
            return new(QualityTier.Low, QualityReasons.BelowMinimumSample, null);

        return new(QualityTier.High, null, null);
    }

    // ── Display context rules ─────────────────────────────────────────────────

    /// <summary>
    /// Returns true when the metric value should be visible in the given context.
    /// NotApplicable is always rendered as "—" regardless of context.
    /// </summary>
    public static bool ShouldShow(QualityTierResult result, AnalyticsDisplayContext context)
    {
        if (result.Tier == QualityTier.NotApplicable) return false;

        return context switch
        {
            // Rankings: exclude LOW — prevents misleading leaderboard positions
            AnalyticsDisplayContext.Rankings => result.Tier is QualityTier.High or QualityTier.Medium,

            // Individual analysis: always show (with quality annotation)
            AnalyticsDisplayContext.IndividualAnalysis => true,

            // Compare: show both sides, but CanDeclareWinner governs winner highlight
            AnalyticsDisplayContext.Compare => true,

            // Context splits: show even LOW rows; caller decides whether to grey them out
            AnalyticsDisplayContext.ContextSplits => true,

            _ => true
        };
    }

    /// <summary>
    /// Evaluates quality for the Assist:Turnover ratio (CAT-05).
    /// Both assists &gt;= 5 and turnovers &gt;= 3 are required for a reliable signal.
    /// </summary>
    public static QualityTierResult EvaluateAssistTurnoverQuality(int assists, int turnovers)
    {
        if (turnovers == 0) return new(QualityTier.NotApplicable, QualityReasons.ZeroDenominator, 0);
        if (assists < 5 || turnovers < 3)
            return new(QualityTier.Low, QualityReasons.BelowMinimumSample, Math.Min(assists, turnovers));
        return new(QualityTier.High, null, Math.Min(assists, turnovers));
    }

    /// <summary>
    /// Returns true when a winner may be visually highlighted in a comparison.
    /// If either player's metric is LOW or NotApplicable, no winner is declared.
    /// </summary>
    public static bool CanDeclareWinner(QualityTierResult resultA, QualityTierResult resultB) =>
        resultA.Tier is QualityTier.High or QualityTier.Medium &&
        resultB.Tier is QualityTier.High or QualityTier.Medium;

    /// <summary>
    /// Returns true when a metric should be excluded from a ranking list.
    /// </summary>
    public static bool ShouldExcludeFromRanking(QualityTierResult result) =>
        result.Tier is QualityTier.Low or QualityTier.NotApplicable;

    // ── User-facing messages ─────────────────────────────────────────────────

    /// <summary>
    /// Maps an API Reason string to a user-readable tooltip message.
    /// Single source — no inline string duplication in Razor components.
    /// </summary>
    public static string ReasonToUserMessage(string? reason) => reason switch
    {
        QualityReasons.BelowMinimumSample  => "Échantillon insuffisant — valeur indicative uniquement.",
        QualityReasons.ZeroDenominator     => "Données insuffisantes pour calculer cette métrique.",
        QualityReasons.InvalidNumerator    => "Valeur de numérateur invalide.",
        _                                  => "Qualité de données limitée."
    };
}

// ──────────────────────────────────────────────────────────────────────────────
// Well-known reason strings — mirrors backend MetricQuality.Reason values
// ──────────────────────────────────────────────────────────────────────────────

public static class QualityReasons
{
    public const string BelowMinimumSample  = "BELOW_MINIMUM_SAMPLE";
    public const string ZeroDenominator     = "ZERO_OR_INVALID_DENOMINATOR";
    public const string InvalidNumerator    = "INVALID_NUMERATOR";
}
