using HandWStat.Configuration;
using HandWStat.Models.Contracts;

namespace HandWStat.Services.Api;

public sealed class MatchEventsApiClient : ApiClientBase
{
    public MatchEventsApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<MatchEventAnalyticsDto>> GetMatchEventsAsync(int? matchId = null, CancellationToken cancellationToken = default)
    {
        var query = new ApiQueryBuilder()
            .Add("matchId", matchId);

        return GetListAsync<MatchEventAnalyticsDto>("api/MatchEvents", query, cancellationToken);
    }
}
