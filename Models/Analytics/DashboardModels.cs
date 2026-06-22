using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public sealed record DashboardSnapshot
{
    public required OverviewMetrics Overview { get; init; }

    public required IReadOnlyList<PlayerDirectoryItem> Players { get; init; }

    public DashboardGlobalBoards GlobalBoards { get; init; } = DashboardGlobalBoards.Empty;

    public required IReadOnlyList<PlayerRankingItem> TopScorers { get; init; }

    public required IReadOnlyList<PlayerRankingItem> EfficiencyRanking { get; init; }

    public required IReadOnlyList<PlayerRankingItem> RequestedRanking { get; init; }

    public required IReadOnlyList<PlayerRankingItem> InterceptionRanking { get; init; }

    public required IReadOnlyList<MatchRecap> RecentMatches { get; init; }

    public required PlayerSpotlight Spotlight { get; init; }

    public required string RequestedRankingLabel { get; init; }

    public string DataOrigin { get; init; } = "Apercu";

    public string? WarningMessage { get; init; }

    public bool IsDemo { get; init; }
}

public sealed record DashboardGlobalBoards(
    IReadOnlyList<GlobalFieldRankingRow> FieldPlayers,
    IReadOnlyList<GlobalGoalkeeperRankingRow> Goalkeepers)
{
    public static DashboardGlobalBoards Empty { get; } = new([], []);
}

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

public sealed record GlobalFieldRankingRow(
    int PlayerId,
    string FullName,
    string TeamName,
    int? PositionId,
    string PositionLabel,
    bool IsGoalkeeper,
    int MatchesPlayed,
    int Goals,
    int PenaltyGoals,
    int Assists,
    int Interceptions,
    int Blocks,
    int Neutralisations,
    int Turnovers,
    int PenaltiesConceded,
    double ShotSuccessRate);

public sealed record GlobalGoalkeeperRankingRow(
    int PlayerId,
    string FullName,
    string TeamName,
    int? PositionId,
    string PositionLabel,
    int MatchesPlayed,
    int Goals,
    int Assists,
    int Saves,
    int PenaltySaves,
    double SaveRate,
    int GoalsConceded,
    int PenaltyGoalsConceded,
    int ShotsFaced,
    double ShotSuccessRate,
    int Turnovers);

public sealed record PlayerSpotlight
{
    public required PlayerProfileDto Profile { get; init; }

    public required PlayerGlobalStatsDto Global { get; init; }

    public required PlayerOffenseStatsDto Offense { get; init; }

    public required PlayerDefenseStatsDto Defense { get; init; }

    public required PlayerPassingStatsDto Passing { get; init; }

    public required PlayerSanctionStatsDto Sanctions { get; init; }

    public required PlayerGoalkeeperStatsDto Goalkeeper { get; init; }

    public required PlayerTechnicalStatsDto Technical { get; init; }

    public required IReadOnlyList<PlayerMatchItemDto> Matches { get; init; }

    public required IReadOnlyList<ZoneStat> GoalZones { get; init; }

    public required IReadOnlyList<ZoneStat> TriggerZones { get; init; }

    public required IReadOnlyList<SliceValue> Distribution { get; init; }

    public int PlayerId => Profile.PlayerId;

    public string FullName => Profile.FullName;

    public string TeamName => Profile.TeamName ?? "Equipe non renseignee";

    public string PositionName => Profile.PositionName ?? "Poste non renseigne";

    public string? CountryName => Profile.Nationality;

    public int? Age => Profile.Age;

    public bool IsGoalkeeper => Profile.IsGoalkeeper;

    public int MatchesPlayed => Global.MatchesPlayed;

    public int TotalGoals => Global.TotalGoals;

    public int PenaltyGoals => Offense.Buts7m;

    public int MissedShots => Offense.TirsRates;

    public int ShotAttempts => Technical.ShotAttempts;

    public int ShotWaste => Technical.ShotWaste;

    public int Assists => Passing.PasseDecisive;

    public int Turnovers => Passing.TotalPertes;

    public int TechnicalLosses => Technical.TechnicalLosses;

    public int DefensiveStops => Technical.DefensiveImpact;

    public int Saves => Technical.GoalkeeperStops;

    public double ShootingRate => Technical.OpenShotSuccessRate;

    public double OverallShotSuccessRate => Technical.OverallShotSuccessRate;

    public double PenaltyRate => Technical.PenaltySuccessRate;

    public double PenaltyStopRate => Technical.GoalkeeperPenaltyStopRate;

    public int GoalkeeperConcededGoals => Technical.GoalkeeperConcededGoals + Technical.GoalkeeperPenaltyConcededGoals;
}

public sealed record ZoneStat(
    string Key,
    string Label,
    double Rate,
    int Attempts,
    int Successes,
    IReadOnlyList<OutcomeCount> Outcomes)
{
    public int Failures => Math.Max(Attempts - Successes, 0);
}

public sealed record OutcomeCount(string Label, int Count);

public sealed record PlayerRankingItem(
    string PlayerName,
    string TeamName,
    double Value,
    string ValueLabel,
    int? PlayerId = null,
    string? Metric = null);

public sealed record MatchRecap(
    int MatchId,
    string Poster,
    string Score,
    string DayLabel,
    string SeasonDayLabel);

public sealed record SliceValue(string Label, int Value);
