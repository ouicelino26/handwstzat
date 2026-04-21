using HandWStat.Configuration;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Api;

public sealed class LookupsApiClient : ApiClientBase
{
    public LookupsApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<IReadOnlyList<LookupItemDto>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<LookupItemDto>("api/Lookups/events", cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<LookupItemDto>("api/Lookups/positions", cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetNationalitiesAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<LookupItemDto>("api/Lookups/nationalities", cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetAttacksAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<LookupItemDto>("api/Lookups/attacks", cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetDefensesAsync(CancellationToken cancellationToken = default)
    {
        return GetListAsync<LookupItemDto>("api/Lookups/defenses", cancellationToken: cancellationToken);
    }
}
