using HandWStat.Configuration;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Api;

public sealed class MatchesApiClient : ApiClientBase
{
    public MatchesApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<MatchListItemDto>> GetMatchesAsync(
        int? competitionId = null,
        int? teamId = null,
        int? playerId = null,
        DateTime? from = null,
        DateTime? to = null,
        int? year = null,
        string? season = null,
        string? day = null,
        int page = 1,
        int pageSize = 250,
        CancellationToken cancellationToken = default)
    {
        var query = new ApiQueryBuilder()
            .Add("competitionId", competitionId)
            .Add("teamId", teamId)
            .Add("playerId", playerId)
            .Add("from", from)
            .Add("to", to)
            .Add("year", year)
            .Add("season", season)
            .Add("day", day)
            .Add("page", page)
            .Add("pageSize", pageSize);

        return GetListAsync<MatchListItemDto>("api/Matches", query, cancellationToken);
    }

    public Task<MatchListItemDto?> GetMatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MatchListItemDto>($"api/Matches/{matchId}", cancellationToken: cancellationToken);
    }

    public Task<MatchSummaryDto?> GetMatchSummaryAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MatchSummaryDto>($"api/Matches/{matchId}/summary", cancellationToken: cancellationToken);
    }
}
