using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Services.Analytics;

public interface ILeagueAnalyticsGateway
{
    Task<LeagueGatewayResult> GetPlayerAsync(
        int playerId,
        StatsQueryOptionsDto options,
        IReadOnlyCollection<string> include,
        CancellationToken cancellationToken);
}
