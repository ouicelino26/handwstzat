using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA8Tests
{
    // ── GetGroupCompatibility ────────────────────────────────────────────────────

    [Fact]
    public void GetGroupCompatibility_Empty_Compatible()
    {
        var result = CompareAnalyticsBuilder.GetGroupCompatibility([]);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result);
    }

    [Fact]
    public void GetGroupCompatibility_Single_Compatible()
    {
        var result = CompareAnalyticsBuilder.GetGroupCompatibility([AnalyticsPosition.AR]);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result);
    }

    [Fact]
    public void GetGroupCompatibility_ArVsAr_Compatible()
    {
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(
            [AnalyticsPosition.AR, AnalyticsPosition.AR]);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result);
    }

    [Fact]
    public void GetGroupCompatibility_GkVsGk_Compatible()
    {
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(
            [AnalyticsPosition.GK, AnalyticsPosition.GK]);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result);
    }

    [Fact]
    public void GetGroupCompatibility_ArVsDc_CommonMetricsOnly()
    {
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(
            [AnalyticsPosition.AR, AnalyticsPosition.DC]);
        Assert.Equal(PositionComparisonCompatibility.CommonMetricsOnly, result);
    }

    [Fact]
    public void GetGroupCompatibility_GkVsAr_IncompatibleGkVsField()
    {
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(
            [AnalyticsPosition.GK, AnalyticsPosition.AR]);
        Assert.Equal(PositionComparisonCompatibility.IncompatibleGkVsField, result);
    }

    [Fact]
    public void GetGroupCompatibility_SixAr_Compatible()
    {
        var positions = Enumerable.Repeat(AnalyticsPosition.AR, 6).ToList();
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(positions);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result);
    }

    [Fact]
    public void GetGroupCompatibility_GkPlusFiveAr_IncompatibleGkVsField()
    {
        var positions = new[] { AnalyticsPosition.GK }
            .Concat(Enumerable.Repeat(AnalyticsPosition.AR, 5))
            .ToList();
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(positions);
        Assert.Equal(PositionComparisonCompatibility.IncompatibleGkVsField, result);
    }

    [Fact]
    public void GetGroupCompatibility_ShortCircuitsOnGkVsField()
    {
        // GK at the end — still detected immediately via worst-case pairwise
        var positions = new[]
        {
            AnalyticsPosition.AR, AnalyticsPosition.DC,
            AnalyticsPosition.PIV, AnalyticsPosition.GK,
        };
        var result = CompareAnalyticsBuilder.GetGroupCompatibility(positions);
        Assert.Equal(PositionComparisonCompatibility.IncompatibleGkVsField, result);
    }

    // ── GetCompatibility ─────────────────────────────────────────────────────────

    [Fact]
    public void GetCompatibility_Empty_CompatibleNoGkVsField()
    {
        var result = CompareAnalyticsBuilder.GetCompatibility([]);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result.Compatibility);
        Assert.False(result.HasGkVsField);
    }

    [Fact]
    public void GetCompatibility_GkAndField_HasGkVsFieldTrue()
    {
        var players = new[]
        {
            new ComparePlayerProfile(1, AnalyticsPosition.GK),
            new ComparePlayerProfile(2, AnalyticsPosition.AR),
        };
        var result = CompareAnalyticsBuilder.GetCompatibility(players);
        Assert.True(result.HasGkVsField);
        Assert.Equal(PositionComparisonCompatibility.IncompatibleGkVsField, result.Compatibility);
    }

    [Fact]
    public void GetCompatibility_OnlyField_HasGkVsFieldFalse()
    {
        var players = new[]
        {
            new ComparePlayerProfile(1, AnalyticsPosition.AR),
            new ComparePlayerProfile(2, AnalyticsPosition.DC),
        };
        var result = CompareAnalyticsBuilder.GetCompatibility(players);
        Assert.False(result.HasGkVsField);
    }

    [Fact]
    public void GetCompatibility_OnlyGk_HasGkVsFieldFalse()
    {
        var players = new[]
        {
            new ComparePlayerProfile(1, AnalyticsPosition.GK),
            new ComparePlayerProfile(2, AnalyticsPosition.GK),
        };
        var result = CompareAnalyticsBuilder.GetCompatibility(players);
        Assert.False(result.HasGkVsField);
    }

    [Fact]
    public void GetCompatibility_DuplicatePlayers_NoError()
    {
        var players = Enumerable.Range(1, 6)
            .Select(i => new ComparePlayerProfile(i, AnalyticsPosition.AR))
            .ToList();
        var result = CompareAnalyticsBuilder.GetCompatibility(players);
        Assert.Equal(PositionComparisonCompatibility.Compatible, result.Compatibility);
        Assert.Equal(6, result.Positions.Count);
    }

    // ── GetComparableMetricCodes ──────────────────────────────────────────────────

    [Fact]
    public void GetComparableMetricCodes_GkVsAr_OnlyAllScope()
    {
        var codes = CompareAnalyticsBuilder.GetComparableMetricCodes(
            [AnalyticsPosition.GK, AnalyticsPosition.AR],
            PositionComparisonCompatibility.IncompatibleGkVsField);

        // Active All-scope metrics: CAT-11 and CAT-20
        Assert.Contains("CAT-11", codes);
        Assert.Contains("CAT-20", codes);

        // No position-specific metrics
        Assert.DoesNotContain("CAT-01", codes); // AllField only
        Assert.DoesNotContain("CAT-13", codes); // GK only
        Assert.DoesNotContain("CAT-06", codes); // AR/AIL/PIV only
    }

    [Fact]
    public void GetComparableMetricCodes_ArAndDc_CommonIntersection_IncludesCat01AndCat08()
    {
        var codes = CompareAnalyticsBuilder.GetComparableMetricCodes(
            [AnalyticsPosition.AR, AnalyticsPosition.DC],
            PositionComparisonCompatibility.CommonMetricsOnly);

        Assert.Contains("CAT-01", codes); // AllField — in both AR and DC
        Assert.Contains("CAT-08", codes); // DC|AR|AIL|PIV — in both
    }

    [Fact]
    public void GetComparableMetricCodes_ArAndDc_ExcludesCat06_ArOnly()
    {
        var codes = CompareAnalyticsBuilder.GetComparableMetricCodes(
            [AnalyticsPosition.AR, AnalyticsPosition.DC],
            PositionComparisonCompatibility.CommonMetricsOnly);

        // CAT-06 (PIV|AR|AIL) — not applicable to DC, excluded from intersection
        Assert.DoesNotContain("CAT-06", codes);
    }

    [Fact]
    public void GetComparableMetricCodes_ArAndDc_ExcludesCat07_DcOnly()
    {
        var codes = CompareAnalyticsBuilder.GetComparableMetricCodes(
            [AnalyticsPosition.AR, AnalyticsPosition.DC],
            PositionComparisonCompatibility.CommonMetricsOnly);

        // CAT-07 (PIV|DC|AIL) — not applicable to AR, excluded from intersection
        Assert.DoesNotContain("CAT-07", codes);
    }

    [Fact]
    public void GetComparableMetricCodes_GkVsGk_IncludesGkSpecificMetrics()
    {
        var codes = CompareAnalyticsBuilder.GetComparableMetricCodes(
            [AnalyticsPosition.GK, AnalyticsPosition.GK],
            PositionComparisonCompatibility.Compatible);

        Assert.Contains("CAT-13", codes);
        Assert.Contains("CAT-14", codes);
        Assert.Contains("CAT-21", codes);
        Assert.Contains("CAT-22", codes);
    }

    [Fact]
    public void GetComparableMetricCodes_Empty_ReturnsEmpty()
    {
        var codes = CompareAnalyticsBuilder.GetComparableMetricCodes(
            [], PositionComparisonCompatibility.Compatible);
        Assert.Empty(codes);
    }

    // ── CanDeclareWinner ──────────────────────────────────────────────────────────

    private static CompareMetricValue MakeValue(int playerId, double? value, QualityTier tier, bool isApplicable = true)
    {
        var quality = new QualityTierResult(tier, null, null);
        return new CompareMetricValue(playerId, value, quality, isApplicable);
    }

    [Fact]
    public void CanDeclareWinner_TwoHighQuality_ReturnsTrue()
    {
        var values = new[]
        {
            MakeValue(1, 75.0, QualityTier.High),
            MakeValue(2, 65.0, QualityTier.High),
        };
        Assert.True(CompareAnalyticsBuilder.CanDeclareWinner(values, higherIsBetter: true));
    }

    [Fact]
    public void CanDeclareWinner_MediumQuality_ReturnsTrue()
    {
        var values = new[]
        {
            MakeValue(1, 75.0, QualityTier.Medium),
            MakeValue(2, 65.0, QualityTier.Medium),
        };
        Assert.True(CompareAnalyticsBuilder.CanDeclareWinner(values, higherIsBetter: true));
    }

    [Fact]
    public void CanDeclareWinner_OneLow_ReturnsFalse()
    {
        var values = new[]
        {
            MakeValue(1, 75.0, QualityTier.High),
            MakeValue(2, 65.0, QualityTier.Low),
        };
        Assert.False(CompareAnalyticsBuilder.CanDeclareWinner(values, higherIsBetter: true));
    }

    [Fact]
    public void CanDeclareWinner_OneNotApplicable_ReturnsFalse()
    {
        var values = new[]
        {
            MakeValue(1, 75.0, QualityTier.High),
            MakeValue(2, null, QualityTier.NotApplicable, isApplicable: false),
        };
        Assert.False(CompareAnalyticsBuilder.CanDeclareWinner(values, higherIsBetter: true));
    }

    [Fact]
    public void CanDeclareWinner_SingleValue_ReturnsFalse()
    {
        var values = new[] { MakeValue(1, 75.0, QualityTier.High) };
        Assert.False(CompareAnalyticsBuilder.CanDeclareWinner(values, higherIsBetter: true));
    }

    [Fact]
    public void CanDeclareWinner_NullValues_ReturnsFalse()
    {
        var values = new[]
        {
            MakeValue(1, null, QualityTier.High),
            MakeValue(2, null, QualityTier.High),
        };
        Assert.False(CompareAnalyticsBuilder.CanDeclareWinner(values, higherIsBetter: true));
    }

    // ── GetWinnerPlayerId ─────────────────────────────────────────────────────────

    [Fact]
    public void GetWinnerPlayerId_HigherIsBetter_ReturnsHigher()
    {
        var values = new[]
        {
            MakeValue(1, 80.0, QualityTier.High),
            MakeValue(2, 60.0, QualityTier.High),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: true);
        Assert.Equal(1, winner);
    }

    [Fact]
    public void GetWinnerPlayerId_LowerIsBetter_ReturnsLower()
    {
        // CAT-08 turnovers/60: lower is better
        var values = new[]
        {
            MakeValue(1, 3.0, QualityTier.High),  // fewer turnovers
            MakeValue(2, 7.0, QualityTier.High),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: false);
        Assert.Equal(1, winner);
    }

    [Fact]
    public void GetWinnerPlayerId_Tie_ReturnsNull()
    {
        var values = new[]
        {
            MakeValue(1, 70.0, QualityTier.High),
            MakeValue(2, 70.0, QualityTier.High),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: true);
        Assert.Null(winner);
    }

    [Fact]
    public void GetWinnerPlayerId_TieWithinEpsilon_ReturnsNull()
    {
        // Values differ by less than 1e-9 — treated as tie
        var values = new[]
        {
            MakeValue(1, 70.0, QualityTier.High),
            MakeValue(2, 70.0 + 1e-10, QualityTier.High),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: true);
        Assert.Null(winner);
    }

    [Fact]
    public void GetWinnerPlayerId_LowQuality_ReturnsNull()
    {
        var values = new[]
        {
            MakeValue(1, 80.0, QualityTier.Low),
            MakeValue(2, 60.0, QualityTier.Low),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: true);
        Assert.Null(winner);
    }

    [Fact]
    public void GetWinnerPlayerId_ZeroValueApplicable_CanWin()
    {
        // Value=0.0 is applicable (not null) — zero can beat a non-zero lower-is-better
        var values = new[]
        {
            MakeValue(1, 0.0, QualityTier.High),
            MakeValue(2, 5.0, QualityTier.High),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: false);
        Assert.Equal(1, winner); // 0 turnovers/60 wins vs 5
    }

    [Fact]
    public void GetWinnerPlayerId_NullValue_NotApplicableIgnored()
    {
        // null value player is not applicable — cannot be declared winner
        var values = new[]
        {
            MakeValue(1, 70.0, QualityTier.High, isApplicable: true),
            MakeValue(2, null, QualityTier.High, isApplicable: false),
        };
        var winner = CompareAnalyticsBuilder.GetWinnerPlayerId(values, higherIsBetter: true);
        Assert.Null(winner); // < 2 applicable → no winner
    }

    // ── GetPositionRadarAxes ──────────────────────────────────────────────────────

    [Fact]
    public void GetPositionRadarAxes_BackendGap_IsOnRadarFalse()
    {
        // AR has "Création" (IsBackendGap=true)
        var arAxes = PositionRadarAxisConfig.GetAxes(AnalyticsPosition.AR);
        var gapAxis = arAxes.First(a => a.IsBackendGap);

        // Build perAxisValues matching config order
        var perAxisValues = arAxes
            .Select(_ => new[] { (PlayerId: 1, Value: (double?)50.0) })
            .ToList();

        var result = CompareAnalyticsBuilder.GetPositionRadarAxes(AnalyticsPosition.AR, perAxisValues);
        var gapResult = result.First(r => r.IsBackendGap);
        Assert.False(gapResult.IsOnRadar);
    }

    [Fact]
    public void GetPositionRadarAxes_NonGapAxis_IsOnRadarTrue()
    {
        var gkAxes = PositionRadarAxisConfig.GetAxes(AnalyticsPosition.GK);

        // GK has no backend gaps — all axes should be on radar
        var perAxisValues = gkAxes
            .Select(_ => new[] { (PlayerId: 1, Value: (double?)30.0) })
            .ToList();

        var result = CompareAnalyticsBuilder.GetPositionRadarAxes(AnalyticsPosition.GK, perAxisValues);
        Assert.All(result, r => Assert.True(r.IsOnRadar));
    }

    [Fact]
    public void GetPositionRadarAxes_UsesAbsoluteValues()
    {
        // Absolute value 75.5 must be preserved exactly — no normalization
        var gkAxes = PositionRadarAxisConfig.GetAxes(AnalyticsPosition.GK);
        var perAxisValues = gkAxes
            .Select(_ => new[] { (PlayerId: 1, Value: (double?)75.5) })
            .ToList();

        var result = CompareAnalyticsBuilder.GetPositionRadarAxes(AnalyticsPosition.GK, perAxisValues);
        Assert.All(result, r =>
        {
            var playerVal = Assert.Single(r.PlayerValues);
            Assert.Equal(75.5, playerVal.Value, precision: 5);
        });
    }

    [Fact]
    public void GetPositionRadarAxes_NullValueFiltered()
    {
        var gkAxes = PositionRadarAxisConfig.GetAxes(AnalyticsPosition.GK);
        var perAxisValues = gkAxes
            .Select(_ => new[] { (PlayerId: 1, Value: (double?)null) })
            .ToList();

        var result = CompareAnalyticsBuilder.GetPositionRadarAxes(AnalyticsPosition.GK, perAxisValues);
        Assert.All(result, r => Assert.Empty(r.PlayerValues));
    }

    // ── GetCommonRadarAxisLabels ──────────────────────────────────────────────────

    [Fact]
    public void GetCommonRadarAxisLabels_ArAndDc_ReturnsIntersectionOfNonGapAxes()
    {
        // AR non-gap: Production, Finition, 7m obtenus, Maîtrise
        // DC non-gap: A:T, Maîtrise, Défense, Production
        // Intersection: Production, Maîtrise
        var labels = CompareAnalyticsBuilder.GetCommonRadarAxisLabels(
            [AnalyticsPosition.AR, AnalyticsPosition.DC]);

        Assert.Contains("Production", labels);
        Assert.Contains("Maîtrise", labels);
    }

    [Fact]
    public void GetCommonRadarAxisLabels_ArAndDc_DoesNotIncludeGapAxes()
    {
        // "Création" is backend gap for both AR and DC — must not appear
        var labels = CompareAnalyticsBuilder.GetCommonRadarAxisLabels(
            [AnalyticsPosition.AR, AnalyticsPosition.DC]);
        Assert.DoesNotContain("Création", labels);
    }

    [Fact]
    public void GetCommonRadarAxisLabels_GkAndAr_Empty()
    {
        // GK non-gap: Arrêts, Jeu ouvert, 7 mètres, Arrêts /60, Charge subie
        // AR non-gap: Production, Finition, 7m obtenus, Maîtrise
        // No labels in common
        var labels = CompareAnalyticsBuilder.GetCommonRadarAxisLabels(
            [AnalyticsPosition.GK, AnalyticsPosition.AR]);
        Assert.Empty(labels);
    }

    [Fact]
    public void GetCommonRadarAxisLabels_Empty_ReturnsEmpty()
    {
        var labels = CompareAnalyticsBuilder.GetCommonRadarAxisLabels([]);
        Assert.Empty(labels);
    }

    [Fact]
    public void GetCommonRadarAxisLabels_SinglePosition_ReturnsNonGapAxes()
    {
        // Single position: all non-gap axes for that position
        var labels = CompareAnalyticsBuilder.GetCommonRadarAxisLabels([AnalyticsPosition.GK]);
        Assert.Contains("Arrêts", labels);
        Assert.Contains("Jeu ouvert", labels);
        Assert.DoesNotContain("Création", labels); // not a GK axis
    }
}
