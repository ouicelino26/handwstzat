using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public sealed record KpiTile(
    string Label,
    string Value,
    string Caption,
    string Tone = "neutral",
    string? Context = null);

public static class HandballKpiHelper
{
    public static double PerMatch(double total, int matches)
    {
        return matches > 0 ? total / matches : 0;
    }

    public static double Ratio(double numerator, double denominator)
    {
        if (denominator <= 0)
        {
            return numerator > 0 ? numerator : 0;
        }

        return numerator / denominator;
    }

    public static double Share(double numerator, double denominator)
    {
        return denominator > 0 ? (numerator / denominator) * 100d : 0;
    }

    public static double SuccessVsWasteShare(double successes, double failures)
    {
        var total = successes + failures;
        return total > 0 ? (successes / total) * 100d : 0;
    }

    public static int DefensiveImpact(PlayerDefenseStatsDto? defense)
    {
        if (defense is null)
        {
            return 0;
        }

        return defense.Interceptions + defense.Contres + defense.Neutralisations + defense.PassageForce;
    }

    public static int GoalkeeperStops(PlayerGoalkeeperStatsDto? goalkeeper)
    {
        if (goalkeeper is null)
        {
            return 0;
        }

        return goalkeeper.Arrets + goalkeeper.ArretsPenalty;
    }

    public static int TotalSanctions(PlayerSanctionStatsDto? sanctions)
    {
        if (sanctions is null)
        {
            return 0;
        }

        return sanctions.Exclusions + sanctions.Avertissements + sanctions.DeuxMinutes + sanctions.PenaltyConcede;
    }

    public static int ShotAttempts(PlayerOffenseStatsDto? offense)
    {
        if (offense is null)
        {
            return 0;
        }

        return offense.TotalButs + offense.TirsRates + offense.PenaltyRate + offense.TirContre;
    }

    public static int ShotWaste(PlayerOffenseStatsDto? offense)
    {
        if (offense is null)
        {
            return 0;
        }

        return offense.TirsRates + offense.PenaltyRate + offense.TirContre;
    }

    public static int PenaltyAttempts(PlayerOffenseStatsDto? offense)
    {
        if (offense is null)
        {
            return 0;
        }

        return offense.Buts7m + offense.PenaltyRate;
    }

    public static double OverallShotSuccessRate(PlayerOffenseStatsDto? offense)
    {
        if (offense is null)
        {
            return 0;
        }

        return Share(offense.TotalButs, ShotAttempts(offense));
    }

    public static int PassingActions(PlayerPassingStatsDto? passing)
    {
        if (passing is null)
        {
            return 0;
        }

        return passing.PasseDecisive + passing.TotalPertes;
    }

    public static int TechnicalLosses(PlayerPassingStatsDto? passing)
    {
        if (passing is null)
        {
            return 0;
        }

        return passing.MauvaisePasse + passing.PerteDeBalle + passing.FauteTechnique + passing.PassageEnForce;
    }

    public static double GoalkeeperPenaltyStopRate(PlayerGoalkeeperStatsDto? goalkeeper)
    {
        if (goalkeeper is null)
        {
            return 0;
        }

        return Share(goalkeeper.ArretsPenalty, goalkeeper.ArretsPenalty + goalkeeper.ButsPenalty);
    }

    public static int GoalkeeperConcededGoals(PlayerGoalkeeperStatsDto? goalkeeper)
    {
        if (goalkeeper is null)
        {
            return 0;
        }

        return goalkeeper.ButsPris + goalkeeper.ButsPenalty;
    }

    public static double TechnicalBalanceScore(
        PlayerGlobalStatsDto? player,
        PlayerOffenseStatsDto? offense,
        PlayerDefenseStatsDto? defense,
        PlayerPassingStatsDto? passing,
        PlayerSanctionStatsDto? sanctions,
        PlayerGoalkeeperStatsDto? goalkeeper)
    {
        var positive = DirectContributions(player) + DefensiveImpact(defense) + GoalkeeperStops(goalkeeper);
        var negative = ShotWaste(offense) + TechnicalLosses(passing) + TotalSanctions(sanctions) + GoalkeeperConcededGoals(goalkeeper);
        return SuccessVsWasteShare(positive, negative);
    }

    public static int DirectContributions(PlayerGlobalStatsDto? player)
    {
        if (player is null)
        {
            return 0;
        }

        return player.TotalGoals + player.AssistCount;
    }

    public static int DirectContributions(PlayerMatchItemDto? match)
    {
        if (match is null)
        {
            return 0;
        }

        return match.Goals + match.Assists;
    }

    public static int DefensiveImpact(PlayerMatchItemDto? match)
    {
        if (match is null)
        {
            return 0;
        }

        return match.Interceptions + match.Saves;
    }

    public static string FormatNumber(double value)
    {
        var rounded = Math.Round(value, 1, MidpointRounding.AwayFromZero);
        return Math.Abs(rounded - Math.Round(rounded, 0, MidpointRounding.AwayFromZero)) < 0.05
            ? Math.Round(rounded, 0, MidpointRounding.AwayFromZero).ToString("0")
            : rounded.ToString("0.0");
    }

    public static string FormatPercent(double value)
    {
        return $"{value:0.#}%";
    }

    public static string HigherIsBetterTone(double value, double positiveThreshold = 65d, double goodThreshold = 50d, double warningThreshold = 35d)
    {
        if (value >= positiveThreshold)
        {
            return "positive";
        }

        if (value >= goodThreshold)
        {
            return "good";
        }

        if (value >= warningThreshold)
        {
            return "warning";
        }

        return "danger";
    }

    public static string LowerIsBetterTone(double value, double positiveThreshold = 2d, double goodThreshold = 4d, double warningThreshold = 6d)
    {
        if (value <= positiveThreshold)
        {
            return "positive";
        }

        if (value <= goodThreshold)
        {
            return "good";
        }

        if (value <= warningThreshold)
        {
            return "warning";
        }

        return "danger";
    }

    public static string FieldSuccessRateTone(double value)
    {
        return HigherIsBetterTone(value, 70d, 55d, 45d);
    }

    public static string GoalkeeperSaveRateTone(double value)
    {
        return HigherIsBetterTone(value, 40d, 34d, 28d);
    }

    public static string GoalkeeperPenaltyStopRateTone(double value)
    {
        return HigherIsBetterTone(value, 35d, 25d, 15d);
    }

    public static string BallRetentionTone(double value, bool isGoalkeeper)
    {
        return isGoalkeeper
            ? HigherIsBetterTone(value, 60d, 45d, 30d)
            : HigherIsBetterTone(value, 70d, 55d, 40d);
    }

    public static string DirectActionsTone(double value, bool isGoalkeeper)
    {
        return isGoalkeeper
            ? HigherIsBetterTone(value, 3d, 2d, 1d)
            : HigherIsBetterTone(value, 5d, 4d, 3d);
    }

    public static string DefensiveImpactTone(double value, bool isGoalkeeper)
    {
        return isGoalkeeper
            ? HigherIsBetterTone(value, 10d, 7d, 4d)
            : HigherIsBetterTone(value, 5d, 3d, 1.5d);
    }

    public static string GoalkeeperStopsPerMatchTone(double value)
    {
        return HigherIsBetterTone(value, 12d, 9d, 6d);
    }

    public static string GoalkeeperConcededGoalsTone(double value)
    {
        return LowerIsBetterTone(value, 22d, 26d, 30d);
    }

    public static string FieldWasteTone(double value)
    {
        return LowerIsBetterTone(value, 1.5d, 3d, 5d);
    }

    public static string KeeperWasteTone(double value)
    {
        return LowerIsBetterTone(value, 0.75d, 1.5d, 2.5d);
    }

    public static string SanctionsTone(double value, bool isGoalkeeper)
    {
        return isGoalkeeper
            ? LowerIsBetterTone(value, 0.4d, 0.8d, 1.2d)
            : LowerIsBetterTone(value, 0.5d, 1d, 1.5d);
    }

    public static string TeamShotWasteTone(double value)
    {
        return LowerIsBetterTone(value, 12d, 16d, 20d);
    }

    public static string TeamWasteTone(double value)
    {
        return LowerIsBetterTone(value, 10d, 14d, 18d);
    }

    public static string TeamSanctionsTone(double value)
    {
        return LowerIsBetterTone(value, 2d, 4d, 6d);
    }

    public static string TechnicalBalanceTone(double value, bool isGoalkeeper)
    {
        return isGoalkeeper
            ? HigherIsBetterTone(value, 58d, 44d, 30d)
            : HigherIsBetterTone(value, 65d, 50d, 35d);
    }

    public static string TrendTone(double value)
    {
        if (value > 0)
        {
            return "positive";
        }

        if (value < 0)
        {
            return "warning";
        }

        return "neutral";
    }

    public static string FormatRatio(double value)
    {
        return value.ToString("0.00");
    }

    public static string FormatBase(double numerator, double denominator, string label)
    {
        return $"{FormatNumber(numerator)}/{FormatNumber(denominator)} {label}";
    }

    public static string FormatPerMatchContext(double total, int matches, string label)
    {
        var matchLabel = matches > 1 ? "matchs" : "match";
        return $"{FormatNumber(total)} {label} sur {matches:0} {matchLabel}";
    }

    public static string FormatSigned(double value)
    {
        return value > 0
            ? $"+{value:0.#}"
            : value.ToString("0.#");
    }
}
