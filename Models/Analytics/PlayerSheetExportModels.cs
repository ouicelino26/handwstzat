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
        bool sanctionsDrawnUnavailable = false,
        PositionProfileResponseDto? positionProfile = null)
    {
        var penShots = offense.Buts7m + offense.PenaltyRate;
        var penWonValue    = penaltiesWon >= 0   ? penaltiesWon.ToString()   : (penaltiesWonUnavailable   ? "N/D" : "—");
        var sanctDrawnValue = sanctionsDrawn >= 0 ? sanctionsDrawn.ToString() : (sanctionsDrawnUnavailable ? "N/D" : "—");

        var axes = positionProfile?.SelectedPlayer?.Axes ?? [];
        var cohortSize = positionProfile?.CohortPlayerCount ?? 0;

        return
        [
            // open_goals_per60 · label "Buts jeu /60"
            WithPct(new PlayerSheetRowV2("Buts", global.TotalGoals.ToString(), $"{offense.Buts} jeu + {offense.Buts7m} sur 7m", "positive"),
                axes, new[]{"open_goals_per60","open_goal","buts jeu","jeu /60","finition"}, cohortSize),
            // no axis for 7m goals specifically
            WithPct(new PlayerSheetRowV2("Buts sur 7m", offense.Buts7m.ToString(), string.Empty, "positive"),
                axes, new[]{"penalty_goal","7m_goal","but_7m","buts_7m"}, cohortSize),
            // no axis for 7m success rate — shows "—"
            WithPct(new PlayerSheetRowV2("% tir 7m", FormatRateOrDash(technical.PenaltySuccessRate, penShots), penShots > 0 ? $"{offense.Buts7m} / {penShots}" : string.Empty, technical.PenaltySuccessRate >= 60 ? "positive" : "warning"),
                axes, new[]{"penalty_success_rate","7m_success_rate","taux_7m_reussi"}, cohortSize),
            // assists_per60 · label "PD /60"
            WithPct(new PlayerSheetRowV2("Passes decisives", global.AssistCount.ToString(), string.Empty, "good"),
                axes, new[]{"assists_per60","assists_per","pd /60","passe","assist"}, cohortSize),
            // turnovers_per60 · label "PDB /60"
            WithPct(new PlayerSheetRowV2("Pertes de balle", passing.TotalPertes.ToString(), string.Empty, passing.TotalPertes > 15 ? "warning" : "neutral"),
                axes, new[]{"turnovers_per60","turnovers_per","pdb /60","turnover","perte"}, cohortSize),
            // penalties_won_per60 · label "7m obt. /60"
            WithPct(new PlayerSheetRowV2("7m obtenus", penWonValue, string.Empty, "positive"),
                axes, new[]{"penalties_won_per60","penalties_won","7m obt","pen_won","penalty_won"}, cohortSize),
            // sanctions_won_per60 · label "Sanctions obt. /60"
            WithPct(new PlayerSheetRowV2("Sanctions obtenues", sanctDrawnValue, string.Empty, "neutral"),
                axes, new[]{"sanctions_won_per60","sanctions_won","sanctions obt","sanction_won","sanction_drawn"}, cohortSize),
        ];
    }

    /// <summary>Build defensive table rows for a field player.</summary>
    public static IReadOnlyList<PlayerSheetRowV2> BuildDefensiveRows(
        PlayerProfileDto profile,
        PlayerDefenseStatsDto defense,
        PlayerSanctionStatsDto sanctions,
        PositionProfileResponseDto? positionProfile = null)
    {
        var totalSanctions = sanctions.Avertissements + sanctions.DeuxMinutes + sanctions.Exclusions;
        var axes = positionProfile?.SelectedPlayer?.Axes ?? [];
        var cohortSize = positionProfile?.CohortPlayerCount ?? 0;

        return
        [
            // interceptions_per60 · label "INTS /60"
            WithPct(new PlayerSheetRowV2("Interceptions", defense.Interceptions.ToString(), string.Empty, "good"),
                axes, new[]{"interceptions_per60","interceptions_per","ints /60","interception","intercept"}, cohortSize),
            // blocks_per60 · label "Contres /60"
            WithPct(new PlayerSheetRowV2("Contres", defense.Contres.ToString(), string.Empty, "good"),
                axes, new[]{"blocks_per60","blocks_per","contres /60","block","contre"}, cohortSize),
            // penalties_conceded_per60 · label "7m conc. /60"
            WithPct(new PlayerSheetRowV2("7m concedes", sanctions.PenaltyConcede.ToString(), string.Empty, sanctions.PenaltyConcede > 3 ? "warning" : "neutral"),
                axes, new[]{"penalties_conceded_per60","penalties_conceded","7m conc","pen_conc","penalty_conceded"}, cohortSize),
            // two_minutes_per60 · label "2 min /60"
            WithPct(new PlayerSheetRowV2("Sanctions concedees", totalSanctions.ToString(), $"{sanctions.Avertissements} avert. · {sanctions.DeuxMinutes} x 2min · {sanctions.Exclusions} excl.", totalSanctions > 5 ? "danger" : "neutral"),
                axes, new[]{"two_minutes_per60","two_minutes","2 min /60","two_min","discipline"}, cohortSize),
        ];
    }

    // ── Percentile benchmark helpers ─────────────────────────────────────────

    /// <summary>
    /// Returns (label, tone) for a 0-100 favorable-direction percentile.
    /// Si la taille de cohorte est connue et le rang estimé est ≤10 : affiche "#N".
    /// Sinon : Fort ≥75 · Bon ≥50 · Moyen ≥25 · Faible &lt;25.
    /// </summary>
    public static (string Label, string Tone) PercentileToLabel(double percentile, int cohortSize = 0)
    {
        if (cohortSize >= 10)
        {
            // Estimate rank: percentile 100 = rank 1, percentile 0 = rank cohortSize
            var estimatedRank = (int)Math.Round((100d - percentile) / 100d * cohortSize) + 1;
            if (estimatedRank <= 10)
                return ($"#{estimatedRank}", "positive");
        }
        return percentile switch
        {
            >= 75 => ("Fort",   "positive"),
            >= 50 => ("Bon",    "good"),
            >= 25 => ("Moyen",  "warning"),
            _      => ("Faible", "danger"),
        };
    }

    /// <summary>
    /// Finds the best-matching axis by key/label keywords and enriches the row with percentile label.
    /// Inverts the percentile for !HigherIsBetter axes so the badge always reads as favorable direction.
    /// Returns the row unchanged when no axis matches.
    /// </summary>
    public static PlayerSheetRowV2 WithPct(
        PlayerSheetRowV2 row,
        IReadOnlyList<PositionProfileAxisDto> axes,
        string[] keywords,
        int cohortSize = 0)
    {
        // Match by explicit keyword in Key or Label only — no fuzzy fallback
        var axis = axes.FirstOrDefault(a =>
            keywords.Any(k =>
                a.Key.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                a.Label.Contains(k, StringComparison.OrdinalIgnoreCase)));

        if (axis is null)
        {
            // Profile is loaded (axes not empty) but no axis covers this metric → neutral placeholder
            return axes.Count > 0
                ? row with { PercentileLabel = "—", PercentileTone = "neutral" }
                : row;
        }

        // Invert for negative-direction metrics so the badge reads in the favorable direction
        var effectivePercentile = axis.HigherIsBetter ? axis.Percentile : (100d - axis.Percentile);

        var (label, tone) = PercentileToLabel(effectivePercentile, cohortSize);
        return row with { PercentileLabel = label, PercentileTone = tone };
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
