using HandWStat.Models.Analytics;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Analytics;

public interface ILeagueAnalyticsGateway
{
    Task<LeagueGatewayResult> GetPlayerAsync(
        int playerId,
        StatsQueryOptionsDto options,
        IReadOnlyCollection<string> include,
        CancellationToken cancellationToken);
}
