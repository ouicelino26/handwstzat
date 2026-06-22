using HandWStat.Configuration;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Api;

public sealed class StatsApiClient : ApiClientBase
{
    public StatsApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<StatsOverviewDto?> GetOverviewAsync(StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<StatsOverviewDto>("api/Stats/overview", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<RankingItemDto>> GetRankingsAsync(
        string? metric,
        StatsQueryOptionsDto? options = null,
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        var query = ApiQueryBuilder.FromStatsOptions(options)
            .Add("metric", metric)
            .Add("top", top);

        return GetListAsync<RankingItemDto>("api/Stats/rankings", query, cancellationToken);
    }

    public Task<IReadOnlyList<PlayerGlobalStatsDto>> GetPlayersAsync(StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetListAsync<PlayerGlobalStatsDto>("api/Stats/players", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerGlobalStatsDto?> GetPlayerGlobalAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerGlobalStatsDto>($"api/Stats/players/{playerId}/global", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerOffenseStatsDto?> GetPlayerOffenseAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerOffenseStatsDto>($"api/Stats/players/{playerId}/offense", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerTechnicalStatsDto?> GetPlayerTechnicalAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerTechnicalStatsDto>($"api/Stats/players/{playerId}/technical", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerDefenseStatsDto?> GetPlayerDefenseAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerDefenseStatsDto>($"api/Stats/players/{playerId}/defense", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerPassingStatsDto?> GetPlayerPassingAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerPassingStatsDto>($"api/Stats/players/{playerId}/passing", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerSanctionStatsDto?> GetPlayerSanctionsAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerSanctionStatsDto>($"api/Stats/players/{playerId}/sanctions", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerGoalkeeperStatsDto?> GetPlayerGoalkeeperAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerGoalkeeperStatsDto>($"api/Stats/players/{playerId}/goalkeeper", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PagedEventAnalyticsDto?> GetEventsAsync(StatsQueryOptionsDto? options = null, int page = 1, int pageSize = 250, CancellationToken cancellationToken = default)
    {
        var query = ApiQueryBuilder.FromStatsOptions(options)
            .Add("page", page)
            .Add("pageSize", pageSize);

        return GetAsync<PagedEventAnalyticsDto>("api/Stats/events", query, cancellationToken);
    }

    public Task<EventContextBreakdownDto?> GetEventContextsAsync(StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<EventContextBreakdownDto>("api/Stats/events/contexts", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PlayerSpatialStatsDto?> GetPlayerSpatialAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerSpatialStatsDto>($"api/Stats/players/{playerId}/spatial", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<ZoneStatDto>> GetPlayerSpatialZonesAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetListAsync<ZoneStatDto>($"api/Stats/players/{playerId}/spatial/zones", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<TriggerZoneStatDto>> GetPlayerSpatialTriggersAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetListAsync<TriggerZoneStatDto>($"api/Stats/players/{playerId}/spatial/triggers", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<ZoneStatDto>> GetPlayerEventsByZoneAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetListAsync<ZoneStatDto>($"api/Stats/players/{playerId}/spatial/events-by-zone", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<PlayerGlobalStatsDto>> GetMatchPlayersAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return GetListAsync<PlayerGlobalStatsDto>($"api/Stats/matches/{matchId}/players", cancellationToken: cancellationToken);
    }

    public Task<MatchSpatialStatsDto?> GetMatchSpatialAsync(
        int matchId,
        int? teamId = null,
        StatsQueryOptionsDto? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApiQueryBuilder.FromStatsOptions(options)
            .Add("teamId", teamId);

        return GetAsync<MatchSpatialStatsDto>($"api/Stats/matches/{matchId}/spatial", query, cancellationToken);
    }

    public Task<MatchSummaryDto?> GetAnalyticsMatchSummaryAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MatchSummaryDto>($"api/Stats/matches/{matchId}/summary", cancellationToken: cancellationToken);
    }

    public Task<ComparePlayersResponseDto?> ComparePlayersAsync(ComparePlayersRequestDto request, CancellationToken cancellationToken = default)
    {
        return PostAsync<ComparePlayersRequestDto, ComparePlayersResponseDto>("api/Stats/compare/players", request, cancellationToken);
    }

    public Task<TeamStatsDto?> GetTeamStatsAsync(int teamId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<TeamStatsDto>($"api/Stats/teams/{teamId}", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<PlayerGlobalStatsDto>> GetTeamPlayersAsync(int teamId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetListAsync<PlayerGlobalStatsDto>($"api/Stats/teams/{teamId}/players", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }
}
