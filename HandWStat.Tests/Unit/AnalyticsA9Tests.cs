using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

// Phase A9 — PositionBenchmarkBuilder unit tests
// TEST_BASELINE_A9 = 1011
// Spec references: sections 40-47

public class AnalyticsA9Tests
{
    // ── ComputePercentile ─────────────────────────────────────────────────────

    [Fact]
    public void ComputePercentile_EmptyCohort_ReturnsNull()
    {
        var result = PositionBenchmarkBuilder.ComputePercentile([], 5.0, higherIsBetter: true);
        Assert.Null(result);
    }

    [Fact]
    public void ComputePercentile_HigherIsBetter_BottomValue_ReturnsNearZero()
    {
        // target = min value → 0 below, 1 equal → (0 + 0.5) / 5 * 100 = 10
        var cohort = new double[] { 1, 2, 3, 4, 5 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 1.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(10.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_HigherIsBetter_TopValue_ReturnsNear90()
    {
        // target = max value → 4 below, 1 equal → (4 + 0.5) / 5 * 100 = 90
        var cohort = new double[] { 1, 2, 3, 4, 5 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 5.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(90.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_HigherIsBetter_MiddleValue_Returns50()
    {
        // cohort of 1 element identical to target → (0 + 0.5) / 1 * 100 = 50
        var cohort = new double[] { 10.0 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 10.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(50.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_LowerIsBetter_LowestValue_ReturnsNear90()
    {
        // lower = better: target=1 (lowest) → below = count(v > 1) = 4, equal = 1
        // (4 + 0.5) / 5 * 100 = 90
        var cohort = new double[] { 1, 2, 3, 4, 5 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 1.0, higherIsBetter: false);
        Assert.NotNull(result);
        Assert.Equal(90.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_LowerIsBetter_HighestValue_ReturnsNear10()
    {
        // target=5 (highest) → below = count(v > 5) = 0, equal = 1
        // (0 + 0.5) / 5 * 100 = 10
        var cohort = new double[] { 1, 2, 3, 4, 5 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 5.0, higherIsBetter: false);
        Assert.NotNull(result);
        Assert.Equal(10.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_Ties_UsesHalfEqualFormula()
    {
        // cohort = [5, 5, 5], target = 5 → below=0, equal=3, n=3
        // (0 + 0.5*3) / 3 * 100 = 50
        var cohort = new double[] { 5, 5, 5 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 5.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(50.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_TargetAboveCohort_Returns100()
    {
        // all values below target → (n + 0) / n * 100 = 100
        var cohort = new double[] { 1, 2, 3 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 99.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(100.0, result!.Value, precision: 6);
    }

    [Fact]
    public void ComputePercentile_TargetBelowCohort_Returns0()
    {
        // no values below target → (0 + 0) / n * 100 = 0
        var cohort = new double[] { 10, 20, 30 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 1.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(0.0, result!.Value, precision: 6);
    }

    // ── ComputeMedian ─────────────────────────────────────────────────────────

    [Fact]
    public void ComputeMedian_EmptyList_ReturnsNull()
    {
        var result = PositionBenchmarkBuilder.ComputeMedian([]);
        Assert.Null(result);
    }

    [Fact]
    public void ComputeMedian_AllNulls_ReturnsNull()
    {
        var result = PositionBenchmarkBuilder.ComputeMedian([null, null, null]);
        Assert.Null(result);
    }

    [Fact]
    public void ComputeMedian_OddCount_ReturnsMiddleValue()
    {
        var result = PositionBenchmarkBuilder.ComputeMedian([1.0, 3.0, 5.0]);
        Assert.Equal(3.0, result!.Value, precision: 9);
    }

    [Fact]
    public void ComputeMedian_EvenCount_ReturnsAverageOfMiddleTwo()
    {
        var result = PositionBenchmarkBuilder.ComputeMedian([1.0, 2.0, 3.0, 4.0]);
        Assert.Equal(2.5, result!.Value, precision: 9);
    }

    [Fact]
    public void ComputeMedian_MixedNulls_IgnoresNulls()
    {
        // non-null: [2, 4] → median = 3
        var result = PositionBenchmarkBuilder.ComputeMedian([null, 2.0, null, 4.0, null]);
        Assert.Equal(3.0, result!.Value, precision: 9);
    }

    [Fact]
    public void ComputeMedian_SingleValue_ReturnsThatValue()
    {
        var result = PositionBenchmarkBuilder.ComputeMedian([7.5]);
        Assert.Equal(7.5, result!.Value, precision: 9);
    }

    // ── BuildHistogram ────────────────────────────────────────────────────────

    [Fact]
    public void BuildHistogram_BinCountZero_ReturnsEmpty()
    {
        var result = PositionBenchmarkBuilder.BuildHistogram([1.0, 2.0, 3.0], binCount: 0);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildHistogram_EmptyValues_ReturnsEmpty()
    {
        var result = PositionBenchmarkBuilder.BuildHistogram([], binCount: 5);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildHistogram_AllNullValues_ReturnsEmpty()
    {
        var result = PositionBenchmarkBuilder.BuildHistogram([null, null], binCount: 5);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildHistogram_NormalCase_CountInvariantHolds()
    {
        // sum of all bin counts must equal count of non-null values
        var values = Enumerable.Range(0, 20).Select(i => (double?)((double)i)).ToList();
        var result = PositionBenchmarkBuilder.BuildHistogram(values, binCount: 5);
        Assert.Equal(5, result.Count);
        Assert.Equal(20, result.Sum(b => b.Count));
    }

    [Fact]
    public void BuildHistogram_AllValuesSame_SingleBinContainsAll()
    {
        var values = new double?[] { 3.0, 3.0, 3.0 };
        var result = PositionBenchmarkBuilder.BuildHistogram(values, binCount: 5);
        Assert.Single(result);
        Assert.Equal(3, result[0].Count);
    }

    [Fact]
    public void BuildHistogram_MixedNulls_NullsExcludedFromCount()
    {
        var values = new double?[] { 1.0, null, 2.0, null, 3.0 };
        var result = PositionBenchmarkBuilder.BuildHistogram(values, binCount: 3);
        Assert.Equal(3, result.Sum(b => b.Count));
    }

    [Fact]
    public void BuildHistogram_BinCount_ExactlyBinCountBinsReturned()
    {
        var values = Enumerable.Range(1, 10).Select(i => (double?)((double)i)).ToList();
        var result = PositionBenchmarkBuilder.BuildHistogram(values, binCount: 4);
        Assert.Equal(4, result.Count);
    }

    // ── GetCohortQuality ──────────────────────────────────────────────────────

    [Fact]
    public void GetCohortQuality_ZeroSize_NotApplicable()
    {
        var result = PositionBenchmarkBuilder.GetCohortQuality(0);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
    }

    [Fact]
    public void GetCohortQuality_NegativeSize_NotApplicable()
    {
        var result = PositionBenchmarkBuilder.GetCohortQuality(-1);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
    }

    [Fact]
    public void GetCohortQuality_BelowMinimum_LowTier()
    {
        var result = PositionBenchmarkBuilder.GetCohortQuality(3, minSize: 5);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(3, result.SampleCount);
    }

    [Fact]
    public void GetCohortQuality_ExactlyMinimum_HighTier()
    {
        var result = PositionBenchmarkBuilder.GetCohortQuality(5, minSize: 5);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    [Fact]
    public void GetCohortQuality_AboveMinimum_HighTier()
    {
        var result = PositionBenchmarkBuilder.GetCohortQuality(20, minSize: 5);
        Assert.Equal(QualityTier.High, result.Tier);
        Assert.Equal(20, result.SampleCount);
    }

    [Fact]
    public void GetCohortQuality_DefaultMinSize_Used()
    {
        // Default minSize = 5
        var belowDefault = PositionBenchmarkBuilder.GetCohortQuality(4);
        var atDefault = PositionBenchmarkBuilder.GetCohortQuality(5);
        Assert.Equal(QualityTier.Low, belowDefault.Tier);
        Assert.Equal(QualityTier.High, atDefault.Tier);
    }

    // ── GetNonGapRadarAxes / IsBackendGapAxis ─────────────────────────────────

    [Fact]
    public void GetNonGapRadarAxes_Ar_ExcludesCreationGap()
    {
        // AR has "Création" as backend gap
        var axes = PositionBenchmarkBuilder.GetNonGapRadarAxes(AnalyticsPosition.AR);
        Assert.DoesNotContain(axes, a => string.Equals(a.Label, "Création", StringComparison.Ordinal));
    }

    [Fact]
    public void GetNonGapRadarAxes_Dc_ExcludesCreationGap()
    {
        var axes = PositionBenchmarkBuilder.GetNonGapRadarAxes(AnalyticsPosition.DC);
        Assert.DoesNotContain(axes, a => string.Equals(a.Label, "Création", StringComparison.Ordinal));
    }

    [Fact]
    public void GetNonGapRadarAxes_Ail_ExcludesVolumeOffensifGap()
    {
        var axes = PositionBenchmarkBuilder.GetNonGapRadarAxes(AnalyticsPosition.AIL);
        Assert.DoesNotContain(axes, a => string.Equals(a.Label, "Volume offensif", StringComparison.Ordinal));
    }

    [Fact]
    public void GetNonGapRadarAxes_AllPositions_NoGapAxesReturned()
    {
        foreach (var position in Enum.GetValues<AnalyticsPosition>())
        {
            var axes = PositionBenchmarkBuilder.GetNonGapRadarAxes(position);
            Assert.All(axes, a => Assert.False(a.IsBackendGap, $"Position {position}: axis '{a.Label}' has IsBackendGap=true but was returned by GetNonGapRadarAxes"));
        }
    }

    [Fact]
    public void IsBackendGapAxis_Ar_CreationIsGap()
    {
        var result = PositionBenchmarkBuilder.IsBackendGapAxis(AnalyticsPosition.AR, "Création");
        Assert.True(result);
    }

    [Fact]
    public void IsBackendGapAxis_Ar_NonGapAxis_ReturnsFalse()
    {
        var result = PositionBenchmarkBuilder.IsBackendGapAxis(AnalyticsPosition.AR, "Production");
        Assert.False(result);
    }

    [Fact]
    public void IsBackendGapAxis_Dc_CreationIsGap()
    {
        var result = PositionBenchmarkBuilder.IsBackendGapAxis(AnalyticsPosition.DC, "Création");
        Assert.True(result);
    }

    [Fact]
    public void IsBackendGapAxis_Ail_VolumeOffensifIsGap()
    {
        var result = PositionBenchmarkBuilder.IsBackendGapAxis(AnalyticsPosition.AIL, "Volume offensif");
        Assert.True(result);
    }

    [Fact]
    public void IsBackendGapAxis_UnknownLabel_ReturnsFalse()
    {
        var result = PositionBenchmarkBuilder.IsBackendGapAxis(AnalyticsPosition.AR, "Axe inconnu XYZ");
        Assert.False(result);
    }

    // ── IsRadarAvailable ──────────────────────────────────────────────────────

    [Fact]
    public void IsRadarAvailable_EnoughNonGapAxes_ReturnsTrue()
    {
        var nonGap = PositionBenchmarkBuilder.GetNonGapRadarAxes(AnalyticsPosition.PIV);
        // Pass all non-gap labels → must exceed MinRadarAxes
        var labels = nonGap.Select(a => a.Label).ToList();
        var result = PositionBenchmarkBuilder.IsRadarAvailable(AnalyticsPosition.PIV, labels);
        Assert.True(result);
    }

    [Fact]
    public void IsRadarAvailable_EmptyAvailableAxes_ReturnsFalse()
    {
        var result = PositionBenchmarkBuilder.IsRadarAvailable(AnalyticsPosition.AR, []);
        Assert.False(result);
    }

    [Fact]
    public void IsRadarAvailable_FewerThanMinRadarAxes_ReturnsFalse()
    {
        // Provide only 2 labels (< MinRadarAxes = 3)
        var nonGap = PositionBenchmarkBuilder.GetNonGapRadarAxes(AnalyticsPosition.AR);
        var twoLabels = nonGap.Take(2).Select(a => a.Label).ToList();
        var result = PositionBenchmarkBuilder.IsRadarAvailable(AnalyticsPosition.AR, twoLabels);
        Assert.False(result);
    }

    [Fact]
    public void IsRadarAvailable_ExactlyMinRadarAxes_ReturnsTrue()
    {
        var nonGap = PositionBenchmarkBuilder.GetNonGapRadarAxes(AnalyticsPosition.GK);
        var threeLabels = nonGap.Take(PositionBenchmarkBuilder.MinRadarAxes).Select(a => a.Label).ToList();
        Assert.Equal(PositionBenchmarkBuilder.MinRadarAxes, threeLabels.Count);
        var result = PositionBenchmarkBuilder.IsRadarAvailable(AnalyticsPosition.GK, threeLabels);
        Assert.True(result);
    }
}
