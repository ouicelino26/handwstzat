using System.Globalization;
using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

// ── V2 Player Sheet Export Data Models ────────────────────────────────────────

/// <summary>Table row for the V2 player sheet: indicator + value + evidence (fraction or context) + position benchmark.</summary>
public sealed record PlayerSheetRowV2(string Label, string Value, string Evidence, string Tone, string PercentileLabel = "", string PercentileTone = "");

/// <summary>Radar axis: human label + normalized 0-100 player and median values.</summary>
public sealed record PlayerSheetRadarAxis(string Label, double PlayerValue, double MedianValue);

/// <summary>
/// Static helpers for building the V2 player export sheet.
/// Pure logic — no UI dependencies — fully unit-testable.
/// </summary>
public static class PlayerSheetExportHelper
{
    // ── Radar axis selection ──────────────────────────────────────────────────

    /// <summary>
    /// Select at most 6 offensive axes from the position profile.
    /// Higher-is-better axes are preferred; radar values are already favorable-direction normalized.
    /// </summary>
    public static IReadOnlyList<PlayerSheetRadarAxis> BuildOffensiveRadarAxes(
        PositionProfileResponseDto? positionProfile,
        bool isGoalkeeper)
    {
        var allAxes = positionProfile?.SelectedPlayer?.Axes ?? [];
        if (allAxes.Count == 0 || HasMissingPlayingTime(positionProfile))
        {
            return [];
        }

        var offenseCategories = isGoalkeeper
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "goalkeeper", "offense", "passing" }
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "offense", "passing", "technical" };

        var offenseKeywords = isGoalkeeper
            ? new[] { "save", "stop", "arret", "penalty", "save_rate", "goalkeeper" }
            : new[] { "goal", "but", "assist", "passe", "shot", "tir", "7m", "penalty_won", "sanction_drawn", "turnover", "perte" };

        return allAxes
            .Where(axis => axis is not null && !string.IsNullOrWhiteSpace(axis.Label))
            .Where(axis => IsOffensiveAxis(axis, offenseCategories, offenseKeywords))
            .OrderByDescending(axis => axis.HigherIsBetter ? 1 : 0)
            .ThenBy(axis => GetRadarAxisSortRank(axis))
            .Take(6)
            .Select(axis => new PlayerSheetRadarAxis(
                HumanizeRadarLabel(axis.Label),
                Math.Clamp(axis.Percentile, 0d, 100d),
                50.0))
            .ToList();
    }

    /// <summary>
    /// Select at most 6 defensive axes from the position profile.
    /// For HigherIsBetter=false metrics (turnovers, sanctions, penalties conceded),
    /// the radar value is inverted so the polygon always reflects a favorable direction.
    /// </summary>
    public static IReadOnlyList<PlayerSheetRadarAxis> BuildDefensiveRadarAxes(
        PositionProfileResponseDto? positionProfile,
        bool isGoalkeeper)
    {
        var allAxes = positionProfile?.SelectedPlayer?.Axes ?? [];
        if (allAxes.Count == 0 || HasMissingPlayingTime(positionProfile))
        {
            return [];
        }

        if (isGoalkeeper)
        {
            var gkDefKeywords = new[] { "conceded", "goal_against", "penalty_conceded", "sanction_conceded", "buts_pris" };
            return allAxes
                .Where(axis => axis is not null && !string.IsNullOrWhiteSpace(axis.Label))
                .Where(axis => IsDefensiveAxis(axis, gkDefKeywords))
                .Take(6)
                .Select(axis => new PlayerSheetRadarAxis(
                    HumanizeRadarLabel(axis.Label),
                    Math.Clamp(axis.Percentile, 0d, 100d),
                    50.0))
                .ToList();
        }

        var defKeywords = new[] { "interception", "block", "contre", "def", "sanction_conceded", "penalty_conceded", "7m_concede", "concede", "discipline", "prise" };
        return allAxes
            .Where(axis => axis is not null && !string.IsNullOrWhiteSpace(axis.Label))
            .Where(axis => IsDefensiveAxis(axis, defKeywords))
            .Take(6)
            .Select(axis => new PlayerSheetRadarAxis(
                HumanizeRadarLabel(axis.Label),
                Math.Clamp(axis.Percentile, 0d, 100d),
                50.0))
            .ToList();
    }

    // ── Normalization ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the API-provided percentile, clamped to 0-100.
    /// The percentile is already direction-aware (API handles HigherIsBetter).
    /// Min-max normalization is forbidden per A9 spec §7/§19 — always use percentile.
    /// </summary>
    public static double NormalizeRadarValue(PositionProfileAxisDto axis, double value)
    {
        _ = value;
        return Math.Clamp(axis.Percentile, 0d, 100d);
    }

    // ── Table rows ────────────────────────────────────────────────────────────

    /// <summary>
    /// Build offensive table rows for a field player.
    /// <paramref name="penaltiesWon"/> and <paramref name="sanctionsDrawn"/> come from the
    /// LeagueAnalytics V2 data (codes PENALTIES_WON / SANCTIONS_DRAWN). Pass -1 when unavailable.
    /// </summary>
    public static IReadOnlyList<PlayerSheetRowV2> BuildOffensiveRows(
        PlayerProfileDto profile,
        PlayerGlobalStatsDto global,
        PlayerOffenseStatsDto offense,
        PlayerPassingStatsDto passing,
        PlayerTechnicalStatsDto technical,
        int penaltiesWon = -1,
        bool penaltiesWonUnavailable = false,
        int sanctionsDrawn = -1,
        bool sanctionsDrawnUnavailable = false,
        PositionProfileResponseDto? positionProfile = null)
    {
        var penShots = offense.Buts7m + offense.PenaltyRate;
        var penWonValue    = penaltiesWon >= 0   ? penaltiesWon.ToString()   : (penaltiesWonUnavailable   ? "N/D" : "—");
        var sanctDrawnValue = sanctionsDrawn >= 0 ? sanctionsDrawn.ToString() : (sanctionsDrawnUnavailable ? "N/D" : "—");

        return
        [
            WithPct(new PlayerSheetRowV2("Buts dans le jeu", offense.Buts.ToString(), string.Empty, "positive"),
                positionProfile, "open_goals_per60"),
            WithPct(new PlayerSheetRowV2("Buts sur 7m", offense.Buts7m.ToString(), string.Empty, "positive"),
                positionProfile, "penalty_goals_per60"),
            WithPct(new PlayerSheetRowV2("% tir 7m", FormatRateOrDash(technical.PenaltySuccessRate, penShots), penShots > 0 ? $"{offense.Buts7m} / {penShots}" : string.Empty, technical.PenaltySuccessRate >= 60 ? "positive" : "warning"),
                positionProfile, "penalty_success_rate"),
            WithPct(new PlayerSheetRowV2("Passes decisives", global.AssistCount.ToString(), string.Empty, "good"),
                positionProfile, "assists_per60"),
            WithPct(new PlayerSheetRowV2("Pertes de balle", passing.TotalPertes.ToString(), string.Empty, passing.TotalPertes > 15 ? "warning" : "neutral"),
                positionProfile, "turnovers_per60"),
            WithPct(new PlayerSheetRowV2("7m obtenus", penWonValue, string.Empty, "positive"),
                positionProfile, "penalties_won_per60"),
            WithPct(new PlayerSheetRowV2("Sanctions obtenues", sanctDrawnValue, string.Empty, "neutral"),
                positionProfile, "sanctions_won_per60"),
        ];
    }

    /// <summary>Build defensive table rows for a field player.</summary>
    public static IReadOnlyList<PlayerSheetRowV2> BuildDefensiveRows(
        PlayerProfileDto profile,
        PlayerDefenseStatsDto defense,
        PlayerSanctionStatsDto sanctions,
        PositionProfileResponseDto? positionProfile = null)
    {
        return
        [
            WithPct(new PlayerSheetRowV2("Interceptions", defense.Interceptions.ToString(), string.Empty, "good"),
                positionProfile, "interceptions_per60"),
            WithPct(new PlayerSheetRowV2("Contres", defense.Contres.ToString(), string.Empty, "good"),
                positionProfile, "blocks_per60"),
            WithPct(new PlayerSheetRowV2("7m concedes", sanctions.PenaltyConcede.ToString(), string.Empty, sanctions.PenaltyConcede > 3 ? "warning" : "neutral"),
                positionProfile, "penalties_conceded_per60"),
            WithPct(new PlayerSheetRowV2("2 minutes", sanctions.DeuxMinutes.ToString(), $"{sanctions.Avertissements} avert. · {sanctions.Exclusions} excl.", sanctions.DeuxMinutes > 5 ? "danger" : "neutral"),
                positionProfile, "two_minutes_per60"),
        ];
    }

    /// <summary>Build offensive table rows for a goalkeeper with exact position-axis mappings.</summary>
    public static IReadOnlyList<PlayerSheetRowV2> BuildGoalkeeperOffensiveRows(
        PlayerGlobalStatsDto global,
        PlayerGoalkeeperStatsDto goalkeeper,
        PlayerTechnicalStatsDto technical,
        PositionProfileResponseDto? positionProfile = null)
    {
        var totalSaves = goalkeeper.Arrets + goalkeeper.ArretsPenalty;
        var penaltyShotsFaced = goalkeeper.ArretsPenalty + goalkeeper.ButsPenalty;

        return
        [
            new PlayerSheetRowV2("Matchs", global.MatchesPlayed.ToString(), string.Empty, "neutral"),
            WithPct(new PlayerSheetRowV2("Arrets", totalSaves.ToString(), $"{goalkeeper.Arrets} jeu + {goalkeeper.ArretsPenalty} sur 7m", "positive"),
                positionProfile, "saves_per60"),
            WithPct(new PlayerSheetRowV2("Taux d'arret", FormatRateOrDash(technical.GoalkeeperSaveRate, goalkeeper.TirsSubis), goalkeeper.TirsSubis > 0 ? $"{totalSaves} / {goalkeeper.TirsSubis}" : string.Empty, "good"),
                positionProfile, "save_rate"),
            WithPct(new PlayerSheetRowV2("Arrets sur 7m", goalkeeper.ArretsPenalty.ToString(), penaltyShotsFaced > 0 ? $"{goalkeeper.ArretsPenalty} / {penaltyShotsFaced}" : string.Empty, "good"),
                positionProfile, "penalty_stops_per60"),
            WithPct(new PlayerSheetRowV2("Passes decisives", goalkeeper.PasseDecisives.ToString(), string.Empty, "good"),
                positionProfile, "assists_per60"),
            WithPct(new PlayerSheetRowV2("Buts", goalkeeper.Buts.ToString(), string.Empty, "positive"),
                positionProfile, "goalkeeper_goals_per60"),
        ];
    }

    /// <summary>Build defensive table rows for a goalkeeper with exact position-axis mappings.</summary>
    public static IReadOnlyList<PlayerSheetRowV2> BuildGoalkeeperDefensiveRows(
        PlayerGoalkeeperStatsDto goalkeeper,
        PlayerTechnicalStatsDto technical,
        PositionProfileResponseDto? positionProfile = null)
    {
        var concededGoals = goalkeeper.ButsPris + goalkeeper.ButsPenalty;

        return
        [
            WithPct(new PlayerSheetRowV2("Tirs subis", goalkeeper.TirsSubis.ToString(), string.Empty, "neutral"),
                positionProfile, "shots_faced_per60"),
            WithPct(new PlayerSheetRowV2("Buts encaisses", concededGoals.ToString(), $"{goalkeeper.ButsPris} jeu + {goalkeeper.ButsPenalty} sur 7m", "warning"),
                positionProfile, "goals_conceded_per60"),
            WithPct(new PlayerSheetRowV2("Pertes de balle", goalkeeper.PerteDeBalle.ToString(), string.Empty, goalkeeper.PerteDeBalle > 5 ? "warning" : "neutral"),
                positionProfile, "turnovers_per60"),
            WithPct(new PlayerSheetRowV2("Sanctions", technical.Sanctions.ToString(), string.Empty, technical.Sanctions > 3 ? "warning" : "neutral"),
                positionProfile, "sanctions_per60"),
        ];
    }

    // ── Percentile benchmark helpers ─────────────────────────────────────────

    /// <summary>
    /// Returns a badge for an API percentile that is already oriented in the favorable direction.
    /// A real top-10 rank replaces "Fort"; no rank is estimated on the client.
    /// </summary>
    public static (string Label, string Tone) PercentileToLabel(
        double percentile,
        int? rank = null,
        bool isReliable = true,
        bool isEligible = true,
        bool isEvaluative = true,
        bool hasPlayingTime = true)
    {
        return PositionBenchmarkHelper.FormatBadge(
            percentile,
            rank,
            isReliable,
            isEligible,
            isEvaluative,
            hasPlayingTime);
    }

    /// <summary>
    /// Enriches a row from one canonical API axis key. API percentiles are never inverted here.
    /// </summary>
    public static PlayerSheetRowV2 WithPct(
        PlayerSheetRowV2 row,
        PositionProfileResponseDto? positionProfile,
        string axisKey)
    {
        var axes = positionProfile?.SelectedPlayer?.Axes ?? [];
        var axis = axes.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, axisKey, StringComparison.OrdinalIgnoreCase));

        if (axis is null)
        {
            return axes.Count > 0
                ? row with { PercentileLabel = "—", PercentileTone = "neutral" }
                : row;
        }

        var (label, tone) = PercentileToLabel(
            axis.Percentile,
            axis.Rank,
            PositionBenchmarkHelper.IsCohortReliable(positionProfile),
            PositionBenchmarkHelper.IsSelectedPlayerEligible(positionProfile),
            axis.IsEvaluative,
            (positionProfile?.SelectedPlayer?.PlayingTimeMinutes ?? 0d) > 0d);

        var evidence = AppendBenchmarkEvidence(
            row.Evidence,
            axis,
            positionProfile?.SelectedPlayer?.PlayingTimeMinutes ?? 0d);
        return row with
        {
            Evidence = evidence,
            Tone = tone,
            PercentileLabel = label,
            PercentileTone = tone
        };
    }

    private static string AppendBenchmarkEvidence(
        string evidence,
        PositionProfileAxisDto axis,
        double playingTimeMinutes)
    {
        if (axis.Key.EndsWith("_per60", StringComparison.OrdinalIgnoreCase) && playingTimeMinutes <= 0d)
        {
            return string.IsNullOrWhiteSpace(evidence)
                ? "Temps N/D"
                : $"{evidence} · Temps N/D";
        }

        var benchmarkValue = axis.Key.EndsWith("_per60", StringComparison.OrdinalIgnoreCase)
            ? $"{axis.Value.ToString("0.##", CultureInfo.InvariantCulture)} /60"
            : string.Equals(axis.Format, "percent", StringComparison.OrdinalIgnoreCase)
                ? $"{axis.Value.ToString("0.#", CultureInfo.InvariantCulture)} %"
                : axis.Value.ToString("0.##", CultureInfo.InvariantCulture);

        return string.IsNullOrWhiteSpace(evidence)
            ? benchmarkValue
            : $"{evidence} · {benchmarkValue}";
    }

    private static bool HasMissingPlayingTime(PositionProfileResponseDto? positionProfile)
    {
        var selectedPlayer = positionProfile?.SelectedPlayer;
        return selectedPlayer is not null
            && selectedPlayer.PlayingTimeMinutes <= 0d
            && selectedPlayer.Axes.Any(axis => axis.Key.EndsWith("_per60", StringComparison.OrdinalIgnoreCase));
    }

    // ── Display formatting ────────────────────────────────────────────────────

    /// <summary>
    /// Format a percentage rate. Returns "—" when the denominator is zero.
    /// Never returns "0 %" for zero-denominator situations.
    /// </summary>
    public static string FormatRateOrDash(double rate, int denominator)
    {
        if (denominator == 0)
        {
            return "—";
        }

        return $"{rate.ToString("0.#", CultureInfo.InvariantCulture)} %";
    }

    /// <summary>Format a number with a given format string.</summary>
    public static string FormatNumber(double value, string format)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Shorten long API labels to human-readable short labels suitable for a radar chart.
    /// </summary>
    public static string HumanizeRadarLabel(string label)
    {
        if (label.Length <= 14)
        {
            return label;
        }

        return label
            .Replace("Buts jeu ouvert", "Buts jeu")
            .Replace("Passes decisives", "Passes")
            .Replace("Pertes de balle", "Pertes")
            .Replace("Neutralisations", "Neutralisa.")
            .Replace("Taux de tir", "Taux tir")
            .Replace("Sanctions obtenues", "Sanctions obt.")
            .Replace("Sanctions concedees", "Sanctions conc.")
            .Replace("7m concedes", "7m conc.")
            .Replace("7m obtenus", "7m obt.")
            .Replace("Passages en force", "PF provoques");
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    internal static bool IsOffensiveAxis(PositionProfileAxisDto axis, HashSet<string> categories, string[] keywords)
    {
        var cat = (axis.Category ?? string.Empty).ToLowerInvariant();
        if (categories.Contains(cat))
        {
            return true;
        }

        var combined = $"{axis.Key} {axis.Label}".ToLowerInvariant();
        return keywords.Any(kw => combined.Contains(kw));
    }

    internal static bool IsDefensiveAxis(PositionProfileAxisDto axis, string[] keywords)
    {
        var cat = (axis.Category ?? string.Empty).ToLowerInvariant();
        if (cat.Contains("def") || cat.Contains("discipline") || cat.Contains("sanction"))
        {
            return true;
        }

        var combined = $"{axis.Key} {axis.Label}".ToLowerInvariant();
        return keywords.Any(kw => combined.Contains(kw));
    }

    private static int GetRadarAxisSortRank(PositionProfileAxisDto axis)
    {
        var text = $"{axis.Category} {axis.Key} {axis.Label}".ToLowerInvariant();

        if (text.Contains("goalkeeper") || text.Contains("keeper") || text.Contains("save") || text.Contains("arret"))
        {
            return 4;
        }

        if (text.Contains("discip") || text.Contains("sanction") || text.Contains("penalty"))
        {
            return 3;
        }

        if (text.Contains("def") || text.Contains("interception") || text.Contains("block") || text.Contains("neutral"))
        {
            return 2;
        }

        if (text.Contains("pass") || text.Contains("assist") || text.Contains("create") || text.Contains("ball"))
        {
            return 1;
        }

        return 0;
    }
}
