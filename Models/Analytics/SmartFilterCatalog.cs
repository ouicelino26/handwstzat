using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public static class SmartFilterCatalog
{
    public static IReadOnlyList<CompetitionDto> GetCompetitions(
        AnalyticsReferenceData referenceData,
        IReadOnlyList<MatchListItemDto> matches,
        bool constrain)
    {
        if (!constrain || matches.Count == 0)
        {
            return referenceData.Competitions;
        }

        var ids = matches
            .Where(match => match.CompetitionId.HasValue)
            .Select(match => match.CompetitionId!.Value)
            .Distinct()
            .ToHashSet();

        var filtered = referenceData.Competitions
            .Where(competition => ids.Contains(competition.CompetitionId))
            .OrderBy(item => item.CompetitionName)
            .ToList();

        return filtered.Count > 0 ? filtered : referenceData.Competitions;
    }

    public static IReadOnlyList<TeamDto> GetTeams(
        AnalyticsReferenceData referenceData,
        IReadOnlyList<MatchListItemDto> matches,
        bool constrain)
    {
        if (!constrain || matches.Count == 0)
        {
            return referenceData.Teams;
        }

        var ids = matches
            .SelectMany(match => new[] { match.Team1Id, match.Team2Id })
            .Where(teamId => teamId.HasValue)
            .Select(teamId => teamId!.Value)
            .Distinct()
            .ToHashSet();

        var filtered = referenceData.Teams
            .Where(team => ids.Contains(team.TeamId))
            .OrderBy(item => item.TeamName)
            .ToList();

        return filtered.Count > 0 ? filtered : referenceData.Teams;
    }

    public static IReadOnlyList<LookupItemDto> GetPositions(
        IReadOnlyList<LookupItemDto> positions,
        IReadOnlyList<PlayerListItemDto> players,
        bool constrain)
    {
        if (!constrain || players.Count == 0)
        {
            return positions;
        }

        var ids = players
            .Where(player => player.PositionId.HasValue)
            .Select(player => player.PositionId!.Value)
            .Distinct()
            .ToHashSet();

        var filtered = positions
            .Where(position => ids.Contains(position.Id))
            .OrderBy(item => item.Name)
            .ToList();

        return filtered.Count > 0 ? filtered : positions;
    }
}
