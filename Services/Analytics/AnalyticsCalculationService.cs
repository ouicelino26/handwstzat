namespace HandWStat.Services.Analytics;

// ──────────────────────────────────────────────────────────────────────────────
// Analytics V3 — Central calculation service
//
// Rules:
//  - All methods are pure static functions with no side effects.
//  - Returns double? where null means NotApplicable (never return 0 as a proxy for N/A).
//  - PlayingTimeMinutes <= 0 → null for all /60 metrics.
//  - All denominators are guarded — no division by zero.
//  - Do NOT add formulas here that are already computed server-side and
//    returned directly in the DTO (TurnoversPer60, InterceptionsPer60, etc.).
//    Use NormalizeApiPer60() for those — it converts the API's 0.0 to null when
//    playing time is unknown.
// ──────────────────────────────────────────────────────────────────────────────

public static class AnalyticsCalculationService
{
    // ── CAT-01 — Buts créés /60 ──────────────────────────────────────────────
    // (TotalGoals + AssistCount) / PlayingTimeMinutes × 60
    // TotalGoals = GoalCount + PenaltyGoalCount (7m included intentionally)
    public static double? ComputeGoalsCreatedPer60(int totalGoals, int assists, double playingTimeMinutes)
    {
        if (playingTimeMinutes <= 0) return null;
        return (totalGoals + assists) / playingTimeMinutes * 60.0;
    }

    // ── CAT-02 — Volume offensif /60 ────────────────────────────────────────
    // (ShotAttempts + AssistCount) / PlayingTimeMinutes × 60
    // ShotAttempts = TotalGoals + ShotMisses(open) + PenaltyMisses — all shots
    public static double? ComputeOffensiveVolumePer60(int shotAttempts, int assists, double playingTimeMinutes)
    {
        if (playingTimeMinutes <= 0) return null;
        return (shotAttempts + assists) / playingTimeMinutes * 60.0;
    }

    // ── CAT-04 — Réussite jeu ouvert ────────────────────────────────────────
    // GoalCount / OpenShotAttempts × 100
    // GoalCount = open-play goals only (NOT TotalGoals — penalty goals excluded)
    // OpenShotAttempts = GoalCount + ShotMisses (open play only, including blocks)
    public static double? ComputeOpenPlaySuccessRate(int goalCount, int openShotAttempts)
    {
        if (openShotAttempts <= 0) return null;
        return (double)goalCount / openShotAttempts * 100.0;
    }

    // ── CAT-05 — Assist / Turnover ratio ────────────────────────────────────
    // AssistCount / TurnoverCount
    // Returns null (not 0, not infinity) when TurnoverCount = 0.
    // A player with no turnovers is excellent — the ratio is simply undefined.
    public static double? ComputeAssistTurnoverRatio(int assists, int turnovers)
    {
        if (turnovers <= 0) return null;
        return (double)assists / turnovers;
    }

    // ── CAT-06 — 7m obtenus /match ──────────────────────────────────────────
    // PenaltiesWon / MatchesPlayed
    // Source: LeaguePlayerAnalyticsResponseDto.Offense.PenaltiesWon (Attack, NOT Defense)
    public static double? ComputePenaltiesWonPerMatch(int penaltiesWon, int matchesPlayed)
    {
        if (matchesPlayed <= 0) return null;
        return (double)penaltiesWon / matchesPlayed;
    }

    // ── CAT-07 — Passages en force provoqués /match ──────────────────────────
    // OffensiveFoulsDrawn / MatchesPlayed
    // Source: LeaguePlayerAnalyticsResponseDto.Defense.OffensiveFoulsDrawn
    // Note: OffensiveFoulsDrawn maps to the same events as PlayerDefenseStatsDto.PassageForce
    public static double? ComputeOffensiveFoulsDrawnPerMatch(int offensiveFoulsDrawn, int matchesPlayed)
    {
        if (matchesPlayed <= 0) return null;
        return (double)offensiveFoulsDrawn / matchesPlayed;
    }

    // ── CAT-10 — Impact défensif /60 ────────────────────────────────────────
    // (Interceptions + Contres + Neutralisations + PassageForce) / PlayingTimeMinutes × 60
    // IMPORTANT: field name in PlayerDefenseStatsDto is PassageForce (NOT PassageEnForce)
    // PassageEnForce is the offensive foul in PlayerPassingStatsDto — different event
    public static double? ComputeDefensiveImpactPer60(
        int interceptions, int contres, int neutralisations, int passageForce,
        double playingTimeMinutes)
    {
        if (playingTimeMinutes <= 0) return null;
        return (interceptions + contres + neutralisations + passageForce) / playingTimeMinutes * 60.0;
    }

    // ── CAT-12 — Taux d'erreur offensif ────────────────────────────────────
    // TurnoverCount / (OpenShotAttempts + PenaltyAttempts + AssistCount + TurnoverCount) × 100
    // Minimum 20 actions at denominator
    public static double? ComputeOffensiveWasteRate(
        int turnovers, int openShotAttempts, int penaltyAttempts, int assists)
    {
        var denominator = openShotAttempts + penaltyAttempts + assists + turnovers;
        if (denominator <= 0) return null;
        return (double)turnovers / denominator * 100.0;
    }

    // ── CAT-13 — GK taux d'arrêt jeu ouvert ────────────────────────────────
    // OpenPlaySaves / OpenPlayShotsFaced × 100
    // Prefer the API-provided LeagueMetricValueDto.Value (OpenPlaySaveRate) when available
    public static double? ComputeOpenPlaySaveRate(int openPlaySaves, int openPlayShotsFaced)
    {
        if (openPlayShotsFaced <= 0) return null;
        return (double)openPlaySaves / openPlayShotsFaced * 100.0;
    }

    // ── CAT-14 — GK taux d'arrêt 7m ────────────────────────────────────────
    // PenaltySaves / PenaltyShotsFaced × 100
    public static double? ComputePenaltySaveRate(int penaltySaves, int penaltyShotsFaced)
    {
        if (penaltyShotsFaced <= 0) return null;
        return (double)penaltySaves / penaltyShotsFaced * 100.0;
    }

    // ── CAT-16 — Tirs subis /60 (GK) ───────────────────────────────────────
    // TirsSubis / PlayingTimeMinutes × 60
    // TirsSubis = Arrets + ArretsPenalty + ButsPris + ButsPenalty
    public static double? ComputeShotsFacedPer60(int tirsSubis, double playingTimeMinutes)
    {
        if (playingTimeMinutes <= 0) return null;
        return tirsSubis / playingTimeMinutes * 60.0;
    }

    // ── CAT-21 — Taux d'arrêt global (GK) ──────────────────────────────────
    // TotalSaves / TotalShotsFaced × 100
    // Prefer the API-provided LeagueMetricValueDto.Value (TOTAL_SAVE_RATE) when available
    public static double? ComputeTotalSaveRate(int totalSaves, int totalShotsFaced)
    {
        if (totalShotsFaced <= 0) return null;
        return (double)totalSaves / totalShotsFaced * 100.0;
    }

    // ── CAT-22 — Buts encaissés /60 (GK) ───────────────────────────────────
    // GoalsConceded / PlayingTimeMinutes × 60
    // GoalsConceded = ButsPris + ButsPenalty
    public static double? ComputeGoalsConcededPer60(int goalsConceded, double playingTimeMinutes)
    {
        if (playingTimeMinutes <= 0) return null;
        return goalsConceded / playingTimeMinutes * 60.0;
    }

    // ── CAT-17A — Part des buts de l'équipe ────────────────────────────────
    // TotalGoals (GoalCount + PenaltyGoalCount) / TeamGoalsFor × 100  →  bounded 0–100%
    // Pass TotalGoals, not open-play GoalCount alone — 7m goals count toward team total.
    public static double? ComputeGoalsSharePct(int totalGoals, int teamGoalsFor)
    {
        if (teamGoalsFor <= 0) return null;
        return (double)totalGoals / teamGoalsFor * 100.0;
    }

    // ── CAT-17B — Implication directe (buts + passes) ───────────────────────
    // (GoalCount + AssistCount) / TeamGoalsFor × 100
    // May exceed 100% — do NOT present as an exclusive share percentage
    public static double? ComputeDirectInvolvement(int goalCount, int assists, int teamGoalsFor)
    {
        if (teamGoalsFor <= 0) return null;
        return (double)(goalCount + assists) / teamGoalsFor * 100.0;
    }

    // ── CAT-18 — Efficacité par déclenchement ──────────────────────────────
    // SuccessCount / Attempts × 100 for a single TriggerZoneStatDto
    public static double? ComputeTriggerSuccessRate(int successCount, int attempts)
    {
        if (attempts <= 0) return null;
        return (double)successCount / attempts * 100.0;
    }

    // ── API pass-through — /60 fields already computed server-side ──────────
    // The API returns 0.0 when PlayingTimeMinutes = 0 (ComputePer60 backend guard).
    // We must convert that 0.0 to null so the UI shows "—" instead of 0.
    // Applies to: TurnoversPer60, InterceptionsPer60, SanctionsPer60, SavesPer60,
    //             GoalsPer60, AssistsPer60.

    /// <summary>
    /// Normalizes an API-provided /60 value: returns null when the player has no
    /// playing time (distinguishes "no production" from "no data").
    /// </summary>
    public static double? NormalizeApiPer60(double apiValue, double playingTimeMinutes) =>
        playingTimeMinutes <= 0 ? null : apiValue;
}
