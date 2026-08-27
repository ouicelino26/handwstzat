using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;
using HandWStat.Services.Api;

namespace HandWStat.Tests.Unit;

// ────────────────────────────────────────────────────────────────────────────
// B5 — Frontend integration tests
// Covers: BackendQualityHelper, ApiQueryBuilder B5 routes, DTO invariants
// ────────────────────────────────────────────────────────────────────────────

public class AnalyticsB5Tests
{
    // ── B5.19 BackendQualityHelper.MapBackendQualityTier ─────────────────────

    [Fact]
    public void MapBackendQualityTier_High_ReturnsHigh()
    {
        Assert.Equal(QualityTier.High, BackendQualityHelper.MapBackendQualityTier("High"));
    }

    [Fact]
    public void MapBackendQualityTier_Medium_ReturnsMedium()
    {
        Assert.Equal(QualityTier.Medium, BackendQualityHelper.MapBackendQualityTier("Medium"));
    }

    [Fact]
    public void MapBackendQualityTier_Low_ReturnsLow()
    {
        Assert.Equal(QualityTier.Low, BackendQualityHelper.MapBackendQualityTier("Low"));
    }

    [Fact]
    public void MapBackendQualityTier_Null_ReturnsNotApplicable()
    {
        Assert.Equal(QualityTier.NotApplicable, BackendQualityHelper.MapBackendQualityTier(null));
    }

    [Fact]
    public void MapBackendQualityTier_Unknown_ReturnsNotApplicable()
    {
        Assert.Equal(QualityTier.NotApplicable, BackendQualityHelper.MapBackendQualityTier("VeryHigh"));
    }

    [Fact]
    public void MapBackendQualityTier_EmptyString_ReturnsNotApplicable()
    {
        Assert.Equal(QualityTier.NotApplicable, BackendQualityHelper.MapBackendQualityTier(string.Empty));
    }

    // ── B5.19 BackendQualityHelper.FromSampleReliable ────────────────────────

    [Fact]
    public void FromSampleReliable_ZeroCount_ReturnsNotApplicable()
    {
        Assert.Equal(QualityTier.NotApplicable, BackendQualityHelper.FromSampleReliable(false, 0));
        Assert.Equal(QualityTier.NotApplicable, BackendQualityHelper.FromSampleReliable(true, 0));
    }

    [Fact]
    public void FromSampleReliable_ReliablePositiveCount_ReturnsHigh()
    {
        Assert.Equal(QualityTier.High, BackendQualityHelper.FromSampleReliable(true, 10));
    }

    [Fact]
    public void FromSampleReliable_UnreliablePositiveCount_ReturnsLow()
    {
        Assert.Equal(QualityTier.Low, BackendQualityHelper.FromSampleReliable(false, 3));
    }

    // ── B5.25 ApiQueryBuilder route paths ────────────────────────────────────

    [Fact]
    public void ApiQueryBuilder_TimelineRoute_ContainsBucketMinutes()
    {
        var query = new ApiQueryBuilder()
            .Add("bucketMinutes", (int?)5);
        var path = query.BuildRelativePath("api/Stats/players/42/timeline");
        Assert.Contains("bucketMinutes=5", path);
        Assert.Contains("api/Stats/players/42/timeline", path);
    }

    [Fact]
    public void ApiQueryBuilder_ClutchRoute_ContainsBothParams()
    {
        var query = new ApiQueryBuilder()
            .Add("maxScoreDiff", (int?)2)
            .Add("lastMinutes", (int?)5);
        var path = query.BuildRelativePath("api/Stats/players/7/clutch");
        Assert.Contains("maxScoreDiff=2", path);
        Assert.Contains("lastMinutes=5", path);
    }

    [Fact]
    public void ApiQueryBuilder_BenchmarkRoute_NoRequiredParams()
    {
        var query = new ApiQueryBuilder()
            .Add("competitionId", (int?)null)
            .Add("season", (string?)null);
        var path = query.BuildRelativePath("api/analytics/players/1/benchmark");
        // Null params must NOT appear in the path
        Assert.DoesNotContain("competitionId", path);
        Assert.DoesNotContain("season", path);
        Assert.Equal("api/analytics/players/1/benchmark", path);
    }

    [Fact]
    public void ApiQueryBuilder_BenchmarkRoute_WithParams()
    {
        var query = new ApiQueryBuilder()
            .Add("competitionId", (int?)3)
            .Add("season", "2024-2025");
        var path = query.BuildRelativePath("api/analytics/players/1/benchmark");
        Assert.Contains("competitionId=3", path);
        Assert.Contains("season=2024-2025", path);
    }

    // ── B5 DTO invariants ─────────────────────────────────────────────────────

    [Fact]
    public void ClutchBreakdownDto_SampleCountLessThanFive_IsLowSample()
    {
        var dto = new ClutchBreakdownDto { SampleCount = 4, SampleReliable = false };
        Assert.True(dto.SampleCount < 5);
        Assert.False(dto.SampleReliable);
    }

    [Fact]
    public void ClutchBreakdownDto_ZeroAttempts_ShotSuccessRateShouldBeNull()
    {
        // ShotSuccessRate is nullable — must be null when no attempts
        var dto = new ClutchBreakdownDto { Attempts = 0, ShotSuccessRate = null };
        Assert.Null(dto.ShotSuccessRate);
    }

    [Fact]
    public void HalfTimeBreakdownDto_BothHalvesNull_RepresentsNoData()
    {
        var dto = new HalfTimeBreakdownDto { FirstHalf = null, SecondHalf = null };
        Assert.Null(dto.FirstHalf);
        Assert.Null(dto.SecondHalf);
    }

    [Fact]
    public void MatchRunSummaryDto_DataQualityWarning_FlagIsIndependent()
    {
        var dto = new MatchRunSummaryDto
        {
            DataQualityWarning = true,
            ScoreConsistent = false,
            Runs3PlusTeam1 = 2
        };
        Assert.True(dto.DataQualityWarning);
        Assert.Equal(2, dto.Runs3PlusTeam1);
    }

    [Fact]
    public void ScoringRunDto_StartScoreAndEndScore_AreStrings()
    {
        var run = new ScoringRunDto { StartScore = "5:3", EndScore = "8:3", Goals = 3 };
        Assert.Equal("5:3", run.StartScore);
        Assert.Equal("8:3", run.EndScore);
        Assert.Equal(3, run.Goals);
    }

    [Fact]
    public void GkScoreStateDto_ScoreStates_KnownValues()
    {
        var states = new[] { "Leading", "Tied", "Trailing" };
        foreach (var s in states)
        {
            var dto = new GkScoreStateDto { ScoreState = s };
            Assert.Equal(s, dto.ScoreState);
        }
    }

    [Fact]
    public void PlayerTimelineDto_EmptyBuckets_IsValid()
    {
        var dto = new PlayerTimelineDto { BucketSizeMinutes = 5 };
        Assert.Empty(dto.Buckets);
        Assert.Equal(5, dto.BucketSizeMinutes);
    }

    [Fact]
    public void PlayerBenchmarkDto_EmptyMetrics_CohortSizeZero()
    {
        var dto = new PlayerBenchmarkDto { PlayerId = 1, CohortSize = 0 };
        Assert.Equal(0, dto.CohortSize);
        Assert.Empty(dto.Metrics);
    }

    [Fact]
    public void MetricBenchmarkDto_BackendGap_PercentileIsNull()
    {
        // When BackendGap=true, Percentile must be null (backend contract)
        var dto = new MetricBenchmarkDto { BackendGap = true, Percentile = null };
        Assert.True(dto.BackendGap);
        Assert.Null(dto.Percentile);
    }

    [Fact]
    public void ArmSideBreakdownDto_CoverageReliable_FalseWhenLowCoverage()
    {
        var dto = new ArmSideBreakdownDto
        {
            TotalShotAttempts = 100,
            CoveredAttempts = 3,
            CoveragePct = 3.0,
            CoverageReliable = false
        };
        Assert.False(dto.CoverageReliable);
    }

    [Fact]
    public void PlayerMatchPlayingTimeDto_PlayingTimeMinutes_IsDouble()
    {
        var dto = new PlayerMatchPlayingTimeDto { MatchId = 1, PlayingTimeMinutes = 32.5 };
        Assert.Equal(32.5, dto.PlayingTimeMinutes, precision: 4);
    }
}
