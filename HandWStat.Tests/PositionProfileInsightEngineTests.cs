using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

#pragma warning disable CA1814

namespace HandWStat.Tests;

public sealed class PositionProfileInsightEngineTests
{
    // ── Factory helpers ──────────────────────────────────────────────────────

    private static PositionProfileResponseDto MakeProfile(
        bool isGoalkeeper = false,
        string? positionCode = null)
    {
        return new PositionProfileResponseDto
        {
            IsGoalkeeperProfile = isGoalkeeper,
            CohortPlayerCount = 10,
            SelectedPlayer = new PositionProfilePlayerDto
            {
                PlayerId = 1,
                FullName = "Test Player",
                PositionCode = positionCode,
                MatchesPlayed = 10,
                Axes = []
            },
            MedianProfile = new PositionProfilePlayerDto
            {
                PlayerId = 0,
                FullName = "Median",
                Axes = []
            }
        };
    }

    private static PositionProfileAxisViewModel MakeAxis(
        string key = "goals_per60",
        double playerValue = 5d,
        double medianValue = 4d,
        double percentile = 60d,
        bool higherIsBetter = true,
        bool isEvaluative = true)
    {
        return new PositionProfileAxisViewModel(
            Key: key,
            Label: key,
            Category: "Offense",
            HigherIsBetter: higherIsBetter,
            Format: "number",
            PlayerValue: playerValue,
            MedianValue: medianValue,
            Percentile: percentile,
            Tone: "neutral",
            PlayerDisplayValue: playerValue.ToString("0.#"),
            MedianDisplayValue: medianValue.ToString("0.#"),
            DeltaDisplayValue: (playerValue - medianValue).ToString("+0.##;-0.##;0"),
            DirectionLabel: "Au-dessus",
            Summary: "Test",
            CoachLegend: "Test legend",
            DisplayPlayerValue: playerValue,
            DisplayMedianValue: medianValue,
            MinValue: 0d,
            MaxValue: 10d,
            IsEvaluative: isEvaluative);
    }

    // ── Null / empty guard tests ─────────────────────────────────────────────

    [Fact]
    public void Build_NullProfile_ReturnsEmpty()
    {
        var result = PositionProfileInsightEngine.Build(null, [], [], null);

        Assert.Same(PositionProfileInsightBundle.Empty, result);
    }

    [Fact]
    public void Build_NullSelectedPlayer_ReturnsEmpty()
    {
        var profile = new PositionProfileResponseDto
        {
            SelectedPlayer = null,
            MedianProfile = new PositionProfilePlayerDto { PlayerId = 0, FullName = "Median" }
        };

        var result = PositionProfileInsightEngine.Build(profile, [], [], null);

        Assert.Same(PositionProfileInsightBundle.Empty, result);
    }

    [Fact]
    public void Build_NullMedianProfile_ReturnsEmpty()
    {
        var profile = MakeProfile();
        profile.MedianProfile = null;

        var result = PositionProfileInsightEngine.Build(profile, [], [], null);

        Assert.Same(PositionProfileInsightBundle.Empty, result);
    }

    [Fact]
    public void Build_EmptyAxes_ReturnsEmpty()
    {
        var profile = MakeProfile();

        var result = PositionProfileInsightEngine.Build(profile, [], [], null);

        Assert.Same(PositionProfileInsightBundle.Empty, result);
    }

    [Fact]
    public void Build_AllNonEvaluativeAxes_ReturnsEmpty()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("context_pct_jeu", isEvaluative: false),
            MakeAxis("context_matchs", isEvaluative: false)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Same(PositionProfileInsightBundle.Empty, result);
    }

    // ── Performance level tests ──────────────────────────────────────────────

    [Fact]
    public void Build_AllAxesAbove75_WithHighAverage_ReturnsElite()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("goals_per60",        percentile: 85d),
            MakeAxis("assists_per60",      percentile: 80d),
            MakeAxis("interceptions_per60",percentile: 82d),
            MakeAxis("turnovers_per60",    percentile: 78d, higherIsBetter: false)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, "2024-2025");

        Assert.Equal("Elite", result.PerformanceLevel);
        Assert.NotEmpty(result.Strengths);
    }

    [Fact]
    public void Build_AllAxesAbove50_NotEnoughStrong_ReturnsAboveMedian()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("goals_per60",   percentile: 60d),
            MakeAxis("assists_per60", percentile: 55d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Equal("Au-dessus de la mediane", result.PerformanceLevel);
    }

    [Fact]
    public void Build_LowAveragePercentile_ReturnsBelowStandard()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("goals_per60",   percentile: 20d),
            MakeAxis("assists_per60", percentile: 30d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Equal("Sous le standard", result.PerformanceLevel);
    }

    // ── Strengths / weaknesses tests ─────────────────────────────────────────

    [Fact]
    public void Build_OneStrongAxis_StrengthsHasOneEntry()
    {
        var profile = MakeProfile();
        var strongAxis = MakeAxis("goals_per60", percentile: 80d);
        var neutralAxis = MakeAxis("assists_per60", percentile: 50d);
        var axes = new List<PositionProfileAxisViewModel> { strongAxis, neutralAxis };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Single(result.Strengths);
        Assert.Contains("goals_per60", result.Strengths[0]);
        Assert.Empty(result.Weaknesses);
    }

    [Fact]
    public void Build_OneWeakAxis_WeaknessesHasOneEntry()
    {
        var profile = MakeProfile();
        var weakAxis = MakeAxis("turnovers_per60", percentile: 20d, higherIsBetter: false);
        var neutralAxis = MakeAxis("assists_per60", percentile: 50d);
        var axes = new List<PositionProfileAxisViewModel> { weakAxis, neutralAxis };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Single(result.Weaknesses);
        Assert.Empty(result.Strengths);
    }

    [Fact]
    public void Build_Strengths_CappedAtThree()
    {
        var profile = MakeProfile();
        var axes = Enumerable.Range(1, 6)
            .Select(i => MakeAxis($"axis_{i}", percentile: 80d + i))
            .ToList();

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.True(result.Strengths.Count <= 3);
    }

    [Fact]
    public void Build_Weaknesses_CappedAtThree()
    {
        var profile = MakeProfile();
        var axes = Enumerable.Range(1, 6)
            .Select(i => MakeAxis($"axis_{i}", percentile: 10d + i))
            .ToList();

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.True(result.Weaknesses.Count <= 3);
    }

    // ── Goalkeeper profile type tests ─────────────────────────────────────────

    [Fact]
    public void Build_GoalkeeperProfile_HighEfficiency_ReturnsGkHighImpactProfileType()
    {
        var profile = MakeProfile(isGoalkeeper: true);
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("save_rate",       percentile: 75d),
            MakeAxis("saves_per60",     percentile: 70d),
            MakeAxis("shots_faced_per60", percentile: 65d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Equal("Gardienne a fort impact", result.ProfileType);
    }

    [Fact]
    public void Build_GoalkeeperProfile_LowEfficiency_ReturnsGkToRegulateProfileType()
    {
        var profile = MakeProfile(isGoalkeeper: true);
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("save_rate",   percentile: 30d),
            MakeAxis("saves_per60", percentile: 30d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Equal("Gardienne a reguler", result.ProfileType);
    }

    // ── Coach cards and highlights ───────────────────────────────────────────

    [Fact]
    public void Build_StrongAxis_GeneratesStrengthCoachCard()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("goals_per60", percentile: 85d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Contains(result.CoachCards, c => c.Type == CoachCardType.Strength);
    }

    [Fact]
    public void Build_WeakAxis_GeneratesWeaknessCoachCard()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("turnovers_per60", percentile: 15d, higherIsBetter: false)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.Contains(result.CoachCards, c => c.Type == CoachCardType.Weakness);
    }

    [Fact]
    public void Build_StrongAxis_HighlightAxisKeysContainsAxisKey()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("goals_per60", percentile: 85d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.NotEmpty(result.HighlightAxisKeys);
    }

    [Fact]
    public void Build_CoachCards_AtMostFour()
    {
        var profile = MakeProfile();
        var axes = Enumerable.Range(1, 10)
            .Select(i => MakeAxis($"axis_{i}", percentile: i < 5 ? 85d : 20d))
            .ToList();

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.True(result.CoachCards.Count <= 4);
    }

    // ── Snapshot KPIs ────────────────────────────────────────────────────────

    [Fact]
    public void Build_ValidProfile_GeneratesSnapshotKpis()
    {
        var profile = MakeProfile();
        var axes = new List<PositionProfileAxisViewModel>
        {
            MakeAxis("goals_per60", percentile: 70d),
            MakeAxis("assists_per60", percentile: 45d)
        };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, "2024-2025");

        Assert.NotEmpty(result.SnapshotKpis);
    }

    // ── Non-infinite axis values ──────────────────────────────────────────────

    [Fact]
    public void Build_AxisWithNonFinitePercentile_IsFilteredOut()
    {
        var profile = MakeProfile();
        var validAxis = MakeAxis("goals_per60",   percentile: 70d);
        var nanAxis   = MakeAxis("bad_axis",      percentile: double.NaN);
        var infAxis   = MakeAxis("inf_axis",      percentile: double.PositiveInfinity);
        var axes = new List<PositionProfileAxisViewModel> { validAxis, nanAxis, infAxis };

        var result = PositionProfileInsightEngine.Build(profile, axes, axes, null);

        Assert.NotSame(PositionProfileInsightBundle.Empty, result);
        Assert.Equal("Au-dessus de la mediane", result.PerformanceLevel);
    }
}
