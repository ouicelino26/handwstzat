namespace HandWStat.Models.Analytics;

public enum PlayerGamesWindow { Last5, Last10, Season }
public enum PlayerGamesResultFilter { All, Win, Draw, Loss }
public enum PlayerMatchResult { Win, Draw, Loss, Unknown }
public enum PlayerGamesDisplayMode { Values, DeltaVsSeason }

public sealed record MatchIdentity(
    string MatchId,
    DateTime Date,
    string Season,
    int Day,
    string CompetitionId,
    string CompetitionName,
    string PlayerTeamId,
    string PlayerTeamName,
    string OpponentId,
    string OpponentName,
    bool? IsHome,
    int? Team1Score,
    int? Team2Score,
    PlayerMatchResult Result
);

public sealed record GamePlayingTime(
    double? Minutes,
    PlayingTimeAvailability Availability
);

public sealed record GameFieldMetrics(
    int? Goals,
    int? Assists,
    int? ShotAttempts,
    int? ShotGoals,
    double? ShotRate,
    int? OpenPlayShotAttempts,
    int? OpenPlayShotGoals,
    double? OpenPlayShotRate,
    int? Interceptions,
    int? Blocks,
    int? Turnovers,
    int? Warnings,
    int? TwoMinutes,
    int? Disqualifications,
    int? PenaltiesConceded
);

public sealed record GameGoalkeeperMetrics(
    int? Saves,
    int? OpenPlaySaves,
    int? PenaltySaves,
    int? ShotsFaced,
    int? OpenPlayShotsFaced,
    int? PenaltyShotsFaced,
    int? GoalsConceded,
    int? PenaltyGoalsConceded,
    double? SaveRate,
    double? OpenPlaySaveRate,
    double? PenaltySaveRate,
    int? Assists,
    int? Turnovers
);

public sealed record PlayerGameAnalysisRow(
    MatchIdentity Identity,
    GamePlayingTime PlayingTime,
    GameFieldMetrics? FieldMetrics,
    GameGoalkeeperMetrics? GkMetrics
);

public sealed record GameSeasonBaseline(
    double? GoalsPerMatch,
    double? AssistsPerMatch,
    double? ShotRate,
    double? OpenPlayShotRate,
    double? InterceptionsPerMatch,
    double? TurnoversPerMatch,
    double? SavesPerMatch,
    double? SaveRate,
    double? OpenPlaySaveRate,
    double? PenaltySaveRate,
    double? ShotsFacedPerMatch,
    double? GoalsConcededPerMatch
);
