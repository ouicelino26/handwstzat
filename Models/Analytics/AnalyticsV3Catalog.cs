namespace HandWStat.Models.Analytics;

// ──────────────────────────────────────────────────────────────────────────────
// Position scopes — flags for flexible metric applicability
// ──────────────────────────────────────────────────────────────────────────────

[Flags]
public enum AnalyticsPositionScope
{
    None     = 0,
    GK       = 1 << 0,  // Gardienne de but
    AIL      = 1 << 1,  // Ailière
    AR       = 1 << 2,  // Arrière
    DC       = 1 << 3,  // Demi-centre
    PIV      = 1 << 4,  // Pivot
    AllField = AIL | AR | DC | PIV,
    All      = GK | AIL | AR | DC | PIV,
}

// ──────────────────────────────────────────────────────────────────────────────
// Metric status
// ──────────────────────────────────────────────────────────────────────────────

public enum AnalyticsMetricStatus
{
    Active,       // Displayed in the UI
    Expert,       // Valid metric shown only in advanced/expert views
    Experimental, // Defined in dictionary but never displayed
    Removed       // Withdrawn from the catalog — dictionary entry retained for reference
}

// ──────────────────────────────────────────────────────────────────────────────
// Metric unit
// ──────────────────────────────────────────────────────────────────────────────

public enum AnalyticsMetricUnit
{
    Per60,    // per 60 minutes of playing time
    Percent,  // percentage (0–100)
    Ratio,    // dimensionless ratio
    PerMatch, // per match played
    Count,    // integer event count
}

// ──────────────────────────────────────────────────────────────────────────────
// Metric definition record
// ──────────────────────────────────────────────────────────────────────────────

public sealed record AnalyticsMetricDefinition(
    string Code,
    string TechnicalName,
    string DisplayName,
    string Definition,
    AnalyticsMetricUnit Unit,
    int MinimumSampleCount,
    double MinimumPlayingTimeMinutes,
    AnalyticsPositionScope ApplicablePositions,
    AnalyticsMetricStatus Status,
    bool HigherIsBetter,
    string? RemovedReason = null);

// ──────────────────────────────────────────────────────────────────────────────
// Analytics V3 catalog
// ──────────────────────────────────────────────────────────────────────────────

public static class AnalyticsV3Catalog
{
    public static IReadOnlyDictionary<string, AnalyticsMetricDefinition> All { get; } = Build();

    private static Dictionary<string, AnalyticsMetricDefinition> Build() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ── CAT-01 ──────────────────────────────────────────────────────
            ["CAT-01"] = new(
                Code: "CAT-01",
                TechnicalName: "goals_created_per60",
                DisplayName: "Buts créés /60",
                Definition: "Contribution offensive totale (buts jeu ouvert + 7m + passes décisives) normalisée à 60 min. " +
                            "Formule: (TotalGoals + AssistCount) / PlayingTimeMinutes × 60. " +
                            "TotalGoals = GoalCount + PenaltyGoalCount — 7m inclus volontairement.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 1,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.AllField,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-02 ──────────────────────────────────────────────────────
            ["CAT-02"] = new(
                Code: "CAT-02",
                TechnicalName: "offensive_volume_per60",
                DisplayName: "Volume offensif /60",
                Definition: "Volume total de tirs (jeu ouvert + 7m) et passes décisives normalisé à 60 min. " +
                            "Formule: (ShotAttempts + AssistCount) / PlayingTimeMinutes × 60. " +
                            "ShotAttempts = TotalGoals + ShotMisses(open) + PenaltyMisses — inclut tous les tirs.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.AllField,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-03 — REMOVED ────────────────────────────────────────────
            ["CAT-03"] = new(
                Code: "CAT-03",
                TechnicalName: "offensive_yield_ratio",
                DisplayName: "Rendement offensif",
                Definition: "REMOVED — Métrique retirée : mélange dimensions offensives (Goals + Assists) et " +
                            "défensives (Interceptions) dans un seul dénominateur offensif (Turnovers) sans " +
                            "justification analytique. Utiliser CAT-01, CAT-05 et CAT-10 séparément.",
                Unit: AnalyticsMetricUnit.Ratio,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.None,
                Status: AnalyticsMetricStatus.Removed,
                HigherIsBetter: true,
                RemovedReason: "REMOVED — see CAT-01, CAT-05, CAT-10"),

            // ── CAT-04 ──────────────────────────────────────────────────────
            ["CAT-04"] = new(
                Code: "CAT-04",
                TechnicalName: "open_play_shot_success_rate",
                DisplayName: "Réussite jeu ouvert",
                Definition: "Pourcentage de tirs en jeu ouvert convertis en buts. " +
                            "Formule: GoalCount / OpenShotAttempts × 100. " +
                            "GoalCount = buts hors 7m uniquement. " +
                            "OpenShotAttempts = GoalCount + ShotMisses (jeu ouvert — inclut tirs contre).",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 10,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.AIL | AnalyticsPositionScope.AR |
                                     AnalyticsPositionScope.PIV | AnalyticsPositionScope.DC,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-05 ──────────────────────────────────────────────────────
            ["CAT-05"] = new(
                Code: "CAT-05",
                TechnicalName: "assist_turnover_ratio",
                DisplayName: "Assist / Turnover",
                Definition: "Nombre de passes décisives pour chaque perte de balle. Mesure l'économie de création. " +
                            "Formule: AssistCount / TurnoverCount. " +
                            "Retourne N/A si TurnoverCount = 0 (joueuse sans perte — ratio sans sens).",
                Unit: AnalyticsMetricUnit.Ratio,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.DC | AnalyticsPositionScope.AR |
                                     AnalyticsPositionScope.AIL | AnalyticsPositionScope.PIV,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-06 ──────────────────────────────────────────────────────
            ["CAT-06"] = new(
                Code: "CAT-06",
                TechnicalName: "penalties_won_per_match",
                DisplayName: "7m obtenus /match",
                Definition: "Nombre moyen de 7m obtenus par match joué. " +
                            "Formule: PenaltiesWon / MatchesPlayed. " +
                            "Source: LeaguePlayerAnalyticsResponseDto.Offense.PenaltiesWon (LeagueAttackMetricsDto — champ Attack, pas Defense).",
                Unit: AnalyticsMetricUnit.PerMatch,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.PIV | AnalyticsPositionScope.AR | AnalyticsPositionScope.AIL,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-07 ──────────────────────────────────────────────────────
            ["CAT-07"] = new(
                Code: "CAT-07",
                TechnicalName: "offensive_fouls_drawn_per_match",
                DisplayName: "Passages en force provoqués /match",
                Definition: "Nombre moyen de passages en force adverses provoqués par match. " +
                            "Formule: OffensiveFoulsDrawn / MatchesPlayed. " +
                            "Source canonique: LeaguePlayerAnalyticsResponseDto.Defense.OffensiveFoulsDrawn (v2 analytics). " +
                            "Fallback si v2 indisponible: PlayerDefenseStatsDto.PassageForce. " +
                            "Ces deux sources couvrent le même événement métier 'Provoque Passage force' — ne jamais cumuler les deux. " +
                            "Note: PassageForce est aussi une composante de CAT-10.",
                Unit: AnalyticsMetricUnit.PerMatch,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.PIV | AnalyticsPositionScope.DC | AnalyticsPositionScope.AIL,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-08 ──────────────────────────────────────────────────────
            ["CAT-08"] = new(
                Code: "CAT-08",
                TechnicalName: "turnovers_per60",
                DisplayName: "Pertes /60",
                Definition: "Nombre de pertes de balle normalisé à 60 min. " +
                            "Déjà calculé côté API: PlayerGlobalStatsDto.TurnoversPer60. " +
                            "Retourne N/A si PlayingTimeMinutes <= 0.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.DC | AnalyticsPositionScope.AR |
                                     AnalyticsPositionScope.AIL | AnalyticsPositionScope.PIV,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: false),

            // ── CAT-09 ──────────────────────────────────────────────────────
            ["CAT-09"] = new(
                Code: "CAT-09",
                TechnicalName: "interceptions_per60",
                DisplayName: "Interceptions /60",
                Definition: "Nombre d'interceptions normalisé à 60 min. " +
                            "Déjà calculé côté API: PlayerGlobalStatsDto.InterceptionsPer60. " +
                            "Retourne N/A si PlayingTimeMinutes <= 0.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.DC | AnalyticsPositionScope.AIL | AnalyticsPositionScope.AR,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-10 ──────────────────────────────────────────────────────
            ["CAT-10"] = new(
                Code: "CAT-10",
                TechnicalName: "defensive_impact_per60",
                DisplayName: "Impact défensif /60",
                Definition: "Somme des actions défensives positives normalisée à 60 min. " +
                            "Formule: (Interceptions + Contres + Neutralisations + PassageForce) / PlayingTimeMinutes × 60. " +
                            "Source: PlayerDefenseStatsDto — champ PassageForce (≠ PassageEnForce de PlayerPassingStatsDto). " +
                            "Note: PassageForce correspond aux mêmes événements que OffensiveFoulsDrawn (CAT-07).",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.DC | AnalyticsPositionScope.PIV |
                                     AnalyticsPositionScope.AIL | AnalyticsPositionScope.AR,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-11 ──────────────────────────────────────────────────────
            ["CAT-11"] = new(
                Code: "CAT-11",
                TechnicalName: "sanctions_per60",
                DisplayName: "Sanctions /60",
                Definition: "Nombre de sanctions normalisé à 60 min. " +
                            "Déjà calculé côté API: PlayerGlobalStatsDto.SanctionsPer60. " +
                            "Retourne N/A si PlayingTimeMinutes <= 0.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.All,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: false),

            // ── CAT-12 ──────────────────────────────────────────────────────
            ["CAT-12"] = new(
                Code: "CAT-12",
                TechnicalName: "offensive_waste_rate",
                DisplayName: "Taux d'erreur offensif",
                Definition: "Proportion d'actions offensives terminant en perte. " +
                            "Formule: TurnoverCount / (OpenShotAttempts + PenaltyAttempts + AssistCount + TurnoverCount) × 100. " +
                            "Minimum 20 actions au dénominateur.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 20,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.DC | AnalyticsPositionScope.AR |
                                     AnalyticsPositionScope.AIL | AnalyticsPositionScope.PIV,
                Status: AnalyticsMetricStatus.Expert,
                HigherIsBetter: false),

            // ── CAT-13 ──────────────────────────────────────────────────────
            ["CAT-13"] = new(
                Code: "CAT-13",
                TechnicalName: "gk_open_play_save_rate",
                DisplayName: "Taux d'arrêt — jeu ouvert",
                Definition: "Pourcentage de tirs en jeu ouvert arrêtés par la gardienne. " +
                            "Formule: OpenPlaySaves / OpenPlayShotsFaced × 100. " +
                            "Source: LeagueGoalkeeperMetricsDto — préférer OpenPlaySaveRate (MetricValue avec Quality).",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 20,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.GK,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-14 ──────────────────────────────────────────────────────
            ["CAT-14"] = new(
                Code: "CAT-14",
                TechnicalName: "gk_penalty_save_rate",
                DisplayName: "Taux d'arrêt — 7 mètres",
                Definition: "Pourcentage de 7m arrêtés par la gardienne. " +
                            "Formule: PenaltySaves / PenaltyShotsFaced × 100. " +
                            "Minimum 5 penalties — afficher ratio brut X/Y si n < 5.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.GK,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-15 ──────────────────────────────────────────────────────
            ["CAT-15"] = new(
                Code: "CAT-15",
                TechnicalName: "gk_saves_per60",
                DisplayName: "Arrêts /60",
                Definition: "Volume d'arrêts normalisé à 60 min. " +
                            "Déjà calculé côté API: PlayerGlobalStatsDto.SavesPer60. " +
                            "Retourne N/A si PlayingTimeMinutes <= 0.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.GK,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-16 ──────────────────────────────────────────────────────
            ["CAT-16"] = new(
                Code: "CAT-16",
                TechnicalName: "gk_shots_faced_per60",
                DisplayName: "Tirs subis /60",
                Definition: "Nombre de tirs reçus normalisé à 60 min. " +
                            "Formule: TirsSubis / PlayingTimeMinutes × 60. " +
                            "Source: PlayerGoalkeeperStatsDto.TirsSubis = Arrets + ArretsPenalty + ButsPris + ButsPenalty.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.GK,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: false),

            // ── CAT-17A ─────────────────────────────────────────────────────
            ["CAT-17A"] = new(
                Code: "CAT-17A",
                TechnicalName: "goals_share_pct",
                DisplayName: "Part des buts de l'équipe",
                Definition: "Pourcentage des buts d'équipe marqués par la joueuse (jeu ouvert + 7m inclus). Bornée 0–100%. " +
                            "Formule: TotalGoals / TeamStatsDto.GoalsFor × 100. " +
                            "TotalGoals = GoalCount + PenaltyGoalCount. " +
                            "TeamGoalsFor non disponible dans Players.razor — affichage réservé à PositionProfiles et Analyse.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 3,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.AllField,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-17B ─────────────────────────────────────────────────────
            ["CAT-17B"] = new(
                Code: "CAT-17B",
                TechnicalName: "direct_involvement_pct",
                DisplayName: "Implication directe (buts + passes)",
                Definition: "Nombre de buts auxquels la joueuse a participé (marquant ou assistant) rapporté aux buts d'équipe. " +
                            "Peut dépasser 100% — ne pas présenter comme part exclusive. " +
                            "Formule: (GoalCount + AssistCount) / TeamStatsDto.GoalsFor × 100.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 3,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.AllField,
                Status: AnalyticsMetricStatus.Expert,
                HigherIsBetter: true),

            // ── CAT-18 ──────────────────────────────────────────────────────
            ["CAT-18"] = new(
                Code: "CAT-18",
                TechnicalName: "trigger_success_rate",
                DisplayName: "Efficacité par déclenchement",
                Definition: "Taux de réussite des tirs selon le type de déclenchement. " +
                            "Formule: TriggerZoneStatDto.SuccessCount / TriggerZoneStatDto.Attempts × 100. " +
                            "Masquer les déclenchements avec < 5 tentatives.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.AIL | AnalyticsPositionScope.AR | AnalyticsPositionScope.PIV,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-19 ──────────────────────────────────────────────────────
            ["CAT-19"] = new(
                Code: "CAT-19",
                TechnicalName: "contextual_splits",
                DisplayName: "Analyse contextuelle",
                Definition: "Distribution des métriques selon le contexte de jeu " +
                            "(état du score, type d'attaque, type de défense, système offensif). " +
                            "Source: EventContextBreakdownDto via GET api/stats/events/contexts?playerId=X. " +
                            "Minimum 10 événements par ligne de contexte.",
                Unit: AnalyticsMetricUnit.Count,
                MinimumSampleCount: 10,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.All,
                Status: AnalyticsMetricStatus.Expert,
                HigherIsBetter: true),

            // ── CAT-20 ──────────────────────────────────────────────────────
            ["CAT-20"] = new(
                Code: "CAT-20",
                TechnicalName: "position_percentile",
                DisplayName: "Percentile de poste",
                Definition: "Rang de la joueuse dans sa cohorte de poste (0–100). " +
                            "Source: PositionProfileAxisDto.Percentile (calculé côté API). " +
                            "Cohorte minimum: 5 joueuses au même poste.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.All,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-21 ──────────────────────────────────────────────────────
            ["CAT-21"] = new(
                Code: "CAT-21",
                TechnicalName: "goalkeeper_total_save_rate",
                DisplayName: "Taux d'arrêt global",
                Definition: "TotalSaves / TotalShotsFaced × 100. " +
                            "Source prioritaire: LeagueMetricValueDto.Value (TOTAL_SAVE_RATE, API v2). " +
                            "Fallback calculé: ComputeTotalSaveRate(Arrets+ArretsPenalty, TirsSubis). " +
                            "Fallback legacy: PlayerTechnicalStatsDto.GoalkeeperSaveRate. " +
                            "TirsSubis = Arrets + ArretsPenalty + ButsPris + ButsPenalty. " +
                            "Seuil qualité: 20 tirs minimum (SampleReliable).",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 20,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.GK,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: true),

            // ── CAT-22 ──────────────────────────────────────────────────────
            ["CAT-22"] = new(
                Code: "CAT-22",
                TechnicalName: "goalkeeper_goals_conceded_per60",
                DisplayName: "Buts encaissés /60",
                Definition: "(ButsPris + ButsPenalty) / PlayingTimeMinutes × 60. " +
                            "Exprime le volume de buts encaissés normalisé à 60 minutes de jeu. " +
                            "PlayingTimeMinutes = 0 → N/A. " +
                            "Seuil qualité: 150 minutes minimum de temps de jeu.",
                Unit: AnalyticsMetricUnit.Per60,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 150,
                ApplicablePositions: AnalyticsPositionScope.GK,
                Status: AnalyticsMetricStatus.Active,
                HigherIsBetter: false),

            // ── CAT-23 ──────────────────────────────────────────────────────
            ["CAT-23"] = new(
                Code: "CAT-23",
                TechnicalName: "spatial_shot_success_rate",
                DisplayName: "Réussite par secteur but",
                Definition: "Taux de réussite des tirs par zone du cadre. " +
                            "Source: ZoneStatDto.SuccessRate (valeur API préférée). " +
                            "Fallback calculé: ZoneStatDto.SuccessCount / ZoneStatDto.Attempts × 100. " +
                            "Pour les gardiennes: taux d'arrêt par secteur (SuccessCount = arrêts). " +
                            "Masquer si < 5 tentatives (MinimumSampleCount).",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.All,
                Status: AnalyticsMetricStatus.Expert,
                HigherIsBetter: true),

            // ── CAT-24 ──────────────────────────────────────────────────────
            ["CAT-24"] = new(
                Code: "CAT-24",
                TechnicalName: "spatial_attempt_share",
                DisplayName: "Part des tirs par zone",
                Definition: "Proportion des tirs cartographiés de la joueuse dirigés vers cette zone. " +
                            "Formule: zone.Attempts / sum(allZones.Attempts) × 100. " +
                            "Métrique descriptive — interprétation dépend du poste et du style de jeu.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 5,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.All,
                Status: AnalyticsMetricStatus.Expert,
                HigherIsBetter: false),

            // ── CAT-25 ──────────────────────────────────────────────────────
            ["CAT-25"] = new(
                Code: "CAT-25",
                TechnicalName: "spatial_goal_share",
                DisplayName: "Part des buts par zone",
                Definition: "Proportion des buts cartographiés marqués dans cette zone. " +
                            "Formule: zone.Successes / sum(allZones.Successes) × 100. " +
                            "Pour les gardiennes: part des arrêts effectués dans cette zone. " +
                            "N/A si total Successes = 0.",
                Unit: AnalyticsMetricUnit.Percent,
                MinimumSampleCount: 1,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.All,
                Status: AnalyticsMetricStatus.Expert,
                HigherIsBetter: false),

            // ── Composite metrics — EXPERIMENTAL, not displayed ──────────────

            ["MC-01"] = new(
                Code: "MC-01",
                TechnicalName: "player_impact_score",
                DisplayName: "Player Impact Score",
                Definition: "EXPERIMENTAL — Composite avec pondérations arbitraires. Non affiché. " +
                            "Formule indicative: (Goals×1.0 + Assists×0.8 + Interceptions×0.6 - Turnovers×0.5 - Sanctions×1.0) / MatchesPlayed.",
                Unit: AnalyticsMetricUnit.Ratio,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.None,
                Status: AnalyticsMetricStatus.Experimental,
                HigherIsBetter: true),

            ["MC-02"] = new(
                Code: "MC-02",
                TechnicalName: "offensive_efficiency_index",
                DisplayName: "Offensive Efficiency Index",
                Definition: "EXPERIMENTAL — Composite avec pondérations arbitraires et biais de poste. Non affiché.",
                Unit: AnalyticsMetricUnit.Ratio,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.None,
                Status: AnalyticsMetricStatus.Experimental,
                HigherIsBetter: true),

            ["MC-03"] = new(
                Code: "MC-03",
                TechnicalName: "gk_performance_index",
                DisplayName: "GK Performance Index",
                Definition: "EXPERIMENTAL — Composite GK (OpenSaveRate×0.6 + PenaltySaveRate×0.4). " +
                            "Non affiché. Traitement en mission dédiée.",
                Unit: AnalyticsMetricUnit.Ratio,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.None,
                Status: AnalyticsMetricStatus.Experimental,
                HigherIsBetter: true),

            ["MC-04"] = new(
                Code: "MC-04",
                TechnicalName: "defensive_profile_score",
                DisplayName: "Defensive Profile Score",
                Definition: "EXPERIMENTAL — Composite défensif. Non affiché. Traitement en mission dédiée.",
                Unit: AnalyticsMetricUnit.Ratio,
                MinimumSampleCount: 0,
                MinimumPlayingTimeMinutes: 0,
                ApplicablePositions: AnalyticsPositionScope.None,
                Status: AnalyticsMetricStatus.Experimental,
                HigherIsBetter: true),
        };

    // ── Query helpers ────────────────────────────────────────────────────────

    public static AnalyticsMetricDefinition? Get(string code) =>
        All.TryGetValue(code, out var def) ? def : null;

    public static IEnumerable<AnalyticsMetricDefinition> Active =>
        All.Values.Where(m => m.Status == AnalyticsMetricStatus.Active);

    public static MetricDictionaryEntry GetDictionaryEntry(string code)
    {
        var m = Get(code) ?? throw new KeyNotFoundException($"Metric code '{code}' not found in AnalyticsV3Catalog.");
        return new MetricDictionaryEntry(
            Code: m.Code,
            Label: m.DisplayName,
            TechnicalName: m.TechnicalName,
            Definition: m.Definition,
            Formula: ExtractFormula(m.Definition),
            Unit: m.Unit,
            Grain: ResolveGrain(m.Code),
            Category: ResolveCategory(m.Code),
            ApplicablePositions: m.ApplicablePositions,
            MinimumSample: m.MinimumSampleCount,
            HigherIsBetter: m.HigherIsBetter,
            Status: m.Status);
    }

    public static IEnumerable<MetricDictionaryEntry> ActiveDictionaryEntries =>
        Active.Select(m => GetDictionaryEntry(m.Code));

    private static string? ExtractFormula(string definition)
    {
        const string marker = "Formule: ";
        var idx = definition.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return null;
        var start = idx + marker.Length;
        var end = definition.IndexOf('.', start);
        return end > start ? definition[start..end] : definition[start..];
    }

    private static AnalyticsMetricGrain ResolveGrain(string code) => code switch
    {
        "CAT-23" or "CAT-24" or "CAT-25" => AnalyticsMetricGrain.Zone,
        _ => AnalyticsMetricGrain.Player,
    };

    private static AnalyticsMetricCategory ResolveCategory(string code) => code switch
    {
        "CAT-01" or "CAT-02" or "CAT-04" or "CAT-05" or "CAT-06" or "CAT-07"
            or "CAT-12" or "CAT-17A" or "CAT-17B" or "CAT-18" or "CAT-20"
            => AnalyticsMetricCategory.Offensive,
        "CAT-08" or "CAT-09" or "CAT-10" or "CAT-11"
            => AnalyticsMetricCategory.Defensive,
        "CAT-13" or "CAT-14" or "CAT-15" or "CAT-16" or "CAT-21" or "CAT-22"
            => AnalyticsMetricCategory.Goalkeeper,
        "CAT-19" => AnalyticsMetricCategory.Contextual,
        "CAT-23" or "CAT-24" or "CAT-25" => AnalyticsMetricCategory.Spatial,
        _ when All.TryGetValue(code, out var m) && m.Status == AnalyticsMetricStatus.Experimental
            => AnalyticsMetricCategory.Composite,
        _ => AnalyticsMetricCategory.Offensive,
    };

    public static IEnumerable<AnalyticsMetricDefinition> ForPosition(string? positionCode) =>
        Active.Where(m => IsApplicable(m, positionCode));

    public static bool IsApplicable(AnalyticsMetricDefinition metric, string? positionCode)
    {
        if (metric.ApplicablePositions == AnalyticsPositionScope.None) return false;
        var scope = ParsePositionScope(positionCode);
        return (metric.ApplicablePositions & scope) != 0;
    }

    public static AnalyticsPositionScope ParsePositionScope(string? positionCode) =>
        positionCode?.ToUpperInvariant() switch
        {
            "GK" or "GB" or "G" or "GOAL" or "GARDIENNE" or "GARDEN" or "GOALKEEPER"
                => AnalyticsPositionScope.GK,

            "AIL" or "AL" or "A"
                or "AILG" or "AILD" or "AIL-G" or "AIL-D"
                or "ALG"  or "ALD"
                or "AILIERE" or "AILIÈRE"
                => AnalyticsPositionScope.AIL,

            "AR" or "ARG" or "ARD" or "ARR" or "ARRIERE" or "ARRIÈRE"
                => AnalyticsPositionScope.AR,

            "DC" or "DCE" or "DEMI" or "DEMI-CENTRE" or "DEMI-CENTER"
                => AnalyticsPositionScope.DC,

            "PIV" or "P" or "PIVOT"
                => AnalyticsPositionScope.PIV,

            _ => AnalyticsPositionScope.AllField,
        };
}
