using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests.Unit;

public class AnalyticsA5Tests
{
    // ── Position-aware default dimension ─────────────────────────────────────

    [Fact]
    public void GetDefaultDimension_GK_ReturnsScoreState()
    {
        Assert.Equal(ContextDimension.ScoreState, ContextAnalyticsHelper.GetDefaultDimension(AnalyticsPosition.GK));
    }

    [Fact]
    public void GetDefaultDimension_DC_ReturnsAttackSystem()
    {
        Assert.Equal(ContextDimension.AttackSystem, ContextAnalyticsHelper.GetDefaultDimension(AnalyticsPosition.DC));
    }

    [Fact]
    public void GetDefaultDimension_AR_ReturnsDefenseStructure()
    {
        Assert.Equal(ContextDimension.DefenseStructure, ContextAnalyticsHelper.GetDefaultDimension(AnalyticsPosition.AR));
    }

    [Fact]
    public void GetDefaultDimension_AIL_ReturnsAttackSituation()
    {
        Assert.Equal(ContextDimension.AttackSituation, ContextAnalyticsHelper.GetDefaultDimension(AnalyticsPosition.AIL));
    }

    [Fact]
    public void GetDefaultDimension_PIV_ReturnsDefenseStructure()
    {
        Assert.Equal(ContextDimension.DefenseStructure, ContextAnalyticsHelper.GetDefaultDimension(AnalyticsPosition.PIV));
    }

    [Fact]
    public void GetDefaultDimension_Unknown_ReturnsScoreState()
    {
        Assert.Equal(ContextDimension.ScoreState, ContextAnalyticsHelper.GetDefaultDimension(AnalyticsPosition.Unknown));
    }

    // ── Scope options propagation ─────────────────────────────────────────────

    [Fact]
    public void BuildContextOptions_PlayerIdAlwaysSet()
    {
        var opts = ContextAnalyticsHelper.BuildContextOptions(playerId: 42);
        Assert.Equal(42, opts.PlayerId);
    }

    [Fact]
    public void BuildContextOptions_CompetitionIdPropagated()
    {
        var opts = ContextAnalyticsHelper.BuildContextOptions(playerId: 1, competitionId: 5);
        Assert.Equal(5, opts.CompetitionId);
    }

    [Fact]
    public void BuildContextOptions_TeamIdPropagated()
    {
        var opts = ContextAnalyticsHelper.BuildContextOptions(playerId: 1, teamId: 12);
        Assert.Equal(12, opts.TeamId);
    }

    [Fact]
    public void BuildContextOptions_SeasonPropagated()
    {
        var opts = ContextAnalyticsHelper.BuildContextOptions(playerId: 1, season: "2024-2025");
        Assert.Equal("2024-2025", opts.Season);
    }

    [Fact]
    public void BuildContextOptions_DayPropagated()
    {
        var opts = ContextAnalyticsHelper.BuildContextOptions(playerId: 1, day: "J12");
        Assert.Equal("J12", opts.Day);
    }

    [Fact]
    public void BuildContextOptions_NullFilters_OnlyPlayerIdSet()
    {
        var opts = ContextAnalyticsHelper.BuildContextOptions(playerId: 7);
        Assert.Equal(7, opts.PlayerId);
        Assert.Null(opts.CompetitionId);
        Assert.Null(opts.TeamId);
        Assert.Null(opts.Season);
        Assert.Null(opts.Day);
    }

    // ── Row retrieval ─────────────────────────────────────────────────────────

    [Fact]
    public void GetRows_ScoreState_ReturnsScoreStates()
    {
        var breakdown = MakeBreakdown(scoreStates: [MakeRow("WINNING", events: 20)]);
        var rows = ContextAnalyticsHelper.GetRows(breakdown, ContextDimension.ScoreState);
        Assert.Single(rows);
        Assert.Equal("WINNING", rows[0].ContextCode);
    }

    [Fact]
    public void GetRows_AttackSituation_ReturnsAttackSituations()
    {
        var breakdown = MakeBreakdown(attackSituations: [MakeRow("TRANSITION", events: 15)]);
        var rows = ContextAnalyticsHelper.GetRows(breakdown, ContextDimension.AttackSituation);
        Assert.Single(rows);
        Assert.Equal("TRANSITION", rows[0].ContextCode);
    }

    [Fact]
    public void GetRows_DefenseStructure_ReturnsDefenseStructures()
    {
        var breakdown = MakeBreakdown(defenseStructures: [MakeRow("6-0", events: 30)]);
        var rows = ContextAnalyticsHelper.GetRows(breakdown, ContextDimension.DefenseStructure);
        Assert.Single(rows);
        Assert.Equal("6-0", rows[0].ContextCode);
    }

    [Fact]
    public void GetRows_AttackSystem_ReturnsAttackSystems()
    {
        var breakdown = MakeBreakdown(attackSystems: [MakeRow("SYS-A", events: 10)]);
        var rows = ContextAnalyticsHelper.GetRows(breakdown, ContextDimension.AttackSystem);
        Assert.Single(rows);
        Assert.Equal("SYS-A", rows[0].ContextCode);
    }

    // ── Available dimensions ──────────────────────────────────────────────────

    [Fact]
    public void GetAvailableDimensions_EmptyBreakdown_ReturnsEmpty()
    {
        var breakdown = MakeBreakdown();
        var dims = ContextAnalyticsHelper.GetAvailableDimensions(breakdown);
        Assert.Empty(dims);
    }

    [Fact]
    public void GetAvailableDimensions_WithScoreStateData_IncludesScoreState()
    {
        var breakdown = MakeBreakdown(scoreStates: [MakeRow("WINNING", events: 5)]);
        var dims = ContextAnalyticsHelper.GetAvailableDimensions(breakdown);
        Assert.Contains(ContextDimension.ScoreState, dims);
    }

    [Fact]
    public void GetAvailableDimensions_ZeroEventRows_Excluded()
    {
        // A row with Events=0 should not cause the dimension to be "available"
        var breakdown = MakeBreakdown(scoreStates: [MakeRow("WINNING", events: 0)]);
        var dims = ContextAnalyticsHelper.GetAvailableDimensions(breakdown);
        Assert.DoesNotContain(ContextDimension.ScoreState, dims);
    }

    // ── FR labels ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("WINNING",  null,      "En avance")]
    [InlineData("AHEAD",    null,      "En avance")]
    [InlineData("TIED",     null,      "Égalité")]
    [InlineData("EQUAL",    null,      "Égalité")]
    [InlineData("TRAILING", null,      "En retard")]
    [InlineData("BEHIND",   null,      "En retard")]
    public void GetContextFrLabel_KnownEnglishCodes_MapsToFrench(
        string? code, string? label, string expected)
    {
        Assert.Equal(expected, ContextAnalyticsHelper.GetContextFrLabel(code, label));
    }

    [Fact]
    public void GetContextFrLabel_ApiLabelTakesPrecedence()
    {
        // When API returns a ContextLabel, it is used regardless of code
        var result = ContextAnalyticsHelper.GetContextFrLabel("WINNING", "Devant au score");
        Assert.Equal("Devant au score", result);
    }

    [Fact]
    public void GetContextFrLabel_UnknownCode_ReturnCode()
    {
        var result = ContextAnalyticsHelper.GetContextFrLabel("CUSTOM_CODE", null);
        Assert.Equal("CUSTOM_CODE", result);
    }

    // ── Sample count ─────────────────────────────────────────────────────────

    [Fact]
    public void GetSampleCount_FieldPlayer_ReturnsShotAttempts()
    {
        var row = MakeRow("X", events: 30, shotAttempts: 25);
        Assert.Equal(25, ContextAnalyticsHelper.GetSampleCount(row, isGoalkeeper: false));
    }

    [Fact]
    public void GetSampleCount_FieldPlayer_FallsBackToEventsWhenZeroShots()
    {
        var row = MakeRow("X", events: 12, shotAttempts: 0);
        Assert.Equal(12, ContextAnalyticsHelper.GetSampleCount(row, isGoalkeeper: false));
    }

    [Fact]
    public void GetSampleCount_GK_ReturnsTotalShotsFaced()
    {
        // 10 saves + 2 penalty saves + 5 goals conceded + 1 penalty conceded = 18
        var row = MakeRow("X", events: 18,
            gkSaves: 10, gkPenaltySaves: 2,
            gkConceded: 5, gkPenaltyConceded: 1);
        Assert.Equal(18, ContextAnalyticsHelper.GetSampleCount(row, isGoalkeeper: true));
    }

    // ── GetGkShotsFaced ───────────────────────────────────────────────────────

    [Fact]
    public void GetGkShotsFaced_SumsAllFourFields()
    {
        var row = MakeRow("X", events: 0,
            gkSaves: 8, gkPenaltySaves: 3,
            gkConceded: 4, gkPenaltyConceded: 2);
        Assert.Equal(17, ContextAnalyticsHelper.GetGkShotsFaced(row));
    }

    [Fact]
    public void GetGkShotsFaced_AllZero_ReturnsZero()
    {
        Assert.Equal(0, ContextAnalyticsHelper.GetGkShotsFaced(MakeRow("X", events: 0)));
    }

    // ── Quality tiers per row ─────────────────────────────────────────────────

    [Fact]
    public void RowQuality_BelowMinimum_IsLow()
    {
        var row = MakeRow("X", events: 5, shotAttempts: 5);
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, ContextAnalyticsHelper.GetSampleCount(row, false), 10);
        Assert.Equal(QualityTier.Low, quality.Tier);
    }

    [Fact]
    public void RowQuality_AtOrAboveMinimum_IsHigh()
    {
        var row = MakeRow("X", events: 20, shotAttempts: 20);
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, ContextAnalyticsHelper.GetSampleCount(row, false), 10);
        Assert.Equal(QualityTier.High, quality.Tier);
    }

    [Fact]
    public void RowQuality_ZeroSample_IsNotApplicable()
    {
        var row = MakeRow("X", events: 0, shotAttempts: 0);
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, ContextAnalyticsHelper.GetSampleCount(row, false), 10);
        Assert.Equal(QualityTier.NotApplicable, quality.Tier);
    }

    [Fact]
    public void RowQuality_LowRemains_Visible()
    {
        // LOW rows must remain visible (not hidden like NotApplicable)
        var low = new QualityTierResult(QualityTier.Low, null, 3);
        Assert.NotEqual(QualityTier.NotApplicable, low.Tier);
    }

    // ── API ShotSuccessRate preserved ─────────────────────────────────────────

    [Fact]
    public void ApiShotSuccessRate_ValuePreservedOnRow()
    {
        var row = MakeRow("WINNING", events: 30, shotAttempts: 25, shotSuccessRate: 66.7);
        // Verify the value is directly accessible from the DTO (no recalculation needed)
        Assert.Equal(66.7, row.ShotSuccessRate);
    }

    // ── GK uses GoalkeeperSaveRate ────────────────────────────────────────────

    [Fact]
    public void GkRow_GoalkeeperSaveRateDirectlyAvailable()
    {
        var row = MakeRow("WINNING", events: 20,
            gkSaves: 15, gkPenaltySaves: 2,
            gkConceded: 3, gkPenaltyConceded: 0,
            gkSaveRate: 89.5);
        Assert.Equal(89.5, row.GoalkeeperSaveRate);
    }

    // ── CAT-19 catalog ────────────────────────────────────────────────────────

    [Fact]
    public void Catalog_CAT19_Exists()
    {
        Assert.NotNull(AnalyticsV3Catalog.Get("CAT-19"));
    }

    [Fact]
    public void Catalog_CAT19_StatusIsExpert()
    {
        Assert.Equal(AnalyticsMetricStatus.Expert, AnalyticsV3Catalog.Get("CAT-19")!.Status);
    }

    [Fact]
    public void Catalog_CAT19_AppliesToAll()
    {
        Assert.Equal(AnalyticsPositionScope.All, AnalyticsV3Catalog.Get("CAT-19")!.ApplicablePositions);
    }

    [Fact]
    public void Catalog_CAT19_MinimumSampleIs10()
    {
        Assert.Equal(10, AnalyticsV3Catalog.Get("CAT-19")!.MinimumSampleCount);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static EventContextSplitDto MakeRow(
        string code,
        int events = 0,
        int shotAttempts = 0,
        double shotSuccessRate = 0,
        int gkSaves = 0,
        int gkPenaltySaves = 0,
        int gkConceded = 0,
        int gkPenaltyConceded = 0,
        double gkSaveRate = 0,
        string? label = null) =>
        new()
        {
            ContextCode                      = code,
            ContextLabel                     = label ?? string.Empty,
            Events                           = events,
            ShotAttempts                     = shotAttempts,
            ShotSuccessRate                  = shotSuccessRate,
            GoalkeeperSaves                  = gkSaves,
            GoalkeeperPenaltySaves           = gkPenaltySaves,
            GoalkeeperConcededGoals          = gkConceded,
            GoalkeeperPenaltyConcededGoals   = gkPenaltyConceded,
            GoalkeeperSaveRate               = gkSaveRate,
        };

    private static EventContextBreakdownDto MakeBreakdown(
        List<EventContextSplitDto>? scoreStates = null,
        List<EventContextSplitDto>? attackSituations = null,
        List<EventContextSplitDto>? defenseStructures = null,
        List<EventContextSplitDto>? attackSystems = null) =>
        new()
        {
            ScoreStates       = scoreStates       ?? [],
            AttackSituations  = attackSituations  ?? [],
            DefenseStructures = defenseStructures ?? [],
            AttackSystems     = attackSystems     ?? [],
        };
}
