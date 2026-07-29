using System.Diagnostics;
using System.Net;
using System.Text.Json;
using HandWStat.Configuration;
using HandWStat.Models.Analytics;
using HandWStat.Services.Api;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Analytics;

public sealed class V2AnalyticsGateway : ApiClientBase, ILeagueAnalyticsGateway
{
    public V2AnalyticsGateway(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public async Task<LeagueGatewayResult> GetPlayerAsync(
        int playerId,
        StatsQueryOptionsDto options,
        IReadOnlyCollection<string> include,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(include);

        var normalizedInclude = include
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        try
        {
            var response = await GetAsync<LeaguePlayerAnalyticsResponseDto>(
                $"api/v2/analytics/players/{playerId}",
                BuildQuery(options, normalizedInclude),
                cancellationToken);

            var contractError = LeagueAnalyticsContractValidator.Validate(
                response,
                playerId,
                normalizedInclude);

            if (contractError is not null)
            {
                Debug.WriteLine($"[HandWStat League v2] Contract validation failed: {contractError}");
                return LeagueGatewayResult.Failure(
                    LeagueGatewayOutcome.ContractError,
                    new LeagueAnalyticsError(
                        "La réponse statistique v2 ne respecte pas le contrat attendu.",
                        "LEAGUE_V2_CONTRACT_INVALID",
                        null,
                        Retryable: false,
                        StatusCode: HttpStatusCode.OK));
            }

            return LeagueGatewayResult.Success(response!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[HandWStat League v2] Invalid JSON contract: {ex}");
            return ContractFailure(ex.Message.Contains("required properties", StringComparison.OrdinalIgnoreCase)
                ? "LEAGUE_V2_CONTRACT_INCOMPLETE"
                : "LEAGUE_V2_JSON_INVALID");
        }
        catch (NotSupportedException ex)
        {
            Debug.WriteLine($"[HandWStat League v2] Unsupported JSON contract: {ex}");
            return ContractFailure("LEAGUE_V2_JSON_UNSUPPORTED");
        }
        catch (ApiRequestException ex)
        {
            return MapRequestFailure(ex);
        }
    }

    private static ApiQueryBuilder BuildQuery(
        StatsQueryOptionsDto options,
        IReadOnlyCollection<string> include)
    {
        return new ApiQueryBuilder()
            .Add("include", string.Join(",", include))
            .Add("competitionId", options.CompetitionId)
            .Add("teamId", options.TeamId)
            .Add("matchId", options.MatchId)
            .Add("from", options.From)
            .Add("to", options.To)
            .Add("year", options.Year)
            .Add("season", options.Season)
            .Add("day", options.Day)
            .Add("attackId", options.AttackId)
            .Add("defenseId", options.DefenseId)
            .Add("trigger", options.Trigger)
            .Add("shootShade", options.ShootShade);
    }

    private static LeagueGatewayResult MapRequestFailure(ApiRequestException error)
    {
        var outcome = error.StatusCode switch
        {
            HttpStatusCode.NotFound => LeagueGatewayOutcome.NotFound,
            HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented => LeagueGatewayOutcome.Unavailable,
            _ when string.Equals(error.TechnicalCode, "API_TIMEOUT", StringComparison.Ordinal) =>
                LeagueGatewayOutcome.Timeout,
            _ when error.StatusCode.HasValue && (int)error.StatusCode.Value >= 500 =>
                LeagueGatewayOutcome.ServerError,
            _ => LeagueGatewayOutcome.RequestError
        };

        var userMessage = outcome switch
        {
            LeagueGatewayOutcome.NotFound =>
                "Cette joueuse est introuvable dans le périmètre demandé.",
            LeagueGatewayOutcome.Unavailable =>
                "L'endpoint Ligue v2 n'est pas disponible sur ce serveur.",
            LeagueGatewayOutcome.Timeout =>
                "Le service Ligue v2 met trop de temps à répondre.",
            LeagueGatewayOutcome.ServerError =>
                "Le service Ligue v2 rencontre un problème temporaire.",
            _ => error.UserMessage
        };

        return LeagueGatewayResult.Failure(
            outcome,
            new LeagueAnalyticsError(
                userMessage,
                error.TechnicalCode,
                error.CorrelationId,
                error.Retryable,
                error.StatusCode));
    }

    private static LeagueGatewayResult ContractFailure(string code) =>
        LeagueGatewayResult.Failure(
            LeagueGatewayOutcome.ContractError,
            new LeagueAnalyticsError(
                "La réponse statistique v2 ne respecte pas le contrat attendu.",
                code,
                null,
                Retryable: false,
                StatusCode: HttpStatusCode.OK));
}
