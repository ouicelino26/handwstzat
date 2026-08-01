using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Analytics;

namespace HandWStat.Tests;

/// <summary>
/// Phase G.1 validation tests — score timeline, position slot resolution,
/// export request shape, scope service, and match scenario analysis.
/// </summary>
public sealed class PhaseG1ValidationTests
{
    // ── MatchScenarioAnalyzer.BuildScoreTimeline ──────────────────────────────

    [Fact]
    public void Timeline_EmptyEvents_ReturnsOnlyStartAndEndMarkers()
    {
        var result = MatchScenarioAnalyzer.BuildScoreTimeline([], match: null);

        Assert.True(result.Count >= 2, "Expected at least start and end markers");
        Assert.Equal("00:00", result[0].Label);
        Assert.Equal(0, result[0].Team1Score);
        Assert.Equal(0, result[0].Team2Score);
    }

    [Fact]
    public void Timeline_EventsWithScores_BuildsCorrectProgression()
    {
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:10:00", "MT1", team1: 1, team2: 0),
            MakeEvent(2, "00:20:00", "MT1", team1: 1, team2: 1),
            MakeEvent(3, "00:30:00", "MT1", team1: 2, team2: 1),
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match: null);

        Assert.Contains(result, p => p.Team1Score == 1 && p.Team2Score == 0);
        Assert.Contains(result, p => p.Team1Score == 1 && p.Team2Score == 1);
        Assert.Contains(result, p => p.Team1Score == 2 && p.Team2Score == 1);
    }

    [Fact]
    public void Timeline_SecondHalfEvents_ClockOffsetBy30Minutes()
    {
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:05:00", "MT2", team1: 10, team2: 9),
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match: null);

        var point = result.FirstOrDefault(p => p.Team1Score == 10 && p.Team2Score == 9);
        Assert.NotNull(point);
        Assert.True(point.Minute >= 35, $"Expected minute >= 35 for MT2 event at 5:00, got {point.Minute}");
    }

    [Fact]
    public void Timeline_AllNullScores_OnlyMarkers()
    {
        // Events where both scores are null should be skipped entirely
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:10:00", "MT1", team1: null, team2: null),
            MakeEvent(2, "00:20:00", "MT1", team1: null, team2: null),
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match: null);

        var nonMarkers = result.Where(p => !p.IsMarker).ToList();
        Assert.Empty(nonMarkers);
    }

    [Fact]
    public void Timeline_PartialNullScores_UsesFallback()
    {
        // Team2 has a value but Team1 is null — should use last known Team1 value
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:05:00", "MT1", team1: 1, team2: 0),
            MakeEvent(2, "00:10:00", "MT1", team1: null, team2: 1), // team1 null → use 1
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match: null);

        var second = result.FirstOrDefault(p => p.Team2Score == 1 && !p.IsMarker);
        Assert.NotNull(second);
        Assert.Equal(1, second.Team1Score);
    }

    [Fact]
    public void Timeline_DuplicateScores_DeduplicatedBySkipping()
    {
        // Two events with identical (team1, team2) should not produce duplicate points
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:10:00", "MT1", team1: 3, team2: 2),
            MakeEvent(2, "00:11:00", "MT1", team1: 3, team2: 2), // same score — no-op
            MakeEvent(3, "00:15:00", "MT1", team1: 4, team2: 2),
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match: null);

        var at3to2 = result.Where(p => p.Team1Score == 3 && p.Team2Score == 2 && !p.IsMarker).ToList();
        Assert.Single(at3to2);
    }

    [Fact]
    public void Timeline_HalftimeMarker_IsInsertedAt30Minutes()
    {
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:15:00", "MT1", team1: 7, team2: 6),
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match: null);

        var halftime = result.FirstOrDefault(p => p.IsMarker && p.MarkerLabel == "Mi-temps");
        Assert.NotNull(halftime);
        Assert.InRange(halftime.Minute, 29.9d, 30.1d);
    }

    [Fact]
    public void Timeline_FinalMarker_ReflectsMatchScoreWhenProvided()
    {
        var match = new MatchListItemDto { MatchId = 1, Team1Score = 28, Team2Score = 25, Season = "2025-2026" };
        var events = new List<MatchEventAnalyticsDto>
        {
            MakeEvent(1, "00:15:00", "MT1", team1: 5, team2: 3),
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match);

        var fin = result.LastOrDefault();
        Assert.NotNull(fin);
        Assert.Equal(28, fin.Team1Score);
        Assert.Equal(25, fin.Team2Score);
    }

    // ── BuildTimelineKpis ─────────────────────────────────────────────────────

    [Fact]
    public void BuildTimelineKpis_TwoPointTimeline_ReturnsKpis()
    {
        var points = BuildSimpleTimeline(10, 8);
        var kpis = MatchScenarioAnalyzer.BuildTimelineKpis(points, "Eq1", "Eq2");

        Assert.NotEmpty(kpis);
        Assert.Contains(kpis, k => k.Label == "Score a la pause");
        Assert.Contains(kpis, k => k.Label == "Ecart final");
    }

    [Fact]
    public void BuildTimelineKpis_EmptyPoints_ReturnsEmpty()
    {
        var kpis = MatchScenarioAnalyzer.BuildTimelineKpis(
            [new ScoreTimelinePoint("00:00", 0, 0, 0)],
            "Eq1", "Eq2");

        Assert.Empty(kpis);
    }

    // ── AnalysisScopeService ──────────────────────────────────────────────────

    [Fact]
    public void ScopeService_Update_FiresChangedEvent()
    {
        var svc = new AnalysisScopeService();
        var fired = false;
        svc.Changed += () => fired = true;

        svc.Update(new AnalysisScopeSnapshot(1, "LFH", null, null, "2025-2026", null));

        Assert.True(fired);
    }

    [Fact]
    public void ScopeService_UpdateSameValue_DoesNotFireEvent()
    {
        var svc = new AnalysisScopeService();
        var snapshot = new AnalysisScopeSnapshot(1, "LFH", null, null, "2025-2026", null);
        svc.Update(snapshot);

        var firedCount = 0;
        svc.Changed += () => firedCount++;
        svc.Update(snapshot); // same value

        Assert.Equal(0, firedCount);
    }

    [Fact]
    public void ScopeService_Reset_ClearsCurrentSnapshot()
    {
        var svc = new AnalysisScopeService();
        svc.Update(new AnalysisScopeSnapshot(1, "LFH", 5, "Paris", "2025-2026", "J12"));

        svc.Reset();

        Assert.Equal(AnalysisScopeSnapshot.Empty, svc.Current);
        Assert.False(svc.Current.HasValue);
    }

    [Fact]
    public void ScopeService_HasValue_FalseForEmptySnapshot()
    {
        Assert.False(AnalysisScopeSnapshot.Empty.HasValue);
    }

    [Fact]
    public void ScopeService_HasValue_TrueWhenSeasonSet()
    {
        var snap = new AnalysisScopeSnapshot(null, null, null, null, "2025-2026", null);
        Assert.True(snap.HasValue);
    }

    [Fact]
    public void ScopeService_HasValue_TrueWhenCompetitionSet()
    {
        var snap = new AnalysisScopeSnapshot(1, "LFH", null, null, null, null);
        Assert.True(snap.HasValue);
    }

    // ── MatchFilterCatalog ────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("  ", null)]
    [InlineData("J12", "J12")]
    [InlineData("2025-2026", "2025-2026")]
    [InlineData("  J8  ", "J8")]
    public void MatchFilterCatalog_NormalizeSelection_WorksCorrectly(string? input, string? expected)
    {
        var result = MatchFilterCatalog.NormalizeSelection(input);
        Assert.Equal(expected, result);
    }

    // ── HandballKpiHelper ─────────────────────────────────────────────────────

    [Fact]
    public void HandballKpiHelper_FormatSigned_PositiveAddsPlusSign()
    {
        Assert.Equal("+3", HandballKpiHelper.FormatSigned(3));
    }

    [Fact]
    public void HandballKpiHelper_FormatSigned_NegativeKeepsSign()
    {
        Assert.Equal("-2", HandballKpiHelper.FormatSigned(-2));
    }

    [Fact]
    public void HandballKpiHelper_FormatSigned_ZeroIsZero()
    {
        Assert.Equal("0", HandballKpiHelper.FormatSigned(0));
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static MatchEventAnalyticsDto MakeEvent(
        int id, string time, string miTemps, int? team1, int? team2)
    {
        return new MatchEventAnalyticsDto
        {
            MatchEventId = id,
            Time = TimeSpan.TryParse(time, out var ts) ? ts : TimeSpan.Zero,
            MiTemps = miTemps,
            TeamScore1 = team1,
            TeamScore2 = team2,
            EventName = "But"
        };
    }

    private static IReadOnlyList<ScoreTimelinePoint> BuildSimpleTimeline(int finalTeam1, int finalTeam2)
    {
        return
        [
            new ScoreTimelinePoint("00:00", 0, 0, 0, true, "Debut"),
            new ScoreTimelinePoint("15:00", 15, 5, 4),
            new ScoreTimelinePoint("30:00", 30, finalTeam1 / 2, finalTeam2 / 2, true, "Mi-temps"),
            new ScoreTimelinePoint("45:00", 45, finalTeam1 - 3, finalTeam2 - 2),
            new ScoreTimelinePoint("60:00", 60, finalTeam1, finalTeam2, true, "Fin"),
        ];
    }
}
