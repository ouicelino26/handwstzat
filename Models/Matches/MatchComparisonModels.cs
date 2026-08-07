namespace HandWStat.Models.Matches;

public enum MetricDirection { HigherIsBetter, LowerIsBetter, Neutral }
public enum MetricAvailability { Available, DataMissing, ZeroDenominator, Partial }
public enum ComparisonFamily { Attack, Defense, Mastery }

public sealed record MatchComparisonMetric(
    string MetricCode,
    string Label,
    ComparisonFamily Family,
    MetricDirection Direction,
    double? Team1Value,
    double? Team2Value,
    int? Team1Numerator,
    int? Team1Denominator,
    int? Team2Numerator,
    int? Team2Denominator,
    MetricAvailability Availability,
    bool IsRate = false
);

public sealed record MatchComparisonSection(
    ComparisonFamily Family,
    string FamilyLabel,
    IReadOnlyList<MatchComparisonMetric> Metrics
);

public sealed record MatchContextKpi(
    string Label,
    string Value,
    string? SubLabel = null,
    bool IsAvailable = true
);

public sealed record MatchScenarioData(
    IReadOnlyList<MatchScenarioTimelinePoint> Timeline,
    MatchContextKpi? HalfTimeScore,
    MatchContextKpi MaxLead,
    MatchContextKpi LeadChanges,
    MatchContextKpi MeaningfulTies,
    MatchContextKpi TopRun,
    IReadOnlyList<MatchKeyMoment> KeyMoments
);

public sealed record MatchScenarioTimelinePoint(
    int Minute,
    int Second,
    int Team1Score,
    int Team2Score,
    string? ScoringTeamId,
    string? ScoringPlayerName
);

public sealed record MatchKeyMoment(
    int Minute,
    int Second,
    string Description,
    KeyMomentType Type
);

public enum KeyMomentType
{
    LeadChange,
    LateTie,
    BigRun,
    MaxLead,
    FinalMoment
}
