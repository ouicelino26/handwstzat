using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA2Tests
{
    // ── CAT-05 — EvaluateAssistTurnoverQuality ───────────────────────────────

    [Fact]
    public void EvaluateAssistTurnoverQuality_ZeroTurnovers_ReturnsNotApplicable()
    {
        var result = AnalyticsQualityPolicy.EvaluateAssistTurnoverQuality(10, 0);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
        Assert.Equal(QualityReasons.ZeroDenominator, result.Reason);
    }

    [Fact]
    public void EvaluateAssistTurnoverQuality_BelowAssistThreshold_ReturnsLow()
    {
        // assists < 5, turnovers >= 3
        var result = AnalyticsQualityPolicy.EvaluateAssistTurnoverQuality(4, 3);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    [Fact]
    public void EvaluateAssistTurnoverQuality_BelowTurnoverThreshold_ReturnsLow()
    {
        // assists >= 5, turnovers < 3
        var result = AnalyticsQualityPolicy.EvaluateAssistTurnoverQuality(6, 2);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    [Fact]
    public void EvaluateAssistTurnoverQuality_BothThresholdsMet_ReturnsHigh()
    {
        var result = AnalyticsQualityPolicy.EvaluateAssistTurnoverQuality(7, 4);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    [Fact]
    public void EvaluateAssistTurnoverQuality_ExactMinimums_ReturnsHigh()
    {
        // assists == 5, turnovers == 3 → exact boundary → High
        var result = AnalyticsQualityPolicy.EvaluateAssistTurnoverQuality(5, 3);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    [Fact]
    public void EvaluateAssistTurnoverQuality_SampleCount_IsMinOfAssistsTurnovers()
    {
        var result = AnalyticsQualityPolicy.EvaluateAssistTurnoverQuality(6, 4);
        Assert.Equal(4, result.SampleCount);
    }

    // ── CAT-05 — ComputeAssistTurnoverRatio ──────────────────────────────────

    [Fact]
    public void ComputeAssistTurnoverRatio_Normal_ReturnsRatio()
    {
        var ratio = AnalyticsCalculationService.ComputeAssistTurnoverRatio(10, 5);
        Assert.Equal(2.0, ratio);
    }

    [Fact]
    public void ComputeAssistTurnoverRatio_ZeroTurnovers_ReturnsNull()
    {
        var ratio = AnalyticsCalculationService.ComputeAssistTurnoverRatio(5, 0);
        Assert.Null(ratio);
    }

    // ── CAT-01 — ComputeGoalsCreatedPer60 ────────────────────────────────────

    [Fact]
    public void ComputeGoalsCreatedPer60_Normal_ReturnsValue()
    {
        // 10 goals + 5 assists over 90 min → 15 / 90 * 60 = 10.0
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(10, 5, 90);
        Assert.NotNull(result);
        Assert.Equal(10.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeGoalsCreatedPer60_ZeroPlayingTime_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(10, 5, 0);
        Assert.Null(result);
    }

    // ── CAT-10 — ComputeDefensiveImpactPer60 ─────────────────────────────────

    [Fact]
    public void ComputeDefensiveImpactPer60_Normal_ReturnsValue()
    {
        // 3 intercepts + 2 blocks + 1 neutral + 4 passageForce = 10 over 60 min → 10.0/60*60 = 10.0
        var result = AnalyticsCalculationService.ComputeDefensiveImpactPer60(3, 2, 1, 4, 60);
        Assert.NotNull(result);
        Assert.Equal(10.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeDefensiveImpactPer60_ZeroTime_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeDefensiveImpactPer60(3, 2, 1, 4, 0);
        Assert.Null(result);
    }

    // ── CAT-15 / CAT-08 / CAT-09 / CAT-11 — NormalizeApiPer60 ───────────────

    [Fact]
    public void NormalizeApiPer60_SufficientPlayingTime_ReturnsScaled()
    {
        // apiPer60 = 4.0, playingTime = 60 min → effectively same as 4.0 (scale by 60/60)
        var result = AnalyticsCalculationService.NormalizeApiPer60(4.0, 60);
        Assert.NotNull(result);
        Assert.Equal(4.0, result!.Value, precision: 5);
    }

    [Fact]
    public void NormalizeApiPer60_ZeroPlayingTime_ReturnsNull()
    {
        var result = AnalyticsCalculationService.NormalizeApiPer60(4.0, 0);
        Assert.Null(result);
    }

    [Fact]
    public void NormalizeApiPer60_ZeroApiValue_ReturnsZeroOrNull()
    {
        // API sends 0.0 when metric has no data — NormalizeApiPer60 returns null or 0
        var result = AnalyticsCalculationService.NormalizeApiPer60(0.0, 60);
        // Either 0.0 mapped to null, or passed through as 0.0 — both are acceptable contract behaviours
        Assert.True(result is null || result.Value == 0.0);
    }

    // ── CAT-16 — ComputeShotsFacedPer60 ──────────────────────────────────────

    [Fact]
    public void ComputeShotsFacedPer60_Normal_ReturnsValue()
    {
        // 18 shots over 60 min → 18.0
        var result = AnalyticsCalculationService.ComputeShotsFacedPer60(18, 60);
        Assert.NotNull(result);
        Assert.Equal(18.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeShotsFacedPer60_ZeroPlayingTime_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeShotsFacedPer60(18, 0);
        Assert.Null(result);
    }

    // ── Position-aware catalog — GK only metrics ─────────────────────────────

    [Theory]
    [InlineData("CAT-13", "GK",  true)]
    [InlineData("CAT-13", "DC",  false)]
    [InlineData("CAT-13", "AIL", false)]
    [InlineData("CAT-15", "GK",  true)]
    [InlineData("CAT-15", "PIV", false)]
    [InlineData("CAT-16", "GK",  true)]
    [InlineData("CAT-16", "AR",  false)]
    public void Catalog_GkOnlyMetrics_ApplicableOnlyForGK(string code, string position, bool expected)
    {
        var def = AnalyticsV3Catalog.Get(code);
        Assert.NotNull(def);
        Assert.Equal(expected, AnalyticsV3Catalog.IsApplicable(def!, position));
    }

    // ── Position-aware catalog — field-player metrics not for GK ─────────────

    [Theory]
    [InlineData("CAT-06", "PIV", true)]
    [InlineData("CAT-06", "AR",  true)]
    [InlineData("CAT-06", "AIL", true)]
    [InlineData("CAT-06", "DC",  false)]
    [InlineData("CAT-06", "GK",  false)]
    [InlineData("CAT-05", "DC",  true)]
    [InlineData("CAT-05", "GK",  false)]
    public void Catalog_FieldMetrics_ApplicableByPosition(string code, string position, bool expected)
    {
        var def = AnalyticsV3Catalog.Get(code);
        Assert.NotNull(def);
        Assert.Equal(expected, AnalyticsV3Catalog.IsApplicable(def!, position));
    }

    // ── GK /60 quality with zero playing time ─────────────────────────────────

    [Fact]
    public void EvaluatePlayingTimeTier_ZeroPlayingTime_ReturnsNotApplicable()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(0, 150);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
        Assert.Equal(QualityReasons.ZeroDenominator, result.Reason);
    }

    [Fact]
    public void EvaluatePlayingTimeTier_BelowMinimum_ReturnsLow()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(100, 150);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    [Fact]
    public void EvaluatePlayingTimeTier_AtMinimum_ReturnsHigh()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(150, 150);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    // ── ShouldShow — Rankings excludes LOW ───────────────────────────────────

    [Fact]
    public void ShouldShow_LowInRankings_ReturnsFalse()
    {
        var low = new QualityTierResult(QualityTier.Low, QualityReasons.BelowMinimumSample, 2);
        Assert.False(AnalyticsQualityPolicy.ShouldShow(low, AnalyticsDisplayContext.Rankings));
    }

    [Fact]
    public void ShouldShow_LowInIndividualAnalysis_ReturnsTrue()
    {
        var low = new QualityTierResult(QualityTier.Low, QualityReasons.BelowMinimumSample, 2);
        Assert.True(AnalyticsQualityPolicy.ShouldShow(low, AnalyticsDisplayContext.IndividualAnalysis));
    }

    [Fact]
    public void ShouldShow_NotApplicable_ReturnsFalse()
    {
        var na = new QualityTierResult(QualityTier.NotApplicable, null, null);
        Assert.False(AnalyticsQualityPolicy.ShouldShow(na, AnalyticsDisplayContext.IndividualAnalysis));
        Assert.False(AnalyticsQualityPolicy.ShouldShow(na, AnalyticsDisplayContext.Rankings));
        Assert.False(AnalyticsQualityPolicy.ShouldShow(na, AnalyticsDisplayContext.Compare));
    }

    [Fact]
    public void ShouldShow_LowInCompare_ReturnsTrue()
    {
        var low = new QualityTierResult(QualityTier.Low, QualityReasons.BelowMinimumSample, 2);
        Assert.True(AnalyticsQualityPolicy.ShouldShow(low, AnalyticsDisplayContext.Compare));
    }

    // ── CAT-03 must be removed ────────────────────────────────────────────────

    [Fact]
    public void Catalog_CAT03_IsRemoved()
    {
        var def = AnalyticsV3Catalog.Get("CAT-03");
        Assert.NotNull(def);
        Assert.Equal(AnalyticsMetricStatus.Removed, def!.Status);
    }
}
