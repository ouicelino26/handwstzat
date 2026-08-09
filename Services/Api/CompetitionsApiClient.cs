using HandWStat.Configuration;
using HandWStat.Models.Contracts;

namespace HandWStat.Services.Api;

public sealed class CompetitionsApiClient : ApiClientBase
{
    public CompetitionsApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<CompetitionDto>> GetCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<CompetitionDto>("api/Competitions", cancellationToken: cancellationToken);
    }

    public Task<CompetitionDto?> GetCompetitionAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        return GetAsync<CompetitionDto>($"api/Competitions/{competitionId}", cancellationToken: cancellationToken);
    }
}
