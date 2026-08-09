using System.Net;
using System.Text.Json.Serialization;
using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

public static class LeagueAnalyticsContract
{
    public const string MetricVersion = "1.0";
    public const string FailedPivotPassReason =
        "MatchEvent requires an explicit FAILED_PIVOT_PASS subtype and a typed pivot target (TargetPlayerId, TargetPositionId or PassTargetCode).";

    public static IReadOnlyList<string> AllSections { get; } =
        ["overview", "offense", "defense", "goalkeeper"];
}

public sealed record LeaguePlayerAnalyticsResponseDto
{
    [JsonPropertyName("playerId")]
    public required int PlayerId { get; init; }

    [JsonPropertyName("metricVersion")]
    public required string MetricVersion { get; init; }

    [JsonPropertyName("included")]
    public required IReadOnlyList<string> Included { get; init; }

    [JsonPropertyName("overview")]
    public LeaguePlayerOverviewDto? Overview { get; init; }

    [JsonPropertyName("offense")]
    public LeagueAttackMetricsDto? Offense { get; init; }

    [JsonPropertyName("defense")]
    public LeagueDefenseMetricsDto? Defense { get; init; }

    [JsonPropertyName("goalkeeper")]
    public LeagueGoalkeeperMetricsDto? Goalkeeper { get; init; }
}

public sealed record LeaguePlayerOverviewDto
{
    [JsonPropertyName("playerId")]
    public required int PlayerId { get; init; }

    [JsonPropertyName("fullName")]
    public required string FullName { get; init; }

    [JsonPropertyName("teamId")]
    public int? TeamId { get; init; }

    [JsonPropertyName("teamName")]
    public string? TeamName { get; init; }

    [JsonPropertyName("positionId")]
    public int? PositionId { get; init; }

    [JsonPropertyName("positionCode")]
    public string? PositionCode { get; init; }

    [JsonPropertyName("positionName")]
    public string? PositionName { get; init; }

    [JsonPropertyName("isGoalkeeper")]
    public required bool IsGoalkeeper { get; init; }

    [JsonPropertyName("matchesPlayed")]
    public required int MatchesPlayed { get; init; }
}

public sealed record LeagueAttackMetricsDto
{
    [JsonPropertyName("totalGoals")]
    public required int TotalGoals { get; init; }

    [JsonPropertyName("openPlayGoals")]
    public required int OpenPlayGoals { get; init; }

    [JsonPropertyName("penaltyGoals")]
    public required int PenaltyGoals { get; init; }

    [JsonPropertyName("assists")]
    public required int Assists { get; init; }

    [JsonPropertyName("penaltiesWon")]
    public required int PenaltiesWon { get; init; }

    [JsonPropertyName("sanctionsDrawn")]
    public required int SanctionsDrawn { get; init; }

    [JsonPropertyName("totalTurnovers")]
    public required int TotalTurnovers { get; init; }

    [JsonPropertyName("badPasses")]
    public required int BadPasses { get; init; }

    [JsonPropertyName("failedPivotPasses")]
    public required LeagueCountMetricDto FailedPivotPasses { get; init; }

    [JsonPropertyName("totalShotRate")]
    public required LeagueMetricValueDto TotalShotRate { get; init; }

    [JsonPropertyName("openPlayShotRate")]
    public required LeagueMetricValueDto OpenPlayShotRate { get; init; }

    [JsonPropertyName("penaltyShotRate")]
    public required LeagueMetricValueDto PenaltyShotRate { get; init; }
}

public sealed record LeagueDefenseMetricsDto
{
    [JsonPropertyName("interceptions")]
    public required int Interceptions { get; init; }

    [JsonPropertyName("blocks")]
    public required int Blocks { get; init; }

    [JsonPropertyName("offensiveFoulsDrawn")]
    public required int OffensiveFoulsDrawn { get; init; }

    [JsonPropertyName("neutralizations")]
    public required int Neutralizations { get; init; }

    [JsonPropertyName("penaltiesConceded")]
    public required int PenaltiesConceded { get; init; }

    [JsonPropertyName("sanctionsConceded")]
    public required int SanctionsConceded { get; init; }

    [JsonPropertyName("warningsConceded")]
    public required int WarningsConceded { get; init; }

    [JsonPropertyName("twoMinuteSuspensionsConceded")]
    public required int TwoMinuteSuspensionsConceded { get; init; }

    [JsonPropertyName("disqualificationsConceded")]
    public required int DisqualificationsConceded { get; init; }
}

public sealed record LeagueGoalkeeperMetricsDto
{
    [JsonPropertyName("totalSaves")]
    public required int TotalSaves { get; init; }

    [JsonPropertyName("openPlaySaves")]
    public required int OpenPlaySaves { get; init; }

    [JsonPropertyName("penaltySaves")]
    public required int PenaltySaves { get; init; }

    [JsonPropertyName("totalShotsFaced")]
    public required int TotalShotsFaced { get; init; }

    [JsonPropertyName("openPlayShotsFaced")]
    public required int OpenPlayShotsFaced { get; init; }

    [JsonPropertyName("penaltyShotsFaced")]
    public required int PenaltyShotsFaced { get; init; }

    [JsonPropertyName("totalSaveRate")]
    public required LeagueMetricValueDto TotalSaveRate { get; init; }

    [JsonPropertyName("openPlaySaveRate")]
    public required LeagueMetricValueDto OpenPlaySaveRate { get; init; }

    [JsonPropertyName("penaltySaveRate")]
    public required LeagueMetricValueDto PenaltySaveRate { get; init; }

    [JsonPropertyName("assists")]
    public required int Assists { get; init; }

    [JsonPropertyName("goals")]
    public required int Goals { get; init; }

    [JsonPropertyName("totalTurnovers")]
    public required int TotalTurnovers { get; init; }

    [JsonPropertyName("missedShots")]
    public required int MissedShots { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<LeagueMetricAvailability>))]
public enum LeagueMetricAvailability
{
    AVAILABLE,
    PARTIALLY_AVAILABLE,
    UNAVAILABLE_FROM_CURRENT_DATA,
    AMBIGUOUS,
    REQUIRES_ADDITIVE_SCHEMA_CHANGE,
    PARTIAL,
    DATA_MISSING
}

public sealed record LeagueCountMetricDto
{
    [JsonPropertyName("metricCode")]
    public required string MetricCode { get; init; }

    [JsonPropertyName("metricVersion")]
    public required string MetricVersion { get; init; }

    [JsonPropertyName("value")]
    public int? Value { get; init; }

    [JsonPropertyName("availability")]
    public required LeagueMetricAvailability Availability { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}

public sealed record LeagueMetricValueDto
{
    [JsonPropertyName("metricCode")]
    public required string MetricCode { get; init; }

    [JsonPropertyName("metricVersion")]
    public required string MetricVersion { get; init; }

    [JsonPropertyName("value")]
    public double? Value { get; init; }

    [JsonPropertyName("unit")]
    public required string Unit { get; init; }

    [JsonPropertyName("sample")]
    public required LeagueMetricSampleDto Sample { get; init; }

    [JsonPropertyName("quality")]
    public required LeagueMetricQualityDto Quality { get; init; }

    [JsonPropertyName("numerator")]
    public double? Numerator { get; init; }

    [JsonPropertyName("denominator")]
    public double? Denominator { get; init; }

    [JsonPropertyName("minimumSample")]
    public required double MinimumSample { get; init; }

    [JsonPropertyName("sampleReliable")]
    public required bool SampleReliable { get; init; }

    [JsonPropertyName("qualityScore")]
    public required double QualityScore { get; init; }
}

public sealed record LeagueMetricSampleDto
{
    [JsonPropertyName("numerator")]
    public double? Numerator { get; init; }

    [JsonPropertyName("denominator")]
    public double? Denominator { get; init; }

    [JsonPropertyName("minimumSample")]
    public required double MinimumSample { get; init; }
}

public sealed record LeagueMetricQualityDto
{
    [JsonPropertyName("sampleReliable")]
    public required bool SampleReliable { get; init; }

    [JsonPropertyName("qualityScore")]
    public required double QualityScore { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}

public enum LeagueGatewayOutcome
{
    Success,
    NotFound,
    Timeout,
    Unavailable,
    ServiceUnavailable,
    ServerError,
    RequestError,
    ContractError
}

public sealed record LeagueAnalyticsError(
    string UserMessage,
    string TechnicalCode,
    string? CorrelationId,
    bool Retryable,
    HttpStatusCode? StatusCode,
    int? RetryAfterSeconds = null);

public sealed record LeagueGatewayResult(
    LeagueGatewayOutcome Outcome,
    LeaguePlayerAnalyticsResponseDto? Response,
    LeagueAnalyticsError? Error)
{
    public static LeagueGatewayResult Success(LeaguePlayerAnalyticsResponseDto? response) =>
        new(LeagueGatewayOutcome.Success, response, null);

    public static LeagueGatewayResult Failure(LeagueGatewayOutcome outcome, LeagueAnalyticsError error) =>
        new(outcome, null, error);
}

public enum AnalyticsSourceStatus
{
    V2Complete,
    V1Compatible,
    V1Partial,
    Unavailable,
    ContractError
}

public static class AnalyticsSourceStatusExtensions
{
    public static string Code(this AnalyticsSourceStatus source) => source switch
    {
        AnalyticsSourceStatus.V2Complete => "V2_COMPLETE",
        AnalyticsSourceStatus.V1Compatible => "V1_COMPATIBLE",
        AnalyticsSourceStatus.V1Partial => "V1_PARTIAL",
        AnalyticsSourceStatus.ContractError => "CONTRACT_ERROR",
        _ => "UNAVAILABLE"
    };

    public static string Label(this AnalyticsSourceStatus source) => source switch
    {
        AnalyticsSourceStatus.V2Complete => "API v2 complète",
        AnalyticsSourceStatus.V1Compatible => "API v1 compatible",
        AnalyticsSourceStatus.V1Partial => "API v1 partielle",
        AnalyticsSourceStatus.ContractError => "Erreur de contrat v2",
        _ => "Donnée indisponible"
    };
}

public enum LeagueMetricDisplayKind
{
    Count,
    Rate
}

public sealed record LeagueMetricBreakdownItem(string Label, int? Value);

public sealed record LeagueMetricDisplayModel
{
    public required string MetricCode { get; init; }

    public required string Label { get; init; }

    public required LeagueMetricDisplayKind Kind { get; init; }

    public int? CountValue { get; init; }

    public RateDisplayModel? Rate { get; init; }

    public required AnalyticsSourceStatus Source { get; init; }

    public string? Availability { get; init; }

    public string? UnavailableMessage { get; init; }

    public string? HelpText { get; init; }

    public IReadOnlyList<LeagueMetricBreakdownItem> Breakdown { get; init; } = [];

    public bool IsUnavailable => Kind == LeagueMetricDisplayKind.Count && !CountValue.HasValue;
}

public sealed record LeaguePlayerAnalyticsViewModel
{
    public required int PlayerId { get; init; }

    public required bool IsGoalkeeper { get; init; }

    public required AnalyticsSourceStatus Source { get; init; }

    public string? MetricVersion { get; init; }

    public required AnalysisScopeDisplayModel Scope { get; init; }

    public required IReadOnlyList<LeagueMetricDisplayModel> Offense { get; init; }

    public required IReadOnlyList<LeagueMetricDisplayModel> Defense { get; init; }

    public required IReadOnlyList<LeagueMetricDisplayModel> Goalkeeper { get; init; }
}

public sealed record LeaguePlayerAnalyticsLoadResult
{
    public LeaguePlayerAnalyticsViewModel? Analytics { get; init; }

    public required AnalyticsSourceStatus Source { get; init; }

    public LeagueAnalyticsError? Error { get; init; }

    public bool IsSuccess => Analytics is not null;
}

public sealed record LeagueV1Snapshot(
    PlayerGlobalStatsDto Global,
    PlayerOffenseStatsDto Offense,
    PlayerDefenseStatsDto Defense,
    PlayerPassingStatsDto Passing,
    PlayerSanctionStatsDto Sanctions,
    PlayerGoalkeeperStatsDto Goalkeeper,
    PlayerTechnicalStatsDto Technical,
    bool IsGoalkeeper);
