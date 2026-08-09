using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Tests;

public sealed class HandballKpiHelperTests
{
    // ── Ratio / Percentage ────────────────────────────────────────────────────

    [Fact]
    public void Ratio_WithZeroDenominator_IsNotCalculable()
    {
        Assert.Null(HandballKpiHelper.Ratio(5, 0));
    }

    [Fact]
    public void Ratio_WithFinitePositiveDenominator_ReturnsQuotient()
    {
        Assert.Equal(2.5d, HandballKpiHelper.Ratio(5, 2));
    }

    [Fact]
    public void Ratio_WithNanNumerator_ReturnsNull()
    {
        Assert.Null(HandballKpiHelper.Ratio(double.NaN, 10));
    }

    [Fact]
    public void Ratio_WithInfinityNumerator_ReturnsNull()
    {
        Assert.Null(HandballKpiHelper.Ratio(double.PositiveInfinity, 10));
    }

    [Fact]
    public void Percentage_WithZeroDenominator_ReturnsNull()
    {
        Assert.Null(HandballKpiHelper.Percentage(5, 0));
    }

    [Fact]
    public void Percentage_WithValidInputs_Returns100TimesRatio()
    {
        Assert.Equal(50d, HandballKpiHelper.Percentage(5, 10));
    }

    // ── PerMatch ──────────────────────────────────────────────────────────────

    [Fact]
    public void PerMatch_WithZeroMatches_ReturnsZero()
    {
        Assert.Equal(0d, HandballKpiHelper.PerMatch(10d, 0));
    }

    [Fact]
    public void PerMatch_WithPositiveMatches_ReturnsQuotient()
    {
        Assert.Equal(2.5d, HandballKpiHelper.PerMatch(10d, 4));
    }

    // ── Shot stats ────────────────────────────────────────────────────────────

    [Fact]
    public void ShotAttempts_DoesNotCountBlockedShotTwice()
    {
        var offense = new PlayerOffenseStatsDto
        {
            TotalButs = 2,
            TirsRates = 4,
            PenaltyRate = 1,
            TirContre = 1
        };

        Assert.Equal(7, HandballKpiHelper.ShotAttempts(offense));
        Assert.Equal(5, HandballKpiHelper.ShotWaste(offense));
    }

    [Fact]
    public void ShotAttempts_WithNullOffense_ReturnsZero()
    {
        Assert.Equal(0, HandballKpiHelper.ShotAttempts(null));
    }

    [Fact]
    public void ShotWaste_WithNullOffense_ReturnsZero()
    {
        Assert.Equal(0, HandballKpiHelper.ShotWaste(null));
    }

    [Fact]
    public void PenaltyAttempts_SumsButsAndRates()
    {
        var offense = new PlayerOffenseStatsDto { Buts7m = 3, PenaltyRate = 2 };
        Assert.Equal(5, HandballKpiHelper.PenaltyAttempts(offense));
    }

    [Fact]
    public void OverallShotSuccessRate_WithNoAttempts_ReturnsZero()
    {
        var offense = new PlayerOffenseStatsDto();
        Assert.Equal(0d, HandballKpiHelper.OverallShotSuccessRate(offense));
    }

    [Fact]
    public void OverallShotSuccessRate_With6Goals10Attempts_Returns60Percent()
    {
        var offense = new PlayerOffenseStatsDto { TotalButs = 6, TirsRates = 4 };
        Assert.Equal(60d, HandballKpiHelper.OverallShotSuccessRate(offense));
    }

    // ── TotalSanctions — P0 fix: PenaltyConcede must NOT be included ──────────

    [Fact]
    public void TotalSanctions_ExcludesPenaltyConcede_ContractInvariant()
    {
        // Contract: "Les 7m concédés restent hors du total disciplinaire"
        // HANDWSTAT_METRIC_DISPLAY_CONTRACT.md + LEAGUE_STATS_UI_MAPPING.md
        var sanctions = new PlayerSanctionStatsDto
        {
            Avertissements = 1,
            DeuxMinutes = 2,
            Exclusions = 1,
            PenaltyConcede = 3
        };

        Assert.Equal(4, HandballKpiHelper.TotalSanctions(sanctions));
    }

    [Fact]
    public void TotalSanctions_WithNullSanctions_ReturnsZero()
    {
        Assert.Equal(0, HandballKpiHelper.TotalSanctions(null));
    }

    [Fact]
    public void TotalSanctions_WithAllZero_ReturnsZero()
    {
        var sanctions = new PlayerSanctionStatsDto();
        Assert.Equal(0, HandballKpiHelper.TotalSanctions(sanctions));
    }

    [Fact]
    public void TotalSanctions_CountsAvertissementsDeuxMinutesExclusions()
    {
        var sanctions = new PlayerSanctionStatsDto
        {
            Avertissements = 2,
            DeuxMinutes = 3,
            Exclusions = 1,
            PenaltyConcede = 5
        };

        // 2+3+1 = 6, PenaltyConcede excluded
        Assert.Equal(6, HandballKpiHelper.TotalSanctions(sanctions));
    }

    // ── Goalkeeper stats ──────────────────────────────────────────────────────

    [Fact]
    public void GoalkeeperStops_SumsArretsPlusArretsPenalty()
    {
        var gk = new PlayerGoalkeeperStatsDto { Arrets = 8, ArretsPenalty = 2 };
        Assert.Equal(10, HandballKpiHelper.GoalkeeperStops(gk));
    }

    [Fact]
    public void GoalkeeperStops_WithNull_ReturnsZero()
    {
        Assert.Equal(0, HandballKpiHelper.GoalkeeperStops(null));
    }

    [Fact]
    public void GoalkeeperConcededGoals_SumsButsPrisAndButsPenalty()
    {
        var gk = new PlayerGoalkeeperStatsDto { ButsPris = 20, ButsPenalty = 4 };
        Assert.Equal(24, HandballKpiHelper.GoalkeeperConcededGoals(gk));
    }

    [Fact]
    public void GoalkeeperPenaltyStopRate_With2Stops3Conceded_Returns40Percent()
    {
        // 2 saves / (2 saves + 3 conceded) = 40%
        var gk = new PlayerGoalkeeperStatsDto { ArretsPenalty = 2, ButsPenalty = 3 };
        Assert.Equal(40d, HandballKpiHelper.GoalkeeperPenaltyStopRate(gk));
    }

    [Fact]
    public void GoalkeeperPenaltyStopRate_WithNoAttempts_ReturnsZero()
    {
        var gk = new PlayerGoalkeeperStatsDto { ArretsPenalty = 0, ButsPenalty = 0 };
        Assert.Equal(0d, HandballKpiHelper.GoalkeeperPenaltyStopRate(gk));
    }

    // ── Defensive impact ──────────────────────────────────────────────────────

    [Fact]
    public void DefensiveImpact_SumsAllFourCategories()
    {
        var defense = new PlayerDefenseStatsDto
        {
            Interceptions = 2,
            Contres = 1,
            Neutralisations = 1,
            PassageForce = 1
        };
        Assert.Equal(5, HandballKpiHelper.DefensiveImpact(defense));
    }

    [Fact]
    public void DefensiveImpact_WithNull_ReturnsZero()
    {
        Assert.Equal(0, HandballKpiHelper.DefensiveImpact((PlayerDefenseStatsDto?)null));
    }

    // ── Technical losses ──────────────────────────────────────────────────────

    [Fact]
    public void TechnicalLosses_SumsAllFourTypes()
    {
        var passing = new PlayerPassingStatsDto
        {
            MauvaisePasse = 1,
            PerteDeBalle = 2,
            FauteTechnique = 1,
            PassageEnForce = 1
        };
        Assert.Equal(5, HandballKpiHelper.TechnicalLosses(passing));
    }

    // ── TechnicalBalanceScore — propagates TotalSanctions P0 fix ─────────────

    [Fact]
    public void TechnicalBalanceScore_SanctionsExcludePenaltyConcede()
    {
        // Sanction total = 1 (Avertissement only), PenaltyConcede=5 must not inflate negative side
        var sanctions = new PlayerSanctionStatsDto { Avertissements = 1, PenaltyConcede = 5 };
        var player = new PlayerGlobalStatsDto { TotalGoals = 5, AssistCount = 2 };

        var score = HandballKpiHelper.TechnicalBalanceScore(player, null, null, null, sanctions, null);

        // positive=7, negative=1 → share = 7/(7+1)*100 = 87.5%
        Assert.Equal(87.5d, score, precision: 1);
    }

    // ── FormatRatio ───────────────────────────────────────────────────────────

    [Fact]
    public void FormatRatio_WithNullValue_ReturnsNA()
    {
        Assert.Equal("N/A", HandballKpiHelper.FormatRatio(null));
    }

    [Fact]
    public void FormatRatio_WithFiniteValue_ReturnsFormatted()
    {
        Assert.Equal("1.50", HandballKpiHelper.FormatRatio(1.5d));
    }

    [Fact]
    public void FormatRatio_WithNaN_ReturnsNA()
    {
        Assert.Equal("N/A", HandballKpiHelper.FormatRatio(double.NaN));
    }

    // ── Share / SuccessVsWasteShare ───────────────────────────────────────────

    [Fact]
    public void Share_WithZeroDenominator_ReturnsZero()
    {
        Assert.Equal(0d, HandballKpiHelper.Share(5d, 0d));
    }

    [Fact]
    public void Share_With3Over10_Returns30Percent()
    {
        Assert.Equal(30d, HandballKpiHelper.Share(3d, 10d));
    }

    [Fact]
    public void SuccessVsWasteShare_WithZeroTotal_ReturnsZero()
    {
        Assert.Equal(0d, HandballKpiHelper.SuccessVsWasteShare(0d, 0d));
    }

    [Fact]
    public void SuccessVsWasteShare_With7Success1Waste_Returns87Point5()
    {
        Assert.Equal(87.5d, HandballKpiHelper.SuccessVsWasteShare(7d, 1d));
    }
}
