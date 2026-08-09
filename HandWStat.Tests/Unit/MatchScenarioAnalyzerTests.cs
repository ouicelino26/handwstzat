using HandWStat.Models.Analytics;
using HandballManagerCore.DTO;
using HandballManagerCore.Models;

namespace HandWStat.Tests.Unit;

public class MatchScenarioAnalyzerTests
{
    // BuildScoreTimeline always starts at 00:00 and ends with a "Fin" marker.

    [Fact]
    public void BuildScoreTimeline_EmptyEvents_HasStartAndFin()
    {
        var result = MatchScenarioAnalyzer.BuildScoreTimeline([], null);

        Assert.Contains(result, p => p.Label == "00:00");
        Assert.Contains(result, p => p.IsMarker && p.MarkerLabel == "Fin");
    }

    [Fact]
    public void BuildScoreTimeline_EmptyEvents_HasMiTempsMarker()
    {
        var result = MatchScenarioAnalyzer.BuildScoreTimeline([], null);

        Assert.Contains(result, p => p.IsMarker && p.MarkerLabel == "Mi-temps");
    }

    [Fact]
    public void BuildScoreTimeline_NullMatchUsesLastKnownScore()
    {
        var events = new List<MatchEvent>
        {
            new() { Time = TimeSpan.FromMinutes(10), MiTemps = "1", TeamScore1 = 5, TeamScore2 = 3 }
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, null);

        var fin = result.Last(p => p.IsMarker && p.MarkerLabel == "Fin");
        Assert.Equal(5, fin.Team1Score);
        Assert.Equal(3, fin.Team2Score);
    }

    [Fact]
    public void BuildScoreTimeline_MatchScoreOverridesLastEvent()
    {
        var events = new List<MatchEvent>
        {
            new() { Time = TimeSpan.FromMinutes(10), MiTemps = "1", TeamScore1 = 5, TeamScore2 = 3 }
        };
        var match = new MatchListItemDto { Team1Score = 31, Team2Score = 24 };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, match);

        var fin = result.Last(p => p.IsMarker && p.MarkerLabel == "Fin");
        Assert.Equal(31, fin.Team1Score);
        Assert.Equal(24, fin.Team2Score);
    }

    [Fact]
    public void BuildScoreTimeline_SecondHalfEvent_ShiftedByThirtyMinutes()
    {
        var events = new List<MatchEvent>
        {
            new() { Time = TimeSpan.FromMinutes(5), MiTemps = "2", TeamScore1 = 20, TeamScore2 = 18 }
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, null);

        // A second-half event at 5 min should appear at minute 35
        Assert.Contains(result, p => p.Minute >= 34.9 && p.Minute <= 35.1 && !p.IsMarker);
    }

    [Fact]
    public void BuildScoreTimeline_DuplicateScores_DeduplicatesPoints()
    {
        var events = new List<MatchEvent>
        {
            new() { Id = 1, Time = TimeSpan.FromMinutes(10), MiTemps = "1", TeamScore1 = 5, TeamScore2 = 3 },
            new() { Id = 2, Time = TimeSpan.FromMinutes(11), MiTemps = "1", TeamScore1 = 5, TeamScore2 = 3 },
        };

        var result = MatchScenarioAnalyzer.BuildScoreTimeline(events, null);

        // Only one non-marker point should represent score 5-3 (deduplication)
        var scorePoints = result.Where(p => !p.IsMarker && p.Team1Score == 5 && p.Team2Score == 3).ToList();
        Assert.Single(scorePoints);
    }

    // BuildKeyMoments always includes at least halftime and end.

    [Fact]
    public void BuildKeyMoments_ReturnsAtLeastTwoMoments()
    {
        var timeline = MatchScenarioAnalyzer.BuildScoreTimeline([], null);
        var moments = MatchScenarioAnalyzer.BuildKeyMoments(timeline, "Team A", "Team B");

        Assert.True(moments.Count >= 2);
    }

    // BuildTimelineKpis returns non-null list.

    [Fact]
    public void BuildTimelineKpis_EmptyTimeline_ReturnsKpiList()
    {
        var timeline = MatchScenarioAnalyzer.BuildScoreTimeline([], null);
        var kpis = MatchScenarioAnalyzer.BuildTimelineKpis(timeline, "A", "B");

        Assert.NotNull(kpis);
        Assert.NotEmpty(kpis);
    }
}
