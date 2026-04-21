namespace HandWStat.Models.Analytics;

public sealed record MetricPlotPoint(string Label, double Value);

public sealed record ScoreTimelinePoint(
    string Label,
    double Minute,
    int Team1Score,
    int Team2Score,
    bool IsMarker = false,
    string? MarkerLabel = null);

public sealed record MatchTrendPoint(
    string Label,
    double DirectContributions,
    double DefensiveImpact,
    double Turnovers,
    double Saves,
    string? MatchLabel = null,
    string? SeasonDayLabel = null,
    string? TeamLabel = null,
    string? CompetitionLabel = null,
    string? OpponentLabel = null);

public sealed record MatchTimelineMoment(
    double Minute,
    string Title,
    string ClockLabel,
    string ScoreLabel,
    string Detail,
    string Tone = "neutral");

public sealed record MatchTimelineInsight(
    string Label,
    string Value,
    string Detail);
