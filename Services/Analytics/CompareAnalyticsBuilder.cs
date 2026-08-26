using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

// ── Models ─────────────────────────────────────────────────────────────────────

public sealed record ComparePlayerProfile(int PlayerId, AnalyticsPosition Position);

public sealed record CompareMetricValue(
    int PlayerId,
    double? Value,
    QualityTierResult Quality,
    bool IsApplicable);

public sealed record CompareCompatibilityResult(
    PositionComparisonCompatibility Compatibility,
    IReadOnlyList<AnalyticsPosition> Positions,
    bool HasGkVsField,
    IReadOnlyList<string> ComparableMetricCodes);

public sealed record CompareRadarAxisData(
    string Label,
    string Description,
    IReadOnlyList<(int PlayerId, double Value)> PlayerValues,
    bool IsBackendGap,
    bool IsOnRadar);

// ── Builder ────────────────────────────────────────────────────────────────────

public static class CompareAnalyticsBuilder
{
    // ── Group compatibility ──────────────────────────────────────────────────

    /// <summary>
    /// Returns the worst-case pairwise compatibility for a group of players.
    /// IncompatibleGkVsField is returned immediately when any GK vs field pair is found.
    /// </summary>
    public static PositionComparisonCompatibility GetGroupCompatibility(
        IReadOnlyList<AnalyticsPosition> positions)
    {
        if (positions.Count <= 1)
            return PositionComparisonCompatibility.Compatible;

        var worst = PositionComparisonCompatibility.Compatible;
        for (int i = 0; i < positions.Count; i++)
        for (int j = i + 1; j < positions.Count; j++)
        {
            var pair = AnalyticsPositionResolver.CanComparePositions(positions[i], positions[j]);
            if (pair == PositionComparisonCompatibility.IncompatibleGkVsField)
                return PositionComparisonCompatibility.IncompatibleGkVsField;
            if (pair == PositionComparisonCompatibility.CommonMetricsOnly)
                worst = PositionComparisonCompatibility.CommonMetricsOnly;
        }
        return worst;
    }

    public static CompareCompatibilityResult GetCompatibility(
        IReadOnlyList<ComparePlayerProfile> players)
    {
        if (players.Count == 0)
            return new(PositionComparisonCompatibility.Compatible, [], false, []);

        var positions = players.Select(p => p.Position).ToList();
        var compat = GetGroupCompatibility(positions);
        var hasGkVsField = positions.Contains(AnalyticsPosition.GK)
                        && positions.Any(p => p != AnalyticsPosition.GK);
        var codes = GetComparableMetricCodes(positions, compat);
        return new(compat, positions, hasGkVsField, codes);
    }

    // ── Metric intersection ──────────────────────────────────────────────────

    /// <summary>
    /// Returns the metric codes that can be meaningfully compared across all positions.
    /// IncompatibleGkVsField → only All-scope metrics.
    /// Otherwise → intersection of metrics applicable to each position.
    /// </summary>
    public static IReadOnlyList<string> GetComparableMetricCodes(
        IReadOnlyList<AnalyticsPosition> positions,
        PositionComparisonCompatibility compat)
    {
        if (positions.Count == 0) return [];

        if (compat == PositionComparisonCompatibility.IncompatibleGkVsField)
        {
            return AnalyticsV3Catalog.Active
                .Where(m => m.ApplicablePositions == AnalyticsPositionScope.All)
                .Select(m => m.Code)
                .OrderBy(c => c)
                .ToList();
        }

        HashSet<string>? intersection = null;
        foreach (var pos in positions)
        {
            var scope = AnalyticsPositionResolver.ToScope(pos);
            var codes = AnalyticsV3Catalog.Active
                .Where(m => (m.ApplicablePositions & scope) != 0)
                .Select(m => m.Code)
                .ToHashSet();

            if (intersection is null) intersection = codes;
            else intersection.IntersectWith(codes);
        }

        return intersection?.OrderBy(c => c).ToList() ?? [];
    }

    // ── Winner logic ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true when a winner can be highlighted for the set of metric values.
    /// Requires ≥ 2 applicable values all with quality High or Medium (no Low, no NotApplicable).
    /// </summary>
    public static bool CanDeclareWinner(
        IReadOnlyList<CompareMetricValue> values,
        bool higherIsBetter)
    {
        var applicable = values
            .Where(v => v.IsApplicable && v.Value.HasValue)
            .ToList();

        if (applicable.Count < 2) return false;
        return applicable.All(v => v.Quality.Tier is QualityTier.High or QualityTier.Medium);
    }

    /// <summary>
    /// Returns the PlayerId of the winner, or null when tie (within 1e-9) or CanDeclareWinner is false.
    /// HigherIsBetter=false means the player with the LOWER value wins.
    /// </summary>
    public static int? GetWinnerPlayerId(
        IReadOnlyList<CompareMetricValue> values,
        bool higherIsBetter)
    {
        if (!CanDeclareWinner(values, higherIsBetter)) return null;

        var applicable = values
            .Where(v => v.IsApplicable && v.Value.HasValue)
            .ToList();

        var ordered = higherIsBetter
            ? applicable.OrderByDescending(v => v.Value!.Value).ToList()
            : applicable.OrderBy(v => v.Value!.Value).ToList();

        if (Math.Abs(ordered[0].Value!.Value - ordered[1].Value!.Value) < 1e-9)
            return null; // tie

        return ordered[0].PlayerId;
    }

    // ── Radar axes ───────────────────────────────────────────────────────────

    /// <summary>
    /// Builds radar axis data for a Compatible comparison.
    /// perAxisValues: one entry per axis from PositionRadarAxisConfig.GetAxes(position).
    /// Backend-gap axes are included with IsOnRadar=false — caller must exclude them from the chart.
    /// Absolute values only — no normalization by max(players).
    /// </summary>
    public static IReadOnlyList<CompareRadarAxisData> GetPositionRadarAxes(
        AnalyticsPosition position,
        IReadOnlyList<(int PlayerId, double? Value)[]> perAxisValues)
    {
        var configAxes = PositionRadarAxisConfig.GetAxes(position);
        var result = new List<CompareRadarAxisData>();

        for (int i = 0; i < configAxes.Count; i++)
        {
            var axis = configAxes[i];
            var rawValues = i < perAxisValues.Count ? perAxisValues[i] : [];
            var playerValues = rawValues
                .Where(v => v.Value.HasValue)
                .Select(v => (v.PlayerId, v.Value!.Value))
                .ToList();

            result.Add(new CompareRadarAxisData(
                axis.Label,
                axis.Description,
                playerValues,
                axis.IsBackendGap,
                IsOnRadar: !axis.IsBackendGap));
        }

        return result;
    }

    /// <summary>
    /// Returns axis labels that are present AND non-gap in ALL supplied positions.
    /// Used to determine the common radar axes for CommonMetricsOnly comparisons.
    /// </summary>
    // Stable key for ApexChart @key — forces chart reinitialization when the player set identity changes.
    // Using player count alone would produce the same key when swapping players (same count, different ids).
    public static string BuildChartKey(IEnumerable<int> playerIds)
        => string.Join("_", playerIds);

    public static IReadOnlyList<string> GetCommonRadarAxisLabels(
        IReadOnlyList<AnalyticsPosition> positions)
    {
        if (positions.Count == 0) return [];

        HashSet<string>? intersection = null;
        foreach (var pos in positions)
        {
            var labels = PositionRadarAxisConfig.GetAxes(pos)
                .Where(a => !a.IsBackendGap)
                .Select(a => a.Label)
                .ToHashSet(StringComparer.Ordinal);

            if (intersection is null) intersection = labels;
            else intersection.IntersectWith(labels);
        }

        return intersection?.ToList() ?? [];
    }
}
