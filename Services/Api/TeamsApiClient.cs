using HandWStat.Configuration;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Api;

public sealed class TeamsApiClient : ApiClientBase
{
    public TeamsApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<TeamDto>> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<TeamDto>("api/Teams", cancellationToken: cancellationToken);
    }

    public Task<TeamDto?> GetTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        return GetAsync<TeamDto>($"api/Teams/{teamId}", cancellationToken: cancellationToken);
    }

    public Task<TeamDto?> GetTeamByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return GetAsync<TeamDto>($"api/Teams/by-code/{Uri.EscapeDataString(code)}", cancellationToken: cancellationToken);
    }
}
