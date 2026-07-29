using HandWStat.Models.Analytics;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Analytics;

public static class LeaguePlayerAnalyticsMapper
{
    private const string MissingDataMessage = "Donnée non disponible";
    private const string MissingPivotMessage = "Donnée non disponible avec les fichiers actuels";
    private const string MissingPivotHelp =
        "La destination de la mauvaise passe n'est pas encore disponible dans les données importées.";

    public static LeaguePlayerAnalyticsViewModel FromV2(
        LeaguePlayerAnalyticsResponseDto response,
        AnalysisScopeDisplayModel scope)
    {
        var overview = response.Overview
            ?? throw new InvalidOperationException("La section overview v2 est requise pour la présentation.");
        var offense = response.Offense
            ?? throw new InvalidOperationException("La section offense v2 est requise pour la présentation.");
        var defense = response.Defense
            ?? throw new InvalidOperationException("La section defense v2 est requise pour la présentation.");
        var goalkeeper = response.Goalkeeper
            ?? throw new InvalidOperationException("La section goalkeeper v2 est requise pour la présentation.");

        var failedPivot = offense.FailedPivotPasses.Value.HasValue
            ? Count(
                offense.FailedPivotPasses.MetricCode,
                "Passes pivot ratées",
                offense.FailedPivotPasses.Value,
                AnalyticsSourceStatus.V2Complete,
                offense.FailedPivotPasses.Availability.ToString(),
                helpText: offense.FailedPivotPasses.Reason)
            : Count(
                offense.FailedPivotPasses.MetricCode,
                "Passes pivot ratées",
                null,
                AnalyticsSourceStatus.Unavailable,
                offense.FailedPivotPasses.Availability.ToString(),
                MissingPivotMessage,
                MissingPivotHelp);

        return new LeaguePlayerAnalyticsViewModel
        {
            PlayerId = response.PlayerId,
            IsGoalkeeper = overview.IsGoalkeeper,
            Source = AnalyticsSourceStatus.V2Complete,
            MetricVersion = response.MetricVersion,
            Scope = scope with { MatchCount = overview.MatchesPlayed },
            Offense =
            [
                Count("TOTAL_GOALS", "Buts total", offense.TotalGoals, AnalyticsSourceStatus.V2Complete),
                Count("OPEN_PLAY_GOALS", "Buts dans le jeu", offense.OpenPlayGoals, AnalyticsSourceStatus.V2Complete),
                Count("PENALTY_GOALS", "Buts sur 7 m", offense.PenaltyGoals, AnalyticsSourceStatus.V2Complete),
                Count("ASSISTS", "Passes décisives", offense.Assists, AnalyticsSourceStatus.V2Complete),
                Count("PENALTIES_WON", "7 m obtenus", offense.PenaltiesWon, AnalyticsSourceStatus.V2Complete),
                Count("SANCTIONS_DRAWN", "Sanctions obtenues", offense.SanctionsDrawn, AnalyticsSourceStatus.V2Complete),
                Count(
                    "TOTAL_TURNOVERS",
                    "Pertes de balle",
                    offense.TotalTurnovers,
                    AnalyticsSourceStatus.V2Complete,
                    breakdown:
                    [
                        new("Mauvaises passes", offense.BadPasses),
                        new("Passes pivot ratées", offense.FailedPivotPasses.Value)
                    ]),
                Count("BAD_PASSES", "Mauvaises passes", offense.BadPasses, AnalyticsSourceStatus.V2Complete),
                failedPivot,
                Rate("Taux de tir total", offense.TotalShotRate, "buts", "tirs"),
                Rate("Taux de tir dans le jeu", offense.OpenPlayShotRate, "buts", "tirs"),
                Rate("Taux de tir sur 7 m", offense.PenaltyShotRate, "buts", "tirs")
            ],
            Defense =
            [
                Count("INTERCEPTIONS", "Interceptions", defense.Interceptions, AnalyticsSourceStatus.V2Complete),
                Count("BLOCKS", "Contres", defense.Blocks, AnalyticsSourceStatus.V2Complete),
                Count("OFFENSIVE_FOULS_DRAWN", "Passages en force provoqués", defense.OffensiveFoulsDrawn, AnalyticsSourceStatus.V2Complete),
                Count("NEUTRALIZATIONS", "Neutralisations", defense.Neutralizations, AnalyticsSourceStatus.V2Complete),
                Count("PENALTIES_CONCEDED", "7 m concédés", defense.PenaltiesConceded, AnalyticsSourceStatus.V2Complete),
                Count(
                    "SANCTIONS_CONCEDED",
                    "Sanctions concédées",
                    defense.SanctionsConceded,
                    AnalyticsSourceStatus.V2Complete,
                    breakdown:
                    [
                        new("Avertissements", defense.WarningsConceded),
                        new("Exclusions de deux minutes", defense.TwoMinuteSuspensionsConceded),
                        new("Disqualifications", defense.DisqualificationsConceded)
                    ]),
                Count("WARNINGS_CONCEDED", "Avertissements", defense.WarningsConceded, AnalyticsSourceStatus.V2Complete),
                Count("TWO_MINUTE_SUSPENSIONS_CONCEDED", "Exclusions de deux minutes", defense.TwoMinuteSuspensionsConceded, AnalyticsSourceStatus.V2Complete),
                Count("DISQUALIFICATIONS_CONCEDED", "Disqualifications", defense.DisqualificationsConceded, AnalyticsSourceStatus.V2Complete)
            ],
            Goalkeeper =
            [
                Count("TOTAL_SAVES", "Arrêts total", goalkeeper.TotalSaves, AnalyticsSourceStatus.V2Complete),
                Count("OPEN_PLAY_SAVES", "Arrêts dans le jeu", goalkeeper.OpenPlaySaves, AnalyticsSourceStatus.V2Complete),
                Count("PENALTY_SAVES", "Arrêts sur 7 m", goalkeeper.PenaltySaves, AnalyticsSourceStatus.V2Complete),
                Count("TOTAL_SHOTS_FACED", "Tirs subis total", goalkeeper.TotalShotsFaced, AnalyticsSourceStatus.V2Complete),
                Count("OPEN_PLAY_SHOTS_FACED", "Tirs subis dans le jeu", goalkeeper.OpenPlayShotsFaced, AnalyticsSourceStatus.V2Complete),
                Count("PENALTY_SHOTS_FACED", "Tirs subis sur 7 m", goalkeeper.PenaltyShotsFaced, AnalyticsSourceStatus.V2Complete),
                Rate("Taux d'arrêt général", goalkeeper.TotalSaveRate, "arrêts", "tirs subis"),
                Rate("Taux d'arrêt dans le jeu", goalkeeper.OpenPlaySaveRate, "arrêts", "tirs subis"),
                Rate("Taux d'arrêt sur 7 m", goalkeeper.PenaltySaveRate, "arrêts", "tirs subis"),
                Count("GOALKEEPER_ASSISTS", "Passes décisives", goalkeeper.Assists, AnalyticsSourceStatus.V2Complete),
                Count("GOALKEEPER_GOALS", "Buts", goalkeeper.Goals, AnalyticsSourceStatus.V2Complete),
                Count("GOALKEEPER_TURNOVERS", "Pertes de balle", goalkeeper.TotalTurnovers, AnalyticsSourceStatus.V2Complete),
                Count("GOALKEEPER_MISSED_SHOTS", "Tirs ratés", goalkeeper.MissedShots, AnalyticsSourceStatus.V2Complete)
            ]
        };
    }

    public static LeaguePlayerAnalyticsViewModel FromV1(
        int playerId,
        LeagueV1Snapshot snapshot,
        AnalysisScopeDisplayModel scope)
    {
        var totalSaves = snapshot.Global.SaveCount;
        var totalShotsFaced = snapshot.Global.ShotsFaced;
        var openPlayShotsFaced = snapshot.Goalkeeper.Arrets + snapshot.Goalkeeper.ButsPris;
        var penaltyShotsFaced = snapshot.Goalkeeper.ArretsPenalty + snapshot.Goalkeeper.ButsPenalty;

        return new LeaguePlayerAnalyticsViewModel
        {
            PlayerId = playerId,
            IsGoalkeeper = snapshot.IsGoalkeeper,
            Source = AnalyticsSourceStatus.V1Partial,
            MetricVersion = null,
            Scope = scope,
            Offense =
            [
                Count("TOTAL_GOALS", "Buts total", snapshot.Global.TotalGoals, AnalyticsSourceStatus.V1Compatible),
                Count("OPEN_PLAY_GOALS", "Buts dans le jeu", snapshot.Global.GoalCount, AnalyticsSourceStatus.V1Compatible),
                Count("PENALTY_GOALS", "Buts sur 7 m", snapshot.Global.PenaltyGoalCount, AnalyticsSourceStatus.V1Compatible),
                Count("ASSISTS", "Passes décisives", snapshot.Global.AssistCount, AnalyticsSourceStatus.V1Compatible),
                UnavailableCount("PENALTIES_WON", "7 m obtenus"),
                UnavailableCount("SANCTIONS_DRAWN", "Sanctions obtenues"),
                Count(
                    "TOTAL_TURNOVERS",
                    "Pertes de balle",
                    snapshot.Global.TurnoverCount,
                    AnalyticsSourceStatus.V1Compatible,
                    breakdown:
                    [
                        new("Mauvaises passes", snapshot.Passing.MauvaisePasse),
                        new("Pertes simples", snapshot.Passing.PerteDeBalle),
                        new("Fautes techniques", snapshot.Passing.FauteTechnique),
                        new("Passages en force", snapshot.Passing.PassageEnForce),
                        new("Passes pivot ratées", null)
                    ]),
                Count("BAD_PASSES", "Mauvaises passes", snapshot.Passing.MauvaisePasse, AnalyticsSourceStatus.V1Compatible),
                Count(
                    "FAILED_PIVOT_PASSES",
                    "Passes pivot ratées",
                    null,
                    AnalyticsSourceStatus.Unavailable,
                    "DATA_MISSING",
                    MissingPivotMessage,
                    MissingPivotHelp),
                PartialRate(
                    "TOTAL_SHOT_RATE",
                    "Taux de tir total",
                    snapshot.Global.TotalGoals,
                    snapshot.Global.ShotAttempts,
                    "buts",
                    "tirs"),
                PartialRate(
                    "OPEN_PLAY_SHOT_RATE",
                    "Taux de tir dans le jeu",
                    snapshot.Global.GoalCount,
                    snapshot.Global.OpenShotAttempts,
                    "buts",
                    "tirs"),
                PartialRate(
                    "PENALTY_SHOT_RATE",
                    "Taux de tir sur 7 m",
                    snapshot.Global.PenaltyGoalCount,
                    snapshot.Global.PenaltyAttempts,
                    "buts",
                    "tirs")
            ],
            Defense =
            [
                Count("INTERCEPTIONS", "Interceptions", snapshot.Defense.Interceptions, AnalyticsSourceStatus.V1Compatible),
                Count("BLOCKS", "Contres", snapshot.Defense.Contres, AnalyticsSourceStatus.V1Compatible),
                Count("OFFENSIVE_FOULS_DRAWN", "Passages en force provoqués", snapshot.Defense.PassageForce, AnalyticsSourceStatus.V1Compatible),
                Count("NEUTRALIZATIONS", "Neutralisations", snapshot.Defense.Neutralisations, AnalyticsSourceStatus.V1Compatible),
                Count("PENALTIES_CONCEDED", "7 m concédés", snapshot.Sanctions.PenaltyConcede, AnalyticsSourceStatus.V1Compatible),
                Count(
                    "SANCTIONS_CONCEDED",
                    "Sanctions concédées",
                    snapshot.Sanctions.Avertissements + snapshot.Sanctions.DeuxMinutes + snapshot.Sanctions.Exclusions,
                    AnalyticsSourceStatus.V1Partial,
                    breakdown:
                    [
                        new("Avertissements", snapshot.Sanctions.Avertissements),
                        new("Exclusions de deux minutes", snapshot.Sanctions.DeuxMinutes),
                        new("Disqualifications", snapshot.Sanctions.Exclusions)
                    ]),
                Count("WARNINGS_CONCEDED", "Avertissements", snapshot.Sanctions.Avertissements, AnalyticsSourceStatus.V1Compatible),
                Count("TWO_MINUTE_SUSPENSIONS_CONCEDED", "Exclusions de deux minutes", snapshot.Sanctions.DeuxMinutes, AnalyticsSourceStatus.V1Compatible),
                Count("DISQUALIFICATIONS_CONCEDED", "Disqualifications", snapshot.Sanctions.Exclusions, AnalyticsSourceStatus.V1Compatible)
            ],
            Goalkeeper =
            [
                Count("TOTAL_SAVES", "Arrêts total", totalSaves, AnalyticsSourceStatus.V1Compatible),
                Count("OPEN_PLAY_SAVES", "Arrêts dans le jeu", snapshot.Goalkeeper.Arrets, AnalyticsSourceStatus.V1Compatible),
                Count("PENALTY_SAVES", "Arrêts sur 7 m", snapshot.Goalkeeper.ArretsPenalty, AnalyticsSourceStatus.V1Compatible),
                Count("TOTAL_SHOTS_FACED", "Tirs subis total", totalShotsFaced, AnalyticsSourceStatus.V1Compatible),
                Count("OPEN_PLAY_SHOTS_FACED", "Tirs subis dans le jeu", openPlayShotsFaced, AnalyticsSourceStatus.V1Partial),
                Count("PENALTY_SHOTS_FACED", "Tirs subis sur 7 m", penaltyShotsFaced, AnalyticsSourceStatus.V1Partial),
                PartialRate("TOTAL_SAVE_RATE", "Taux d'arrêt général", totalSaves, totalShotsFaced, "arrêts", "tirs subis"),
                PartialRate("OPEN_PLAY_SAVE_RATE", "Taux d'arrêt dans le jeu", snapshot.Goalkeeper.Arrets, openPlayShotsFaced, "arrêts", "tirs subis"),
                PartialRate("PENALTY_SAVE_RATE", "Taux d'arrêt sur 7 m", snapshot.Goalkeeper.ArretsPenalty, penaltyShotsFaced, "arrêts", "tirs subis"),
                Count("GOALKEEPER_ASSISTS", "Passes décisives", snapshot.Global.AssistCount, AnalyticsSourceStatus.V1Compatible),
                Count("GOALKEEPER_GOALS", "Buts", snapshot.Global.TotalGoals, AnalyticsSourceStatus.V1Compatible),
                Count("GOALKEEPER_TURNOVERS", "Pertes de balle", snapshot.Global.TurnoverCount, AnalyticsSourceStatus.V1Compatible),
                Count("GOALKEEPER_MISSED_SHOTS", "Tirs ratés", snapshot.Goalkeeper.TirsLoupes, AnalyticsSourceStatus.V1Compatible)
            ]
        };
    }

    private static LeagueMetricDisplayModel Count(
        string code,
        string label,
        int? value,
        AnalyticsSourceStatus source,
        string? availability = null,
        string? unavailableMessage = null,
        string? helpText = null,
        IReadOnlyList<LeagueMetricBreakdownItem>? breakdown = null)
    {
        return new LeagueMetricDisplayModel
        {
            MetricCode = code,
            Label = label,
            Kind = LeagueMetricDisplayKind.Count,
            CountValue = value,
            Source = source,
            Availability = availability,
            UnavailableMessage = unavailableMessage,
            HelpText = helpText,
            Breakdown = breakdown ?? []
        };
    }

    private static LeagueMetricDisplayModel UnavailableCount(string code, string label) =>
        Count(
            code,
            label,
            null,
            AnalyticsSourceStatus.Unavailable,
            "UNAVAILABLE",
            MissingDataMessage);

    private static LeagueMetricDisplayModel Rate(
        string label,
        LeagueMetricValueDto metric,
        string numeratorUnit,
        string denominatorUnit)
    {
        return new LeagueMetricDisplayModel
        {
            MetricCode = metric.MetricCode,
            Label = label,
            Kind = LeagueMetricDisplayKind.Rate,
            Rate = RateDisplayModel.FromV2(label, metric, numeratorUnit, denominatorUnit),
            Source = AnalyticsSourceStatus.V2Complete
        };
    }

    private static LeagueMetricDisplayModel PartialRate(
        string code,
        string label,
        double numerator,
        double denominator,
        string numeratorUnit,
        string denominatorUnit)
    {
        double? value = denominator > 0 && double.IsFinite(numerator) && double.IsFinite(denominator)
            ? Math.Round(numerator * 100d / denominator, 2, MidpointRounding.AwayFromZero)
            : null;

        return new LeagueMetricDisplayModel
        {
            MetricCode = code,
            Label = label,
            Kind = LeagueMetricDisplayKind.Rate,
            Source = AnalyticsSourceStatus.V1Partial,
            Rate = new RateDisplayModel
            {
                MetricCode = code,
                Label = label,
                Value = value,
                Numerator = numerator,
                Denominator = denominator,
                Unit = "%",
                SampleReliable = false,
                MinimumSample = null,
                QualityScore = null,
                QualityReason = null,
                QualityKnown = false,
                MetricVersion = null,
                SourceLabel = AnalyticsSourceStatus.V1Partial.Label(),
                NumeratorUnit = numeratorUnit,
                DenominatorUnit = denominatorUnit,
                QualityLabel = "Qualité non fournie par l'API v1",
                Tooltip = $"{label} recomposé uniquement depuis les atomes exacts disponibles en API v1.",
                Tone = value.HasValue ? "neutral" : "warning"
            }
        };
    }
}

public sealed class LeaguePlayerAnalyticsService
{
    private readonly ILeagueAnalyticsGateway _gateway;

    public LeaguePlayerAnalyticsService(ILeagueAnalyticsGateway gateway)
    {
        _gateway = gateway;
    }

    public Task<LeagueGatewayResult> LoadV2Async(
        int playerId,
        StatsQueryOptionsDto options,
        CancellationToken cancellationToken = default) =>
        _gateway.GetPlayerAsync(
            playerId,
            options,
            LeagueAnalyticsContract.AllSections,
            cancellationToken);

    public LeaguePlayerAnalyticsLoadResult Resolve(
        int playerId,
        LeagueGatewayResult gatewayResult,
        LeagueV1Snapshot fallback,
        AnalysisScopeDisplayModel scope)
    {
        if (gatewayResult.Outcome == LeagueGatewayOutcome.Success)
        {
            return new LeaguePlayerAnalyticsLoadResult
            {
                Analytics = LeaguePlayerAnalyticsMapper.FromV2(gatewayResult.Response!, scope),
                Source = AnalyticsSourceStatus.V2Complete
            };
        }

        if (gatewayResult.Outcome == LeagueGatewayOutcome.Unavailable)
        {
            return new LeaguePlayerAnalyticsLoadResult
            {
                Analytics = LeaguePlayerAnalyticsMapper.FromV1(playerId, fallback, scope),
                Source = AnalyticsSourceStatus.V1Partial,
                Error = gatewayResult.Error
            };
        }

        return new LeaguePlayerAnalyticsLoadResult
        {
            Source = gatewayResult.Outcome == LeagueGatewayOutcome.ContractError
                ? AnalyticsSourceStatus.ContractError
                : AnalyticsSourceStatus.Unavailable,
            Error = gatewayResult.Error
        };
    }
}
