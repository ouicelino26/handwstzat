using HandWStat.Configuration;
using HandballManagerCore.Models;

namespace HandWStat.Services.Api;

public sealed class MatchEventsApiClient : ApiClientBase
{
    public MatchEventsApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<MatchEvent>> GetMatchEventsAsync(int? matchId = null, CancellationToken cancellationToken = default)
    {
        var query = new ApiQueryBuilder()
            .Add("matchId", matchId);

        return GetListAsync<MatchEvent>("api/MatchEvents", query, cancellationToken);
    }
}
