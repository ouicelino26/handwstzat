using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA4Tests
{
    private const double Tol = 1e-9;

    // ── ComputeTotalSaveRate ──────────────────────────────────────────────────

    [Fact]
    public void TotalSaveRate_Normal_ReturnsCorrectValue()
    {
        // 10 saves / 30 shots = 33.33%
        var result = AnalyticsCalculationService.ComputeTotalSaveRate(10, 30);
        Assert.NotNull(result);
        Assert.Equal(10.0 / 30.0 * 100.0, result.Value, Tol);
    }

    [Fact]
    public void TotalSaveRate_ZeroSaves_ReturnsZero()
    {
        var result = AnalyticsCalculationService.ComputeTotalSaveRate(0, 10);
        Assert.NotNull(result);
        Assert.Equal(0.0, result.Value, Tol);
    }

    [Fact]
    public void TotalSaveRate_ZeroFaced_ReturnsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeTotalSaveRate(0, 0));
    }

    [Fact]
    public void TotalSaveRate_PerfectRate_Returns100()
    {
        var result = AnalyticsCalculationService.ComputeTotalSaveRate(15, 15);
        Assert.NotNull(result);
        Assert.Equal(100.0, result.Value, Tol);
    }

    // ── ComputeGoalsConcededPer60 ─────────────────────────────────────────────

    [Fact]
    public void GoalsConcededPer60_Normal_ReturnsCorrectValue()
    {
        // 6 goals in 90 min = 4.0 /60
        var result = AnalyticsCalculationService.ComputeGoalsConcededPer60(6, 90);
        Assert.NotNull(result);
        Assert.Equal(4.0, result.Value, Tol);
    }

    [Fact]
    public void GoalsConcededPer60_ZeroPlayingTime_ReturnsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeGoalsConcededPer60(3, 0));
    }

    [Fact]
    public void GoalsConcededPer60_NegativePlayingTime_ReturnsNull()
    {
        Assert.Null(AnalyticsCalculationService.ComputeGoalsConcededPer60(3, -10));
    }

    [Fact]
    public void GoalsConcededPer60_ZeroGoals_ReturnsZero()
    {
        var result = AnalyticsCalculationService.ComputeGoalsConcededPer60(0, 200);
        Assert.NotNull(result);
        Assert.Equal(0.0, result.Value, Tol);
    }

    // ── GoalkeeperAnalyticsBuilder.Build — computation ────────────────────────

    [Fact]
    public void Build_TotalSaveRate_ComputedFromCounts()
    {
        // No API rate provided — should fall back to computed formula
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 10, totalShotsFaced: 30,
            openPlaySaves: 8, openPlayShotsFaced: 25,
            penaltySaves: 2, penaltyShotsFaced: 5,
            goalsConceded: 20, playingTimeMinutes: 90, matchesPlayed: 5);

        Assert.NotNull(vm.TotalSaveRate);
        Assert.Equal(10.0 / 30.0 * 100.0, vm.TotalSaveRate.Value, Tol);
    }

    [Fact]
    public void Build_TotalSaveRate_ApiTakesPrecedenceOverComputed()
    {
        // API rate = 55.0, computed would be 10/30 = 33.33%
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 10, totalShotsFaced: 30,
            openPlaySaves: 8, openPlayShotsFaced: 25,
            penaltySaves: 2, penaltyShotsFaced: 5,
            goalsConceded: 20, playingTimeMinutes: 90, matchesPlayed: 5,
            apiTotalSaveRate: 55.0);

        Assert.Equal(55.0, vm.TotalSaveRate);
    }

    [Fact]
    public void Build_LegacyRateFallsBackWhenNoApiAndZeroFaced()
    {
        // totalShotsFaced = 0 → computed returns null → should use legacy
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 0, totalShotsFaced: 0,
            openPlaySaves: 0, openPlayShotsFaced: 0,
            penaltySaves: 0, penaltyShotsFaced: 0,
            goalsConceded: 0, playingTimeMinutes: 0, matchesPlayed: 0,
            legacyTotalSaveRate: 68.5);

        Assert.Equal(68.5, vm.TotalSaveRate);
    }

    [Fact]
    public void Build_ZeroShotsFacedNoLegacy_TotalSaveRateIsNull()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 0, totalShotsFaced: 0,
            openPlaySaves: 0, openPlayShotsFaced: 0,
            penaltySaves: 0, penaltyShotsFaced: 0,
            goalsConceded: 0, playingTimeMinutes: 0, matchesPlayed: 0);

        Assert.Null(vm.TotalSaveRate);
    }

    [Fact]
    public void Build_PenaltyShotsFacedZero_PenaltySaveRateIsNull()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 20, totalShotsFaced: 30,
            openPlaySaves: 20, openPlayShotsFaced: 30,
            penaltySaves: 0, penaltyShotsFaced: 0,
            goalsConceded: 10, playingTimeMinutes: 90, matchesPlayed: 5);

        Assert.Null(vm.PenaltySaveRate);
    }

    // ── Per-60 edge cases ─────────────────────────────────────────────────────

    [Fact]
    public void Build_ZeroPlayingTime_AllPer60AreNull()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 20, totalShotsFaced: 30,
            openPlaySaves: 18, openPlayShotsFaced: 27,
            penaltySaves: 2, penaltyShotsFaced: 3,
            goalsConceded: 10, playingTimeMinutes: 0, matchesPlayed: 5,
            apiSavesPer60: 4.0);

        Assert.Null(vm.SavesPer60);
        Assert.Null(vm.ShotsFacedPer60);
        Assert.Null(vm.GoalsConcededPer60);
    }

    [Fact]
    public void Build_GoalsConcededPer60_ComputedCorrectly()
    {
        // 4 goals in 60 min = 4.0 /60
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 20, totalShotsFaced: 24,
            openPlaySaves: 18, openPlayShotsFaced: 22,
            penaltySaves: 2, penaltyShotsFaced: 2,
            goalsConceded: 4, playingTimeMinutes: 60, matchesPlayed: 3);

        Assert.NotNull(vm.GoalsConcededPer60);
        Assert.Equal(4.0, vm.GoalsConcededPer60.Value, Tol);
    }

    // ── Data consistency ──────────────────────────────────────────────────────

    [Fact]
    public void Build_SavesGreaterThanShotsFaced_FlagsInconsistency()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 20, totalShotsFaced: 15,  // invalid: saves > shots faced
            openPlaySaves: 18, openPlayShotsFaced: 13,
            penaltySaves: 2, penaltyShotsFaced: 2,
            goalsConceded: 0, playingTimeMinutes: 90, matchesPlayed: 5);

        Assert.True(vm.HasDataInconsistency);
        Assert.NotNull(vm.DataInconsistencyReason);
    }

    [Fact]
    public void Build_ConsistentData_NoInconsistencyFlag()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 20, totalShotsFaced: 30,
            openPlaySaves: 18, openPlayShotsFaced: 27,
            penaltySaves: 2, penaltyShotsFaced: 3,
            goalsConceded: 10, playingTimeMinutes: 90, matchesPlayed: 5);

        Assert.False(vm.HasDataInconsistency);
        Assert.Null(vm.DataInconsistencyReason);
    }

    // ── Quality thresholds ────────────────────────────────────────────────────

    [Fact]
    public void Build_OpenPlaySampleBelowMinimum_QualityIsLow()
    {
        // Open play minimum = 20; give only 5 shots
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 3, totalShotsFaced: 5,
            openPlaySaves: 3, openPlayShotsFaced: 5,
            penaltySaves: 0, penaltyShotsFaced: 0,
            goalsConceded: 2, playingTimeMinutes: 60, matchesPlayed: 2);

        Assert.Equal(QualityTier.Low, vm.OpenPlaySaveRateQuality.Tier);
    }

    [Fact]
    public void Build_OpenPlaySampleAboveMinimum_QualityIsHigh()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 16, totalShotsFaced: 25,
            openPlaySaves: 16, openPlayShotsFaced: 25,
            penaltySaves: 0, penaltyShotsFaced: 0,
            goalsConceded: 9, playingTimeMinutes: 200, matchesPlayed: 8);

        Assert.Equal(QualityTier.High, vm.OpenPlaySaveRateQuality.Tier);
    }

    [Fact]
    public void Build_PenaltySampleBelowMinimum_QualityIsLow()
    {
        // Penalty minimum = 5; give only 2 shots
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 1, totalShotsFaced: 2,
            openPlaySaves: 0, openPlayShotsFaced: 0,
            penaltySaves: 1, penaltyShotsFaced: 2,
            goalsConceded: 1, playingTimeMinutes: 60, matchesPlayed: 2);

        Assert.Equal(QualityTier.Low, vm.PenaltySaveRateQuality.Tier);
    }

    [Fact]
    public void Build_ZeroShotsFaced_TotalQualityIsNotApplicable()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 0, totalShotsFaced: 0,
            openPlaySaves: 0, openPlayShotsFaced: 0,
            penaltySaves: 0, penaltyShotsFaced: 0,
            goalsConceded: 0, playingTimeMinutes: 0, matchesPlayed: 0);

        Assert.Equal(QualityTier.NotApplicable, vm.TotalSaveRateQuality.Tier);
    }

    [Fact]
    public void Build_PlayingTimeBelowMinimum_Per60QualityIsLow()
    {
        // /60 minimum = 150 min; give only 60 min
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 20, totalShotsFaced: 30,
            openPlaySaves: 18, openPlayShotsFaced: 27,
            penaltySaves: 2, penaltyShotsFaced: 3,
            goalsConceded: 10, playingTimeMinutes: 60, matchesPlayed: 3);

        Assert.Equal(QualityTier.Low, vm.Per60Quality.Tier);
    }

    [Fact]
    public void Build_PlayingTimeAboveMinimum_Per60QualityIsHigh()
    {
        var vm = GoalkeeperAnalyticsBuilder.Build(
            totalSaves: 80, totalShotsFaced: 100,
            openPlaySaves: 70, openPlayShotsFaced: 90,
            penaltySaves: 10, penaltyShotsFaced: 10,
            goalsConceded: 20, playingTimeMinutes: 300, matchesPlayed: 10);

        Assert.Equal(QualityTier.High, vm.Per60Quality.Tier);
    }

    // ── CanDeclareWinner with LOW quality ────────────────────────────────────

    [Fact]
    public void CanDeclareWinner_LowVsHigh_ReturnsFalse()
    {
        var low  = new QualityTierResult(QualityTier.Low, null, 3);
        var high = new QualityTierResult(QualityTier.High, null, 50);
        Assert.False(AnalyticsQualityPolicy.CanDeclareWinner(low, high));
    }

    [Fact]
    public void CanDeclareWinner_HighVsHigh_ReturnsTrue()
    {
        var a = new QualityTierResult(QualityTier.High, null, 50);
        var b = new QualityTierResult(QualityTier.High, null, 40);
        Assert.True(AnalyticsQualityPolicy.CanDeclareWinner(a, b));
    }

    // ── Team aggregation (weighted sum) ───────────────────────────────────────

    [Fact]
    public void AggregateTeamSaveRate_WeightedSum_NotAverageOfPercentages()
    {
        // GK1: 20/30 = 66.67%, GK2: 5/25 = 20.00%
        // Average of percentages = (66.67 + 20.00) / 2 = 43.33% ← WRONG
        // Weighted sum = 25/55 × 100 = 45.45% ← CORRECT
        var rate = GoalkeeperAnalyticsBuilder.AggregateTeamSaveRate([
            (Saves: 20, ShotsFaced: 30),
            (Saves: 5,  ShotsFaced: 25),
        ]);

        Assert.NotNull(rate);
        Assert.Equal(25.0 / 55.0 * 100.0, rate.Value, Tol);
    }

    [Fact]
    public void AggregateTeamSaveRate_AllZeroFaced_ReturnsNull()
    {
        var rate = GoalkeeperAnalyticsBuilder.AggregateTeamSaveRate([
            (Saves: 0, ShotsFaced: 0),
            (Saves: 0, ShotsFaced: 0),
        ]);

        Assert.Null(rate);
    }

    [Fact]
    public void AggregateTeamSaveRate_SingleGK_MatchesDirectFormula()
    {
        var direct = AnalyticsCalculationService.ComputeTotalSaveRate(15, 20);
        var aggregated = GoalkeeperAnalyticsBuilder.AggregateTeamSaveRate([
            (Saves: 15, ShotsFaced: 20),
        ]);

        Assert.Equal(direct, aggregated);
    }

    // ── Position applicability ────────────────────────────────────────────────

    [Fact]
    public void Catalog_CAT21_AppliesToGKOnly()
    {
        var def = AnalyticsV3Catalog.Get("CAT-21");
        Assert.NotNull(def);
        Assert.Equal(AnalyticsPositionScope.GK, def!.ApplicablePositions);
    }

    [Fact]
    public void Catalog_CAT22_AppliesToGKOnly()
    {
        var def = AnalyticsV3Catalog.Get("CAT-22");
        Assert.NotNull(def);
        Assert.Equal(AnalyticsPositionScope.GK, def!.ApplicablePositions);
    }

    [Fact]
    public void Catalog_CAT21_NotApplicableToFieldPlayers()
    {
        var def = AnalyticsV3Catalog.Get("CAT-21");
        Assert.NotNull(def);
        Assert.False(def!.ApplicablePositions.HasFlag(AnalyticsPositionScope.AIL));
        Assert.False(def.ApplicablePositions.HasFlag(AnalyticsPositionScope.DC));
    }

    // ── CAT-21/22 catalog metadata ────────────────────────────────────────────

    [Fact]
    public void Catalog_CAT21_DisplayName_IsTauxArretGlobal()
    {
        var def = AnalyticsV3Catalog.Get("CAT-21");
        Assert.NotNull(def);
        Assert.Equal("Taux d'arrêt global", def!.DisplayName);
    }

    [Fact]
    public void Catalog_CAT22_DisplayName_IsButsEncaissesPer60()
    {
        var def = AnalyticsV3Catalog.Get("CAT-22");
        Assert.NotNull(def);
        Assert.Equal("Buts encaissés /60", def!.DisplayName);
    }

    [Fact]
    public void Catalog_CAT21_MinimumSampleCount_Is20()
    {
        var def = AnalyticsV3Catalog.Get("CAT-21");
        Assert.NotNull(def);
        Assert.Equal(20, def!.MinimumSampleCount);
    }

    [Fact]
    public void Catalog_CAT22_MinimumPlayingTime_Is150()
    {
        var def = AnalyticsV3Catalog.Get("CAT-22");
        Assert.NotNull(def);
        Assert.Equal(150, def!.MinimumPlayingTimeMinutes);
    }

    [Fact]
    public void Catalog_CAT21_StatusIsActive()
    {
        Assert.Equal(AnalyticsMetricStatus.Active, AnalyticsV3Catalog.Get("CAT-21")!.Status);
    }

    [Fact]
    public void Catalog_CAT22_StatusIsActive()
    {
        Assert.Equal(AnalyticsMetricStatus.Active, AnalyticsV3Catalog.Get("CAT-22")!.Status);
    }

    [Fact]
    public void Catalog_CAT21_HigherIsBetter()
    {
        Assert.True(AnalyticsV3Catalog.Get("CAT-21")!.HigherIsBetter);
    }

    [Fact]
    public void Catalog_CAT22_HigherIsNotBetter()
    {
        // Fewer goals conceded = better
        Assert.False(AnalyticsV3Catalog.Get("CAT-22")!.HigherIsBetter);
    }
}
