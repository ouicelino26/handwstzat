using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

public static class AnalyticsMetricLineage
{
    public static IReadOnlyDictionary<string, MetricLineageDefinition> All { get; } = Build();

    private static Dictionary<string, MetricLineageDefinition> Build() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["CAT-01"] = new("CAT-01", MetricPrimarySource.ComputedFromApi,
                "PlayerGlobalStatsDto.TotalGoals + PlayerGlobalStatsDto.AssistCount / PlayerGlobalStatsDto.PlayingTimeMinutes",
                "api/stats/players",
                "AnalyticsCalculationService.ComputeGoalsCreatedPer60"),

            ["CAT-02"] = new("CAT-02", MetricPrimarySource.ComputedFromApi,
                "PlayerGlobalStatsDto.ShotAttempts + PlayerGlobalStatsDto.AssistCount / PlayerGlobalStatsDto.PlayingTimeMinutes",
                "api/stats/players",
                "AnalyticsCalculationService.ComputeOffensiveVolumePer60"),

            ["CAT-04"] = new("CAT-04", MetricPrimarySource.ComputedFromApi,
                "PlayerGlobalStatsDto.GoalCount / PlayerGlobalStatsDto.OpenShotAttempts",
                "api/stats/players",
                "AnalyticsCalculationService.ComputeOpenPlaySuccessRate"),

            ["CAT-05"] = new("CAT-05", MetricPrimarySource.ComputedFromApi,
                "PlayerGlobalStatsDto.AssistCount / PlayerGlobalStatsDto.TurnoverCount",
                "api/stats/players",
                "AnalyticsCalculationService.ComputeAssistTurnoverRatio"),

            ["CAT-06"] = new("CAT-06", MetricPrimarySource.Api,
                "LeaguePlayerAnalyticsResponseDto.Attack.PenaltiesWon",
                "api/v2/analytics/players/{id}",
                "AnalyticsCalculationService.ComputePenaltiesWonPerMatch",
                FallbackSource: "UNKNOWN",
                Notes: "PenaltiesWon est dans le sous-objet Attack (LeagueAttackMetricsDto), pas Defense."),

            ["CAT-07"] = new("CAT-07", MetricPrimarySource.Api,
                "LeaguePlayerAnalyticsResponseDto.Defense.OffensiveFoulsDrawn",
                "api/v2/analytics/players/{id}",
                "AnalyticsCalculationService.ComputeOffensiveFoulsDrawnPerMatch",
                FallbackSource: "PlayerDefenseStatsDto.PassageForce via api/stats/players/defense",
                Notes: "Source v2 canonique. Ne jamais cumuler avec le fallback v1 PassageForce."),

            ["CAT-08"] = new("CAT-08", MetricPrimarySource.Api,
                "PlayerGlobalStatsDto.TurnoversPer60",
                "api/stats/players",
                CalculationSource: null,
                Notes: "Calculé par l'API. NormalizeApiPer60 convertit 0.0 (PT=0) en null."),

            ["CAT-09"] = new("CAT-09", MetricPrimarySource.Api,
                "PlayerGlobalStatsDto.InterceptionsPer60",
                "api/stats/players",
                CalculationSource: null),

            ["CAT-10"] = new("CAT-10", MetricPrimarySource.ComputedFromApi,
                "PlayerDefenseStatsDto.Interceptions + Contres + Neutralisations + PassageForce",
                "api/stats/players/defense",
                "AnalyticsCalculationService.ComputeDefensiveImpactPer60",
                Notes: "PassageForce depuis PlayerDefenseStatsDto (≠ PassageEnForce de PlayerPassingStatsDto)."),

            ["CAT-11"] = new("CAT-11", MetricPrimarySource.Api,
                "PlayerGlobalStatsDto.SanctionsPer60",
                "api/stats/players",
                CalculationSource: null),

            ["CAT-12"] = new("CAT-12", MetricPrimarySource.ComputedFromApi,
                "PlayerGlobalStatsDto.TurnoverCount / (OpenShotAttempts + PenaltyAttempts + AssistCount + TurnoverCount)",
                "api/stats/players",
                "AnalyticsCalculationService.ComputeOffensiveWasteRate"),

            ["CAT-13"] = new("CAT-13", MetricPrimarySource.Api,
                "LeagueGoalkeeperMetricsDto.OpenPlaySaveRate",
                "api/v2/analytics/players/{id}",
                "AnalyticsCalculationService.ComputeOpenPlaySaveRate",
                Notes: "Préférer la valeur v2 avec métadonnées qualité."),

            ["CAT-14"] = new("CAT-14", MetricPrimarySource.Api,
                "LeagueGoalkeeperMetricsDto.PenaltySaveRate",
                "api/v2/analytics/players/{id}",
                "AnalyticsCalculationService.ComputePenaltySaveRate"),

            ["CAT-15"] = new("CAT-15", MetricPrimarySource.Api,
                "PlayerGlobalStatsDto.SavesPer60",
                "api/stats/players",
                CalculationSource: null,
                Notes: "Calculé par l'API. NormalizeApiPer60 convertit 0.0 (PT=0) en null."),

            ["CAT-16"] = new("CAT-16", MetricPrimarySource.ComputedFromApi,
                "PlayerGoalkeeperStatsDto.TirsSubis",
                "api/stats/players/goalkeeper",
                "AnalyticsCalculationService.ComputeShotsFacedPer60"),

            ["CAT-17A"] = new("CAT-17A", MetricPrimarySource.ComputedFromApi,
                "PlayerGlobalStatsDto.TotalGoals / TeamStatsDto.GoalsFor",
                "api/stats/players + api/stats/teams",
                "AnalyticsCalculationService.ComputeGoalsSharePct"),

            ["CAT-17B"] = new("CAT-17B", MetricPrimarySource.ComputedFromApi,
                "(PlayerGlobalStatsDto.GoalCount + PlayerGlobalStatsDto.AssistCount) / TeamStatsDto.GoalsFor",
                "api/stats/players + api/stats/teams",
                "AnalyticsCalculationService.ComputeDirectInvolvement"),

            ["CAT-18"] = new("CAT-18", MetricPrimarySource.ComputedFromApi,
                "TriggerZoneStatDto.SuccessCount / TriggerZoneStatDto.Attempts",
                "api/stats/events/triggers",
                "AnalyticsCalculationService.ComputeTriggerSuccessRate"),

            ["CAT-19"] = new("CAT-19", MetricPrimarySource.Api,
                "EventContextBreakdownDto",
                "api/stats/events/contexts?playerId={id}",
                CalculationSource: null),

            ["CAT-20"] = new("CAT-20", MetricPrimarySource.Api,
                "PositionProfileAxisDto.Percentile",
                "api/v2/analytics/positions/{code}",
                CalculationSource: null,
                Notes: "Cohorte minimum: 5 joueuses au même poste."),

            ["CAT-21"] = new("CAT-21", MetricPrimarySource.Api,
                "LeagueMetricValueDto.Value (TOTAL_SAVE_RATE)",
                "api/v2/analytics/players/{id}",
                "AnalyticsCalculationService.ComputeTotalSaveRate",
                FallbackSource: "PlayerTechnicalStatsDto.GoalkeeperSaveRate",
                Notes: "TirsSubis = Arrets + ArretsPenalty + ButsPris + ButsPenalty."),

            ["CAT-22"] = new("CAT-22", MetricPrimarySource.ComputedFromApi,
                "PlayerGoalkeeperStatsDto.ButsPris + ButsPenalty",
                "api/stats/players/goalkeeper",
                "AnalyticsCalculationService.ComputeGoalsConcededPer60"),

            ["CAT-23"] = new("CAT-23", MetricPrimarySource.ComputedFromApi,
                "ZoneStatDto.SuccessRate (API préféré) / ZoneStatDto.SuccessCount / ZoneStatDto.Attempts",
                "api/stats/events/zones",
                "SpatialAnalyticsBuilder",
                Notes: "Pour GK: taux d'arrêt par secteur (SuccessCount = arrêts). Masquer si < 5 tentatives."),

            ["CAT-24"] = new("CAT-24", MetricPrimarySource.ComputedFromApi,
                "ZoneStatDto.Attempts / sum(allZones.Attempts)",
                "api/stats/events/zones",
                "SpatialAnalyticsBuilder"),

            ["CAT-25"] = new("CAT-25", MetricPrimarySource.ComputedFromApi,
                "ZoneStatDto.Successes / sum(allZones.Successes)",
                "api/stats/events/zones",
                "SpatialAnalyticsBuilder",
                Notes: "Pour GK: part des arrêts dans cette zone."),
        };

    public static MetricLineageDefinition? Get(string code) =>
        All.TryGetValue(code, out var def) ? def : null;
}
