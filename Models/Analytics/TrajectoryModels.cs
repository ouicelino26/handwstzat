namespace HandWStat.Models.Analytics;

// Direction métrique
public enum TrajectoryMetricDirection { HigherIsBetter, LowerIsBetter, Neutral }

// Type de métrique
public enum TrajectoryMetricType { Count, PerMatch, Rate, Minutes, Score }

// Fenêtre temporelle
public enum TrajectoryWindow { Last5, Last10, Season }

// Scène graphique
public enum TrajectoryScene { Curve, ScatterImpact }

// État de tendance
public enum TrendState { Progressing, Stable, Declining, InsufficientData }

// Disponibilité temps de jeu
public enum PlayingTimeAvailability
{
    RecordedDirect,
    RecordedHistoricalId,
    MatchedStrongIdentity,
    MatchedUniqueMatchRoster,
    DerivedFromSubstitutions,
    PartialData,
    DataMissing,
    IdentityConflict
}

// Définition d'une métrique de trajectoire
public sealed record TrajectoryMetricDefinition(
    string Code,
    string Label,
    string Unit,
    TrajectoryMetricType Type,
    TrajectoryMetricDirection Direction,
    bool IsGoalkeeperOnly,
    bool IsFieldPlayerOnly,
    int MinimumSample,
    string Format,
    string AvailabilityRule
);

// Un point de trajectoire (un match)
public sealed record PlayerTrajectoryPoint(
    string MatchId,
    DateTime Date,
    string Season,
    int Day,
    string CompetitionId,
    string CompetitionName,
    string TeamId,
    string TeamName,
    string OpponentId,
    string OpponentName,
    bool IsHome,
    double? MinutesPlayed,
    PlayingTimeAvailability PlayingTimeStatus,
    double? MetricValue,
    double? Numerator,
    double? Denominator,
    bool SampleReliable,
    string Availability,
    int Goals,
    int Assists,
    int Interceptions,
    int Turnovers,
    int Saves,
    int? ShotsFaced,
    string? ResultLabel
);

// Résumé d'une fenêtre temporelle
public sealed record TrajectoryWindowSummary(
    TrajectoryWindow Window,
    double? AggregatedValue,
    double? SeasonValue,
    double? DeltaVsSeason,
    string DeltaLabel,
    int EligibleMatches,
    int TotalMatchesInWindow,
    TrendState Trend
);
