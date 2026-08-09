using System.Net;
using System.Text;
using System.Text.Json;
using HandWStat.Configuration;
using HandWStat.Models.Contracts;
using HandWStat.Services;
using HandWStat.Services.Api;

namespace HandWStat.Tests;

internal static class ApiTestHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    public static HttpResponseMessage JsonOk<T>(T value, string? etag = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Serialize(value), Encoding.UTF8, "application/json")
        };
        if (etag is not null)
        {
            response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue($"\"{etag}\"");
        }
        return response;
    }

    public static HttpResponseMessage JsonOkList<T>(IEnumerable<T> values, string? etag = null) =>
        JsonOk(values.ToList(), etag);

    public static HttpResponseMessage NotModified() =>
        new(HttpStatusCode.NotModified);

    public static HttpResponseMessage StatusOnly(HttpStatusCode code, string? body = null, string? correlationId = null, int? retryAfterSeconds = null)
    {
        var response = new HttpResponseMessage(code)
        {
            Content = new StringContent(body ?? string.Empty, Encoding.UTF8, "application/json")
        };
        if (correlationId is not null)
        {
            response.Headers.Add("X-Correlation-ID", correlationId);
        }
        if (retryAfterSeconds.HasValue)
        {
            response.Headers.Add("Retry-After", retryAfterSeconds.Value.ToString());
        }
        return response;
    }

    public static HttpResponseMessage TooManyRequests(int retryAfterSeconds = 30, string? correlationId = null)
    {
        var body = Serialize(new { code = "RATE_LIMIT_EXCEEDED", correlationId });
        return StatusOnly(HttpStatusCode.TooManyRequests, body, correlationId, retryAfterSeconds);
    }

    public static HttpResponseMessage ServiceUnavailable(string? correlationId = null)
    {
        var body = Serialize(new { code = "SERVICE_UNAVAILABLE", correlationId });
        return StatusOnly(HttpStatusCode.ServiceUnavailable, body, correlationId);
    }

    public static HttpResponseMessage NotFound(string? correlationId = null)
    {
        var body = Serialize(new { code = "NOT_FOUND", correlationId });
        return StatusOnly(HttpStatusCode.NotFound, body, correlationId);
    }

    public static HttpResponseMessage InvalidJson() =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent("{{not valid json", Encoding.UTF8, "application/json")
        };

    public static ComparePlayersResponseDto MakeCompareResponse(params int[] playerIds)
    {
        return new ComparePlayersResponseDto
        {
            Players = playerIds.Select(id => new PlayerGlobalStatsDto
            {
                PlayerId = id,
                FullName = $"Player {id}",
                TeamName = "Team A",
                GoalCount = id,
                TotalGoals = id,
                MatchesPlayed = 5
            }).ToList(),
            Offense = playerIds.Select(id => new PlayerOffenseStatsDto { PlayerId = id }).ToList(),
            Defense = playerIds.Select(id => new PlayerDefenseStatsDto { PlayerId = id }).ToList(),
            Passing = playerIds.Select(id => new PlayerPassingStatsDto { PlayerId = id }).ToList(),
            Technical = playerIds.Select(id => new PlayerTechnicalStatsDto { PlayerId = id }).ToList(),
            Sanctions = playerIds.Select(id => new PlayerSanctionStatsDto { PlayerId = id }).ToList(),
            Goalkeeper = playerIds.Select(id => new PlayerGoalkeeperStatsDto { PlayerId = id }).ToList()
        };
    }

    public static TeamStatsDto MakeTeamStats(int teamId = 1) => new()
    {
        TeamId = teamId,
        TeamName = $"Team {teamId}",
        MatchesPlayed = 10,
        GoalsFor = 25,
        GoalsAgainst = 18,
        Overview = new StatsOverviewDto { MatchCount = 10, GoalCount = 25, TeamCount = 1, PlayerCount = 14 },
        Technical = new TechnicalStatsDto()
    };

    public static MatchSummaryDto MakeMatchSummary(int matchId = 1) => new()
    {
        MatchId = matchId,
        Team1Id = 1,
        Team2Id = 2,
        Team1Name = "Home Team",
        Team2Name = "Away Team",
        Team1Score = 28,
        Team2Score = 25,
        Season = "2025-2026",
        Day = "J12"
    };

    public static MatchListItemDto MakeMatch(int matchId = 1) => new()
    {
        MatchId = matchId,
        Team1Name = "Home",
        Team2Name = "Away",
        Team1Score = 28,
        Team2Score = 25,
        Season = "2025-2026",
        Day = "J12"
    };

    public static PositionProfileResponseDto MakePositionProfile(int playerId = 42) => new()
    {
        PositionId = 3,
        PositionCode = "ARG",
        PositionName = "Arrière gauche",
        IsGoalkeeperProfile = false,
        CohortPlayerCount = 24,
        SelectedPlayer = new PositionProfilePlayerDto
        {
            PlayerId = playerId,
            FullName = $"Player {playerId}",
            MatchesPlayed = 8,
            PlayingTimeMinutes = 420,
            Axes =
            [
                new PositionProfileAxisDto { Key = "GOALS_PER60", Label = "Buts /60", Category = "offense", Format = "decimal", HigherIsBetter = true, Value = 1.2, MedianValue = 0.9, Percentile = 72, MinValue = 0, MaxValue = 3 },
                new PositionProfileAxisDto { Key = "TURNOVERS_PER60", Label = "Pertes /60", Category = "passing", Format = "decimal", HigherIsBetter = false, Value = 0.5, MedianValue = 0.8, Percentile = 68, MinValue = 0, MaxValue = 3 }
            ]
        },
        MedianProfile = new PositionProfilePlayerDto
        {
            PlayerId = 0,
            FullName = "Médiane cohorte",
            Axes =
            [
                new PositionProfileAxisDto { Key = "GOALS_PER60", Value = 0.9, MedianValue = 0.9, Percentile = 50 },
                new PositionProfileAxisDto { Key = "TURNOVERS_PER60", Value = 0.8, MedianValue = 0.8, Percentile = 50 }
            ]
        },
        Players = []
    };
}

internal sealed class MockMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses;
    public List<HttpRequestMessage> Requests { get; } = [];

    public MockMessageHandler(Queue<HttpResponseMessage> responses)
    {
        _responses = responses;
    }

    public MockMessageHandler(HttpResponseMessage single)
    {
        _responses = new Queue<HttpResponseMessage>();
        _responses.Enqueue(single);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Requests.Add(request);
        if (_responses.Count > 0)
        {
            return Task.FromResult(_responses.Dequeue());
        }
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });
    }
}

internal sealed class StubAuthService : IApiAuthService
{
    public ApiSession Session { get; } = new ApiSession(true, "Test", "Analyst", "test-token", null);

    public event Action? SessionChanged;

    public Task<ApiSession> LoginAsync(string username, string password, CancellationToken cancellationToken = default) =>
        Task.FromResult(Session);

    public void Logout() => SessionChanged?.Invoke();

    public void ApplyAuthorization(HttpRequestMessage request)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
    }
}
