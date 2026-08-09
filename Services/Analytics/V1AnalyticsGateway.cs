using HandWStat.Models.Contracts;
using HandWStat.Services.Api;

namespace HandWStat.Services.Analytics;

public sealed class V1AnalyticsGateway : IAnalyticsGateway
{
    private readonly StatsApiClient _client;

    public V1AnalyticsGateway(StatsApiClient client)
    {
        _client = client;
    }

    public Task<StatsOverviewDto?> GetOverviewAsync(StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetOverviewAsync(options, cancellationToken);

    public Task<IReadOnlyList<PlayerGlobalStatsDto>> GetPlayersAsync(StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayersAsync(options, cancellationToken);

    public Task<IReadOnlyList<RankingItemDto>> GetRankingsAsync(
        string metric,
        StatsQueryOptionsDto options,
        int top,
        CancellationToken cancellationToken) =>
        _client.GetRankingsAsync(metric, options, top, cancellationToken);

    public Task<PlayerGlobalStatsDto?> GetPlayerGlobalAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerGlobalAsync(playerId, options, cancellationToken);

    public Task<PlayerOffenseStatsDto?> GetPlayerOffenseAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerOffenseAsync(playerId, options, cancellationToken);

    public Task<PlayerTechnicalStatsDto?> GetPlayerTechnicalAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerTechnicalAsync(playerId, options, cancellationToken);

    public Task<PlayerDefenseStatsDto?> GetPlayerDefenseAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerDefenseAsync(playerId, options, cancellationToken);

    public Task<PlayerPassingStatsDto?> GetPlayerPassingAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerPassingAsync(playerId, options, cancellationToken);

    public Task<PlayerSanctionStatsDto?> GetPlayerSanctionsAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerSanctionsAsync(playerId, options, cancellationToken);

    public Task<PlayerGoalkeeperStatsDto?> GetPlayerGoalkeeperAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerGoalkeeperAsync(playerId, options, cancellationToken);

    public Task<PlayerSpatialStatsDto?> GetPlayerSpatialAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken) =>
        _client.GetPlayerSpatialAsync(playerId, options, cancellationToken);

    public Task<ComparePlayersResponseDto?> ComparePlayersAsync(ComparePlayersRequestDto request, CancellationToken cancellationToken) =>
        _client.ComparePlayersAsync(request, cancellationToken);
}
