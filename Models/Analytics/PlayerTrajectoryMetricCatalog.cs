namespace HandWStat.Models.Analytics;

public static class PlayerTrajectoryMetricCatalog
{
    // Métriques joueuse de champ
    public static readonly TrajectoryMetricDefinition GoalsPerMatch = new(
        "GOALS_PER_MATCH", "Buts / match", "buts", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.HigherIsBetter,
        IsGoalkeeperOnly: false, IsFieldPlayerOnly: true, MinimumSample: 3, Format: "0.0", AvailabilityRule: "Always");

    public static readonly TrajectoryMetricDefinition AssistsPerMatch = new(
        "ASSISTS_PER_MATCH", "Passes déc. / match", "passes", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.HigherIsBetter,
        false, true, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition ShotSuccessRate = new(
        "SHOT_SUCCESS_RATE", "Taux de tir", "%", TrajectoryMetricType.Rate, TrajectoryMetricDirection.HigherIsBetter,
        false, true, 5, "0.0", "Requires denominator > 0");

    public static readonly TrajectoryMetricDefinition OpenPlayShotSuccessRate = new(
        "OPEN_PLAY_SHOT_SUCCESS_RATE", "Taux dans le jeu", "%", TrajectoryMetricType.Rate, TrajectoryMetricDirection.HigherIsBetter,
        false, true, 5, "0.0", "Requires denominator > 0");

    public static readonly TrajectoryMetricDefinition InterceptionsPerMatch = new(
        "INTERCEPTIONS_PER_MATCH", "Interceptions / match", "int.", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.HigherIsBetter,
        false, true, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition TurnoversPerMatch = new(
        "TURNOVERS_PER_MATCH", "Pertes / match", "pertes", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.LowerIsBetter,
        false, true, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition PenaltiesWonPerMatch = new(
        "PENALTIES_WON_PER_MATCH", "7 m obtenus / match", "7m", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.HigherIsBetter,
        false, true, 3, "0.0", "V2 only");

    public static readonly TrajectoryMetricDefinition PlayingTime = new(
        "PLAYING_TIME", "Temps de jeu", "min", TrajectoryMetricType.Minutes, TrajectoryMetricDirection.Neutral,
        false, false, 1, "0", "Requires PlayingTimeStatus != DataMissing");

    // Métriques gardienne
    public static readonly TrajectoryMetricDefinition SavesPerMatch = new(
        "SAVES_PER_MATCH", "Arrêts / match", "arrêts", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.HigherIsBetter,
        true, false, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition SaveRate = new(
        "SAVE_RATE", "Taux d'arrêt", "%", TrajectoryMetricType.Rate, TrajectoryMetricDirection.HigherIsBetter,
        true, false, 5, "0.0", "Requires denominator > 0");

    public static readonly TrajectoryMetricDefinition OpenPlaySaveRate = new(
        "OPEN_PLAY_SAVE_RATE", "Taux d'arrêt dans le jeu", "%", TrajectoryMetricType.Rate, TrajectoryMetricDirection.HigherIsBetter,
        true, false, 5, "0.0", "Requires denominator > 0");

    public static readonly TrajectoryMetricDefinition PenaltySaveRate = new(
        "PENALTY_SAVE_RATE", "Taux d'arrêt 7 m", "%", TrajectoryMetricType.Rate, TrajectoryMetricDirection.HigherIsBetter,
        true, false, 3, "0.0", "Requires denominator > 0");

    public static readonly TrajectoryMetricDefinition ShotsFacedPerMatch = new(
        "SHOTS_FACED_PER_MATCH", "Tirs subis / match", "tirs", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.Neutral,
        true, false, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition GoalsConcededPerMatch = new(
        "GOALS_CONCEDED_PER_MATCH", "Buts encaissés / match", "buts", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.LowerIsBetter,
        true, false, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition GoalkeeperAssistsPerMatch = new(
        "GK_ASSISTS_PER_MATCH", "Passes déc. / match", "passes", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.HigherIsBetter,
        true, false, 3, "0.0", "Always");

    public static readonly TrajectoryMetricDefinition GoalkeeperTurnoversPerMatch = new(
        "GK_TURNOVERS_PER_MATCH", "Pertes / match", "pertes", TrajectoryMetricType.PerMatch, TrajectoryMetricDirection.LowerIsBetter,
        true, false, 3, "0.0", "Always");

    public static IReadOnlyList<TrajectoryMetricDefinition> GetForFieldPlayer() => new[]
    {
        GoalsPerMatch, AssistsPerMatch, ShotSuccessRate, OpenPlayShotSuccessRate,
        InterceptionsPerMatch, TurnoversPerMatch, PenaltiesWonPerMatch, PlayingTime
    };

    public static IReadOnlyList<TrajectoryMetricDefinition> GetForGoalkeeper() => new[]
    {
        SavesPerMatch, SaveRate, OpenPlaySaveRate, PenaltySaveRate,
        ShotsFacedPerMatch, GoalsConcededPerMatch, GoalkeeperAssistsPerMatch,
        GoalkeeperTurnoversPerMatch, PlayingTime
    };

    public static IReadOnlyList<TrajectoryMetricDefinition> GetFor(bool isGoalkeeper) =>
        isGoalkeeper ? GetForGoalkeeper() : GetForFieldPlayer();

    public static TrajectoryMetricDefinition? Find(string code) =>
        GetForFieldPlayer().Concat(GetForGoalkeeper()).FirstOrDefault(m => m.Code == code);
}
