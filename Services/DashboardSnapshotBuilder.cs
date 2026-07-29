using HandWStat.Models.Analytics;
using HandballManagerCore.DTO;

namespace HandWStat.Services;

public sealed class DashboardSnapshotBuilder
{
    public OverviewMetrics BuildOverview(StatsOverviewDto overview)
    {
        return new OverviewMetrics(
            overview.PlayerCount,
            overview.TeamCount,
            overview.MatchCount,
            overview.GoalCount
                + overview.PenaltyGoalCount
                + overview.AssistCount
                + overview.InterceptionCount
                + overview.SaveCount
                + overview.TurnoverCount
                + overview.SanctionCount,
            overview.GoalCount + overview.PenaltyGoalCount,
            overview.AssistCount,
            overview.InterceptionCount,
            overview.SaveCount,
            overview.TurnoverCount,
            overview.SanctionCount);
    }

    public IReadOnlyList<PlayerDirectoryItem> BuildPlayerDirectory(IReadOnlyList<PlayerGlobalStatsDto> players)
    {
        return players
            .Select(player => new PlayerDirectoryItem(
                player.PlayerId,
                player.FullName,
                player.TeamName ?? "Equipe non renseignee",
                player.PositionName ?? "Poste non renseigne",
                player.Nationality,
                player.Age,
                player.IsGoalkeeper))
            .OrderBy(player => player.FullName)
            .ToList();
    }
}
