using System.Globalization;
using System.Text;
using HandWStat.Models.Analytics;
using HandWStat.Services.Api;
using HandballManagerCore.DTO;

namespace HandWStat.Services;

public sealed class TeamOfTheDayService
{
    private static readonly PositionSlot[] KnownSlots =
    [
        new("goalkeeper", "Gardienne", "goalkeeper", 1, ["GB", "GK", "GARDIENNE", "GARDIEN", "GOALKEEPER"]),
        new("left-wing", "Ailiere gauche", "left-wing", 2, ["ALG", "AG", "AIG", "AILIERE GAUCHE", "AILIER GAUCHE", "LEFT WING", "LW"]),
        new("left-back", "Arriere gauche", "left-back", 3, ["ARG", "AR G", "ARRIERE GAUCHE", "LEFT BACK", "LB"]),
        new("center-back", "Demi-centre", "center-back", 4, ["DC", "DEMI CENTRE", "DEMI-CENTRE", "CENTRE", "CENTER BACK", "PLAYMAKER", "CB"]),
        new("pivot", "Pivot", "pivot", 5, ["P", "PIV", "PIVOT", "LINE PLAYER", "LP"]),
        new("right-back", "Arriere droite", "right-back", 6, ["ARD", "AR D", "ARRIERE DROITE", "RIGHT BACK", "RB"]),
        new("right-wing", "Ailiere droite", "right-wing", 7, ["ALD", "AD", "AID", "AILIERE DROITE", "AILIER DROIT", "RIGHT WING", "RW"])
    ];

    private readonly StatsApiClient _statsApiClient;
    private readonly MatchesApiClient _matchesApiClient;
    private readonly IApiAuthService _authService;

    public TeamOfTheDayService(
        StatsApiClient statsApiClient,
        MatchesApiClient matchesApiClient,
        IApiAuthService authService)
    {
        _statsApiClient = statsApiClient;
        _matchesApiClient = matchesApiClient;
        _authService = authService;
    }

    public async Task<TeamOfTheDaySnapshotDto> LoadAsync(
        DashboardFilterState filters,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.Session.IsAuthenticated)
        {
            throw new InvalidOperationException("Connexion requise pour calculer l'equipe type.");
        }

        var scopeMatches = await _matchesApiClient.GetMatchesAsync(
            competitionId: filters.CompetitionId,
            teamId: filters.TeamId,
            from: filters.From,
            to: filters.To,
            year: filters.Year,
            season: filters.Season,
            day: filters.Day,
            page: 1,
            pageSize: 500,
            cancellationToken: cancellationToken);

        if (scopeMatches.Count == 0)
        {
            return TeamOfTheDaySnapshotDto.Empty("Aucun match dans le scope courant pour construire l'equipe type.");
        }

        var anchorMatch = ResolveAnchorMatch(scopeMatches, filters);
        var effectiveSeason = MatchFilterCatalog.NormalizeSelection(filters.Season) ?? MatchFilterCatalog.NormalizeSelection(anchorMatch?.Season);
        var effectiveDay = MatchFilterCatalog.NormalizeSelection(filters.Day) ?? MatchFilterCatalog.NormalizeSelection(anchorMatch?.Day);

        if (string.IsNullOrWhiteSpace(effectiveDay))
        {
            return TeamOfTheDaySnapshotDto.Empty("Selectionnez une journee pour activer l'equipe type.");
        }

        var dayMatches = scopeMatches
            .Where(match => MatchesEffectiveScope(match, effectiveSeason, effectiveDay))
            .OrderBy(match => match.Date ?? DateTime.MinValue)
            .ThenBy(match => match.MatchId)
            .ToList();

        if (dayMatches.Count == 0)
        {
            var refreshedMatches = await _matchesApiClient.GetMatchesAsync(
                competitionId: filters.CompetitionId,
                teamId: filters.TeamId,
                from: filters.From,
                to: filters.To,
                year: filters.Year,
                season: effectiveSeason,
                day: effectiveDay,
                page: 1,
                pageSize: 500,
                cancellationToken: cancellationToken);
            dayMatches = refreshedMatches.ToList();
        }

        if (dayMatches.Count == 0)
        {
            return TeamOfTheDaySnapshotDto.Empty("La journee cible ne contient aucun match exploitable.");
        }

        var queryOptions = filters.ToStatsQueryOptions();
        queryOptions.Season = effectiveSeason;
        queryOptions.Day = effectiveDay;
        queryOptions.MatchId = null;
        queryOptions.PositionId = null;

        var players = await _statsApiClient.GetPlayersAsync(queryOptions, cancellationToken);
        if (players.Count == 0)
        {
            return TeamOfTheDaySnapshotDto.Empty("Aucune joueuse statistique trouvee sur cette journee.");
        }

        var playerIds = players
            .Select(player => player.PlayerId)
            .Where(playerId => playerId > 0)
            .Distinct()
            .ToList();

        var compareResponse = await _statsApiClient.ComparePlayersAsync(new ComparePlayersRequestDto
        {
            PlayerIds = playerIds,
            CompetitionId = queryOptions.CompetitionId,
            TeamId = queryOptions.TeamId,
            MatchId = queryOptions.MatchId,
            From = queryOptions.From,
            To = queryOptions.To,
            Year = queryOptions.Year,
            Season = queryOptions.Season,
            Day = queryOptions.Day
        }, cancellationToken) ?? new ComparePlayersResponseDto();

        var playerMatchMap = await LoadPlayerMatchMapAsync(dayMatches, cancellationToken);
        var candidates = BuildCandidates(compareResponse, players, playerMatchMap, dayMatches);
        var groups = BuildGroups(candidates);

        return new TeamOfTheDaySnapshotDto
        {
            EffectiveSeason = effectiveSeason,
            EffectiveDay = effectiveDay,
            MatchCount = dayMatches.Count,
            CandidateCount = candidates.Count,
            Groups = groups,
            WarningMessage = groups.Count == 0
                ? "Aucune joueuse n'a assez de statistiques valides pour etre selectionnee."
                : null
        };
    }

    private async Task<Dictionary<int, MatchListItemDto>> LoadPlayerMatchMapAsync(
        IReadOnlyList<MatchListItemDto> matches,
        CancellationToken cancellationToken)
    {
        var matchTasks = matches.Select(async match =>
        {
            try
            {
                var players = await _statsApiClient.GetMatchPlayersAsync(match.MatchId, cancellationToken);
                return (Match: match, Players: players);
            }
            catch
            {
                return (Match: match, Players: Array.Empty<PlayerGlobalStatsDto>() as IReadOnlyList<PlayerGlobalStatsDto>);
            }
        });

        var results = await Task.WhenAll(matchTasks);
        var map = new Dictionary<int, MatchListItemDto>();

        foreach (var result in results.OrderByDescending(item => item.Match.Date ?? DateTime.MinValue))
        {
            foreach (var player in result.Players)
            {
                if (player.PlayerId > 0 && !map.ContainsKey(player.PlayerId))
                {
                    map[player.PlayerId] = result.Match;
                }
            }
        }

        return map;
    }

    private static IReadOnlyList<TeamOfTheDayCandidateDto> BuildCandidates(
        ComparePlayersResponseDto compareResponse,
        IReadOnlyList<PlayerGlobalStatsDto> fallbackPlayers,
        IReadOnlyDictionary<int, MatchListItemDto> playerMatchMap,
        IReadOnlyList<MatchListItemDto> dayMatches)
    {
        var players = compareResponse.Players.Count > 0 ? compareResponse.Players : fallbackPlayers;
        var cohortHasPlayingTime = players.Any(player => player.PlayingTimeMinutes > 0 || player.MatchesWithPlayingTime > 0);
        var fallbackByPlayer = fallbackPlayers.ToDictionary(player => player.PlayerId);
        var offenseByPlayer = compareResponse.Offense.ToDictionary(item => item.PlayerId);
        var technicalByPlayer = compareResponse.Technical.ToDictionary(item => item.PlayerId);
        var defenseByPlayer = compareResponse.Defense.ToDictionary(item => item.PlayerId);
        var passingByPlayer = compareResponse.Passing.ToDictionary(item => item.PlayerId);
        var sanctionsByPlayer = compareResponse.Sanctions.ToDictionary(item => item.PlayerId);
        var goalkeeperByPlayer = compareResponse.Goalkeeper.ToDictionary(item => item.PlayerId);

        var candidates = new List<TeamOfTheDayCandidateDto>();

        foreach (var player in players)
        {
            if (player.PlayerId <= 0)
            {
                continue;
            }

            fallbackByPlayer.TryGetValue(player.PlayerId, out var fallbackPlayer);
            offenseByPlayer.TryGetValue(player.PlayerId, out var offense);
            technicalByPlayer.TryGetValue(player.PlayerId, out var technical);
            defenseByPlayer.TryGetValue(player.PlayerId, out var defense);
            passingByPlayer.TryGetValue(player.PlayerId, out var passing);
            sanctionsByPlayer.TryGetValue(player.PlayerId, out var sanctions);
            goalkeeperByPlayer.TryGetValue(player.PlayerId, out var goalkeeper);

            var enrichedPlayer = MergePlayer(player, fallbackPlayer);
            var slot = ResolvePositionSlot(enrichedPlayer);
            if (slot is null)
            {
                continue;
            }

            if (!HasValidParticipation(enrichedPlayer, cohortHasPlayingTime))
            {
                continue;
            }

            var statLine = BuildStatLine(enrichedPlayer, offense, technical, defense, passing, sanctions, goalkeeper);
            if (statLine.TotalVolume <= 0)
            {
                continue;
            }

            var scores = TeamOfTheDayPieScoring.Calculate(statLine, enrichedPlayer.IsGoalkeeper || slot.Key == "goalkeeper");
            var match = playerMatchMap.TryGetValue(enrichedPlayer.PlayerId, out var mappedMatch)
                ? mappedMatch
                : dayMatches.Count == 1 ? dayMatches[0] : null;

            candidates.Add(new TeamOfTheDayCandidateDto
            {
                PlayerId = enrichedPlayer.PlayerId,
                FullName = Clean(enrichedPlayer.FullName, $"Joueuse {enrichedPlayer.PlayerId}"),
                TeamId = enrichedPlayer.TeamId,
                TeamName = Clean(enrichedPlayer.TeamName, "Equipe non renseignee"),
                PositionLabel = slot.Label,
                SlotKey = slot.Key,
                FormationArea = slot.FormationArea,
                IsGoalkeeper = enrichedPlayer.IsGoalkeeper || slot.Key == "goalkeeper",
                MatchesPlayed = enrichedPlayer.MatchesPlayed,
                MatchId = match?.MatchId,
                MatchLabel = match is null ? null : $"{Clean(match.Team1Name, "Equipe A")} vs {Clean(match.Team2Name, "Equipe B")}",
                MatchScore = match is null ? null : $"{match.Team1Score ?? 0} - {match.Team2Score ?? 0}",
                OpponentName = ResolveOpponent(match, enrichedPlayer.TeamId, enrichedPlayer.TeamName),
                PlayingTimeMinutes = enrichedPlayer.PlayingTimeMinutes,
                PieGlobal = scores.Global,
                PieOffense = scores.Offense,
                PieDefense = scores.Defense,
                StatLine = statLine
            });
        }

        return candidates;
    }

    private static IReadOnlyList<TeamOfTheDayPositionGroupDto> BuildGroups(IReadOnlyList<TeamOfTheDayCandidateDto> candidates)
    {
        return candidates
            .GroupBy(candidate => candidate.SlotKey)
            .Select(group =>
            {
                var slot = KnownSlots.FirstOrDefault(item => item.Key == group.Key);
                var first = group.First();

                return new TeamOfTheDayPositionGroupDto
                {
                    SlotKey = group.Key,
                    PositionLabel = slot?.Label ?? first.PositionLabel,
                    FormationArea = slot?.FormationArea ?? first.FormationArea,
                    SortOrder = slot?.SortOrder ?? 99,
                    Candidates = group
                        .OrderByDescending(candidate => candidate.PieGlobal)
                        .ThenBy(candidate => candidate.FullName)
                        .ToList()
                };
            })
            .OrderBy(group => group.SortOrder)
            .ThenBy(group => group.PositionLabel)
            .ToList();
    }

    private static TeamOfTheDayStatLineDto BuildStatLine(
        PlayerGlobalStatsDto player,
        PlayerOffenseStatsDto? offense,
        PlayerTechnicalStatsDto? technical,
        PlayerDefenseStatsDto? defense,
        PlayerPassingStatsDto? passing,
        PlayerSanctionStatsDto? sanctions,
        PlayerGoalkeeperStatsDto? goalkeeper)
    {
        var goals = offense?.TotalButs ?? goalkeeper?.Buts ?? player.TotalGoals;
        var penaltyGoals = offense?.Buts7m ?? player.PenaltyGoalCount;
        var assists = passing?.PasseDecisive ?? goalkeeper?.PasseDecisives ?? player.AssistCount;
        var interceptions = defense?.Interceptions ?? player.InterceptionCount;
        var blocks = defense?.Contres ?? 0;
        var neutralisations = defense?.Neutralisations ?? 0;
        var forcedOffensiveFouls = defense?.PassageForce ?? 0;
        var saves = goalkeeper is null ? player.SaveCount : goalkeeper.Arrets;
        var penaltySaves = goalkeeper?.ArretsPenalty ?? 0;
        var goalsConceded = goalkeeper is null ? 0 : goalkeeper.ButsPris + goalkeeper.ButsPenalty;
        var shotsFaced = goalkeeper?.TirsSubis ?? player.SaveCount;
        var shotAttempts = technical?.ShotAttempts ?? HandballKpiHelper.ShotAttempts(offense);
        var shotWaste = technical?.ShotWaste ?? HandballKpiHelper.ShotWaste(offense);
        var turnovers = passing?.TotalPertes ?? goalkeeper?.PerteDeBalle ?? player.TurnoverCount;
        var technicalLosses = technical?.TechnicalLosses ?? HandballKpiHelper.TechnicalLosses(passing);
        var sanctionsTotal = sanctions is null ? player.SanctionCount : HandballKpiHelper.TotalSanctions(sanctions);
        var penaltiesConceded = sanctions?.PenaltyConcede ?? 0;
        var shotSuccessRate = offense?.TauxReussiteTir ?? goalkeeper?.TauxReussiteTir ?? player.ShotSuccessRate;
        var overallShotSuccessRate = technical?.OverallShotSuccessRate ?? HandballKpiHelper.OverallShotSuccessRate(offense);
        var goalkeeperSaveRate = technical?.GoalkeeperSaveRate ?? goalkeeper?.TauxArret ?? player.GoalkeeperSaveRate;

        return new TeamOfTheDayStatLineDto
        {
            Goals = goals,
            PenaltyGoals = penaltyGoals,
            Assists = assists,
            Interceptions = interceptions,
            Blocks = blocks,
            Neutralisations = neutralisations,
            ForcedOffensiveFouls = forcedOffensiveFouls,
            Saves = saves,
            PenaltySaves = penaltySaves,
            GoalsConceded = goalsConceded,
            ShotsFaced = shotsFaced,
            ShotAttempts = shotAttempts,
            ShotWaste = shotWaste,
            Turnovers = turnovers,
            TechnicalLosses = technicalLosses,
            Sanctions = sanctionsTotal,
            PenaltiesConceded = penaltiesConceded,
            ShotSuccessRate = shotSuccessRate,
            OverallShotSuccessRate = overallShotSuccessRate,
            GoalkeeperSaveRate = goalkeeperSaveRate
        };
    }

    private static PlayerGlobalStatsDto MergePlayer(PlayerGlobalStatsDto player, PlayerGlobalStatsDto? fallbackPlayer)
    {
        if (fallbackPlayer is null)
        {
            return player;
        }

        player.FullName = string.IsNullOrWhiteSpace(player.FullName) ? fallbackPlayer.FullName : player.FullName;
        player.TeamId ??= fallbackPlayer.TeamId;
        player.TeamCode ??= fallbackPlayer.TeamCode;
        player.TeamName ??= fallbackPlayer.TeamName;
        player.PositionId ??= fallbackPlayer.PositionId;
        player.PositionCode ??= fallbackPlayer.PositionCode;
        player.PositionName ??= fallbackPlayer.PositionName;
        player.IsGoalkeeper = player.IsGoalkeeper || fallbackPlayer.IsGoalkeeper;

        if (player.MatchesPlayed <= 0)
        {
            player.MatchesPlayed = fallbackPlayer.MatchesPlayed;
        }

        if (player.PlayingTimeMinutes <= 0)
        {
            player.PlayingTimeMinutes = fallbackPlayer.PlayingTimeMinutes;
        }

        if (player.MatchesWithPlayingTime <= 0)
        {
            player.MatchesWithPlayingTime = fallbackPlayer.MatchesWithPlayingTime;
        }

        return player;
    }

    private static bool HasValidParticipation(PlayerGlobalStatsDto player, bool cohortHasPlayingTime)
    {
        if (cohortHasPlayingTime)
        {
            return player.PlayingTimeMinutes > 0 || player.MatchesWithPlayingTime > 0;
        }

        return player.MatchesPlayed > 0;
    }

    private static PositionSlot? ResolvePositionSlot(PlayerGlobalStatsDto player)
    {
        if (player.IsGoalkeeper)
        {
            return KnownSlots[0];
        }

        var candidates = new[]
        {
            player.PositionCode,
            player.PositionName
        };

        foreach (var value in candidates.Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            var normalized = NormalizePosition(value!);
            var compact = normalized.Replace(" ", string.Empty, StringComparison.Ordinal);
            var slot = KnownSlots.FirstOrDefault(item => item.Aliases.Any(alias =>
            {
                var normalizedAlias = NormalizePosition(alias);
                return normalizedAlias == normalized || normalizedAlias.Replace(" ", string.Empty, StringComparison.Ordinal) == compact;
            }));

            if (slot is not null)
            {
                return slot;
            }
        }

        var fallbackLabel = Clean(player.PositionName ?? player.PositionCode, string.Empty);
        if (string.IsNullOrWhiteSpace(fallbackLabel))
        {
            return null;
        }

        var fallbackKey = NormalizePosition(fallbackLabel).ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal);
        return new PositionSlot(fallbackKey, fallbackLabel, fallbackKey, 99, [fallbackLabel]);
    }

    private static MatchListItemDto? ResolveAnchorMatch(IReadOnlyList<MatchListItemDto> matches, DashboardFilterState filters)
    {
        var query = matches.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.Day))
        {
            query = query.Where(match => SameText(match.Day, filters.Day));
        }

        if (!string.IsNullOrWhiteSpace(filters.Season))
        {
            query = query.Where(match => SameText(match.Season, filters.Season));
        }

        return query
            .OrderByDescending(match => match.Date ?? DateTime.MinValue)
            .ThenByDescending(match => match.MatchId)
            .FirstOrDefault()
            ?? matches
                .OrderByDescending(match => match.Date ?? DateTime.MinValue)
                .ThenByDescending(match => match.MatchId)
                .FirstOrDefault();
    }

    private static bool MatchesEffectiveScope(MatchListItemDto match, string? season, string? day)
    {
        if (!string.IsNullOrWhiteSpace(season) && !SameText(match.Season, season))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(day) || SameText(match.Day, day);
    }

    private static string? ResolveOpponent(MatchListItemDto? match, int? teamId, string? teamName)
    {
        if (match is null)
        {
            return null;
        }

        if (teamId.HasValue)
        {
            if (match.Team1Id == teamId)
            {
                return Clean(match.Team2Name, "Adversaire");
            }

            if (match.Team2Id == teamId)
            {
                return Clean(match.Team1Name, "Adversaire");
            }
        }

        if (!string.IsNullOrWhiteSpace(teamName))
        {
            if (SameText(match.Team1Name, teamName))
            {
                return Clean(match.Team2Name, "Adversaire");
            }

            if (SameText(match.Team2Name, teamName))
            {
                return Clean(match.Team1Name, "Adversaire");
            }
        }

        return null;
    }

    private static bool SameText(string? left, string? right)
    {
        return string.Equals(
            MatchFilterCatalog.NormalizeSelection(left),
            MatchFilterCatalog.NormalizeSelection(right),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePosition(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : ' ');
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string Clean(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private sealed record PositionSlot(
        string Key,
        string Label,
        string FormationArea,
        int SortOrder,
        IReadOnlyList<string> Aliases);
}
