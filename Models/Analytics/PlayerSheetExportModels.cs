using System.Globalization;
using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

// ── V2 Player Sheet Export Data Models ────────────────────────────────────────

/// <summary>Table row for the V2 player sheet: indicator + value + evidence (fraction or context).</summary>
public sealed record PlayerSheetRowV2(string Label, string Value, string Evidence, string Tone);

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
        if (allAxes.Count == 0)
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
                NormalizeRadarValue(axis, axis.Value),
                NormalizeRadarValue(axis, axis.MedianValue)))
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
        if (allAxes.Count == 0)
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
                    NormalizeRadarValue(axis, axis.Value),
                    NormalizeRadarValue(axis, axis.MedianValue)))
                .ToList();
        }

        var defKeywords = new[] { "interception", "block", "contre", "def", "sanction_conceded", "penalty_conceded", "7m_concede", "concede", "discipline", "prise" };
        return allAxes
            .Where(axis => axis is not null && !string.IsNullOrWhiteSpace(axis.Label))
            .Where(axis => IsDefensiveAxis(axis, defKeywords))
            .Take(6)
            .Select(axis => new PlayerSheetRadarAxis(
                HumanizeRadarLabel(axis.Label),
                NormalizeRadarValue(axis, axis.Value),
                NormalizeRadarValue(axis, axis.MedianValue)))
            .ToList();
    }

    // ── Normalization ─────────────────────────────────────────────────────────

    /// <summary>
    /// Normalize a raw axis value to 0-100 in the "favorable" direction.
    /// For HigherIsBetter=false metrics, the score is inverted so that a larger
    /// radar polygon always represents better performance.
    /// Falls back to the API-provided percentile when min/max are not meaningful.
    /// </summary>
    public static double NormalizeRadarValue(PositionProfileAxisDto axis, double value)
    {
        if (!double.IsFinite(axis.MinValue) || !double.IsFinite(axis.MaxValue) || axis.MaxValue <= axis.MinValue)
        {
            // The API percentile is already oriented: higher = favorable.
            return Math.Clamp(axis.Percentile, 0d, 100d);
        }

        var normalized = (value - axis.MinValue) * 100d / (axis.MaxValue - axis.MinValue);

        if (!axis.HigherIsBetter)
        {
            // Invert so that e.g. fewer turnovers → larger radar area.
            normalized = 100d - normalized;
        }

        return Math.Clamp(Math.Round(normalized, 1, MidpointRounding.AwayFromZero), 0d, 100d);
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
        bool sanctionsDrawnUnavailable = false)
    {
        var penShots = offense.Buts7m + offense.PenaltyRate;

        var penWonValue = penaltiesWon >= 0 ? penaltiesWon.ToString() : (penaltiesWonUnavailable ? "N/D" : "—");
        var sanctDrawnValue = sanctionsDrawn >= 0 ? sanctionsDrawn.ToString() : (sanctionsDrawnUnavailable ? "N/D" : "—");

        return
        [
            new PlayerSheetRowV2("Buts", global.TotalGoals.ToString(), $"{offense.Buts} jeu + {offense.Buts7m} sur 7m", "positive"),
            new PlayerSheetRowV2("Buts sur 7m", offense.Buts7m.ToString(), string.Empty, "positive"),
            new PlayerSheetRowV2("% tir 7m", FormatRateOrDash(technical.PenaltySuccessRate, penShots), penShots > 0 ? $"{offense.Buts7m} / {penShots}" : string.Empty, technical.PenaltySuccessRate >= 60 ? "positive" : "warning"),
            new PlayerSheetRowV2("Passes decisives", global.AssistCount.ToString(), string.Empty, "good"),
            new PlayerSheetRowV2("Pertes de balle", passing.TotalPertes.ToString(), string.Empty, passing.TotalPertes > 15 ? "warning" : "neutral"),
            new PlayerSheetRowV2("7m obtenus", penWonValue, string.Empty, "positive"),
            new PlayerSheetRowV2("Sanctions obtenues", sanctDrawnValue, string.Empty, "neutral"),
        ];
    }

    /// <summary>Build defensive table rows for a field player.</summary>
    public static IReadOnlyList<PlayerSheetRowV2> BuildDefensiveRows(
        PlayerProfileDto profile,
        PlayerDefenseStatsDto defense,
        PlayerSanctionStatsDto sanctions)
    {
        var totalSanctions = sanctions.Avertissements + sanctions.DeuxMinutes + sanctions.Exclusions;

        return
        [
            new PlayerSheetRowV2("Interceptions", defense.Interceptions.ToString(), string.Empty, "good"),
            new PlayerSheetRowV2("Contres", defense.Contres.ToString(), string.Empty, "good"),
            new PlayerSheetRowV2("7m concedes", sanctions.PenaltyConcede.ToString(), string.Empty, sanctions.PenaltyConcede > 3 ? "warning" : "neutral"),
            new PlayerSheetRowV2("Sanctions concedees", totalSanctions.ToString(), $"{sanctions.Avertissements} avert. · {sanctions.DeuxMinutes} x 2min · {sanctions.Exclusions} excl.", totalSanctions > 5 ? "danger" : "neutral"),
        ];
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
