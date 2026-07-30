using HandWStat.Models.Contracts;
using HandWStat.Services;

namespace HandWStat.Tests;

public sealed class DashboardSnapshotBuilderTests
{
    [Fact]
    public void BuildOverview_IncludesPenaltyGoalsOnce()
    {
        var builder = new DashboardSnapshotBuilder();
        var overview = builder.BuildOverview(new StatsOverviewDto
        {
            PlayerCount = 9,
            TeamCount = 2,
            MatchCount = 1,
            GoalCount = 5,
            PenaltyGoalCount = 2,
            AssistCount = 3,
            InterceptionCount = 1,
            SaveCount = 4,
            TurnoverCount = 2,
            SanctionCount = 1
        });

        Assert.Equal(7, overview.GoalCount);
        Assert.Equal(18, overview.EventCount);
    }

    [Fact]
    public void BuildPlayerDirectory_UsesSafeV1FallbackLabels()
    {
        var builder = new DashboardSnapshotBuilder();
        var players = builder.BuildPlayerDirectory(
        [
            new PlayerGlobalStatsDto { PlayerId = 1, FullName = "A", TeamName = null, PositionName = null }
        ]);

        var player = Assert.Single(players);
        Assert.Equal("Equipe non renseignee", player.TeamName);
        Assert.Equal("Poste non renseigne", player.PositionName);
    }
}
