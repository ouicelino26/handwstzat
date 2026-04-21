using HandWStat.Models.Analytics;
using HandWStat.Services.Api;

namespace HandWStat.Services;

public sealed class ReferenceDataService
{
    private readonly CompetitionsApiClient _competitionsApiClient;
    private readonly TeamsApiClient _teamsApiClient;
    private readonly LookupsApiClient _lookupsApiClient;
    private readonly IApiAuthService _authService;

    private AnalyticsReferenceData? _cache;

    public ReferenceDataService(
        CompetitionsApiClient competitionsApiClient,
        TeamsApiClient teamsApiClient,
        LookupsApiClient lookupsApiClient,
        IApiAuthService authService)
    {
        _competitionsApiClient = competitionsApiClient;
        _teamsApiClient = teamsApiClient;
        _lookupsApiClient = lookupsApiClient;
        _authService = authService;
        _authService.SessionChanged += ClearCache;
    }

    public async Task<AnalyticsReferenceData> GetReferenceDataAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!_authService.Session.IsAuthenticated)
        {
            return AnalyticsReferenceData.Empty;
        }

        if (!forceRefresh && _cache is not null)
        {
            return _cache;
        }

        var competitionsTask = _competitionsApiClient.GetCompetitionsAsync(cancellationToken);
        var teamsTask = _teamsApiClient.GetTeamsAsync(cancellationToken);
        var positionsTask = _lookupsApiClient.GetPositionsAsync(cancellationToken);
        var eventsTask = _lookupsApiClient.GetEventsAsync(cancellationToken);
        var attacksTask = _lookupsApiClient.GetAttacksAsync(cancellationToken);
        var defensesTask = _lookupsApiClient.GetDefensesAsync(cancellationToken);
        var nationalitiesTask = _lookupsApiClient.GetNationalitiesAsync(cancellationToken);

        await Task.WhenAll(
            competitionsTask,
            teamsTask,
            positionsTask,
            eventsTask,
            attacksTask,
            defensesTask,
            nationalitiesTask);

        _cache = new AnalyticsReferenceData(
            competitionsTask.Result.OrderBy(item => item.CompetitionName).ToList(),
            teamsTask.Result.OrderBy(item => item.TeamName).ToList(),
            positionsTask.Result.OrderBy(item => item.Name).ToList(),
            eventsTask.Result.OrderBy(item => item.Name).ToList(),
            attacksTask.Result.OrderBy(item => item.Name).ToList(),
            defensesTask.Result.OrderBy(item => item.Name).ToList(),
            nationalitiesTask.Result.OrderBy(item => item.Name).ToList());

        return _cache;
    }

    public void ClearCache()
    {
        _cache = null;
    }
}
