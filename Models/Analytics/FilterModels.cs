using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public sealed class DashboardFilterState
{
    public int? CompetitionId { get; set; }

    public int? TeamId { get; set; }

    public int? PositionId { get; set; }

    public int? MatchId { get; set; }

    public int? Year { get; set; }

    public string? Season { get; set; }

    public string? Day { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int? AttackId { get; set; }

    public int? DefenseId { get; set; }

    public string? Trigger { get; set; }

    public string? ShootShade { get; set; }

    public string RankingMetric { get; set; } = "goals";

    public int Top { get; set; } = 8;

    public int? SpotlightPlayerId { get; set; }

    public StatsQueryOptionsDto ToStatsQueryOptions()
    {
        return new StatsQueryOptionsDto
        {
            CompetitionId = CompetitionId,
            TeamId = TeamId,
            PositionId = PositionId,
            MatchId = MatchId,
            Year = Year,
            Season = Season,
            Day = Day,
            From = From,
            To = To,
            AttackId = AttackId,
            DefenseId = DefenseId,
            Trigger = Trigger,
            ShootShade = ShootShade
        };
    }
}

public sealed record AnalyticsReferenceData(
    IReadOnlyList<CompetitionDto> Competitions,
    IReadOnlyList<TeamDto> Teams,
    IReadOnlyList<LookupItemDto> Positions,
    IReadOnlyList<LookupItemDto> Events,
    IReadOnlyList<LookupItemDto> Attacks,
    IReadOnlyList<LookupItemDto> Defenses,
    IReadOnlyList<LookupItemDto> Nationalities)
{
    public static AnalyticsReferenceData Empty { get; } = new(
        [],
        [],
        [],
        [],
        [],
        [],
        []);
}

public sealed record RankingMetricOption(string Value, string Label);

public static class RankingMetricCatalog
{
    public static IReadOnlyList<RankingMetricOption> Default { get; } =
    [
        new("goals", "Buts"),
        new("saves", "Arrets"),
        new("shotsuccess", "Taux reussite tir"),
        new("assists", "Passes decisives"),
        new("interceptions", "Interceptions"),
        new("saverate", "Taux d'arret"),
        new("turnovers", "Pertes de balle"),
        new("sanctions", "Sanctions"),
        new("penaltysuccess", "Taux penalty")
    ];
}
