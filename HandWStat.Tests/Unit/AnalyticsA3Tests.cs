using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA3Tests
{
    // ── Position normalisation ────────────────────────────────────────────────

    [Theory]
    [InlineData("GK",         null,               false, AnalyticsPosition.GK)]
    [InlineData("GB",         null,               false, AnalyticsPosition.GK)]
    [InlineData("GARDIENNE",  null,               false, AnalyticsPosition.GK)]
    [InlineData("GARDEN",     null,               false, AnalyticsPosition.GK)]
    [InlineData("GOALKEEPER", null,               false, AnalyticsPosition.GK)]
    [InlineData(null,         "Gardienne de but", false, AnalyticsPosition.GK)]
    [InlineData(null,         "Goalkeeper",       false, AnalyticsPosition.GK)]
    [InlineData("DC",         null,               true,  AnalyticsPosition.GK)] // isGoalkeeper overrides code
    public void Resolve_GoalkeeperVariants_ReturnsGK(
        string? code, string? name, bool isGk, AnalyticsPosition expected)
    {
        Assert.Equal(expected, AnalyticsPositionResolver.Resolve(code, name, isGk));
    }

    [Theory]
    [InlineData("AIL",    null,                  AnalyticsPosition.AIL)]
    [InlineData("AILG",   null,                  AnalyticsPosition.AIL)]
    [InlineData("AIL-G",  null,                  AnalyticsPosition.AIL)]
    [InlineData("ALG",    null,                  AnalyticsPosition.AIL)]  // mock data alias
    [InlineData("ALD",    null,                  AnalyticsPosition.AIL)]
    [InlineData("AILIERE",null,                  AnalyticsPosition.AIL)]
    [InlineData(null,     "Ailière gauche",       AnalyticsPosition.AIL)]
    [InlineData(null,     "Ailière droite",       AnalyticsPosition.AIL)]
    public void Resolve_AilaVariants_ReturnsAIL(string? code, string? name, AnalyticsPosition expected)
    {
        Assert.Equal(expected, AnalyticsPositionResolver.Resolve(code, name));
    }

    [Theory]
    [InlineData("AR",    null,                AnalyticsPosition.AR)]
    [InlineData("ARG",   null,                AnalyticsPosition.AR)]
    [InlineData("ARD",   null,                AnalyticsPosition.AR)]
    [InlineData(null,    "Arrière gauche",    AnalyticsPosition.AR)]
    [InlineData(null,    "Arrière droite",    AnalyticsPosition.AR)]
    public void Resolve_ArrièreVariants_ReturnsAR(string? code, string? name, AnalyticsPosition expected)
    {
        Assert.Equal(expected, AnalyticsPositionResolver.Resolve(code, name));
    }

    [Theory]
    [InlineData("DC",         null,            AnalyticsPosition.DC)]
    [InlineData("DCE",        null,            AnalyticsPosition.DC)]
    [InlineData("DEMI",       null,            AnalyticsPosition.DC)]
    [InlineData("DEMI-CENTRE",null,            AnalyticsPosition.DC)]
    [InlineData("DEMI-CENTER",null,            AnalyticsPosition.DC)]
    [InlineData(null,         "Demi-centre",   AnalyticsPosition.DC)]
    public void Resolve_DemiCentreVariants_ReturnsDC(string? code, string? name, AnalyticsPosition expected)
    {
        Assert.Equal(expected, AnalyticsPositionResolver.Resolve(code, name));
    }

    [Theory]
    [InlineData("PIV",   null,    AnalyticsPosition.PIV)]
    [InlineData("PIVOT", null,    AnalyticsPosition.PIV)]
    [InlineData("P",     null,    AnalyticsPosition.PIV)]
    [InlineData(null,    "Pivot", AnalyticsPosition.PIV)]
    public void Resolve_PivotVariants_ReturnsPIV(string? code, string? name, AnalyticsPosition expected)
    {
        Assert.Equal(expected, AnalyticsPositionResolver.Resolve(code, name));
    }

    [Fact]
    public void Resolve_NullCodeAndName_ReturnsUnknown()
    {
        Assert.Equal(AnalyticsPosition.Unknown, AnalyticsPositionResolver.Resolve(null, null));
    }

    [Fact]
    public void Resolve_UnrecognisedCode_FallsBackToName()
    {
        // Unknown code but recognisable name → resolves via name
        Assert.Equal(AnalyticsPosition.DC, AnalyticsPositionResolver.Resolve("XYZ", "Demi-centre"));
    }

    // ── ParsePositionScope — extended alias coverage ──────────────────────────

    [Theory]
    [InlineData("GARDIENNE",   AnalyticsPositionScope.GK)]
    [InlineData("GARDEN",      AnalyticsPositionScope.GK)]
    [InlineData("ALG",         AnalyticsPositionScope.AIL)]  // was AllField before A3
    [InlineData("AILG",        AnalyticsPositionScope.AIL)]
    [InlineData("AIL-G",       AnalyticsPositionScope.AIL)]
    [InlineData("ALD",         AnalyticsPositionScope.AIL)]
    [InlineData("AILIERE",     AnalyticsPositionScope.AIL)]
    [InlineData("DEMI-CENTRE", AnalyticsPositionScope.DC)]
    [InlineData("DEMI-CENTER", AnalyticsPositionScope.DC)]
    [InlineData("DCE",         AnalyticsPositionScope.DC)]
    [InlineData("ARRIERE",     AnalyticsPositionScope.AR)]
    [InlineData("ARR",         AnalyticsPositionScope.AR)]
    public void ParsePositionScope_NewAliases_ResolveCorrectly(string code, AnalyticsPositionScope expected)
    {
        Assert.Equal(expected, AnalyticsV3Catalog.ParsePositionScope(code));
    }

    [Fact]
    public void ParsePositionScope_UnknownCode_ReturnsAllField()
    {
        // Unknown codes still fall back to AllField (permissive for metric display)
        Assert.Equal(AnalyticsPositionScope.AllField, AnalyticsV3Catalog.ParsePositionScope("UNKNOWN"));
    }

    // ── Position metric profile ───────────────────────────────────────────────

    [Fact]
    public void PositionMetricProfile_GK_DifferentFromDC()
    {
        var gk = PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.GK);
        var dc = PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.DC);
        Assert.False(gk.SequenceEqual(dc));
    }

    [Fact]
    public void PositionMetricProfile_DC_ContainsCAT05()
    {
        Assert.Contains("CAT-05", PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.DC));
    }

    [Fact]
    public void PositionMetricProfile_PIV_ContainsCAT06AndCAT07()
    {
        var primary = PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.PIV);
        Assert.Contains("CAT-06", primary);
        Assert.Contains("CAT-07", primary);
    }

    [Fact]
    public void PositionMetricProfile_AIL_ContainsCAT04()
    {
        Assert.Contains("CAT-04", PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.AIL));
    }

    [Fact]
    public void PositionMetricProfile_AR_ContainsCAT01AndCAT06()
    {
        var primary = PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.AR);
        Assert.Contains("CAT-01", primary);
        Assert.Contains("CAT-06", primary);
    }

    [Fact]
    public void PositionMetricProfile_Unknown_ReturnsEmpty()
    {
        Assert.Empty(PositionMetricProfile.GetPrimaryMetrics(AnalyticsPosition.Unknown));
        Assert.Empty(PositionMetricProfile.GetSecondaryMetrics(AnalyticsPosition.Unknown));
    }

    // ── CanComparePositions ───────────────────────────────────────────────────

    [Fact]
    public void CanComparePositions_SamePosition_Compatible()
    {
        Assert.Equal(
            PositionComparisonCompatibility.Compatible,
            AnalyticsPositionResolver.CanComparePositions(AnalyticsPosition.DC, AnalyticsPosition.DC));
    }

    [Fact]
    public void CanComparePositions_GKVsField_Incompatible()
    {
        Assert.Equal(
            PositionComparisonCompatibility.IncompatibleGkVsField,
            AnalyticsPositionResolver.CanComparePositions(AnalyticsPosition.GK, AnalyticsPosition.DC));
    }

    [Fact]
    public void CanComparePositions_FieldVsGK_Incompatible()
    {
        Assert.Equal(
            PositionComparisonCompatibility.IncompatibleGkVsField,
            AnalyticsPositionResolver.CanComparePositions(AnalyticsPosition.AR, AnalyticsPosition.GK));
    }

    [Fact]
    public void CanComparePositions_DifferentFieldPositions_CommonMetricsOnly()
    {
        Assert.Equal(
            PositionComparisonCompatibility.CommonMetricsOnly,
            AnalyticsPositionResolver.CanComparePositions(AnalyticsPosition.AIL, AnalyticsPosition.AR));
    }

    [Fact]
    public void CanComparePositions_AIL_SameFamily_Compatible()
    {
        // AIL-G and AIL-D both resolve to AIL → Compatible
        var a = AnalyticsPositionResolver.Resolve("AIL-G");
        var b = AnalyticsPositionResolver.Resolve("AIL-D");
        Assert.Equal(PositionComparisonCompatibility.Compatible,
            AnalyticsPositionResolver.CanComparePositions(a, b));
    }

    [Fact]
    public void CanComparePositions_AR_SameFamily_Compatible()
    {
        var a = AnalyticsPositionResolver.Resolve("ARG");
        var b = AnalyticsPositionResolver.Resolve("ARD");
        Assert.Equal(PositionComparisonCompatibility.Compatible,
            AnalyticsPositionResolver.CanComparePositions(a, b));
    }

    // ── ToScope bridge ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(AnalyticsPosition.GK,  AnalyticsPositionScope.GK)]
    [InlineData(AnalyticsPosition.AIL, AnalyticsPositionScope.AIL)]
    [InlineData(AnalyticsPosition.AR,  AnalyticsPositionScope.AR)]
    [InlineData(AnalyticsPosition.DC,  AnalyticsPositionScope.DC)]
    [InlineData(AnalyticsPosition.PIV, AnalyticsPositionScope.PIV)]
    [InlineData(AnalyticsPosition.Unknown, AnalyticsPositionScope.None)]
    public void ToScope_AllPositions_MapsCorrectly(AnalyticsPosition pos, AnalyticsPositionScope expected)
    {
        Assert.Equal(expected, AnalyticsPositionResolver.ToScope(pos));
    }

    // ── CAT-07 canonical source ───────────────────────────────────────────────

    [Fact]
    public void Catalog_CAT07_DefinitionMentionsOffensiveFoulsDrawnAsCanonical()
    {
        var def = AnalyticsV3Catalog.Get("CAT-07");
        Assert.NotNull(def);
        Assert.Contains("OffensiveFoulsDrawn", def!.Definition);
    }

    [Fact]
    public void Catalog_CAT07_DefinitionMentionsPassageForceAsFallback()
    {
        var def = AnalyticsV3Catalog.Get("CAT-07");
        Assert.NotNull(def);
        Assert.Contains("PassageForce", def!.Definition);
        Assert.Contains("Fallback", def.Definition);
    }

    [Fact]
    public void Catalog_CAT07_DefinitionForbidsCumulation()
    {
        var def = AnalyticsV3Catalog.Get("CAT-07");
        Assert.NotNull(def);
        // Must not cumulate both sources
        Assert.Contains("jamais cumuler", def!.Definition);
    }

    // ── CAT-17A definition and formula ────────────────────────────────────────

    [Fact]
    public void Catalog_CAT17A_DisplayName_IsPartDesButs()
    {
        var def = AnalyticsV3Catalog.Get("CAT-17A");
        Assert.NotNull(def);
        Assert.Equal("Part des buts de l'équipe", def!.DisplayName);
    }

    [Fact]
    public void Catalog_CAT17A_Definition_MentionsTotalGoals()
    {
        var def = AnalyticsV3Catalog.Get("CAT-17A");
        Assert.NotNull(def);
        Assert.Contains("TotalGoals", def!.Definition);
    }

    [Fact]
    public void ComputeGoalsSharePct_WithTotalGoals_ReturnsCorrectPct()
    {
        // Player scored 5 total goals, team scored 20 → 25%
        var result = AnalyticsCalculationService.ComputeGoalsSharePct(5, 20);
        Assert.NotNull(result);
        Assert.Equal(25.0, result!.Value, precision: 5);
    }

    [Fact]
    public void ComputeGoalsSharePct_TeamGoalsForZero_ReturnsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeGoalsSharePct(5, 0));
    }

    [Fact]
    public void ComputeGoalsSharePct_ZeroGoals_ReturnsZero()
    {
        var result = AnalyticsCalculationService.ComputeGoalsSharePct(0, 20);
        Assert.NotNull(result);
        Assert.Equal(0.0, result!.Value);
    }

    // ── CAT-20 — status is Active, no frontend percentile recalculation ───────

    [Fact]
    public void Catalog_CAT20_StatusIsActive()
    {
        var def = AnalyticsV3Catalog.Get("CAT-20");
        Assert.NotNull(def);
        Assert.Equal(AnalyticsMetricStatus.Active, def!.Status);
    }

    [Fact]
    public void Catalog_CAT20_DefinitionMentionsApiPercentile()
    {
        var def = AnalyticsV3Catalog.Get("CAT-20");
        Assert.NotNull(def);
        // Must reference API as the percentile source
        Assert.Contains("Percentile", def!.Definition);
        Assert.Contains("API", def.Definition);
    }

    // ── Radar axis config ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(AnalyticsPosition.GK)]
    [InlineData(AnalyticsPosition.AIL)]
    [InlineData(AnalyticsPosition.AR)]
    [InlineData(AnalyticsPosition.DC)]
    [InlineData(AnalyticsPosition.PIV)]
    public void PositionRadarAxisConfig_AllPositions_HasFiveAxes(AnalyticsPosition pos)
    {
        var axes = PositionRadarAxisConfig.GetAxes(pos);
        Assert.Equal(5, axes.Count);
    }

    [Fact]
    public void PositionRadarAxisConfig_GK_HasNoBackendGaps()
    {
        // All GK axes are supported by existing API data
        Assert.False(PositionRadarAxisConfig.HasBackendGaps(AnalyticsPosition.GK));
    }

    [Fact]
    public void PositionRadarAxisConfig_DC_HasBackendGap_ForAssistsPer60()
    {
        // DC "Création" (AssistsPer60) is not yet in the API
        Assert.True(PositionRadarAxisConfig.HasBackendGaps(AnalyticsPosition.DC));
    }

    [Fact]
    public void PositionRadarAxisConfig_Unknown_ReturnsEmpty()
    {
        Assert.Empty(PositionRadarAxisConfig.GetAxes(AnalyticsPosition.Unknown));
    }
}
