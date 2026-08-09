using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

/// <summary>
/// Converts PlayerMatchItemDto records into PlayerTrajectoryPoint for a given metric.
/// NOTE: PlayerMatchItemDto does not include per-match shot attempt counts, so rate metrics
/// (ShotSuccessRate, OpenPlayShotSuccessRate, SaveRate, etc.) are marked DATA_MISSING per match.
/// They can only be shown as per-match counts (Goals, Saves, etc.).
/// </summary>
public static class TrajectoryPointBuilder
{
    private const double PlayingTimeMissingThreshold = 0.01; // below this → DataMissing

    public static IReadOnlyList<PlayerTrajectoryPoint> BuildPoints(
        IReadOnlyList<PlayerMatchItemDto> matches,
        string playerTeamName,
        string metricCode,
        bool isGoalkeeper)
    {
        if (matches.Count == 0) return [];

        return matches
            .Select(m => BuildPoint(m, playerTeamName, metricCode, isGoalkeeper))
            .OrderBy(p => p.Date)
            .ThenBy(p => p.MatchId)
            .ToList();
    }

    private static PlayerTrajectoryPoint BuildPoint(
        PlayerMatchItemDto match,
        string playerTeamName,
        string metricCode,
        bool isGoalkeeper)
    {
        var date = match.Date ?? DateTime.MinValue;
        var season = match.Season ?? string.Empty;
        var day = TryParseDay(match.Day);
        var competitionId = match.CompetitionId?.ToString() ?? string.Empty;
        var competitionName = match.CompetitionName ?? string.Empty;

        // Determine team/opponent
        var isHome = string.Equals(match.Team1Name?.Trim(), playerTeamName?.Trim(), StringComparison.OrdinalIgnoreCase);
        var teamId = isHome ? match.Team1Id?.ToString() ?? string.Empty : match.Team2Id?.ToString() ?? string.Empty;
        var teamName = isHome ? match.Team1Name ?? string.Empty : match.Team2Name ?? string.Empty;
        var opponentId = isHome ? match.Team2Id?.ToString() ?? string.Empty : match.Team1Id?.ToString() ?? string.Empty;
        var opponentName = isHome ? match.Team2Name ?? string.Empty : match.Team1Name ?? string.Empty;

        // Playing time
        var minutesPlayed = match.PlayingTimeMinutes > PlayingTimeMissingThreshold
            ? (double?)match.PlayingTimeMinutes
            : null;
        var playingTimeStatus = DeterminePlayingTimeStatus(match.PlayingTimeMinutes);

        // Result label
        var resultLabel = BuildResultLabel(match, isHome);

        // Metric value extraction
        var (metricValue, numerator, denominator, availability) = ExtractMetricValue(match, metricCode, isGoalkeeper);

        var sampleReliable = availability == "AVAILABLE";

        return new PlayerTrajectoryPoint(
            MatchId: match.MatchId.ToString(),
            Date: date,
            Season: season,
            Day: day,
            CompetitionId: competitionId,
            CompetitionName: competitionName,
            TeamId: teamId,
            TeamName: teamName,
            OpponentId: opponentId,
            OpponentName: opponentName,
            IsHome: isHome,
            MinutesPlayed: minutesPlayed,
            PlayingTimeStatus: playingTimeStatus,
            MetricValue: metricValue,
            Numerator: numerator,
            Denominator: denominator,
            SampleReliable: sampleReliable,
            Availability: availability,
            Goals: match.Goals + match.PenaltyGoals,
            Assists: match.Assists,
            Interceptions: match.Interceptions,
            Turnovers: match.Turnovers,
            Saves: match.Saves,
            ShotsFaced: match.Saves, // Only saves available per match; shots faced = proxy
            ResultLabel: resultLabel
        );
    }

    private static (double? value, double? numerator, double? denominator, string availability)
        ExtractMetricValue(PlayerMatchItemDto match, string metricCode, bool isGoalkeeper)
    {
        return metricCode switch
        {
            // Field player count metrics
            "GOALS_PER_MATCH" => ((double)(match.Goals + match.PenaltyGoals), null, null, "AVAILABLE"),
            "ASSISTS_PER_MATCH" => ((double)match.Assists, null, null, "AVAILABLE"),
            "INTERCEPTIONS_PER_MATCH" => ((double)match.Interceptions, null, null, "AVAILABLE"),
            "TURNOVERS_PER_MATCH" => ((double)match.Turnovers, null, null, "AVAILABLE"),
            "PENALTIES_WON_PER_MATCH" => (null, null, null, "DATA_MISSING"), // V2 only — no per-match data

            // Rate metrics: no per-match denominator available
            "SHOT_SUCCESS_RATE" => (null, null, null, "DATA_MISSING"),
            "OPEN_PLAY_SHOT_SUCCESS_RATE" => (null, null, null, "DATA_MISSING"),

            // Goalkeeper count metrics
            "SAVES_PER_MATCH" => ((double)match.Saves, null, null, "AVAILABLE"),
            "GOALS_CONCEDED_PER_MATCH" => (null, null, null, "DATA_MISSING"), // goals conceded not in DTO
            "SHOTS_FACED_PER_MATCH" => (null, null, null, "DATA_MISSING"),    // not in DTO

            // Goalkeeper rate metrics: no per-match denominator
            "SAVE_RATE" => (null, null, null, "DATA_MISSING"),
            "OPEN_PLAY_SAVE_RATE" => (null, null, null, "DATA_MISSING"),
            "PENALTY_SAVE_RATE" => (null, null, null, "DATA_MISSING"),

            // Shared goalkeeper/field
            "GK_ASSISTS_PER_MATCH" => ((double)match.Assists, null, null, "AVAILABLE"),
            "GK_TURNOVERS_PER_MATCH" => ((double)match.Turnovers, null, null, "AVAILABLE"),

            // Playing time
            "PLAYING_TIME" => match.PlayingTimeMinutes > 0.01
                ? ((double?)match.PlayingTimeMinutes, null, null, "AVAILABLE")
                : (null, null, null, "DATA_MISSING"),

            _ => (null, null, null, "DATA_MISSING")
        };
    }

    private static PlayingTimeAvailability DeterminePlayingTimeStatus(double minutes)
    {
        if (minutes < 0.01) return PlayingTimeAvailability.DataMissing;
        // We only have raw minutes — no availability signal in the DTO,
        // so treat any non-zero value as RecordedDirect.
        return PlayingTimeAvailability.RecordedDirect;
    }

    private static string? BuildResultLabel(PlayerMatchItemDto match, bool isHome)
    {
        var s1 = match.Team1Score;
        var s2 = match.Team2Score;
        if (s1 == null || s2 == null) return null;

        var myScore = isHome ? s1.Value : s2.Value;
        var theirScore = isHome ? s2.Value : s1.Value;
        var result = myScore > theirScore ? "V" : myScore < theirScore ? "D" : "N";
        return $"{result} {myScore}-{theirScore}";
    }

    private static int TryParseDay(string? day)
    {
        if (string.IsNullOrWhiteSpace(day)) return 0;
        if (int.TryParse(day, out var parsed)) return parsed;
        return 0;
    }
}
