using HandWStat.Models.Analytics;
using HandWStat.Services.Analytics;
using HandWStat.Services.Api;

namespace HandWStat.Services;

public sealed class LegendsService
{
    private readonly IAnalyticsGateway _analyticsGateway;
    private readonly IApiAuthService _authService;

    public LegendsService(IAnalyticsGateway analyticsGateway, IApiAuthService authService)
    {
        _analyticsGateway = analyticsGateway;
        _authService = authService;
    }

    public async Task<LegendsSnapshot> LoadLegendsAsync(CancellationToken cancellationToken = default)
    {
        if (!_authService.Session.IsAuthenticated)
            throw new InvalidOperationException("Connexion requise pour acceder au Hall of Legends.");

        var players = await _analyticsGateway.GetLegendsAsync(cancellationToken);
        return new LegendsSnapshot { Players = players };
    }
}
