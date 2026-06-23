using HandWStat.Services.Api;

namespace HandWStat.Services;

public sealed record GlobalSearchResult(
    string Kind,
    string Title,
    string Subtitle,
    string Href,
    string Tone = "neutral");

public sealed class GlobalSearchService
{
    private readonly PlayersApiClient _playersApiClient;
    private readonly ReferenceDataService _referenceDataService;
    private readonly MatchesApiClient _matchesApiClient;
    private readonly IApiAuthService _authService;

    public GlobalSearchService(
        PlayersApiClient playersApiClient,
        ReferenceDataService referenceDataService,
        MatchesApiClient matchesApiClient,
        IApiAuthService authService)
    {
        _playersApiClient = playersApiClient;
        _referenceDataService = referenceDataService;
        _matchesApiClient = matchesApiClient;
        _authService = authService;
    }

    public async Task<IReadOnlyList<GlobalSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.Session.IsAuthenticated || string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalized = query.Trim();
        var referenceTask = _referenceDataService.GetReferenceDataAsync(cancellationToken: cancellationToken);
        var playersTask = _playersApiClient.GetPlayersAsync(
            search: normalized,
            page: 1,
            pageSize: 8,
            cancellationToken: cancellationToken);
        var matchesTask = _matchesApiClient.GetMatchesAsync(
            page: 1,
            pageSize: 120,
            cancellationToken: cancellationToken);

        await Task.WhenAll(referenceTask, playersTask, matchesTask);

        var results = new List<GlobalSearchResult>();

        results.AddRange(playersTask.Result.Take(8).Select(player => new GlobalSearchResult(
            "Joueuse",
            player.FullName,
            $"{player.TeamName ?? "Equipe non renseignee"} - {player.PositionName ?? player.PositionCode ?? "Poste non renseigne"}",
            $"/players?playerId={player.PlayerId}",
            "player")));

        results.AddRange(referenceTask.Result.Teams
            .Where(team => team.TeamName?.Contains(normalized, StringComparison.OrdinalIgnoreCase) == true)
            .Take(5)
            .Select(team => new GlobalSearchResult(
                "Equipe",
                team.TeamName ?? $"Equipe {team.TeamId}",
                "Ouvrir le bilan collectif",
                $"/teams?teamId={team.TeamId}",
                "team")));

        results.AddRange(matchesTask.Result
            .Where(match =>
                Contains(match.Team1Name, normalized)
                || Contains(match.Team2Name, normalized)
                || Contains(match.CompetitionName, normalized)
                || Contains(match.Season, normalized)
                || Contains(match.Day, normalized))
            .Take(6)
            .Select(match => new GlobalSearchResult(
                "Match",
                $"{match.Team1Name ?? "Equipe A"} - {match.Team2Name ?? "Equipe B"}",
                $"{match.Team1Score ?? 0} - {match.Team2Score ?? 0} | {match.CompetitionName ?? "Competition"}",
                $"/matches?matchId={match.MatchId}",
                "match")));

        return results.Take(16).ToList();
    }

    private static bool Contains(string? value, string query)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
