using HandWStat.Models.Analytics;
using HandWStat.Services.Api;
using HandballManagerCore.DTO;

namespace HandWStat.Services;

public sealed class StatsDashboardService
{
    private readonly StatsApiClient _statsApiClient;
    private readonly PlayersApiClient _playersApiClient;
    private readonly MatchesApiClient _matchesApiClient;
    private readonly TeamOfTheDayService _teamOfTheDayService;
    private readonly IApiAuthService _authService;

    public StatsDashboardService(
        StatsApiClient statsApiClient,
        PlayersApiClient playersApiClient,
        MatchesApiClient matchesApiClient,
        TeamOfTheDayService teamOfTheDayService,
        IApiAuthService authService)
    {
        _statsApiClient = statsApiClient;
        _playersApiClient = playersApiClient;
        _matchesApiClient = matchesApiClient;
        _teamOfTheDayService = teamOfTheDayService;
        _authService = authService;
    }

    public async Task<DashboardSnapshot> LoadDashboardAsync(
        DashboardFilterState filters,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        _ = forceRefresh;

        if (!_authService.Session.IsAuthenticated)
        {
            throw new InvalidOperationException("Connexion requise pour ouvrir l'interface live.");
        }

        try
        {
            var queryOptions = filters.ToStatsQueryOptions();
            var rankingMetric = NormalizeRankingMetric(filters.RankingMetric);
            var rankingTop = Math.Clamp(filters.Top, 3, 12);

            var overviewTask = _statsApiClient.GetOverviewAsync(queryOptions, cancellationToken);
            var playersTask = _statsApiClient.GetPlayersAsync(queryOptions, cancellationToken);
            var topScorersTask = _statsApiClient.GetRankingsAsync("goals", queryOptions, rankingTop, cancellationToken);
            var efficiencyTask = _statsApiClient.GetRankingsAsync("shotsuccess", queryOptions, rankingTop, cancellationToken);
            var requestedTask = _statsApiClient.GetRankingsAsync(rankingMetric, queryOptions, rankingTop, cancellationToken);
            var interceptionsTask = _statsApiClient.GetRankingsAsync("interceptions", queryOptions, 5, cancellationToken);
            var recentMatchesTask = _matchesApiClient.GetMatchesAsync(
                competitionId: filters.CompetitionId,
                teamId: filters.TeamId,
                from: filters.From,
                to: filters.To,
                year: filters.Year,
                season: filters.Season,
                day: filters.Day,
                page: 1,
                pageSize: 6,
                cancellationToken: cancellationToken);

            await Task.WhenAll(
                overviewTask,
                playersTask,
                topScorersTask,
                efficiencyTask,
                requestedTask,
                interceptionsTask,
                recentMatchesTask);

            var overview = overviewTask.Result ?? new StatsOverviewDto();
            var playerStats = playersTask.Result;
            var players = playerStats
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

            var selectedPlayerId = ResolveSelectedPlayerId(filters.SpotlightPlayerId, playerStats, topScorersTask.Result, requestedTask.Result);
            var globalBoardsTask = LoadGlobalBoardsAsync(queryOptions, playerStats, cancellationToken);
            var teamOfTheDayTask = LoadTeamOfTheDaySafeAsync(filters, cancellationToken);

            if (!selectedPlayerId.HasValue)
            {
                return new DashboardSnapshot
                {
                    Overview = new OverviewMetrics(
                        overview.PlayerCount,
                        overview.TeamCount,
                        overview.MatchCount,
                        overview.GoalCount + overview.PenaltyGoalCount + overview.AssistCount + overview.InterceptionCount + overview.SaveCount + overview.TurnoverCount + overview.SanctionCount,
                        overview.GoalCount + overview.PenaltyGoalCount,
                        overview.AssistCount,
                        overview.InterceptionCount,
                        overview.SaveCount,
                        overview.TurnoverCount,
                        overview.SanctionCount),
                    Players = players,
                    GlobalBoards = await globalBoardsTask,
                    TeamOfTheDay = await teamOfTheDayTask,
                    TopScorers = MapRanking(topScorersTask.Result),
                    EfficiencyRanking = MapRanking(efficiencyTask.Result),
                    RequestedRanking = MapRanking(requestedTask.Result),
                    InterceptionRanking = MapRanking(interceptionsTask.Result),
                    RequestedRankingLabel = GetRankingLabel(rankingMetric),
                    RecentMatches = MapMatches(recentMatchesTask.Result),
                    Spotlight = CreateEmptySpotlight(),
                    DataOrigin = "Donnees synchronisees",
                    WarningMessage = "Aucune donnee statistique ne correspond aux filtres actuels.",
                    IsDemo = false
                };
            }

            var selectedPlayerQuery = queryOptions;

            var profileTask = _playersApiClient.GetPlayerProfileAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var globalTask = _statsApiClient.GetPlayerGlobalAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var offenseTask = _statsApiClient.GetPlayerOffenseAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var technicalTask = _statsApiClient.GetPlayerTechnicalAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var defenseTask = _statsApiClient.GetPlayerDefenseAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var passingTask = _statsApiClient.GetPlayerPassingAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var sanctionsTask = _statsApiClient.GetPlayerSanctionsAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var goalkeeperTask = _statsApiClient.GetPlayerGoalkeeperAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var spatialTask = _statsApiClient.GetPlayerSpatialAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);
            var playerMatchesTask = _playersApiClient.GetPlayerMatchesAsync(selectedPlayerId.Value, selectedPlayerQuery, cancellationToken);

            await Task.WhenAll(
                profileTask,
                globalTask,
                offenseTask,
                technicalTask,
                defenseTask,
                passingTask,
                sanctionsTask,
                goalkeeperTask,
                spatialTask,
                playerMatchesTask,
                globalBoardsTask,
                teamOfTheDayTask);

            var selectedDirectory = playerStats.FirstOrDefault(player => player.PlayerId == selectedPlayerId.Value);
            var profile = profileTask.Result ?? CreateProfileFallback(selectedDirectory, selectedPlayerId.Value);
            var global = globalTask.Result ?? CreateGlobalFallback(profile);
            var offense = offenseTask.Result ?? CreateOffenseFallback(profile, global);
            var defense = defenseTask.Result ?? CreateDefenseFallback(profile);
            var passing = passingTask.Result ?? CreatePassingFallback(profile);
            var sanctions = sanctionsTask.Result ?? CreateSanctionsFallback(profile);
            var goalkeeper = goalkeeperTask.Result ?? CreateGoalkeeperFallback(profile);
            var technical = technicalTask.Result ?? CreateTechnicalFallback(profile, global, offense, defense, passing, sanctions, goalkeeper);
            var spatial = spatialTask.Result;
            var playerMatches = playerMatchesTask.Result;

            return new DashboardSnapshot
            {
                Overview = new OverviewMetrics(
                    overview.PlayerCount,
                    overview.TeamCount,
                    overview.MatchCount,
                    overview.GoalCount + overview.PenaltyGoalCount + overview.AssistCount + overview.InterceptionCount + overview.SaveCount + overview.TurnoverCount + overview.SanctionCount,
                    overview.GoalCount + overview.PenaltyGoalCount,
                    overview.AssistCount,
                    overview.InterceptionCount,
                    overview.SaveCount,
                    overview.TurnoverCount,
                    overview.SanctionCount),
                Players = players,
                GlobalBoards = globalBoardsTask.Result,
                TeamOfTheDay = teamOfTheDayTask.Result,
                TopScorers = MapRanking(topScorersTask.Result),
                EfficiencyRanking = MapRanking(efficiencyTask.Result),
                RequestedRanking = MapRanking(requestedTask.Result),
                InterceptionRanking = MapRanking(interceptionsTask.Result),
                RequestedRankingLabel = GetRankingLabel(rankingMetric),
                RecentMatches = MapMatches(recentMatchesTask.Result),
                Spotlight = new PlayerSpotlight
                {
                    Profile = profile,
                    Global = global,
                    Offense = offense,
                    Defense = defense,
                    Passing = passing,
                    Sanctions = sanctions,
                    Goalkeeper = goalkeeper,
                    Technical = technical,
                    Matches = playerMatches,
                    GoalZones = MapGoalZones(spatial?.Zones, spatial?.EventsByZone),
                    TriggerZones = MapTriggerZones(spatial?.Triggers),
                    Distribution = BuildDistribution(profile, global, offense, passing, goalkeeper)
                },
                DataOrigin = "Donnees synchronisees",
                WarningMessage = null,
                IsDemo = false
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Impossible de charger les dernieres donnees. {ex.Message}", ex);
        }
    }

    public async Task<(IReadOnlyList<PlayerRankingItem> Ranking, string Label)> LoadRequestedRankingAsync(
        DashboardFilterState filters,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.Session.IsAuthenticated)
        {
            throw new InvalidOperationException("Connexion requise pour ouvrir l'interface live.");
        }

        var queryOptions = filters.ToStatsQueryOptions();
        var rankingMetric = NormalizeRankingMetric(filters.RankingMetric);
        var rankingTop = Math.Clamp(filters.Top, 3, 12);

        var requestedTask = _statsApiClient.GetRankingsAsync(rankingMetric, queryOptions, rankingTop, cancellationToken);
        var requestedRanking = await requestedTask;

        return (MapRanking(requestedRanking), GetRankingLabel(rankingMetric));
    }

    private async Task<TeamOfTheDaySnapshotDto> LoadTeamOfTheDaySafeAsync(
        DashboardFilterState filters,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _teamOfTheDayService.LoadAsync(filters, cancellationToken);
        }
        catch (Exception ex)
        {
            return TeamOfTheDaySnapshotDto.Empty($"Equipe type indisponible : {ex.Message}");
        }
    }

    private async Task<DashboardGlobalBoards> LoadGlobalBoardsAsync(
        StatsQueryOptionsDto queryOptions,
        IReadOnlyList<PlayerGlobalStatsDto> playerStats,
        CancellationToken cancellationToken)
    {
        if (playerStats.Count == 0)
        {
            return DashboardGlobalBoards.Empty;
        }

        var response = await _statsApiClient.ComparePlayersAsync(new ComparePlayersRequestDto
        {
            PlayerIds = playerStats
                .Select(player => player.PlayerId)
                .Distinct()
                .ToList(),
            CompetitionId = queryOptions.CompetitionId,
            TeamId = queryOptions.TeamId,
            PositionId = queryOptions.PositionId,
            MatchId = queryOptions.MatchId,
            From = queryOptions.From,
            To = queryOptions.To,
            Year = queryOptions.Year,
            Season = queryOptions.Season,
            Day = queryOptions.Day
        }, cancellationToken) ?? new ComparePlayersResponseDto();

        return BuildGlobalBoards(response);
    }

    private static DashboardGlobalBoards BuildGlobalBoards(ComparePlayersResponseDto response)
    {
        var offenseByPlayer = response.Offense.ToDictionary(item => item.PlayerId);
        var defenseByPlayer = response.Defense.ToDictionary(item => item.PlayerId);
        var passingByPlayer = response.Passing.ToDictionary(item => item.PlayerId);
        var sanctionsByPlayer = response.Sanctions.ToDictionary(item => item.PlayerId);
        var goalkeeperByPlayer = response.Goalkeeper.ToDictionary(item => item.PlayerId);
        var technicalByPlayer = response.Technical.ToDictionary(item => item.PlayerId);

        var fieldPlayers = response.Players
            .Where(player => !player.IsGoalkeeper)
            .Select(player =>
            {
                offenseByPlayer.TryGetValue(player.PlayerId, out var offense);
                defenseByPlayer.TryGetValue(player.PlayerId, out var defense);
                passingByPlayer.TryGetValue(player.PlayerId, out var passing);
                sanctionsByPlayer.TryGetValue(player.PlayerId, out var sanctions);
                technicalByPlayer.TryGetValue(player.PlayerId, out var technical);

                return new GlobalFieldRankingRow(
                    player.PlayerId,
                    player.FullName,
                    Clean(player.TeamName, "Equipe non renseignee"),
                    player.PositionId,
                    Clean(player.PositionCode ?? player.PositionName, "Poste non renseigne"),
                    player.IsGoalkeeper,
                    player.MatchesPlayed,
                    offense?.TotalButs ?? player.TotalGoals,
                    offense?.Buts7m ?? player.PenaltyGoalCount,
                    passing?.PasseDecisive ?? player.AssistCount,
                    defense?.Interceptions ?? player.InterceptionCount,
                    defense?.Contres ?? 0,
                    defense?.Neutralisations ?? 0,
                    passing?.TotalPertes ?? player.TurnoverCount,
                    sanctions?.PenaltyConcede ?? 0,
                    ResolveOpenShotAttempts(player, offense),
                    ResolveShotAttempts(player, technical, offense),
                    ResolvePenaltyAttempts(player, technical, offense),
                    offense?.TauxReussiteTir ?? player.ShotSuccessRate);
            })
            .OrderByDescending(player => player.Goals)
            .ThenBy(player => player.FullName)
            .ToList();

        var goalkeepers = response.Players
            .Where(player => player.IsGoalkeeper)
            .Select(player =>
            {
                goalkeeperByPlayer.TryGetValue(player.PlayerId, out var goalkeeper);
                technicalByPlayer.TryGetValue(player.PlayerId, out var technical);

                return new GlobalGoalkeeperRankingRow(
                    player.PlayerId,
                    player.FullName,
                    Clean(player.TeamName, "Equipe non renseignee"),
                    player.PositionId,
                    Clean(player.PositionCode ?? player.PositionName, "GB"),
                    player.MatchesPlayed,
                    goalkeeper?.Buts ?? player.TotalGoals,
                    goalkeeper?.PasseDecisives ?? player.AssistCount,
                    (goalkeeper?.Arrets ?? 0) + (goalkeeper?.ArretsPenalty ?? 0),
                    goalkeeper?.ArretsPenalty ?? 0,
                    goalkeeper?.TauxArret ?? player.GoalkeeperSaveRate,
                    (goalkeeper?.ButsPris ?? 0) + (goalkeeper?.ButsPenalty ?? 0),
                    goalkeeper?.ButsPenalty ?? 0,
                    goalkeeper?.TirsSubis ?? player.ShotsFaced,
                    player.OpenShotAttempts,
                    ResolveShotAttempts(player, technical, null),
                    technical?.PenaltyAttempts ?? player.PenaltyAttempts,
                    goalkeeper?.TauxReussiteTir ?? player.ShotSuccessRate,
                    (goalkeeper?.PerteDeBalle ?? 0) + (goalkeeper?.MauvaisePasse ?? 0));
            })
            .OrderByDescending(player => player.Saves)
            .ThenBy(player => player.FullName)
            .ToList();

        return new DashboardGlobalBoards(fieldPlayers, goalkeepers);
    }

    private static int? ResolveSelectedPlayerId(
        int? explicitPlayerId,
        IReadOnlyList<PlayerGlobalStatsDto> players,
        IReadOnlyList<RankingItemDto> topScorers,
        IReadOnlyList<RankingItemDto> requestedRanking)
    {
        if (explicitPlayerId.HasValue && players.Any(player => player.PlayerId == explicitPlayerId.Value))
        {
            return explicitPlayerId.Value;
        }

        var bestRanked = topScorers
            .Concat(requestedRanking)
            .Select(player => player.PlayerId)
            .FirstOrDefault(playerId => playerId > 0);

        if (bestRanked > 0)
        {
            return bestRanked;
        }

        return players.FirstOrDefault()?.PlayerId;
    }

    private static IReadOnlyList<PlayerRankingItem> MapRanking(IReadOnlyList<RankingItemDto> ranking)
    {
        return ranking
            .Select(item => new PlayerRankingItem(
                item.FullName,
                item.TeamName ?? "Equipe non renseignee",
                item.Value,
                FormatRankingValue(item),
                item.PlayerId,
                item.Metric,
                item.SecondaryValue,
                FormatRankingSample(item)))
            .ToList();
    }

    private static int ResolveOpenShotAttempts(PlayerGlobalStatsDto player, PlayerOffenseStatsDto? offense)
    {
        if (player.OpenShotAttempts > 0)
        {
            return player.OpenShotAttempts;
        }

        return offense is null ? 0 : offense.Buts + offense.TirsRates;
    }

    private static int ResolveShotAttempts(
        PlayerGlobalStatsDto player,
        PlayerTechnicalStatsDto? technical,
        PlayerOffenseStatsDto? offense)
    {
        if (technical?.ShotAttempts > 0)
        {
            return technical.ShotAttempts;
        }

        if (player.ShotAttempts > 0)
        {
            return player.ShotAttempts;
        }

        return offense is null ? 0 : offense.TotalButs + offense.TirsRates + offense.PenaltyRate + offense.TirContre;
    }

    private static int ResolvePenaltyAttempts(
        PlayerGlobalStatsDto player,
        PlayerTechnicalStatsDto? technical,
        PlayerOffenseStatsDto? offense)
    {
        if (technical?.PenaltyAttempts > 0)
        {
            return technical.PenaltyAttempts;
        }

        if (player.PenaltyAttempts > 0)
        {
            return player.PenaltyAttempts;
        }

        return offense is null ? 0 : offense.Buts7m + offense.PenaltyRate;
    }

    private static IReadOnlyList<MatchRecap> MapMatches(IReadOnlyList<MatchListItemDto> matches)
    {
        return matches
            .OrderByDescending(match => match.Date)
            .Select(match => new MatchRecap(
                match.MatchId,
                $"{Clean(match.Team1Name, "Equipe A")} vs {Clean(match.Team2Name, "Equipe B")}",
                $"{match.Team1Score ?? 0} - {match.Team2Score ?? 0}",
                Clean(match.Day, "Jour non renseigne"),
                MatchFilterCatalog.FormatSeasonDay(match)))
            .ToList();
    }

    private static IReadOnlyList<ZoneStat> MapGoalZones(IReadOnlyList<ZoneStatDto>? stats, IReadOnlyList<ZoneStatDto>? eventsByZone)
    {
        var outcomesByZone = (eventsByZone ?? [])
            .ToDictionary(zone => zone.ZoneCode, zone => zone.Outcomes, StringComparer.OrdinalIgnoreCase);

        return (stats ?? [])
            .Select(zone => new ZoneStat(
                zone.ZoneCode,
                zone.ZoneCode,
                zone.SuccessRate,
                zone.Attempts,
                zone.SuccessCount,
                outcomesByZone.TryGetValue(zone.ZoneCode, out var outcomes)
                    ? outcomes.Select(outcome => new OutcomeCount(outcome.EventName, outcome.Count)).ToList()
                    : zone.Outcomes.Select(outcome => new OutcomeCount(outcome.EventName, outcome.Count)).ToList()))
            .ToList();
    }

    private static IReadOnlyList<ZoneStat> MapTriggerZones(IReadOnlyList<TriggerZoneStatDto>? stats)
    {
        return (stats ?? [])
            .Select(zone => new ZoneStat(
                SpatialZoneVisuals.ToVisualTriggerKey(zone.TriggerCode),
                SpatialZoneVisuals.ToVisualTriggerKey(zone.TriggerCode),
                zone.SuccessRate,
                zone.Attempts,
                zone.SuccessCount,
                zone.Outcomes.Select(outcome => new OutcomeCount(outcome.EventName, outcome.Count)).ToList()))
            .ToList();
    }

    private static IReadOnlyList<SliceValue> BuildDistribution(
        PlayerProfileDto profile,
        PlayerGlobalStatsDto global,
        PlayerOffenseStatsDto offense,
        PlayerPassingStatsDto passing,
        PlayerGoalkeeperStatsDto goalkeeper)
    {
        if (profile.IsGoalkeeper)
        {
            return
            [
                new SliceValue("Arrets", goalkeeper.Arrets + goalkeeper.ArretsPenalty),
                new SliceValue("Buts encaisses", goalkeeper.ButsPris + goalkeeper.ButsPenalty),
                new SliceValue("Passes decisives", passing.PasseDecisive),
                new SliceValue("Pertes", passing.TotalPertes)
            ];
        }

        return
        [
            new SliceValue("Buts", global.TotalGoals),
            new SliceValue("Tirs rates", offense.TirsRates + offense.PenaltyRate),
            new SliceValue("Passes decisives", passing.PasseDecisive),
            new SliceValue("Pertes", passing.TotalPertes)
        ];
    }

    private static PlayerProfileDto CreateProfileFallback(PlayerGlobalStatsDto? player, int playerId)
    {
        return new PlayerProfileDto
        {
            PlayerId = player?.PlayerId ?? playerId,
            FullName = player?.FullName ?? $"Joueuse {playerId}",
            TeamId = player?.TeamId,
            TeamCode = player?.TeamCode,
            TeamName = player?.TeamName,
            PositionId = player?.PositionId,
            PositionCode = player?.PositionCode,
            PositionName = player?.PositionName,
            Nationality = player?.Nationality,
            Age = player?.Age,
            Number = player?.Number,
            Birthday = player?.Birthday,
            IsGoalkeeper = player?.IsGoalkeeper ?? false,
            MatchesPlayed = player?.MatchesPlayed ?? 0,
            TeamHistory = player?.TeamHistory is null ? [] : [..player.TeamHistory],
            TotalGoals = player?.TotalGoals ?? 0,
            TotalAssists = player?.AssistCount ?? 0,
            TotalInterceptions = player?.InterceptionCount ?? 0,
            TotalSaves = player?.SaveCount ?? 0,
            TotalTurnovers = player?.TurnoverCount ?? 0,
            ShotSuccessRate = player?.ShotSuccessRate ?? 0,
            PenaltySuccessRate = player?.PenaltySuccessRate ?? 0
        };
    }

    private static PlayerGlobalStatsDto CreateGlobalFallback(PlayerProfileDto profile)
    {
        return new PlayerGlobalStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            MatchesPlayed = profile.MatchesPlayed,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory],
            TotalGoals = profile.TotalGoals,
            AssistCount = profile.TotalAssists,
            InterceptionCount = profile.TotalInterceptions,
            SaveCount = profile.TotalSaves,
            TurnoverCount = profile.TotalTurnovers,
            ShotSuccessRate = profile.ShotSuccessRate,
            PenaltySuccessRate = profile.PenaltySuccessRate
        };
    }

    private static PlayerOffenseStatsDto CreateOffenseFallback(PlayerProfileDto profile, PlayerGlobalStatsDto global)
    {
        return new PlayerOffenseStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            MatchesPlayed = global.MatchesPlayed,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory],
            Buts = Math.Max(global.TotalGoals - global.PenaltyGoalCount, 0),
            Buts7m = global.PenaltyGoalCount,
            TotalButs = global.TotalGoals,
            TirsRates = 0,
            PenaltyRate = 0,
            TirContre = 0,
            TauxReussiteTir = global.ShotSuccessRate,
            TauxReussitePenalty = global.PenaltySuccessRate
        };
    }

    private static PlayerTechnicalStatsDto CreateTechnicalFallback(
        PlayerProfileDto profile,
        PlayerGlobalStatsDto global,
        PlayerOffenseStatsDto offense,
        PlayerDefenseStatsDto? defense,
        PlayerPassingStatsDto? passing,
        PlayerSanctionStatsDto? sanctions,
        PlayerGoalkeeperStatsDto? goalkeeper)
    {
        var shotAttempts = offense.TotalButs + offense.TirsRates + offense.PenaltyRate + offense.TirContre;
        var technicalLosses = passing is null
            ? 0
            : passing.MauvaisePasse + passing.PerteDeBalle + passing.FauteTechnique + passing.PassageEnForce;
        var defensiveImpact = defense is null
            ? 0
            : defense.Interceptions + defense.Contres + defense.Neutralisations + defense.PassageForce;
        var goalkeeperStops = goalkeeper is null ? 0 : goalkeeper.Arrets + goalkeeper.ArretsPenalty;
        var goalkeeperPenaltyStops = goalkeeper is null ? 0 : goalkeeper.ArretsPenalty;
        var goalkeeperConcededGoals = goalkeeper is null ? 0 : goalkeeper.ButsPris;
        var goalkeeperPenaltyConcededGoals = goalkeeper is null ? 0 : goalkeeper.ButsPenalty;
        var sanctionsCount = sanctions is null ? 0 : HandballKpiHelper.TotalSanctions(sanctions);

        return new PlayerTechnicalStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            MatchesPlayed = global.MatchesPlayed,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory],
            Technical = new TechnicalStatsDto
            {
                ShotAttempts = shotAttempts,
                ShotWaste = offense.TirsRates + offense.PenaltyRate + offense.TirContre,
                PenaltyAttempts = offense.Buts7m + offense.PenaltyRate,
                TechnicalLosses = technicalLosses,
                DefensiveImpact = defensiveImpact,
                GoalkeeperStops = goalkeeperStops,
                GoalkeeperPenaltyStops = goalkeeperPenaltyStops,
                GoalkeeperConcededGoals = goalkeeperConcededGoals,
                GoalkeeperPenaltyConcededGoals = goalkeeperPenaltyConcededGoals,
                TirsSubis = goalkeeperStops + goalkeeperConcededGoals + goalkeeperPenaltyConcededGoals,
                Sanctions = sanctionsCount,
                OpenShotSuccessRate = offense.TauxReussiteTir,
                OverallShotSuccessRate = HandballKpiHelper.Share(offense.TotalButs, Math.Max(shotAttempts, 1)),
                PenaltySuccessRate = offense.TauxReussitePenalty,
                GoalkeeperSaveRate = goalkeeper?.TauxArret ?? 0,
                GoalkeeperPenaltyStopRate = HandballKpiHelper.Share(goalkeeperPenaltyStops, Math.Max(goalkeeperPenaltyStops + goalkeeperPenaltyConcededGoals, 1))
            }
        };
    }

    private static PlayerDefenseStatsDto CreateDefenseFallback(PlayerProfileDto profile)
    {
        return new PlayerDefenseStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory]
        };
    }

    private static PlayerPassingStatsDto CreatePassingFallback(PlayerProfileDto profile)
    {
        return new PlayerPassingStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory]
        };
    }

    private static PlayerSanctionStatsDto CreateSanctionsFallback(PlayerProfileDto profile)
    {
        return new PlayerSanctionStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory]
        };
    }

    private static PlayerGoalkeeperStatsDto CreateGoalkeeperFallback(PlayerProfileDto profile)
    {
        return new PlayerGoalkeeperStatsDto
        {
            PlayerId = profile.PlayerId,
            FullName = profile.FullName,
            TeamId = profile.TeamId,
            TeamCode = profile.TeamCode,
            TeamName = profile.TeamName,
            PositionId = profile.PositionId,
            PositionCode = profile.PositionCode,
            PositionName = profile.PositionName,
            Nationality = profile.Nationality,
            Age = profile.Age,
            Number = profile.Number,
            Birthday = profile.Birthday,
            IsGoalkeeper = profile.IsGoalkeeper,
            TeamHistory = profile.TeamHistory is null ? [] : [..profile.TeamHistory]
        };
    }

    private static string GetRankingLabel(string metric)
    {
        return RankingMetricCatalog.Default.FirstOrDefault(item => item.Value == metric)?.Label ?? "Classement";
    }

    private static string NormalizeRankingMetric(string? metric)
    {
        return RankingMetricCatalog.Default.Any(item => item.Value == metric)
            ? metric!
            : "goals";
    }

    private static string FormatRankingValue(RankingItemDto item)
    {
        return item.Metric switch
        {
            "shotsuccess" or "penaltysuccess" or "saverate" => $"{item.Value:0.#} %",
            "interceptions" => $"{item.Value:0.#} interceptions",
            "assists" => $"{item.Value:0.#} passes",
            "saves" => $"{item.Value:0.#} arrets",
            "turnovers" => $"{item.Value:0.#} pertes",
            "sanctions" => $"{item.Value:0.#} sanctions",
            _ => $"{item.Value:0.#} buts"
        };
    }

    private static string? FormatRankingSample(RankingItemDto item)
    {
        if (!item.SecondaryValue.HasValue)
        {
            return null;
        }

        var sample = (int)Math.Round(item.SecondaryValue.Value, MidpointRounding.AwayFromZero);
        return item.Metric switch
        {
            "shotsuccess" => $"{sample} tirs ouverts",
            "penaltysuccess" => $"{sample} jets 7m",
            "saverate" => $"{sample} tirs subis",
            _ => null
        };
    }

    private static string Clean(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static PlayerSpotlight CreateEmptySpotlight()
    {
        var profile = new PlayerProfileDto
        {
            PlayerId = 0,
            FullName = "Aucune joueuse disponible",
            TeamName = "Selection vide",
            PositionName = "Aucun poste",
            MatchesPlayed = 0
        };

        var global = CreateGlobalFallback(profile);
        var offense = CreateOffenseFallback(profile, global);
        var defense = CreateDefenseFallback(profile);
        var passing = CreatePassingFallback(profile);
        var sanctions = CreateSanctionsFallback(profile);
        var goalkeeper = CreateGoalkeeperFallback(profile);
        var technical = CreateTechnicalFallback(profile, global, offense, defense, passing, sanctions, goalkeeper);

        return new PlayerSpotlight
        {
            Profile = profile,
            Global = global,
            Offense = offense,
            Defense = defense,
            Passing = passing,
            Sanctions = sanctions,
            Goalkeeper = goalkeeper,
            Technical = technical,
            Matches = [],
            GoalZones = [],
            TriggerZones = [],
            Distribution = []
        };
    }
}
