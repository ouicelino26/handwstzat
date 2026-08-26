using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsQualityPolicyTests
{
    // ── Helper ──────────────────────────────────────────────────────────────

    private static LeagueMetricQualityDto Quality(bool sampleReliable, double qualityScore, string? reason = null) =>
        new() { SampleReliable = sampleReliable, QualityScore = qualityScore, Reason = reason };

    // ── EvaluateTier — API v2 MetricQuality path ────────────────────────────

    [Fact]
    public void EvaluateTier_SampleReliableHighScore_ReturnsHigh()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: Quality(sampleReliable: true, qualityScore: 0.8),
            sampleCount: 30, minSample: 10);
        Assert.Equal(QualityTier.High, result.Tier);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void EvaluateTier_SampleReliableExactlyAtThreshold_ReturnsHigh()
    {
        // QualityScore = 0.5 exactly → High (threshold is >=)
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: Quality(sampleReliable: true, qualityScore: 0.5),
            sampleCount: 25, minSample: 10);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    [Fact]
    public void EvaluateTier_SampleReliableLowScore_ReturnsMedium()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: Quality(sampleReliable: true, qualityScore: 0.3),
            sampleCount: 12, minSample: 10);
        Assert.Equal(QualityTier.Medium, result.Tier);
    }

    [Fact]
    public void EvaluateTier_SampleNotReliable_ReturnsLow()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: Quality(sampleReliable: false, qualityScore: 0.1, reason: QualityReasons.BelowMinimumSample),
            sampleCount: 3, minSample: 10);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    [Fact]
    public void EvaluateTier_SampleNotReliableNoReason_ReturnsLowWithDefaultReason()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: Quality(sampleReliable: false, qualityScore: 0.0),
            sampleCount: 1, minSample: 10);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    // ── EvaluateTier — fallback (no MetricQuality from API) ─────────────────

    [Fact]
    public void EvaluateTier_NullQualityAboveMinSample_ReturnsHigh()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: null, sampleCount: 15, minSample: 10);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    [Fact]
    public void EvaluateTier_NullQualityBelowMinSample_ReturnsLow()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: null, sampleCount: 3, minSample: 10);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    [Fact]
    public void EvaluateTier_NullQualityExactlyAtMinSample_ReturnsHigh()
    {
        // sampleCount == minSample is sufficient (not below)
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: null, sampleCount: 10, minSample: 10);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    // ── EvaluateTier — NotApplicable conditions ──────────────────────────────

    [Fact]
    public void EvaluateTier_ZeroDenominator_ReturnsNotApplicable()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: null, sampleCount: 5, minSample: 3, denominator: 0.0);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
        Assert.Equal(QualityReasons.ZeroDenominator, result.Reason);
    }

    [Fact]
    public void EvaluateTier_ZeroSampleCount_ReturnsNotApplicable()
    {
        var result = AnalyticsQualityPolicy.EvaluateTier(
            quality: null, sampleCount: 0, minSample: 10);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
    }

    // ── EvaluatePlayingTimeTier ──────────────────────────────────────────────

    [Fact]
    public void EvaluatePlayingTimeTier_ZeroTime_ReturnsNotApplicable()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: 0, minimumPlayingTimeMinutes: 150);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
        Assert.Equal(QualityReasons.ZeroDenominator, result.Reason);
    }

    [Fact]
    public void EvaluatePlayingTimeTier_NegativeTime_ReturnsNotApplicable()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: -10, minimumPlayingTimeMinutes: 150);
        Assert.Equal(QualityTier.NotApplicable, result.Tier);
    }

    [Fact]
    public void EvaluatePlayingTimeTier_BelowMinimum_ReturnsLow()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: 80, minimumPlayingTimeMinutes: 150);
        Assert.Equal(QualityTier.Low, result.Tier);
        Assert.Equal(QualityReasons.BelowMinimumSample, result.Reason);
    }

    [Fact]
    public void EvaluatePlayingTimeTier_AtMinimum_ReturnsHigh()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: 150, minimumPlayingTimeMinutes: 150);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    [Fact]
    public void EvaluatePlayingTimeTier_AboveMinimum_ReturnsHigh()
    {
        var result = AnalyticsQualityPolicy.EvaluatePlayingTimeTier(
            playingTimeMinutes: 600, minimumPlayingTimeMinutes: 150);
        Assert.Equal(QualityTier.High, result.Tier);
    }

    // ── ShouldShow by context ────────────────────────────────────────────────

    [Theory]
    [InlineData(QualityTier.High, AnalyticsDisplayContext.Rankings, true)]
    [InlineData(QualityTier.Medium, AnalyticsDisplayContext.Rankings, true)]
    [InlineData(QualityTier.Low, AnalyticsDisplayContext.Rankings, false)]
    [InlineData(QualityTier.NotApplicable, AnalyticsDisplayContext.Rankings, false)]
    [InlineData(QualityTier.High, AnalyticsDisplayContext.IndividualAnalysis, true)]
    [InlineData(QualityTier.Low, AnalyticsDisplayContext.IndividualAnalysis, true)]
    [InlineData(QualityTier.NotApplicable, AnalyticsDisplayContext.IndividualAnalysis, false)]
    [InlineData(QualityTier.High, AnalyticsDisplayContext.Compare, true)]
    [InlineData(QualityTier.Low, AnalyticsDisplayContext.Compare, true)]
    [InlineData(QualityTier.NotApplicable, AnalyticsDisplayContext.Compare, false)]
    [InlineData(QualityTier.Low, AnalyticsDisplayContext.ContextSplits, true)]
    [InlineData(QualityTier.NotApplicable, AnalyticsDisplayContext.ContextSplits, false)]
    public void ShouldShow_ContextMatrix(QualityTier tier, AnalyticsDisplayContext context, bool expected)
    {
        var result = new QualityTierResult(tier, null, null);
        Assert.Equal(expected, AnalyticsQualityPolicy.ShouldShow(result, context));
    }

    // ── ShouldExcludeFromRanking ─────────────────────────────────────────────

    [Theory]
    [InlineData(QualityTier.High, false)]
    [InlineData(QualityTier.Medium, false)]
    [InlineData(QualityTier.Low, true)]
    [InlineData(QualityTier.NotApplicable, true)]
    public void ShouldExcludeFromRanking_ByTier(QualityTier tier, bool expected)
    {
        var result = new QualityTierResult(tier, null, null);
        Assert.Equal(expected, AnalyticsQualityPolicy.ShouldExcludeFromRanking(result));
    }

    // ── CanDeclareWinner ─────────────────────────────────────────────────────

    [Fact]
    public void CanDeclareWinner_BothHigh_ReturnsTrue()
    {
        var a = new QualityTierResult(QualityTier.High, null, null);
        var b = new QualityTierResult(QualityTier.High, null, null);
        Assert.True(AnalyticsQualityPolicy.CanDeclareWinner(a, b));
    }

    [Fact]
    public void CanDeclareWinner_BothMedium_ReturnsTrue()
    {
        var a = new QualityTierResult(QualityTier.Medium, null, null);
        var b = new QualityTierResult(QualityTier.Medium, null, null);
        Assert.True(AnalyticsQualityPolicy.CanDeclareWinner(a, b));
    }

    [Fact]
    public void CanDeclareWinner_PlayerALow_ReturnsFalse()
    {
        var a = new QualityTierResult(QualityTier.Low, QualityReasons.BelowMinimumSample, 3);
        var b = new QualityTierResult(QualityTier.High, null, 50);
        Assert.False(AnalyticsQualityPolicy.CanDeclareWinner(a, b));
    }

    [Fact]
    public void CanDeclareWinner_PlayerBLow_ReturnsFalse()
    {
        var a = new QualityTierResult(QualityTier.High, null, 50);
        var b = new QualityTierResult(QualityTier.Low, QualityReasons.BelowMinimumSample, 2);
        Assert.False(AnalyticsQualityPolicy.CanDeclareWinner(a, b));
    }

    [Fact]
    public void CanDeclareWinner_EitherNotApplicable_ReturnsFalse()
    {
        var a = new QualityTierResult(QualityTier.High, null, 30);
        var b = new QualityTierResult(QualityTier.NotApplicable, QualityReasons.ZeroDenominator, 0);
        Assert.False(AnalyticsQualityPolicy.CanDeclareWinner(a, b));
    }

    // ── ReasonToUserMessage ──────────────────────────────────────────────────

    [Theory]
    [InlineData(QualityReasons.BelowMinimumSample, "Échantillon insuffisant — valeur indicative uniquement.")]
    [InlineData(QualityReasons.ZeroDenominator, "Données insuffisantes pour calculer cette métrique.")]
    [InlineData(QualityReasons.InvalidNumerator, "Valeur de numérateur invalide.")]
    [InlineData(null, "Qualité de données limitée.")]
    [InlineData("UNKNOWN_CODE", "Qualité de données limitée.")]
    public void ReasonToUserMessage_KnownReasons_ReturnExpectedText(string? reason, string expected)
    {
        Assert.Equal(expected, AnalyticsQualityPolicy.ReasonToUserMessage(reason));
    }

    // ── Catalog: CAT-03 must be Removed ─────────────────────────────────────

    [Fact]
    public void Catalog_Cat03_IsRemovedStatus()
    {
        var def = HandWStat.Models.Analytics.AnalyticsV3Catalog.Get("CAT-03");
        Assert.NotNull(def);
        Assert.Equal(AnalyticsMetricStatus.Removed, def.Status);
        Assert.NotNull(def.RemovedReason);
    }

    [Fact]
    public void Catalog_Cat03_NotInActiveMetrics()
    {
        var activeCodes = HandWStat.Models.Analytics.AnalyticsV3Catalog.Active
            .Select(m => m.Code)
            .ToList();
        Assert.DoesNotContain("CAT-03", activeCodes);
    }

    // ── Catalog: basic sanity ────────────────────────────────────────────────

    [Fact]
    public void Catalog_AllActiveMetrics_HaveNonEmptyDisplayName()
    {
        foreach (var metric in HandWStat.Models.Analytics.AnalyticsV3Catalog.Active)
        {
            Assert.False(string.IsNullOrWhiteSpace(metric.DisplayName),
                $"Metric {metric.Code} has empty DisplayName");
        }
    }

    [Fact]
    public void Catalog_CompositeMetrics_AreExperimentalNotActive()
    {
        foreach (var code in new[] { "MC-01", "MC-02", "MC-03", "MC-04" })
        {
            var def = HandWStat.Models.Analytics.AnalyticsV3Catalog.Get(code);
            Assert.NotNull(def);
            Assert.Equal(AnalyticsMetricStatus.Experimental, def.Status);
        }
    }

    [Fact]
    public void Catalog_GkMetrics_ApplyOnlyToGk()
    {
        var gkOnlyCodes = new[] { "CAT-13", "CAT-14", "CAT-15", "CAT-16" };
        foreach (var code in gkOnlyCodes)
        {
            var def = HandWStat.Models.Analytics.AnalyticsV3Catalog.Get(code);
            Assert.NotNull(def);
            Assert.Equal(AnalyticsPositionScope.GK, def.ApplicablePositions);
        }
    }

    [Fact]
    public void Catalog_Cat06_IsInAttackNotDefense()
    {
        var def = HandWStat.Models.Analytics.AnalyticsV3Catalog.Get("CAT-06");
        Assert.NotNull(def);
        // Definition must mention Attack / Offense as source, not Defense
        Assert.Contains("Attack", def.Definition, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Defense.PenaltiesWon", def.Definition, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Catalog_Cat10_MentionsPassageForce_AsCorrectField()
    {
        // CAT-10 uses PlayerDefenseStatsDto.PassageForce (NOT PlayerPassingStatsDto.PassageEnForce).
        // The definition may include a disambiguation note referencing PassageEnForce as the wrong field.
        var def = HandWStat.Models.Analytics.AnalyticsV3Catalog.Get("CAT-10");
        Assert.NotNull(def);
        Assert.Contains("PassageForce", def.Definition);
    }
}
