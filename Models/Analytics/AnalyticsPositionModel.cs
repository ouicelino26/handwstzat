namespace HandWStat.Models.Analytics;

// ─────────────────────────────────────────────────────────────────────────────
// Canonical position enum — single source of truth for position identity
// ─────────────────────────────────────────────────────────────────────────────

public enum AnalyticsPosition
{
    Unknown = 0,
    GK  = 1,  // Gardienne de but
    AIL = 2,  // Ailière
    AR  = 3,  // Arrière
    DC  = 4,  // Demi-centre
    PIV = 5,  // Pivot
}

// ─────────────────────────────────────────────────────────────────────────────
// Comparison compatibility result
// ─────────────────────────────────────────────────────────────────────────────

public enum PositionComparisonCompatibility
{
    Compatible,             // Same position (or same lateral family)
    CommonMetricsOnly,      // Different field positions — only shared metrics comparable
    IncompatibleGkVsField,  // GK vs any field player — position-specific metrics incompatible
}

// ─────────────────────────────────────────────────────────────────────────────
// Position resolver — normalises raw codes / names to AnalyticsPosition
// ─────────────────────────────────────────────────────────────────────────────

public static class AnalyticsPositionResolver
{
    /// <summary>
    /// Returns the canonical AnalyticsPosition for a player.
    /// isGoalkeeper is authoritative when true (no code parsing needed).
    /// Falls back to positionName text matching when positionCode is unrecognised.
    /// </summary>
    public static AnalyticsPosition Resolve(
        string? positionCode,
        string? positionName = null,
        bool isGoalkeeper = false)
    {
        if (isGoalkeeper) return AnalyticsPosition.GK;

        var pos = ParseCode((positionCode ?? string.Empty).Trim().ToUpperInvariant());
        if (pos != AnalyticsPosition.Unknown) return pos;

        if (!string.IsNullOrEmpty(positionName))
        {
            var n = positionName.Trim().ToLowerInvariant();
            if (n.Contains("gardien") || n.Contains("goal") || n.Contains("keeper"))
                return AnalyticsPosition.GK;
            if (n.Contains("aili") || n.Contains("ailier"))
                return AnalyticsPosition.AIL;
            if (n.Contains("arrière") || n.Contains("arriere"))
                return AnalyticsPosition.AR;
            if (n.Contains("demi") || n.Contains("centre") || n.Contains("center"))
                return AnalyticsPosition.DC;
            if (n.Contains("pivot"))
                return AnalyticsPosition.PIV;
        }

        return AnalyticsPosition.Unknown;
    }

    // Maps raw uppercase code string to AnalyticsPosition.
    // All known aliases from API codes, UI chip/card variants, and mock data are listed here.
    private static AnalyticsPosition ParseCode(string upper) => upper switch
    {
        "GK" or "GB" or "G" or "GOAL" or "GARDIENNE" or "GARDEN" or "GOALKEEPER"
            => AnalyticsPosition.GK,

        "AIL" or "AL" or "A"
            or "AILG" or "AILD" or "AIL-G" or "AIL-D"
            or "ALG"  or "ALD"
            or "AILIERE" or "AILIÈRE"
            => AnalyticsPosition.AIL,

        "AR" or "ARG" or "ARD" or "ARR" or "ARRIERE" or "ARRIÈRE"
            => AnalyticsPosition.AR,

        "DC" or "DCE" or "DEMI" or "DEMI-CENTRE" or "DEMI-CENTER"
            => AnalyticsPosition.DC,

        "PIV" or "P" or "PIVOT"
            => AnalyticsPosition.PIV,

        _ => AnalyticsPosition.Unknown,
    };

    /// <summary>
    /// Returns the PositionComparisonCompatibility for two players being compared.
    /// GK vs any field position is incompatible for position-specific metrics.
    /// Different field positions share only common metrics.
    /// Lateral variants (AIL-G vs AIL-D, AR-G vs AR-D) resolve to the same
    /// AnalyticsPosition and are therefore Compatible.
    /// </summary>
    public static PositionComparisonCompatibility CanComparePositions(
        AnalyticsPosition a,
        AnalyticsPosition b)
    {
        if (a == b) return PositionComparisonCompatibility.Compatible;

        if (a == AnalyticsPosition.GK || b == AnalyticsPosition.GK)
            return PositionComparisonCompatibility.IncompatibleGkVsField;

        return PositionComparisonCompatibility.CommonMetricsOnly;
    }

    /// <summary>
    /// Bridges AnalyticsPosition to the flags-based AnalyticsPositionScope used in metric applicability checks.
    /// Unknown → None (no scope match, no metrics shown as applicable).
    /// </summary>
    public static AnalyticsPositionScope ToScope(AnalyticsPosition position) => position switch
    {
        AnalyticsPosition.GK  => AnalyticsPositionScope.GK,
        AnalyticsPosition.AIL => AnalyticsPositionScope.AIL,
        AnalyticsPosition.AR  => AnalyticsPositionScope.AR,
        AnalyticsPosition.DC  => AnalyticsPositionScope.DC,
        AnalyticsPosition.PIV => AnalyticsPositionScope.PIV,
        _                     => AnalyticsPositionScope.None,
    };
}

// ─────────────────────────────────────────────────────────────────────────────
// Per-position metric priority profile
// Primary metrics are the most informative indicators for the position.
// Secondary metrics provide useful context but are not the defining lens.
// ─────────────────────────────────────────────────────────────────────────────

public static class PositionMetricProfile
{
    public static IReadOnlyList<string> GetPrimaryMetrics(AnalyticsPosition position) => position switch
    {
        AnalyticsPosition.GK  => ["CAT-13", "CAT-14", "CAT-15", "CAT-16"],
        AnalyticsPosition.AIL => ["CAT-04", "CAT-01", "CAT-08"],
        AnalyticsPosition.AR  => ["CAT-01", "CAT-04", "CAT-06"],
        AnalyticsPosition.DC  => ["CAT-05", "CAT-08", "CAT-09", "CAT-01"],
        AnalyticsPosition.PIV => ["CAT-04", "CAT-06", "CAT-07", "CAT-10"],
        _                     => [],
    };

    public static IReadOnlyList<string> GetSecondaryMetrics(AnalyticsPosition position) => position switch
    {
        AnalyticsPosition.GK  => [],
        AnalyticsPosition.AIL => ["CAT-06", "CAT-09"],
        AnalyticsPosition.AR  => ["CAT-05", "CAT-08"],
        AnalyticsPosition.DC  => ["CAT-10", "CAT-07"],
        AnalyticsPosition.PIV => ["CAT-09"],
        _                     => [],
    };
}

// ─────────────────────────────────────────────────────────────────────────────
// Radar axis configuration per position (5 axes maximum)
// These define the intended axes for PositionProfiles radar / benchmark tables.
// IsBackendGap = true: no API percentile available yet — display raw value only.
// ─────────────────────────────────────────────────────────────────────────────

public sealed record PositionRadarAxis(
    string Label,
    string Description,
    bool IsBackendGap = false);

public static class PositionRadarAxisConfig
{
    public static IReadOnlyList<PositionRadarAxis> GetAxes(AnalyticsPosition position) => position switch
    {
        AnalyticsPosition.GK => [
            new("Arrêts",       "Volume d'arrêts (CAT-15 /60)"),
            new("Jeu ouvert",   "Taux d'arrêt en jeu ouvert (CAT-13)"),
            new("7 mètres",     "Taux d'arrêt sur penalties (CAT-14)"),
            new("Arrêts /60",   "Arrêts normalisés à 60 min (CAT-15)"),
            new("Charge subie", "Tirs subis /60 (CAT-16)"),
        ],
        AnalyticsPosition.AIL => [
            new("Finition",        "Réussite jeu ouvert (CAT-04)"),
            new("Production",      "Buts créés /60 (CAT-01)"),
            new("Volume offensif", "Actions offensives /60", IsBackendGap: true),
            new("Maîtrise",        "Pertes /60 (CAT-08)"),
            new("7m obtenus",      "7m obtenus /match (CAT-06)"),
        ],
        AnalyticsPosition.AR => [
            new("Production", "Buts créés /60 (CAT-01)"),
            new("Finition",   "Réussite jeu ouvert (CAT-04)"),
            new("7m obtenus", "7m obtenus /match (CAT-06)"),
            new("Création",   "Passes décisives /60", IsBackendGap: true),
            new("Maîtrise",   "Pertes /60 (CAT-08)"),
        ],
        AnalyticsPosition.DC => [
            new("Création",   "Passes décisives /60", IsBackendGap: true),
            new("A:T",        "Ratio Assists/Turnovers (CAT-05)"),
            new("Maîtrise",   "Pertes /60 (CAT-08)"),
            new("Défense",    "Interceptions /60 (CAT-09)"),
            new("Production", "Buts créés /60 (CAT-01)"),
        ],
        AnalyticsPosition.PIV => [
            new("Finition",          "Réussite jeu ouvert (CAT-04)"),
            new("7m obtenus",        "7m obtenus /match (CAT-06)"),
            new("Passages en force", "PF provoqués /match (CAT-07)"),
            new("Impact défensif",   "Impact défensif /60 (CAT-10)"),
            new("Production",        "Buts créés /60 (CAT-01)"),
        ],
        _ => [],
    };

    /// <summary>
    /// Returns true if any of this position's radar axes are missing an API percentile.
    /// Caller should display a POSITION_PROFILE_BACKEND_GAP notice.
    /// </summary>
    public static bool HasBackendGaps(AnalyticsPosition position) =>
        GetAxes(position).Any(a => a.IsBackendGap);
}
