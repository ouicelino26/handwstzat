using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA6Tests
{
    // ── ComputeAttemptShare ───────────────────────────────────────────────────

    [Fact]
    public void ComputeAttemptShare_BasicFormula()
    {
        // 4 / 20 × 100 = 20%
        var result = SpatialAnalyticsBuilder.ComputeAttemptShare(4, 20);
        Assert.NotNull(result);
        Assert.Equal(20.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeAttemptShare_ZeroTotal_ReturnsNull()
    {
        Assert.Null(SpatialAnalyticsBuilder.ComputeAttemptShare(5, 0));
    }

    [Fact]
    public void ComputeAttemptShare_NegativeTotal_ReturnsNull()
    {
        Assert.Null(SpatialAnalyticsBuilder.ComputeAttemptShare(5, -1));
    }

    [Fact]
    public void ComputeAttemptShare_FullShare_Returns100()
    {
        var result = SpatialAnalyticsBuilder.ComputeAttemptShare(10, 10);
        Assert.Equal(100.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeAttemptShare_ZeroZone_ReturnsZero()
    {
        var result = SpatialAnalyticsBuilder.ComputeAttemptShare(0, 20);
        Assert.Equal(0.0, result!.Value, precision: 5);
    }

    // ── ComputeGoalShare ─────────────────────────────────────────────────────

    [Fact]
    public void ComputeGoalShare_BasicFormula()
    {
        // 3 / 12 × 100 = 25%
        var result = SpatialAnalyticsBuilder.ComputeGoalShare(3, 12);
        Assert.NotNull(result);
        Assert.Equal(25.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeGoalShare_ZeroTotalGoals_ReturnsNull()
    {
        Assert.Null(SpatialAnalyticsBuilder.ComputeGoalShare(1, 0));
    }

    [Fact]
    public void ComputeGoalShare_ZeroZoneGoals_ReturnsZero()
    {
        var result = SpatialAnalyticsBuilder.ComputeGoalShare(0, 15);
        Assert.Equal(0.0, result!.Value, precision: 5);
    }

    // ── ComputeShotSuccessRate ────────────────────────────────────────────────

    [Fact]
    public void ComputeShotSuccessRate_BasicFormula()
    {
        // 6 / 10 × 100 = 60%
        var result = SpatialAnalyticsBuilder.ComputeShotSuccessRate(6, 10);
        Assert.Equal(60.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeShotSuccessRate_ZeroAttempts_ReturnsNull()
    {
        Assert.Null(SpatialAnalyticsBuilder.ComputeShotSuccessRate(0, 0));
    }

    [Fact]
    public void ComputeShotSuccessRate_ZeroGoals_ReturnsZero()
    {
        var result = SpatialAnalyticsBuilder.ComputeShotSuccessRate(0, 8);
        Assert.Equal(0.0, result!.Value, precision: 5);
    }

    // ── ComputeSpatialCoverage ────────────────────────────────────────────────

    [Fact]
    public void ComputeSpatialCoverage_AllMapped_Returns100()
    {
        var result = SpatialAnalyticsBuilder.ComputeSpatialCoverage(50, 50);
        Assert.Equal(100.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeSpatialCoverage_PartialCoverage()
    {
        // 40 / 50 × 100 = 80%
        var result = SpatialAnalyticsBuilder.ComputeSpatialCoverage(40, 50);
        Assert.Equal(80.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeSpatialCoverage_ZeroTotal_ReturnsNull()
    {
        Assert.Null(SpatialAnalyticsBuilder.ComputeSpatialCoverage(0, 0));
    }

    // ── AggregateZoneTotals ───────────────────────────────────────────────────

    [Fact]
    public void AggregateZoneTotals_EmptyList_ReturnsZeros()
    {
        var (attempts, goals) = SpatialAnalyticsBuilder.AggregateZoneTotals([]);
        Assert.Equal(0, attempts);
        Assert.Equal(0, goals);
    }

    [Fact]
    public void AggregateZoneTotals_SingleZone_ReturnsCounts()
    {
        var zones = new[] { MakeZone("BG1", attempts: 10, successes: 4) };
        var (attempts, goals) = SpatialAnalyticsBuilder.AggregateZoneTotals(zones);
        Assert.Equal(10, attempts);
        Assert.Equal(4, goals);
    }

    [Fact]
    public void AggregateZoneTotals_MultipleZones_SumsCorrectly()
    {
        var zones = new[]
        {
            MakeZone("BG1", attempts: 10, successes: 4),
            MakeZone("BD1", attempts: 8,  successes: 3),
            MakeZone("BG2", attempts: 5,  successes: 1),
        };
        var (attempts, goals) = SpatialAnalyticsBuilder.AggregateZoneTotals(zones);
        Assert.Equal(23, attempts);
        Assert.Equal(8, goals);
    }

    // ── Quality tiers via AnalyticsQualityPolicy ──────────────────────────────

    [Fact]
    public void ZoneQuality_ZeroAttempts_IsNotApplicable()
    {
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, 0, SpatialAnalyticsBuilder.MinShotZoneSample);
        Assert.Equal(QualityTier.NotApplicable, quality.Tier);
    }

    [Fact]
    public void ZoneQuality_BelowMin_IsLow()
    {
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, 3, SpatialAnalyticsBuilder.MinShotZoneSample);
        Assert.Equal(QualityTier.Low, quality.Tier);
    }

    [Fact]
    public void ZoneQuality_AtMin_IsHigh()
    {
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, SpatialAnalyticsBuilder.MinShotZoneSample, SpatialAnalyticsBuilder.MinShotZoneSample);
        Assert.Equal(QualityTier.High, quality.Tier);
    }

    [Fact]
    public void ZoneQuality_AboveMin_IsHigh()
    {
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, 12, SpatialAnalyticsBuilder.MinShotZoneSample);
        Assert.Equal(QualityTier.High, quality.Tier);
    }

    // ── BuildZoneMetric ───────────────────────────────────────────────────────

    [Fact]
    public void BuildZoneMetric_ComputesAttemptShareAndGoalShare()
    {
        var zone = MakeZone("BG1", attempts: 10, successes: 5);
        var metric = SpatialAnalyticsBuilder.BuildZoneMetric(zone, totalAttempts: 50, totalGoals: 20);

        Assert.Equal(20.0, metric.AttemptShare!.Value, precision: 5);  // 10/50
        Assert.Equal(25.0, metric.GoalShare!.Value, precision: 5);     // 5/20
    }

    [Fact]
    public void BuildZoneMetric_ZeroTotalAttempts_NullShares()
    {
        var zone = MakeZone("BG1", attempts: 0, successes: 0);
        var metric = SpatialAnalyticsBuilder.BuildZoneMetric(zone, totalAttempts: 0, totalGoals: 0);

        Assert.Null(metric.AttemptShare);
        Assert.Null(metric.GoalShare);
        Assert.Equal(QualityTier.NotApplicable, metric.Quality.Tier);
    }

    [Fact]
    public void BuildZoneMetric_LowSample_QualityIsLow()
    {
        var zone = MakeZone("BG1", attempts: 2, successes: 1);
        var metric = SpatialAnalyticsBuilder.BuildZoneMetric(zone, totalAttempts: 20, totalGoals: 10);
        Assert.Equal(QualityTier.Low, metric.Quality.Tier);
    }

    [Fact]
    public void BuildZoneMetric_HighSample_QualityIsHigh()
    {
        var zone = MakeZone("BG1", attempts: 8, successes: 4);
        var metric = SpatialAnalyticsBuilder.BuildZoneMetric(zone, totalAttempts: 30, totalGoals: 12);
        Assert.Equal(QualityTier.High, metric.Quality.Tier);
    }

    // ── BuildZoneMetrics (list) ───────────────────────────────────────────────

    [Fact]
    public void BuildZoneMetrics_EmptyList_ReturnsEmpty()
    {
        var metrics = SpatialAnalyticsBuilder.BuildZoneMetrics([]);
        Assert.Empty(metrics);
    }

    [Fact]
    public void BuildZoneMetrics_TwoZones_SharesSumToHundred()
    {
        var zones = new[]
        {
            MakeZone("BG1", attempts: 30, successes: 10),
            MakeZone("BD1", attempts: 70, successes: 30),
        };
        var metrics = SpatialAnalyticsBuilder.BuildZoneMetrics(zones);

        var totalShare = metrics.Sum(m => m.AttemptShare ?? 0);
        Assert.Equal(100.0, totalShare, precision: 5);
    }

    [Fact]
    public void BuildZoneMetrics_GoalShares_SumToHundred()
    {
        var zones = new[]
        {
            MakeZone("BG1", attempts: 10, successes: 5),
            MakeZone("BD1", attempts: 10, successes: 15),
        };
        var metrics = SpatialAnalyticsBuilder.BuildZoneMetrics(zones);

        var totalGoalShare = metrics.Sum(m => m.GoalShare ?? 0);
        Assert.Equal(100.0, totalGoalShare, precision: 5);
    }

    // ── Catalog CAT-23/24/25 ──────────────────────────────────────────────────

    [Fact]
    public void Catalog_CAT23_Exists()
    {
        Assert.NotNull(AnalyticsV3Catalog.Get("CAT-23"));
    }

    [Fact]
    public void Catalog_CAT23_IsExpert()
    {
        Assert.Equal(AnalyticsMetricStatus.Expert, AnalyticsV3Catalog.Get("CAT-23")!.Status);
    }

    [Fact]
    public void Catalog_CAT23_MinimumSampleIs5()
    {
        Assert.Equal(5, AnalyticsV3Catalog.Get("CAT-23")!.MinimumSampleCount);
    }

    [Fact]
    public void Catalog_CAT23_AppliesToAll()
    {
        Assert.Equal(AnalyticsPositionScope.All, AnalyticsV3Catalog.Get("CAT-23")!.ApplicablePositions);
    }

    [Fact]
    public void Catalog_CAT24_Exists()
    {
        Assert.NotNull(AnalyticsV3Catalog.Get("CAT-24"));
    }

    [Fact]
    public void Catalog_CAT24_TechnicalNameIsAttemptShare()
    {
        Assert.Equal("spatial_attempt_share", AnalyticsV3Catalog.Get("CAT-24")!.TechnicalName);
    }

    [Fact]
    public void Catalog_CAT25_Exists()
    {
        Assert.NotNull(AnalyticsV3Catalog.Get("CAT-25"));
    }

    [Fact]
    public void Catalog_CAT25_TechnicalNameIsGoalShare()
    {
        Assert.Equal("spatial_goal_share", AnalyticsV3Catalog.Get("CAT-25")!.TechnicalName);
    }

    // ── MinShotZoneSample constant ────────────────────────────────────────────

    [Fact]
    public void MinShotZoneSample_MatchesCat23MinimumSampleCount()
    {
        Assert.Equal(SpatialAnalyticsBuilder.MinShotZoneSample, AnalyticsV3Catalog.Get("CAT-23")!.MinimumSampleCount);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CourtZoneStat MakeZone(string key, int attempts, int successes) =>
        new CourtZoneStat(
            Key: key,
            Label: key,
            Rate: attempts > 0 ? (double)successes / attempts * 100.0 : 0.0,
            Attempts: attempts,
            Successes: successes,
            SampleReliable: attempts >= 5,
            IsAvailable: attempts > 0,
            Outcomes: new List<OutcomeCount>());
}
