using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

/// <summary>
/// Single source of truth for favorable position-benchmark badges.
/// Percentiles received from the API are already oriented: a higher percentile is always better.
/// </summary>
public static class PositionBenchmarkHelper
{
    public const int DefaultMinimumCohortPlayerCount = 5;
    public const double DefaultMinimumPlayingTimeMinutes = 30d;

    public static (string Label, string Tone) Classify(double percentile)
    {
        var safePercentile = Math.Clamp(percentile, 0d, 100d);

        return safePercentile switch
        {
            >= 75d => ("Fort", "positive"),
            >= 55d => ("Bon", "good"),
            >= 35d => ("Moyen", "warning"),
            _ => ("Faible", "danger")
        };
    }

    public static (string Label, string Tone) FormatBadge(
        double percentile,
        int? rank,
        bool isReliable,
        bool isEligible,
        bool isEvaluative = true,
        bool hasPlayingTime = true)
    {
        if (!isEvaluative)
        {
            return ("Contexte", "neutral");
        }

        if (!isReliable)
        {
            return ("Éch. faible", "neutral");
        }

        if (!isEligible)
        {
            return hasPlayingTime
                ? ("Éch. faible", "neutral")
                : ("Temps N/D", "neutral");
        }

        var band = Classify(percentile);

        // A real API rank replaces "Fort" only for a genuinely strong top-10 result.
        return band.Label == "Fort" && rank is >= 1 and <= 10
            ? ($"#{rank.Value}", band.Tone)
            : band;
    }

    public static bool IsCohortReliable(PositionProfileResponseDto? profile)
    {
        if (profile is null)
        {
            return false;
        }

        var minimumCount = profile.MinimumCohortPlayerCount > 0
            ? profile.MinimumCohortPlayerCount
            : DefaultMinimumCohortPlayerCount;

        return profile.IsCohortReliable ?? profile.CohortPlayerCount >= minimumCount;
    }

    public static bool IsSelectedPlayerEligible(PositionProfileResponseDto? profile)
    {
        var selectedPlayer = profile?.SelectedPlayer;
        if (selectedPlayer is null)
        {
            return false;
        }

        var minimumMinutes = profile!.MinimumBenchmarkPlayingTimeMinutes > 0d
            ? profile.MinimumBenchmarkPlayingTimeMinutes
            : DefaultMinimumPlayingTimeMinutes;

        return selectedPlayer.IsBenchmarkEligible ?? selectedPlayer.PlayingTimeMinutes >= minimumMinutes;
    }
}
