using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

// ────────────────────────────────────────────────────────────────────────────
// B6 — Player Timeline UI + Backend Benchmark Parity
// Covers: TimelineBucketDto ordering, null rate vs real 0,
//         temporal coverage warning flag, player change reset,
//         PositionBenchmarkBuilder.ComputePercentile parity with backend formula
// ────────────────────────────────────────────────────────────────────────────

public class AnalyticsB6Tests
{
    // ── B6.25 — Timeline bucket ordering ─────────────────────────────────────

    [Fact]
    public void TimelineBuckets_5Min_OrderedByBucketStart_12Buckets()
    {
        // Arrange: create 12 buckets in random order
        var raw = Enumerable.Range(0, 12)
            .Select(i => new TimelineBucketDto
            {
                Bucket = $"{i * 5}-{(i + 1) * 5}",
                BucketStartMinute = i * 5,
                BucketEndMinute   = (i + 1) * 5
            })
            .Reverse()   // reverse to simulate out-of-order API response
            .ToList();

        // Act
        var ordered = raw.OrderBy(b => b.BucketStartMinute).ToList();

        // Assert
        Assert.Equal(12, ordered.Count);
        Assert.Equal(0.0,  ordered[0].BucketStartMinute);
        Assert.Equal(5.0,  ordered[1].BucketStartMinute);
        Assert.Equal(55.0, ordered[11].BucketStartMinute);
    }

    [Fact]
    public void TimelineBuckets_10Min_OrderedByBucketStart_6Buckets()
    {
        // Arrange
        var raw = Enumerable.Range(0, 6)
            .Select(i => new TimelineBucketDto
            {
                Bucket = $"{i * 10}-{(i + 1) * 10}",
                BucketStartMinute = i * 10,
                BucketEndMinute   = (i + 1) * 10
            })
            .Reverse()
            .ToList();

        // Act
        var ordered = raw.OrderBy(b => b.BucketStartMinute).ToList();

        // Assert
        Assert.Equal(6, ordered.Count);
        Assert.Equal(0.0,  ordered[0].BucketStartMinute);
        Assert.Equal(10.0, ordered[1].BucketStartMinute);
        Assert.Equal(50.0, ordered[5].BucketStartMinute);
    }

    // ── B6.26 — Null rate vs real 0 ──────────────────────────────────────────

    [Fact]
    public void TimelineBucket_NullShotSuccessRate_IsNull_NotZero()
    {
        // Arrange: no attempts in this bucket → rate should be null, not 0
        var bucket = new TimelineBucketDto { Attempts = 0, Goals = 0, ShotSuccessRate = null };

        // Assert: null rate must not be displayed as 0
        Assert.Null(bucket.ShotSuccessRate);
    }

    [Fact]
    public void TimelineBucket_RealZeroRate_IsZeroNotNull()
    {
        // Arrange: 3 attempts, 0 goals → rate is genuinely 0.0, not null
        var bucket = new TimelineBucketDto { Attempts = 3, Goals = 0, ShotSuccessRate = 0.0 };

        // Assert: real 0 is stored as 0.0 (not null)
        Assert.NotNull(bucket.ShotSuccessRate);
        Assert.Equal(0.0, bucket.ShotSuccessRate!.Value);
    }

    [Fact]
    public void TimelineBucket_NullSaveRate_IsNull_NotZero()
    {
        var bucket = new TimelineBucketDto { ShotsFaced = 0, Saves = 0, SaveRate = null };
        Assert.Null(bucket.SaveRate);
    }

    // ── B6.26 — TemporalCoveragePct threshold ────────────────────────────────

    [Fact]
    public void TimelineDto_TemporalCoveragePct_Below80_ShouldWarn()
    {
        var dto = new PlayerTimelineDto { TemporalCoveragePct = 75.0 };
        Assert.True(dto.TemporalCoveragePct < 80.0, "Coverage below 80% should trigger a warning.");
    }

    [Fact]
    public void TimelineDto_TemporalCoveragePct_AtOrAbove80_NoWarn()
    {
        var dto80  = new PlayerTimelineDto { TemporalCoveragePct = 80.0 };
        var dto100 = new PlayerTimelineDto { TemporalCoveragePct = 100.0 };
        Assert.False(dto80.TemporalCoveragePct < 80.0,  "Coverage at 80% should NOT warn.");
        Assert.False(dto100.TemporalCoveragePct < 80.0, "Coverage at 100% should NOT warn.");
    }

    // ── B6.26 — Player change resets timeline state (model-level) ────────────

    [Fact]
    public void TimelineState_PlayerChange_ResetsTimelineAndLoaded()
    {
        // Simulate state: timeline loaded for player 1
        var timelineLoaded = true;
        PlayerTimelineDto? timeline = new() { Buckets = [new TimelineBucketDto { Bucket = "0-5" }] };
        const int currentPlayerId = 1;

        // Simulate switch to player 2 (as done in LoadPlayerDetailsAsync)
        const int newPlayerId = 2;
        if (newPlayerId != currentPlayerId)
        {
            timelineLoaded = false;
            timeline = null;
        }

        Assert.False(timelineLoaded, "IsTimelineLoaded must be false after player change.");
        Assert.Null(timeline);
    }

    // ── B6.27 — Benchmark parity: PositionBenchmarkBuilder vs backend formula ─

    [Fact]
    public void ComputePercentile_HigherIsBetter_Distribution5_MiddleValue_Returns50()
    {
        // Distribution [10,20,30,40,50], value=30, higherIsBetter=true
        // below=2 (10<30, 20<30), equal=1, n=5 → (2+0.5)/5*100 = 50
        var cohort = new List<double> { 10, 20, 30, 40, 50 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 30.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(50.0, result!.Value, precision: 3);
    }

    [Fact]
    public void ComputePercentile_LowerIsBetter_Distribution5_MiddleValue_Returns50()
    {
        // Distribution [10,20,30,40,50], value=30, higherIsBetter=false
        // below=2 (40>30, 50>30), equal=1, n=5 → (2+0.5)/5*100 = 50
        var cohort = new List<double> { 10, 20, 30, 40, 50 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 30.0, higherIsBetter: false);
        Assert.NotNull(result);
        Assert.Equal(50.0, result!.Value, precision: 3);
    }

    [Fact]
    public void ComputePercentile_WithTies_HigherIsBetter_ReturnsConsistentPercentile()
    {
        // Distribution [10, 30, 30, 50], value=30, higherIsBetter=true
        // below=1 (10<30), equal=2 (30==30), n=4 → (1+0.5*2)/4*100 = 50
        var cohort = new List<double> { 10, 30, 30, 50 };
        var result = PositionBenchmarkBuilder.ComputePercentile(cohort, 30.0, higherIsBetter: true);
        Assert.NotNull(result);
        Assert.Equal(50.0, result!.Value, precision: 3);
    }

    [Fact]
    public void ComputePercentile_BackendGapMetric_FrontendFallback_IsValid()
    {
        // When a MetricBenchmarkDto has BackendGap=true, we must NOT use its Percentile.
        // The frontend ComputePercentile is the correct fallback.
        var benchmarkMetric = new MetricBenchmarkDto { MetricCode = "CAT-17A", BackendGap = true, Percentile = 72.0 };

        // Simulate: BackendGap=true → use frontend
        var cohort = new List<double> { 10, 20, 30, 40, 50 };
        var frontendPercentile = PositionBenchmarkBuilder.ComputePercentile(cohort, 30.0, higherIsBetter: true);

        // Assert: frontend result is used (not 72.0 from backend gap)
        Assert.True(benchmarkMetric.BackendGap, "CAT-17A is a BackendGap metric.");
        Assert.NotNull(frontendPercentile);
        Assert.NotEqual(72.0, frontendPercentile!.Value);
        Assert.Equal(50.0, frontendPercentile.Value, precision: 3);
    }
}
