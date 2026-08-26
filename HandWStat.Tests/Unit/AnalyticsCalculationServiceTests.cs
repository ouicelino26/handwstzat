using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsCalculationServiceTests
{
    private const double Tolerance = 1e-9;

    // ── CAT-01: ComputeGoalsCreatedPer60 ────────────────────────────────────

    [Fact]
    public void GoalsCreatedPer60_Normal_ReturnsCorrectValue()
    {
        // (8 + 4) / 60 * 60 = 12.0
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(totalGoals: 8, assists: 4, playingTimeMinutes: 60);
        Assert.NotNull(result);
        Assert.Equal(12.0, result.Value, Tolerance);
    }

    [Fact]
    public void GoalsCreatedPer60_PlayingTimeZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(totalGoals: 5, assists: 3, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    [Fact]
    public void GoalsCreatedPer60_PlayingTimeNegative_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(totalGoals: 5, assists: 3, playingTimeMinutes: -1);
        Assert.Null(result);
    }

    [Fact]
    public void GoalsCreatedPer60_ZeroProduction_ReturnsZeroNotNull()
    {
        // A player with 300 minutes and no goals or assists scores 0/60 — valid
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(totalGoals: 0, assists: 0, playingTimeMinutes: 300);
        Assert.NotNull(result);
        Assert.Equal(0.0, result.Value, Tolerance);
    }

    [Fact]
    public void GoalsCreatedPer60_HalfHour_DoubleRate()
    {
        // 6 goals+assists in 30 min = 12/60
        var result = AnalyticsCalculationService.ComputeGoalsCreatedPer60(totalGoals: 4, assists: 2, playingTimeMinutes: 30);
        Assert.NotNull(result);
        Assert.Equal(12.0, result.Value, Tolerance);
    }

    // ── CAT-02: ComputeOffensiveVolumePer60 ─────────────────────────────────

    [Fact]
    public void OffensiveVolumePer60_Normal_ReturnsCorrectValue()
    {
        // (20 + 5) / 120 * 60 = 12.5
        var result = AnalyticsCalculationService.ComputeOffensiveVolumePer60(shotAttempts: 20, assists: 5, playingTimeMinutes: 120);
        Assert.NotNull(result);
        Assert.Equal(12.5, result.Value, Tolerance);
    }

    [Fact]
    public void OffensiveVolumePer60_PlayingTimeZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeOffensiveVolumePer60(shotAttempts: 10, assists: 3, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    // ── CAT-04: ComputeOpenPlaySuccessRate ──────────────────────────────────

    [Fact]
    public void OpenPlaySuccessRate_Normal_ReturnsCorrectValue()
    {
        // 7 goals / 22 attempts = 31.818...%
        var result = AnalyticsCalculationService.ComputeOpenPlaySuccessRate(goalCount: 7, openShotAttempts: 22);
        Assert.NotNull(result);
        Assert.Equal(7.0 / 22.0 * 100.0, result.Value, Tolerance);
    }

    [Fact]
    public void OpenPlaySuccessRate_DenominatorZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeOpenPlaySuccessRate(goalCount: 0, openShotAttempts: 0);
        Assert.Null(result);
    }

    [Fact]
    public void OpenPlaySuccessRate_ZeroGoals_ReturnsZero()
    {
        var result = AnalyticsCalculationService.ComputeOpenPlaySuccessRate(goalCount: 0, openShotAttempts: 10);
        Assert.NotNull(result);
        Assert.Equal(0.0, result.Value, Tolerance);
    }

    [Fact]
    public void OpenPlaySuccessRate_PerfectRate_Returns100()
    {
        var result = AnalyticsCalculationService.ComputeOpenPlaySuccessRate(goalCount: 5, openShotAttempts: 5);
        Assert.NotNull(result);
        Assert.Equal(100.0, result.Value, Tolerance);
    }

    // ── CAT-05: ComputeAssistTurnoverRatio ──────────────────────────────────

    [Fact]
    public void AssistTurnoverRatio_Normal_ReturnsCorrectValue()
    {
        // 8 assists / 4 turnovers = 2.0
        var result = AnalyticsCalculationService.ComputeAssistTurnoverRatio(assists: 8, turnovers: 4);
        Assert.NotNull(result);
        Assert.Equal(2.0, result.Value, Tolerance);
    }

    [Fact]
    public void AssistTurnoverRatio_ZeroTurnovers_ReturnsNull()
    {
        // A player with no turnovers — ratio is undefined, not infinity
        var result = AnalyticsCalculationService.ComputeAssistTurnoverRatio(assists: 5, turnovers: 0);
        Assert.Null(result);
    }

    [Fact]
    public void AssistTurnoverRatio_ZeroAssistsWithTurnovers_ReturnsZero()
    {
        var result = AnalyticsCalculationService.ComputeAssistTurnoverRatio(assists: 0, turnovers: 3);
        Assert.NotNull(result);
        Assert.Equal(0.0, result.Value, Tolerance);
    }

    // ── CAT-10: ComputeDefensiveImpactPer60 ─────────────────────────────────

    [Fact]
    public void DefensiveImpactPer60_Normal_ReturnsCorrectValue()
    {
        // (5 + 3 + 2 + 4) / 120 * 60 = 7.0
        var result = AnalyticsCalculationService.ComputeDefensiveImpactPer60(
            interceptions: 5, contres: 3, neutralisations: 2, passageForce: 4,
            playingTimeMinutes: 120);
        Assert.NotNull(result);
        Assert.Equal(7.0, result.Value, Tolerance);
    }

    [Fact]
    public void DefensiveImpactPer60_PlayingTimeZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeDefensiveImpactPer60(
            interceptions: 3, contres: 1, neutralisations: 1, passageForce: 1,
            playingTimeMinutes: 0);
        Assert.Null(result);
    }

    [Fact]
    public void DefensiveImpactPer60_UsesPassageForce_NotPassageEnForce()
    {
        // Verifies the correct field name is used — PassageForce (defense DTO)
        // A value of 10 for passageForce means 10 "Provoque Passage force" events
        var result = AnalyticsCalculationService.ComputeDefensiveImpactPer60(
            interceptions: 0, contres: 0, neutralisations: 0, passageForce: 10,
            playingTimeMinutes: 60);
        Assert.NotNull(result);
        Assert.Equal(10.0, result.Value, Tolerance);
    }

    // ── CAT-12: ComputeOffensiveWasteRate ───────────────────────────────────

    [Fact]
    public void OffensiveWasteRate_Normal_ReturnsCorrectValue()
    {
        // 5 turnovers / (15 + 5 + 8 + 5) × 100 = 5/33 × 100 ≈ 15.15%
        var result = AnalyticsCalculationService.ComputeOffensiveWasteRate(
            turnovers: 5, openShotAttempts: 15, penaltyAttempts: 5, assists: 8);
        Assert.NotNull(result);
        Assert.Equal(5.0 / 33.0 * 100.0, result.Value, Tolerance);
    }

    [Fact]
    public void OffensiveWasteRate_AllZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeOffensiveWasteRate(0, 0, 0, 0);
        Assert.Null(result);
    }

    // ── CAT-13/14: GK save rates ────────────────────────────────────────────

    [Fact]
    public void OpenPlaySaveRate_Normal_ReturnsCorrectValue()
    {
        // 75 / 100 * 100 = 75.0%
        var result = AnalyticsCalculationService.ComputeOpenPlaySaveRate(openPlaySaves: 75, openPlayShotsFaced: 100);
        Assert.NotNull(result);
        Assert.Equal(75.0, result.Value, Tolerance);
    }

    [Fact]
    public void OpenPlaySaveRate_ZeroFaced_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeOpenPlaySaveRate(openPlaySaves: 0, openPlayShotsFaced: 0);
        Assert.Null(result);
    }

    [Fact]
    public void PenaltySaveRate_Normal_ReturnsCorrectValue()
    {
        // 3 / 8 * 100 = 37.5%
        var result = AnalyticsCalculationService.ComputePenaltySaveRate(penaltySaves: 3, penaltyShotsFaced: 8);
        Assert.NotNull(result);
        Assert.Equal(37.5, result.Value, Tolerance);
    }

    [Fact]
    public void PenaltySaveRate_ZeroFaced_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputePenaltySaveRate(penaltySaves: 0, penaltyShotsFaced: 0);
        Assert.Null(result);
    }

    // ── CAT-16: ComputeShotsFacedPer60 ─────────────────────────────────────

    [Fact]
    public void ShotsFacedPer60_Normal_ReturnsCorrectValue()
    {
        // 50 / 60 * 60 = 50.0
        var result = AnalyticsCalculationService.ComputeShotsFacedPer60(tirsSubis: 50, playingTimeMinutes: 60);
        Assert.NotNull(result);
        Assert.Equal(50.0, result.Value, Tolerance);
    }

    [Fact]
    public void ShotsFacedPer60_PlayingTimeZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeShotsFacedPer60(tirsSubis: 30, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    // ── CAT-17A/17B: team contribution ─────────────────────────────────────

    [Fact]
    public void GoalsSharePct_Normal_ReturnsCorrectValue()
    {
        // 10 / 40 * 100 = 25.0%
        var result = AnalyticsCalculationService.ComputeGoalsSharePct(totalGoals: 10, teamGoalsFor: 40);
        Assert.NotNull(result);
        Assert.Equal(25.0, result.Value, Tolerance);
    }

    [Fact]
    public void GoalsSharePct_TeamGoalsZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeGoalsSharePct(totalGoals: 5, teamGoalsFor: 0);
        Assert.Null(result);
    }

    [Fact]
    public void DirectInvolvement_Normal_ReturnsCorrectValue()
    {
        // (8 + 6) / 30 * 100 ≈ 46.67%
        var result = AnalyticsCalculationService.ComputeDirectInvolvement(goalCount: 8, assists: 6, teamGoalsFor: 30);
        Assert.NotNull(result);
        Assert.Equal(14.0 / 30.0 * 100.0, result.Value, Tolerance);
    }

    [Fact]
    public void DirectInvolvement_CanExceed100Percent()
    {
        // Player scored 20 goals and had 15 assists on a team with 25 goals
        // (20+15)/25 = 140% — valid and expected
        var result = AnalyticsCalculationService.ComputeDirectInvolvement(goalCount: 20, assists: 15, teamGoalsFor: 25);
        Assert.NotNull(result);
        Assert.Equal(140.0, result.Value, Tolerance);
    }

    [Fact]
    public void DirectInvolvement_TeamGoalsZero_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputeDirectInvolvement(goalCount: 3, assists: 2, teamGoalsFor: 0);
        Assert.Null(result);
    }

    // ── NormalizeApiPer60 ───────────────────────────────────────────────────

    [Fact]
    public void NormalizeApiPer60_ValidPlayingTime_ReturnsApiValue()
    {
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 3.75, playingTimeMinutes: 120);
        Assert.NotNull(result);
        Assert.Equal(3.75, result.Value, Tolerance);
    }

    [Fact]
    public void NormalizeApiPer60_ZeroPlayingTime_ReturnsNull()
    {
        // API returns 0.0 when no playing time — we must convert to null (N/A)
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 0.0, playingTimeMinutes: 0);
        Assert.Null(result);
    }

    [Fact]
    public void NormalizeApiPer60_ApiZeroWithValidTime_ReturnsZero()
    {
        // Player truly produced 0 events in 200 minutes — valid 0, not N/A
        var result = AnalyticsCalculationService.NormalizeApiPer60(apiValue: 0.0, playingTimeMinutes: 200);
        Assert.NotNull(result);
        Assert.Equal(0.0, result.Value, Tolerance);
    }

    // ── CAT-06/07 per-match ─────────────────────────────────────────────────

    [Fact]
    public void PenaltiesWonPerMatch_Normal_ReturnsCorrectValue()
    {
        // 9 / 10 = 0.9 per match
        var result = AnalyticsCalculationService.ComputePenaltiesWonPerMatch(penaltiesWon: 9, matchesPlayed: 10);
        Assert.NotNull(result);
        Assert.Equal(0.9, result.Value, Tolerance);
    }

    [Fact]
    public void PenaltiesWonPerMatch_ZeroMatches_ReturnsNull()
    {
        var result = AnalyticsCalculationService.ComputePenaltiesWonPerMatch(penaltiesWon: 3, matchesPlayed: 0);
        Assert.Null(result);
    }

    [Fact]
    public void OffensiveFoulsDrawnPerMatch_Normal_ReturnsCorrectValue()
    {
        var result = AnalyticsCalculationService.ComputeOffensiveFoulsDrawnPerMatch(offensiveFoulsDrawn: 6, matchesPlayed: 12);
        Assert.NotNull(result);
        Assert.Equal(0.5, result.Value, Tolerance);
    }
}
