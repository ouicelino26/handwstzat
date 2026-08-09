using System.Text.Json;
using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Tests;

internal static class LeagueAnalyticsTestData
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);

    public static LeaguePlayerAnalyticsResponseDto CompleteResponse(bool isGoalkeeper = true)
    {
        return new LeaguePlayerAnalyticsResponseDto
        {
            PlayerId = 42,
            MetricVersion = "1.0",
            Included = ["defense", "goalkeeper", "offense", "overview"],
            Overview = new LeaguePlayerOverviewDto
            {
                PlayerId = 42,
                FullName = "Camille Exemple",
                TeamId = 7,
                TeamName = "Handball Club Exemple",
                PositionId = isGoalkeeper ? 1 : 3,
                PositionCode = isGoalkeeper ? "GB" : "ARG",
                PositionName = isGoalkeeper ? "Gardienne" : "Arrière gauche",
                IsGoalkeeper = isGoalkeeper,
                MatchesPlayed = 8
            },
            Offense = new LeagueAttackMetricsDto
            {
                TotalGoals = 6,
                OpenPlayGoals = 5,
                PenaltyGoals = 1,
                Assists = 3,
                PenaltiesWon = 2,
                SanctionsDrawn = 1,
                TotalTurnovers = 4,
                BadPasses = 2,
                FailedPivotPasses = new LeagueCountMetricDto
                {
                    MetricCode = "FAILED_PIVOT_PASSES",
                    MetricVersion = "1.0",
                    Value = null,
                    Availability = LeagueMetricAvailability.DATA_MISSING,
                    Reason = LeagueAnalyticsContract.FailedPivotPassReason
                },
                TotalShotRate = Rate("TOTAL_SHOT_RATE", 6, 10, 4),
                OpenPlayShotRate = Rate("OPEN_PLAY_SHOT_RATE", 5, 8, 4),
                PenaltyShotRate = Rate("PENALTY_SHOT_RATE", 1, 2, 2)
            },
            Defense = new LeagueDefenseMetricsDto
            {
                Interceptions = 4,
                Blocks = 2,
                OffensiveFoulsDrawn = 1,
                Neutralizations = 3,
                PenaltiesConceded = 2,
                SanctionsConceded = 3,
                WarningsConceded = 1,
                TwoMinuteSuspensionsConceded = 1,
                DisqualificationsConceded = 1
            },
            Goalkeeper = new LeagueGoalkeeperMetricsDto
            {
                TotalSaves = 12,
                OpenPlaySaves = 10,
                PenaltySaves = 2,
                TotalShotsFaced = 24,
                OpenPlayShotsFaced = 20,
                PenaltyShotsFaced = 4,
                TotalSaveRate = Rate("TOTAL_SAVE_RATE", 12, 24, 10),
                OpenPlaySaveRate = Rate("OPEN_PLAY_SAVE_RATE", 10, 20, 10),
                PenaltySaveRate = Rate("PENALTY_SAVE_RATE", 2, 4, 2),
                Assists = 3,
                Goals = 6,
                TotalTurnovers = 4,
                MissedShots = 4
            }
        };
    }

    public static LeaguePlayerAnalyticsResponseDto ZeroDenominatorResponse()
    {
        var response = CompleteResponse();
        return response with
        {
            Offense = response.Offense! with
            {
                TotalGoals = 0,
                OpenPlayGoals = 0,
                PenaltyGoals = 0,
                TotalShotRate = Rate("TOTAL_SHOT_RATE", 0, 0, 4),
                OpenPlayShotRate = Rate("OPEN_PLAY_SHOT_RATE", 0, 0, 4),
                PenaltyShotRate = Rate("PENALTY_SHOT_RATE", 0, 0, 2)
            },
            Goalkeeper = response.Goalkeeper! with
            {
                TotalSaves = 0,
                OpenPlaySaves = 0,
                PenaltySaves = 0,
                TotalShotsFaced = 0,
                OpenPlayShotsFaced = 0,
                PenaltyShotsFaced = 0,
                TotalSaveRate = Rate("TOTAL_SAVE_RATE", 0, 0, 10),
                OpenPlaySaveRate = Rate("OPEN_PLAY_SAVE_RATE", 0, 0, 10),
                PenaltySaveRate = Rate("PENALTY_SAVE_RATE", 0, 0, 2)
            }
        };
    }

    public static LeagueMetricValueDto Rate(
        string code,
        double numerator,
        double denominator,
        double minimumSample)
    {
        var value = denominator > 0
            ? Math.Round(numerator * 100d / denominator, 2, MidpointRounding.AwayFromZero)
            : (double?)null;
        var reliable = value.HasValue && denominator >= minimumSample;
        var qualityScore = denominator > 0
            ? Math.Round(Math.Clamp(denominator / minimumSample, 0d, 1d), 2, MidpointRounding.AwayFromZero)
            : 0;
        var reason = denominator <= 0
            ? "ZERO_OR_INVALID_DENOMINATOR"
            : reliable ? null : "BELOW_MINIMUM_SAMPLE";

        return new LeagueMetricValueDto
        {
            MetricCode = code,
            MetricVersion = "1.0",
            Value = value,
            Unit = "percent",
            Sample = new LeagueMetricSampleDto
            {
                Numerator = numerator,
                Denominator = denominator,
                MinimumSample = minimumSample
            },
            Quality = new LeagueMetricQualityDto
            {
                SampleReliable = reliable,
                QualityScore = qualityScore,
                Reason = reason
            },
            Numerator = numerator,
            Denominator = denominator,
            MinimumSample = minimumSample,
            SampleReliable = reliable,
            QualityScore = qualityScore
        };
    }

    public static AnalysisScopeDisplayModel Scope() =>
        new("LBE", "Metz", "2025-2026", "J12", "Toutes les périodes", 7, null);

    public static LeagueV1Snapshot V1Snapshot(bool isGoalkeeper = true)
    {
        return new LeagueV1Snapshot(
            new PlayerGlobalStatsDto
            {
                GoalCount = 5,
                PenaltyGoalCount = 1,
                TotalGoals = 6,
                AssistCount = 3,
                SaveCount = 12,
                TurnoverCount = 10,
                OpenShotAttempts = 8,
                ShotAttempts = 10,
                PenaltyAttempts = 2,
                ShotsFaced = 24,
                MatchesPlayed = 8
            },
            new PlayerOffenseStatsDto
            {
                Buts = 5,
                Buts7m = 1,
                TotalButs = 6
            },
            new PlayerDefenseStatsDto
            {
                Interceptions = 4,
                Contres = 2,
                PassageForce = 1,
                Neutralisations = 3
            },
            new PlayerPassingStatsDto
            {
                PasseDecisive = 3,
                MauvaisePasse = 2,
                PerteDeBalle = 3,
                FauteTechnique = 3,
                PassageEnForce = 2,
                TotalPertes = 10
            },
            new PlayerSanctionStatsDto
            {
                PenaltyConcede = 2,
                Avertissements = 1,
                DeuxMinutes = 5,
                Exclusions = 1
            },
            new PlayerGoalkeeperStatsDto
            {
                Arrets = 10,
                ArretsPenalty = 2,
                ButsPris = 10,
                ButsPenalty = 2,
                TirsSubis = 24,
                TirsLoupes = 4,
                PasseDecisives = 3,
                TirsRates = 99
            },
            new PlayerTechnicalStatsDto
            {
                Technical = new TechnicalStatsDto
                {
                    ShotWaste = 4
                }
            },
            isGoalkeeper);
    }
}
