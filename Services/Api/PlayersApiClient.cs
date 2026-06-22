using HandWStat.Configuration;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Api;

public sealed class PlayersApiClient : ApiClientBase
{
    public PlayersApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<PlayerListItemDto>> GetPlayersAsync(
        int? teamId = null,
        int? positionId = null,
        int? competitionId = null,
        int? year = null,
        string? season = null,
        string? day = null,
        string? search = null,
        int page = 1,
        int pageSize = 250,
        CancellationToken cancellationToken = default)
    {
        var query = new ApiQueryBuilder()
            .Add("teamId", teamId)
            .Add("positionId", positionId)
            .Add("competitionId", competitionId)
            .Add("year", year)
            .Add("season", season)
            .Add("day", day)
            .Add("search", search)
            .Add("page", page)
            .Add("pageSize", pageSize);

        return GetListAsync<PlayerListItemDto>("api/Players", query, cancellationToken);
    }

    public Task<PlayerListItemDto?> GetPlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerListItemDto>($"api/Players/{playerId}", cancellationToken: cancellationToken);
    }

    public Task<PlayerProfileDto?> GetPlayerProfileAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PlayerProfileDto>($"api/Players/{playerId}/profile", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<IReadOnlyList<PlayerMatchItemDto>> GetPlayerMatchesAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetListAsync<PlayerMatchItemDto>($"api/Players/{playerId}/matches", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PositionProfileResponseDto?> GetPlayerPositionProfileAsync(int playerId, StatsQueryOptionsDto? options = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<PositionProfileResponseDto>($"api/Players/{playerId}/position-profile", ApiQueryBuilder.FromStatsOptions(options), cancellationToken);
    }

    public Task<PositionProfileResponseDto?> ComparePositionProfilesAsync(PositionProfileCompareRequestDto request, CancellationToken cancellationToken = default)
    {
        return PostAsync<PositionProfileCompareRequestDto, PositionProfileResponseDto>("api/Players/position-profile/compare", request, cancellationToken);
    }
}
