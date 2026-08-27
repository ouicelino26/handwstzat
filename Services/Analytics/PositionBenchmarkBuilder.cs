using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

// ── Cohort identity ───────────────────────────────────────────────────────────

/// <summary>
/// Identifies the reference population for a position benchmark.
/// Ensures all metrics in a benchmark session use the same scope.
/// </summary>
public sealed record PositionBenchmarkCohort(
    AnalyticsPosition Position,
    int? CompetitionId,
    string? Season,
    string? Day);

// ── Histogram output ──────────────────────────────────────────────────────────

public sealed record PositionBenchmarkHistogramBin(
    double RangeLow,
    double RangeHigh,
    int Count);

// ── Builder ───────────────────────────────────────────────────────────────────

// TEST_REFERENCE: kept for parity tests. Production code (PositionProfiles.razor.cs B6.10)
// now calls GetPlayerBenchmarkAsync to source Percentile and Median from the backend.
// ComputePercentile / ComputeMedian / BuildHistogram remain authoritative for unit tests
// validating formula equivalence with the backend (see AnalyticsB6Tests.cs B6.27).
public static class PositionBenchmarkBuilder
{
    public const int MinRadarAxes = 3;
    public const int DefaultMinCohortSize = 5;

    // ── Percentile ───────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the mid-rank performance percentile of targetValue within cohortValues.
    /// higherIsBetter=true : higher values score higher (goals_per60, save_rate…).
    /// higherIsBetter=false: lower values score higher (turnovers_per60, goals_conceded_per60…).
    /// Formula: (count_below + 0.5 × count_equal) / n × 100
    /// Returns null when the cohort is empty.
    /// </summary>
    public static double? ComputePercentile(
        IReadOnlyList<double> cohortValues,
        double targetValue,
        bool higherIsBetter)
    {
        if (cohortValues.Count == 0) return null;

        var n = cohortValues.Count;
        int below = higherIsBetter
            ? cohortValues.Count(v => v < targetValue - 1e-9)
            : cohortValues.Count(v => v > targetValue + 1e-9);
        int equal = cohortValues.Count(v => Math.Abs(v - targetValue) < 1e-9);

        return (below + 0.5 * equal) / n * 100.0;
    }

    // ── Median ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the median of a collection, ignoring null values.
    /// Odd count  → middle value.
    /// Even count → average of the two middle values.
    /// Returns null when no non-null values are present.
    /// </summary>
    public static double? ComputeMedian(IReadOnlyList<double?> values)
    {
        var sorted = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .OrderBy(v => v)
            .ToList();

        if (sorted.Count == 0) return null;
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }

    // ── Histogram ────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a histogram with exactly binCount equal-width bins.
    /// Invariant: sum(bins.Count) == count of non-null values in the input.
    /// Supports negative values and single-value collections.
    /// Returns empty when values is empty or binCount is 0.
    /// Null values are excluded from the distribution.
    /// </summary>
    public static IReadOnlyList<PositionBenchmarkHistogramBin> BuildHistogram(
        IReadOnlyList<double?> values,
        int binCount)
    {
        if (binCount <= 0) return [];

        var valid = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (valid.Count == 0) return [];

        var min = valid.Min();
        var max = valid.Max();

        // All values identical → single bin
        if (Math.Abs(max - min) < 1e-12)
        {
            return [new PositionBenchmarkHistogramBin(min, max, valid.Count)];
        }

        var width = (max - min) / binCount;
        var counts = new int[binCount];

        foreach (var v in valid)
        {
            var idx = (int)((v - min) / width);
            idx = Math.Clamp(idx, 0, binCount - 1);
            counts[idx]++;
        }

        return Enumerable.Range(0, binCount)
            .Select(i => new PositionBenchmarkHistogramBin(
                min + i * width,
                min + (i + 1) * width,
                counts[i]))
            .ToList();
    }

    // ── Cohort quality ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns a quality tier based on cohort size.
    /// CohortSize = 0 -&gt; NotApplicable (no data).
    /// CohortSize &lt; minSize -&gt; Low (unreliable benchmark).
    /// CohortSize &gt;= minSize -&gt; High.
    /// </summary>
    public static QualityTierResult GetCohortQuality(
        int cohortSize,
        int minSize = DefaultMinCohortSize)
    {
        if (cohortSize <= 0)
            return new QualityTierResult(QualityTier.NotApplicable, null, null);

        if (cohortSize < minSize)
            return new QualityTierResult(QualityTier.Low, null, cohortSize);

        return new QualityTierResult(QualityTier.High, null, cohortSize);
    }

    // ── Radar axes ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the non-gap radar axes for a position.
    /// Backend-gap axes (IsBackendGap=true) are excluded — they cannot form a valid percentile axis.
    /// </summary>
    public static IReadOnlyList<PositionRadarAxis> GetNonGapRadarAxes(AnalyticsPosition position)
        => PositionRadarAxisConfig.GetAxes(position)
            .Where(a => !a.IsBackendGap)
            .ToList();

    /// <summary>
    /// Returns true when the axis label is a backend gap for the given position.
    /// Used to filter axes from the position profile radar.
    /// </summary>
    public static bool IsBackendGapAxis(AnalyticsPosition position, string axisLabel)
        => PositionRadarAxisConfig.GetAxes(position)
            .Any(a => a.IsBackendGap
                   && string.Equals(a.Label, axisLabel, StringComparison.Ordinal));

    /// <summary>
    /// Returns true when at least MinRadarAxes non-gap axes have data available.
    /// If false, the radar chart should not be rendered and an unavailable notice shown instead.
    /// </summary>
    public static bool IsRadarAvailable(
        AnalyticsPosition position,
        IReadOnlyCollection<string> availableAxisLabels)
    {
        var nonGap = GetNonGapRadarAxes(position);
        var validCount = nonGap.Count(
            a => availableAxisLabels.Contains(a.Label, StringComparer.Ordinal));
        return validCount >= MinRadarAxes;
    }
}
