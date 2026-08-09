using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

/// <summary>
/// Converts a PlayerMatchItemDto into a PlayerGameAnalysisRow.
/// NOTE: PlayerMatchItemDto does not expose per-match shot attempt counts,
/// so ShotRate and SaveRate per match are null (DATA_MISSING) — they can only
/// be shown as raw count columns (Goals, Saves, etc.).
/// Playing time: PlayingTimeMinutes == 0 means missing data, not zero minutes.
/// Stat values: Goals/Assists/etc are int (not nullable) in the DTO.
/// A value of 0 from the DTO is treated as real zero (player participated).
/// </summary>
public static class PlayerGameRowBuilder
{
    private const double PlayingTimeMissingThreshold = 0.01;

    /// <summary>
    /// Builds a row from a match DTO.
    /// playerTeamId: the player's team ID as a string (from Profile.TeamId?.ToString()).
    /// </summary>
    public static PlayerGameAnalysisRow Build(PlayerMatchItemDto match, string? playerTeamId, bool isGoalkeeper)
    {
        var matchId = match.MatchId.ToString();
        var date = match.Date ?? DateTime.MinValue;
        var season = match.Season ?? string.Empty;
        var day = TryParseDay(match.Day);
        var competitionId = match.CompetitionId?.ToString() ?? string.Empty;
        var competitionName = match.CompetitionName ?? string.Empty;

        var team1Id = match.Team1Id?.ToString();
        var team2Id = match.Team2Id?.ToString();
        var team1Name = match.Team1Name ?? string.Empty;
        var team2Name = match.Team2Name ?? string.Empty;

        var isHome = PlayerMatchResultResolver.ResolveIsHome(playerTeamId, team1Id);
        var playerTeamName = isHome == true ? team1Name : (isHome == false ? team2Name : string.Empty);
        var opponentId = PlayerMatchResultResolver.ResolveOpponentId(playerTeamId, team1Id, team2Id) ?? string.Empty;
        var opponentName = PlayerMatchResultResolver.ResolveOpponentName(playerTeamId, team1Id, team1Name, team2Id, team2Name) ?? string.Empty;
        var result = PlayerMatchResultResolver.ResolveResult(playerTeamId, team1Id, match.Team1Score, match.Team2Score);

        var identity = new MatchIdentity(
            matchId, date, season, day,
            competitionId, competitionName,
            playerTeamId ?? string.Empty, playerTeamName,
            opponentId, opponentName,
            isHome,
            match.Team1Score, match.Team2Score,
            result
        );

        // Playing time: 0 means DataMissing
        var minutes = match.PlayingTimeMinutes > PlayingTimeMissingThreshold
            ? (double?)match.PlayingTimeMinutes
            : null;
        var ptAvailability = match.PlayingTimeMinutes > PlayingTimeMissingThreshold
            ? PlayingTimeAvailability.RecordedDirect
            : PlayingTimeAvailability.DataMissing;
        var playingTime = new GamePlayingTime(minutes, ptAvailability);

        if (isGoalkeeper)
        {
            // Goalkeeper: Saves in DTO — no shot attempts, no goals conceded per match
            var saves = match.Saves;
            // ShotsFaced = null because we don't have this per-match from DTO
            var gkMetrics = new GameGoalkeeperMetrics(
                Saves: saves,
                OpenPlaySaves: null,
                PenaltySaves: null,
                ShotsFaced: null,
                OpenPlayShotsFaced: null,
                PenaltyShotsFaced: null,
                GoalsConceded: null,
                PenaltyGoalsConceded: null,
                SaveRate: null,    // can't compute without ShotsFaced
                OpenPlaySaveRate: null,
                PenaltySaveRate: null,
                Assists: match.Assists,
                Turnovers: match.Turnovers
            );
            return new PlayerGameAnalysisRow(identity, playingTime, null, gkMetrics);
        }
        else
        {
            // Field player: Goals includes PenaltyGoals per DTO convention
            var totalGoals = match.Goals + match.PenaltyGoals;
            var fieldMetrics = new GameFieldMetrics(
                Goals: totalGoals,
                Assists: match.Assists,
                ShotAttempts: null,    // not available per match in DTO
                ShotGoals: null,       // not available per match in DTO
                ShotRate: null,        // DATA_MISSING — no per-match denominator
                OpenPlayShotAttempts: null,
                OpenPlayShotGoals: null,
                OpenPlayShotRate: null,
                Interceptions: match.Interceptions,
                Blocks: null,
                Turnovers: match.Turnovers,
                Warnings: null,
                TwoMinutes: null,
                Disqualifications: null,
                PenaltiesConceded: null
            );
            return new PlayerGameAnalysisRow(identity, playingTime, fieldMetrics, null);
        }
    }

    private static int TryParseDay(string? day)
    {
        if (string.IsNullOrWhiteSpace(day)) return 0;
        if (int.TryParse(day, out var parsed)) return parsed;
        return 0;
    }
}
