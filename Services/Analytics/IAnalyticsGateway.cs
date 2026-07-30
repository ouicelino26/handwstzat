using HandWStat.Models.Contracts;

namespace HandWStat.Services.Analytics;

public interface IAnalyticsGateway
{
    Task<StatsOverviewDto?> GetOverviewAsync(StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<IReadOnlyList<PlayerGlobalStatsDto>> GetPlayersAsync(StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<IReadOnlyList<RankingItemDto>> GetRankingsAsync(
        string metric,
        StatsQueryOptionsDto options,
        int top,
        CancellationToken cancellationToken);

    Task<PlayerGlobalStatsDto?> GetPlayerGlobalAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerOffenseStatsDto?> GetPlayerOffenseAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerTechnicalStatsDto?> GetPlayerTechnicalAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerDefenseStatsDto?> GetPlayerDefenseAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerPassingStatsDto?> GetPlayerPassingAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerSanctionStatsDto?> GetPlayerSanctionsAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerGoalkeeperStatsDto?> GetPlayerGoalkeeperAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<PlayerSpatialStatsDto?> GetPlayerSpatialAsync(int playerId, StatsQueryOptionsDto options, CancellationToken cancellationToken);

    Task<ComparePlayersResponseDto?> ComparePlayersAsync(ComparePlayersRequestDto request, CancellationToken cancellationToken);
}
