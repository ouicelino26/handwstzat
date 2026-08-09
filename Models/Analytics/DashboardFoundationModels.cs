namespace HandWStat.Models.Analytics;

public sealed record OverviewMetrics(
    int PlayerCount,
    int TeamCount,
    int MatchCount,
    int EventCount,
    int GoalCount,
    int AssistCount,
    int InterceptionCount,
    int SaveCount,
    int TurnoverCount,
    int SanctionCount);

public sealed record PlayerDirectoryItem(
    int Id,
    string FullName,
    string TeamName,
    string PositionName,
    string? CountryName,
    int? Age,
    bool IsGoalkeeper);
