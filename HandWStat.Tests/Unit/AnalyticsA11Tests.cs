using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

// Phase A11 — Metric Dictionary, Data Lineage, UI/Export Parity
// TEST_BASELINE_A11 = 1080

public class AnalyticsA11Tests
{
    // ── §29 — Metric Dictionary tests ─────────────────────────────────────────

    [Fact]
    public void Dictionary_AllActiveMetrics_HaveNonBlankTechnicalName()
    {
        foreach (var m in AnalyticsV3Catalog.Active)
            Assert.False(string.IsNullOrWhiteSpace(m.TechnicalName), $"{m.Code} missing TechnicalName");
    }

    [Fact]
    public void Dictionary_AllActiveMetrics_HaveNonBlankDisplayName()
    {
        foreach (var m in AnalyticsV3Catalog.Active)
            Assert.False(string.IsNullOrWhiteSpace(m.DisplayName), $"{m.Code} missing DisplayName");
    }

    [Fact]
    public void Dictionary_AllActiveMetrics_HaveNonBlankDefinition()
    {
        foreach (var m in AnalyticsV3Catalog.Active)
            Assert.False(string.IsNullOrWhiteSpace(m.Definition), $"{m.Code} missing Definition");
    }

    [Fact]
    public void Dictionary_AllActiveMetrics_HaveApplicablePositionsNotNone()
    {
        foreach (var m in AnalyticsV3Catalog.Active)
            Assert.NotEqual(AnalyticsPositionScope.None, m.ApplicablePositions);
    }

    [Fact]
    public void Dictionary_Catalog_NoDuplicateTechnicalName()
    {
        var names = AnalyticsV3Catalog.All.Values
            .Where(m => m.Status != AnalyticsMetricStatus.Removed)
            .Select(m => m.TechnicalName)
            .ToList();
        var duplicates = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(duplicates);
    }

    [Fact]
    public void Dictionary_Catalog_NoDuplicateCode()
    {
        var codes = AnalyticsV3Catalog.All.Keys.ToList();
        Assert.Equal(codes.Count, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Dictionary_CAT03_IsRemoved()
    {
        var cat03 = AnalyticsV3Catalog.Get("CAT-03");
        Assert.NotNull(cat03);
        Assert.Equal(AnalyticsMetricStatus.Removed, cat03!.Status);
    }

    [Fact]
    public void Dictionary_CAT03_NotInActiveList()
    {
        Assert.DoesNotContain(AnalyticsV3Catalog.Active, m => m.Code == "CAT-03");
    }

    [Fact]
    public void Dictionary_CAT03_ApplicablePositions_IsNone()
    {
        var cat03 = AnalyticsV3Catalog.Get("CAT-03");
        Assert.Equal(AnalyticsPositionScope.None, cat03!.ApplicablePositions);
    }

    [Fact]
    public void Dictionary_ExperimentalMetrics_NotInActiveList()
    {
        var experimental = AnalyticsV3Catalog.All.Values
            .Where(m => m.Status == AnalyticsMetricStatus.Experimental)
            .Select(m => m.Code)
            .ToHashSet();
        Assert.DoesNotContain(AnalyticsV3Catalog.Active, m => experimental.Contains(m.Code));
    }

    // ── §29 — GetDictionaryEntry projection tests ─────────────────────────────

    [Fact]
    public void DictionaryEntry_CAT01_HasCorrectFields()
    {
        var entry = AnalyticsV3Catalog.GetDictionaryEntry("CAT-01");
        Assert.Equal("CAT-01", entry.Code);
        Assert.Equal("goals_created_per60", entry.TechnicalName);
        Assert.Equal(AnalyticsMetricUnit.Per60, entry.Unit);
        Assert.Equal(AnalyticsMetricGrain.Player, entry.Grain);
        Assert.Equal(AnalyticsMetricCategory.Offensive, entry.Category);
        Assert.True(entry.HigherIsBetter);
    }

    [Fact]
    public void DictionaryEntry_CAT23_HasZoneGrain()
    {
        var entry = AnalyticsV3Catalog.GetDictionaryEntry("CAT-23");
        Assert.Equal(AnalyticsMetricGrain.Zone, entry.Grain);
        Assert.Equal(AnalyticsMetricCategory.Spatial, entry.Category);
    }

    [Fact]
    public void DictionaryEntry_CAT15_HasGoalkeeperCategory()
    {
        var entry = AnalyticsV3Catalog.GetDictionaryEntry("CAT-15");
        Assert.Equal(AnalyticsMetricCategory.Goalkeeper, entry.Category);
    }

    [Fact]
    public void DictionaryEntry_CAT01_FormulaExtracted()
    {
        var entry = AnalyticsV3Catalog.GetDictionaryEntry("CAT-01");
        Assert.NotNull(entry.Formula);
        Assert.Contains("TotalGoals", entry.Formula);
    }

    [Fact]
    public void ActiveDictionaryEntries_CountMatchesActiveCatalog()
    {
        var activeCount = AnalyticsV3Catalog.Active.Count();
        var entryCount = AnalyticsV3Catalog.ActiveDictionaryEntries.Count();
        Assert.Equal(activeCount, entryCount);
    }

    // ── §30 — Lineage tests ───────────────────────────────────────────────────

    [Fact]
    public void Lineage_CAT15_PrimaryField_ContainsSavesPer60()
    {
        var lineage = AnalyticsMetricLineage.Get("CAT-15");
        Assert.NotNull(lineage);
        Assert.Contains("SavesPer60", lineage!.PrimaryField, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Lineage_CAT06_PrimaryField_ContainsPenaltiesWon()
    {
        var lineage = AnalyticsMetricLineage.Get("CAT-06");
        Assert.NotNull(lineage);
        Assert.Contains("PenaltiesWon", lineage!.PrimaryField, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Lineage_CAT07_Notes_MentionsV2Source()
    {
        var lineage = AnalyticsMetricLineage.Get("CAT-07");
        Assert.NotNull(lineage);
        Assert.Contains("v2", lineage!.Notes, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Lineage_CAT07_FallbackSource_MentionsPassageForce()
    {
        var lineage = AnalyticsMetricLineage.Get("CAT-07");
        Assert.Contains("PassageForce", lineage!.FallbackSource, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Lineage_CAT21_FallbackSource_NotNull()
    {
        var lineage = AnalyticsMetricLineage.Get("CAT-21");
        Assert.NotNull(lineage);
        Assert.NotNull(lineage!.FallbackSource);
    }

    [Fact]
    public void Lineage_CAT23_CalculationSource_IsSpatialAnalyticsBuilder()
    {
        var lineage = AnalyticsMetricLineage.Get("CAT-23");
        Assert.NotNull(lineage);
        Assert.Contains("SpatialAnalyticsBuilder", lineage!.CalculationSource, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Lineage_AllEntries_HaveMetricCode()
    {
        foreach (var entry in AnalyticsMetricLineage.All.Values)
            Assert.False(string.IsNullOrWhiteSpace(entry.MetricCode));
    }

    [Fact]
    public void Lineage_AllEntries_HavePrimaryField()
    {
        foreach (var entry in AnalyticsMetricLineage.All.Values)
            Assert.False(string.IsNullOrWhiteSpace(entry.PrimaryField),
                $"Lineage for {entry.MetricCode} is missing PrimaryField");
    }

    [Fact]
    public void Lineage_KeyCodesPresent()
    {
        var required = new[] { "CAT-01", "CAT-06", "CAT-07", "CAT-08", "CAT-09", "CAT-10", "CAT-11", "CAT-15", "CAT-21", "CAT-23" };
        foreach (var code in required)
            Assert.NotNull(AnalyticsMetricLineage.Get(code));
    }

    // ── §31 — UI/Export parity tests ─────────────────────────────────────────

    [Fact]
    public void Parity_CAT01_CalculationService_UsesCorrectInputs()
    {
        // Verify that AnalyticsCalculationService.ComputeGoalsCreatedPer60 uses
        // TotalGoals + AssistCount (same formula documented in catalog CAT-01).
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(
            totalGoals: 10, assists: 5, playingTimeMinutes: 300);
        Assert.NotNull(result);
        // (10 + 5) / 300 * 60 = 3.0
        Assert.Equal(3.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Parity_CAT21_CalculationService_SameFormula()
    {
        // CAT-21: TotalSaves / TotalShotsFaced × 100
        var result = AnalyticsCalculationService.ComputeTotalSaveRate(totalSaves: 7, totalShotsFaced: 10);
        Assert.NotNull(result);
        Assert.Equal(70.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Parity_CAT04_CalculationService_SameFormula()
    {
        // CAT-04: GoalCount / OpenShotAttempts × 100
        var result = AnalyticsCalculationService.ComputeOpenPlaySuccessRate(goalCount: 4, openShotAttempts: 10);
        Assert.NotNull(result);
        Assert.Equal(40.0, result!.Value, precision: 9);
    }

    [Fact]
    public void Parity_MatchShotRate_OfficialGoalsDivideAttempts()
    {
        // A10 MatchTeamAnalytics: ShotRate = Goals (official) / Attempts
        // Attempts = officialGoals + (ShotAttempts - TotalGoals) = 3 + (10-3) = 10
        var analytics = MatchAnalyticsBuilder.BuildTeamAnalytics(1, "T", officialGoals: 3,
            teamPlayers: [new() { ShotAttempts = 10, TotalGoals = 3 }]);
        Assert.Equal(3.0 / 10.0, analytics.ShotRate!.Value, precision: 9);
    }

    // ── §32 — Null / Zero tests ───────────────────────────────────────────────

    [Fact]
    public void Formatter_Null_FormatForCsv_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, MetricValueFormatter.FormatForCsv(null, AnalyticsMetricUnit.Percent));
    }

    [Fact]
    public void Formatter_RealZero_FormatForCsv_ReturnsZero()
    {
        Assert.Equal("0", MetricValueFormatter.FormatForCsv(0.0, AnalyticsMetricUnit.Count));
    }

    [Fact]
    public void Formatter_Null_FormatForUi_ReturnsDash()
    {
        Assert.Equal("—", MetricValueFormatter.FormatForUi(null, AnalyticsMetricUnit.Percent));
    }

    [Fact]
    public void Formatter_RealZero_FormatForUi_ReturnsZeroWithUnit()
    {
        var result = MetricValueFormatter.FormatForUi(0.0, AnalyticsMetricUnit.Percent);
        Assert.Contains("0", result);
    }

    [Fact]
    public void Formatter_Null_FormatForPdf_ReturnsDash()
    {
        Assert.Equal("—", MetricValueFormatter.FormatForPdf(null, AnalyticsMetricUnit.Per60));
    }

    // ── §33 — Position export gate ────────────────────────────────────────────

    [Fact]
    public void Position_GkMetrics_NotApplicableToFieldPositions()
    {
        // CAT-15 is GK-only — must not apply to field positions
        var cat15 = AnalyticsV3Catalog.Get("CAT-15")!;
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "AR"));
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "DC"));
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "PIV"));
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat15, "AIL"));
    }

    [Fact]
    public void Position_FieldMetrics_NotApplicableToGk()
    {
        // CAT-01 is AllField — must not apply to GK
        var cat01 = AnalyticsV3Catalog.Get("CAT-01")!;
        Assert.False(AnalyticsV3Catalog.IsApplicable(cat01, "GK"));
    }

    [Fact]
    public void Position_CAT11_AppliesToAll()
    {
        // CAT-11 sanctions — applies to all positions including GK
        var cat11 = AnalyticsV3Catalog.Get("CAT-11")!;
        Assert.True(AnalyticsV3Catalog.IsApplicable(cat11, "GK"));
        Assert.True(AnalyticsV3Catalog.IsApplicable(cat11, "AR"));
        Assert.True(AnalyticsV3Catalog.IsApplicable(cat11, "DC"));
    }

    // ── §34 — Removed metrics ─────────────────────────────────────────────────

    [Fact]
    public void Removed_CAT03_HasRemovedReason()
    {
        var cat03 = AnalyticsV3Catalog.Get("CAT-03")!;
        Assert.False(string.IsNullOrWhiteSpace(cat03.RemovedReason));
    }

    // ── §35 — Format consistency ──────────────────────────────────────────────

    [Fact]
    public void Format_Percent_FormatForUi_FrCulture_UsesCommaDecimal()
    {
        // French locale: 66.6666... → "66,7 %"
        var result = MetricValueFormatter.FormatForUi(66.6666, AnalyticsMetricUnit.Percent, "fr-FR");
        Assert.Equal("66,7 %", result);
    }

    [Fact]
    public void Format_Percent_FormatForCsv_UsesInvariantCulture()
    {
        // CSV: machine-readable, dot decimal separator
        var result = MetricValueFormatter.FormatForCsv(66.6666, AnalyticsMetricUnit.Percent);
        Assert.Contains(".", result);
        Assert.Contains("66", result);
    }

    [Fact]
    public void Format_Per60_FormatForUi_IncludesUnit()
    {
        var result = MetricValueFormatter.FormatForUi(2.5, AnalyticsMetricUnit.Per60, "fr-FR");
        Assert.Contains("/60", result);
    }

    [Fact]
    public void Format_Count_FormatForCsv_ReturnsInteger()
    {
        var result = MetricValueFormatter.FormatForCsv(42.9, AnalyticsMetricUnit.Count);
        Assert.Equal("43", result);
    }

    // ── §6 — Export parity: NormalizeRadarValue uses API percentile ──────────

    [Fact]
    public void ExportParity_NormalizeRadarValue_UsesApiPercentile_NotMinMax()
    {
        // Even when min/max are finite and meaningful, NormalizeRadarValue must return
        // the API percentile (not min-max). This is the A9/A11 parity invariant.
        var axis = new PositionProfileAxisDto
        {
            Label = "Test",
            Key = "test",
            Value = 10.0,
            MedianValue = 5.0,
            MinValue = 0.0,
            MaxValue = 20.0,
            Percentile = 75.0,
            HigherIsBetter = true,
        };
        var result = PlayerSheetExportHelper.NormalizeRadarValue(axis, axis.Value);
        // Must return Percentile=75.0, NOT min-max=(10-0)/(20-0)*100=50.0
        Assert.Equal(75.0, result, precision: 9);
    }
}
