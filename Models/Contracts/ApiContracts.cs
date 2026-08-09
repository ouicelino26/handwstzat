namespace HandWStat.Models.Contracts;

// ──────────────────────────────────────────────────────────────────────────────
// Query options
// ──────────────────────────────────────────────────────────────────────────────

public class StatsQueryOptionsDto
{
    public int? CompetitionId { get; set; }
    public int? TeamId { get; set; }
    public int? PlayerId { get; set; }
    public List<int> PlayerIds { get; set; } = [];
    public int? PositionId { get; set; }
    public int? MatchId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Year { get; set; }
    public string? Season { get; set; }
    public string? Day { get; set; }
    public int? AttackId { get; set; }
    public int? DefenseId { get; set; }
    public string? Trigger { get; set; }
    public string? ShootShade { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// Player list / history
// ──────────────────────────────────────────────────────────────────────────────

public class PlayerTeamHistoryDto
{
    public int? TeamId { get; set; }
    public string? TeamCode { get; set; }
    public string? TeamName { get; set; }
    public int MatchesPlayed { get; set; }
    public int EventCount { get; set; }
    public double PlayingTimeMinutes { get; set; }
}

public class PlayerListItemDto
{
    public int PlayerId { get; set; }
    public int Id { get => PlayerId; set => PlayerId = value; }
    public string FullName { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string? PlayerPhoto { get => Photo; set => Photo = value; }
    public int? TeamId { get; set; }
    public string? TeamCode { get; set; }
    public string? TeamName { get; set; }
    public int? PositionId { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? Nationality { get; set; }
    public int? Age { get; set; }
    public int? Number { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsGoalkeeper { get; set; }
    public bool IsActive { get; set; }
    public List<PlayerTeamHistoryDto> TeamHistory { get; set; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// Player stats DTOs
// ──────────────────────────────────────────────────────────────────────────────

public class TechnicalStatsDto
{
    public int ShotAttempts { get; set; }
    public int ShotWaste { get; set; }
    public int PenaltyAttempts { get; set; }
    public int TechnicalLosses { get; set; }
    public int DefensiveImpact { get; set; }
    public int GoalkeeperStops { get; set; }
    public int GoalkeeperPenaltyStops { get; set; }
    public int GoalkeeperConcededGoals { get; set; }
    public int GoalkeeperPenaltyConcededGoals { get; set; }
    public int TirsSubis { get; set; }
    public int Sanctions { get; set; }
    public double OpenShotSuccessRate { get; set; }
    public double OverallShotSuccessRate { get; set; }
    public double PenaltySuccessRate { get; set; }
    public double GoalkeeperSaveRate { get; set; }
    public double GoalkeeperPenaltyStopRate { get; set; }
}

public class PlayerGlobalStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int MatchesWithPlayingTime { get; set; }
    public int GoalCount { get; set; }
    public int PenaltyGoalCount { get; set; }
    public int TotalGoals { get; set; }
    public int AssistCount { get; set; }
    public int InterceptionCount { get; set; }
    public int SaveCount { get; set; }
    public int TurnoverCount { get; set; }
    public int SanctionCount { get; set; }
    public int OpenShotAttempts { get; set; }
    public int ShotAttempts { get; set; }
    public int PenaltyAttempts { get; set; }
    public int ShotsFaced { get; set; }
    public double PlayingTimeMinutes { get; set; }
    public double AveragePlayingTimePerMatchMinutes { get; set; }
    public double GoalsPer60 { get; set; }
    public double AssistsPer60 { get; set; }
    public double InterceptionsPer60 { get; set; }
    public double SavesPer60 { get; set; }
    public double TurnoversPer60 { get; set; }
    public double SanctionsPer60 { get; set; }
    public double ShotSuccessRate { get; set; }
    public double PenaltySuccessRate { get; set; }
    public double GoalkeeperSaveRate { get; set; }
}

public class PlayerOffenseStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int Buts { get; set; }
    public int Buts7m { get; set; }
    public int TotalButs { get; set; }
    public int TirsRates { get; set; }
    public int PenaltyRate { get; set; }
    public int TirContre { get; set; }
    public double TauxReussiteTir { get; set; }
    public double TauxReussitePenalty { get; set; }
}

public class PlayerDefenseStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int Interceptions { get; set; }
    public int Contres { get; set; }
    public int Neutralisations { get; set; }
    public int PassageForce { get; set; }
}

public class PlayerPassingStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int PasseDecisive { get; set; }
    public int MauvaisePasse { get; set; }
    public int PerteDeBalle { get; set; }
    public int FauteTechnique { get; set; }
    public int PassageEnForce { get; set; }
    public int TotalPertes { get; set; }
}

public class PlayerSanctionStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int Exclusions { get; set; }
    public int Avertissements { get; set; }
    public int DeuxMinutes { get; set; }
    public int PenaltyConcede { get; set; }
}

public class PlayerGoalkeeperStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int Arrets { get; set; }
    public int ArretsPenalty { get; set; }
    public int ButsPris { get; set; }
    public int ButsPenalty { get; set; }
    public int Buts { get; set; }
    public int TirContre { get; set; }
    public int PerteDeBalle { get; set; }
    public int TirsRates { get; set; }
    public int Tirs { get; set; }
    public int TirsLoupes { get; set; }
    public int PasseDecisives { get; set; }
    public int MauvaisePasse { get; set; }
    public int TirsSubis { get; set; }
    public double TauxReussiteTir { get; set; }
    public double TauxArret { get; set; }
}

public class PlayerTechnicalStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public TechnicalStatsDto Technical { get; set; } = new();

    public int ShotAttempts { get => Technical.ShotAttempts; set => Technical.ShotAttempts = value; }
    public int ShotWaste { get => Technical.ShotWaste; set => Technical.ShotWaste = value; }
    public int PenaltyAttempts { get => Technical.PenaltyAttempts; set => Technical.PenaltyAttempts = value; }
    public int TechnicalLosses { get => Technical.TechnicalLosses; set => Technical.TechnicalLosses = value; }
    public int DefensiveImpact { get => Technical.DefensiveImpact; set => Technical.DefensiveImpact = value; }
    public int GoalkeeperStops { get => Technical.GoalkeeperStops; set => Technical.GoalkeeperStops = value; }
    public int GoalkeeperPenaltyStops { get => Technical.GoalkeeperPenaltyStops; set => Technical.GoalkeeperPenaltyStops = value; }
    public int GoalkeeperConcededGoals { get => Technical.GoalkeeperConcededGoals; set => Technical.GoalkeeperConcededGoals = value; }
    public int GoalkeeperPenaltyConcededGoals { get => Technical.GoalkeeperPenaltyConcededGoals; set => Technical.GoalkeeperPenaltyConcededGoals = value; }
    public int TirsSubis { get => Technical.TirsSubis; set => Technical.TirsSubis = value; }
    public int Sanctions { get => Technical.Sanctions; set => Technical.Sanctions = value; }
    public double OpenShotSuccessRate { get => Technical.OpenShotSuccessRate; set => Technical.OpenShotSuccessRate = value; }
    public double OverallShotSuccessRate { get => Technical.OverallShotSuccessRate; set => Technical.OverallShotSuccessRate = value; }
    public double PenaltySuccessRate { get => Technical.PenaltySuccessRate; set => Technical.PenaltySuccessRate = value; }
    public double GoalkeeperSaveRate { get => Technical.GoalkeeperSaveRate; set => Technical.GoalkeeperSaveRate = value; }
    public double GoalkeeperPenaltyStopRate { get => Technical.GoalkeeperPenaltyStopRate; set => Technical.GoalkeeperPenaltyStopRate = value; }
}

// ──────────────────────────────────────────────────────────────────────────────
// Rankings & overview
// ──────────────────────────────────────────────────────────────────────────────

public class RankingItemDto : PlayerListItemDto
{
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public double? SecondaryValue { get; set; }
    public int MatchesPlayed { get; set; }
}

public class StatsOverviewDto
{
    public int MatchCount { get; set; }
    public int PlayerCount { get; set; }
    public int TeamCount { get; set; }
    public int GoalCount { get; set; }
    public int PenaltyGoalCount { get; set; }
    public int AssistCount { get; set; }
    public int InterceptionCount { get; set; }
    public int SaveCount { get; set; }
    public int TurnoverCount { get; set; }
    public int SanctionCount { get; set; }
    public List<RankingItemDto> TopScorers { get; set; } = [];
    public List<RankingItemDto> TopInterceptions { get; set; } = [];
    public List<RankingItemDto> TopShotEfficiency { get; set; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// Match DTOs
// ──────────────────────────────────────────────────────────────────────────────

public class MatchListItemDto
{
    public int MatchId { get; set; }
    public int? CompetitionId { get; set; }
    public string? CompetitionCode { get; set; }
    public string? CompetitionName { get; set; }
    public DateTime? Date { get; set; }
    public int? Year { get; set; }
    public string? Season { get; set; }
    public string? Day { get; set; }
    public int? Team1Id { get; set; }
    public string? Team1Code { get; set; }
    public string? Team1Name { get; set; }
    public int? Team2Id { get; set; }
    public string? Team2Code { get; set; }
    public string? Team2Name { get; set; }
    public int? Team1Score { get; set; }
    public int? Team2Score { get; set; }
}

public class MatchSummaryDto : MatchListItemDto
{
    public int EventCount { get; set; }
    public int GoalCount { get; set; }
    public int PenaltyGoalCount { get; set; }
    public int AssistCount { get; set; }
    public int InterceptionCount { get; set; }
    public int SaveCount { get; set; }
    public int TurnoverCount { get; set; }
    public int SanctionCount { get; set; }
    public TechnicalStatsDto Technical { get; set; } = new();
    public List<RankingItemDto> TopScorers { get; set; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// Event analytics
// ──────────────────────────────────────────────────────────────────────────────

public class PagedEventAnalyticsDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<MatchEventAnalyticsDto> Items { get; set; } = [];
}

public class MatchEventAnalyticsDto
{
    public int MatchEventId { get; set; }
    public int MatchId { get; set; }
    public DateTime? MatchDate { get; set; }
    public int? MatchYear { get; set; }
    public string? MatchSeason { get; set; }
    public string? MatchDay { get; set; }
    public int? CompetitionId { get; set; }
    public string? CompetitionCode { get; set; }
    public string? CompetitionName { get; set; }
    public int? Team1Id { get; set; }
    public string? Team1Name { get; set; }
    public int? Team2Id { get; set; }
    public string? Team2Name { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerFullName { get; set; }
    public string? PlayerPhoto { get; set; }
    public int? PlayerTeamId { get; set; }
    public string? PlayerTeamName { get; set; }
    public int? PositionId { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? Nationality { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public int? AttackId { get; set; }
    public string? AttackName { get; set; }
    public int? DefenseId { get; set; }
    public string? DefenseName { get; set; }
    public string? Action { get; set; }
    public string? ShootZone { get; set; }
    public string? Shade { get; set; }
    public string? ShootShade { get; set; }
    public string? Trigger { get; set; }
    public string? ArmSide { get; set; }
    public string? Jump { get; set; }
    public bool? Goal { get; set; }
    public string? MiTemps { get; set; }
    public TimeSpan? Time { get; set; }
    public int? TeamScore1 { get; set; }
    public int? TeamScore2 { get; set; }
    public int? ScoreDifference { get; set; }
    public string? AttackSituationCode { get; set; }
    public string? AttackSituationLabel { get; set; }
    public string? ScoreStateCode { get; set; }
    public string? ScoreStateLabel { get; set; }
}

public class EventContextSplitDto
{
    public string ContextCode { get; set; } = string.Empty;
    public string ContextLabel { get; set; } = string.Empty;
    public int Events { get; set; }
    public int MatchesPlayed { get; set; }
    public int Goals { get; set; }
    public int PenaltyGoals { get; set; }
    public int ShotMisses { get; set; }
    public int PenaltyMisses { get; set; }
    public int ShotBlocks { get; set; }
    public int ShotAttempts { get; set; }
    public int TechnicalLosses { get; set; }
    public int DefensiveImpact { get; set; }
    public int Saves { get; set; }
    public int GoalkeeperSaves { get; set; }
    public int GoalkeeperPenaltySaves { get; set; }
    public int GoalkeeperConcededGoals { get; set; }
    public int GoalkeeperPenaltyConcededGoals { get; set; }
    public int Sanctions { get; set; }
    public double AverageScoreDifference { get; set; }
    public double ShotSuccessRate { get; set; }
    public double OverallShotSuccessRate { get; set; }
    public double PenaltySuccessRate { get; set; }
    public double GoalkeeperSaveRate { get; set; }
}

public class EventContextBreakdownDto
{
    public List<EventContextSplitDto> AttackSituations { get; set; } = [];
    public List<EventContextSplitDto> AttackSystems { get; set; } = [];
    public List<EventContextSplitDto> DefenseStructures { get; set; } = [];
    public List<EventContextSplitDto> ScoreStates { get; set; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// Team & compare
// ──────────────────────────────────────────────────────────────────────────────

public class TeamStatsDto
{
    public int TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamCode { get; set; }
    public int MatchesPlayed { get; set; }
    public int PlayersWithPlayingTime { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double PlayingTimeMinutes { get; set; }
    public double AveragePlayingTimePerMatchMinutes { get; set; }
    public double AveragePlayingTimePerPlayerMinutes { get; set; }
    public StatsOverviewDto Overview { get; set; } = new();
    public TechnicalStatsDto Technical { get; set; } = new();
}

public class ComparePlayersRequestDto
{
    public List<int> PlayerIds { get; set; } = [];
    public int? CompetitionId { get; set; }
    public int? TeamId { get; set; }
    public int? PositionId { get; set; }
    public int? MatchId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Year { get; set; }
    public string? Season { get; set; }
    public string? Day { get; set; }
}

public class ComparePlayersResponseDto
{
    public List<PlayerGlobalStatsDto> Players { get; set; } = [];
    public List<PlayerTechnicalStatsDto> Technical { get; set; } = [];
    public List<PlayerOffenseStatsDto> Offense { get; set; } = [];
    public List<PlayerDefenseStatsDto> Defense { get; set; } = [];
    public List<PlayerPassingStatsDto> Passing { get; set; } = [];
    public List<PlayerSanctionStatsDto> Sanctions { get; set; } = [];
    public List<PlayerGoalkeeperStatsDto> Goalkeeper { get; set; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// Spatial stats
// ──────────────────────────────────────────────────────────────────────────────

public class ZoneOutcomeDto
{
    public string? EventName { get; set; }
    public int Count { get; set; }
}

public class ZoneStatDto
{
    public string? ZoneCode { get; set; }
    public int Attempts { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public List<ZoneOutcomeDto> Outcomes { get; set; } = [];
}

public class TriggerZoneStatDto
{
    public string? TriggerCode { get; set; }
    public int Attempts { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public List<ZoneOutcomeDto> Outcomes { get; set; } = [];
}

public class PlayerSpatialStatsDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public List<ZoneStatDto> Zones { get; set; } = [];
    public List<TriggerZoneStatDto> Triggers { get; set; } = [];
    public List<ZoneStatDto> EventsByZone { get; set; } = [];
}

public class MatchSpatialStatsDto
{
    public int MatchId { get; set; }
    public List<ZoneStatDto> Zones { get; set; } = [];
    public List<TriggerZoneStatDto> Triggers { get; set; } = [];
    public List<ZoneStatDto> EventsByZone { get; set; } = [];
}

// ──────────────────────────────────────────────────────────────────────────────
// Lookups
// ──────────────────────────────────────────────────────────────────────────────

public class CompetitionDto
{
    public int CompetitionId { get; set; }
    public string? CompetitionCode { get; set; }
    public string? CompetitionName { get; set; }
    public int MatchCount { get; set; }
}

public class TeamDto
{
    public int TeamId { get; set; }
    public string? TeamCode { get; set; }
    public string? TeamName { get; set; }
    public string? TeamLogo { get; set; }
    public int PlayerCount { get; set; }
}

public class LookupItemDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// Player profile & position
// ──────────────────────────────────────────────────────────────────────────────

public class PlayerProfileDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int MatchesWithPlayingTime { get; set; }
    public double PlayingTimeMinutes { get; set; }
    public double AveragePlayingTimePerMatchMinutes { get; set; }
    public double GoalsPer60 { get; set; }
    public double AssistsPer60 { get; set; }
    public double InterceptionsPer60 { get; set; }
    public double SavesPer60 { get; set; }
    public double TurnoversPer60 { get; set; }
    public double SanctionsPer60 { get; set; }
    public int TotalGoals { get; set; }
    public int TotalAssists { get; set; }
    public int TotalInterceptions { get; set; }
    public int TotalSaves { get; set; }
    public int TotalTurnovers { get; set; }
    public double ShotSuccessRate { get; set; }
    public double PenaltySuccessRate { get; set; }
}

public class PlayerMatchItemDto : MatchListItemDto
{
    public double PlayingTimeMinutes { get; set; }
    public int Goals { get; set; }
    public int PenaltyGoals { get; set; }
    public int Assists { get; set; }
    public int Interceptions { get; set; }
    public int Saves { get; set; }
    public int Turnovers { get; set; }
    public int Sanctions { get; set; }
}

public class PositionProfileAxisDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool HigherIsBetter { get; set; }
    public bool IsEvaluative { get; set; } = true;
    public double Value { get; set; }
    public double MedianValue { get; set; }
    public double Percentile { get; set; }
    public int? Rank { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public string Tone { get; set; } = string.Empty;
}

public class PositionProfilePlayerDto : PlayerListItemDto
{
    public int MatchesPlayed { get; set; }
    public int MatchesWithPlayingTime { get; set; }
    public double PlayingTimeMinutes { get; set; }
    public double AveragePlayingTimePerMatchMinutes { get; set; }
    public bool? IsBenchmarkEligible { get; set; }
    public List<PositionProfileAxisDto> Axes { get; set; } = [];
}

public class PositionProfileResponseDto
{
    public int? PositionId { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public bool IsGoalkeeperProfile { get; set; }
    public int CohortPlayerCount { get; set; }
    public bool? IsCohortReliable { get; set; }
    public int MinimumCohortPlayerCount { get; set; }
    public double MinimumBenchmarkPlayingTimeMinutes { get; set; }
    public PositionProfilePlayerDto? SelectedPlayer { get; set; }
    public PositionProfilePlayerDto? MedianProfile { get; set; }
    public List<PositionProfilePlayerDto> Players { get; set; } = [];
}

public class PositionProfileCompareRequestDto
{
    public List<int> PlayerIds { get; set; } = [];
    public int? CompetitionId { get; set; }
    public int? TeamId { get; set; }
    public int? PositionId { get; set; }
    public int? MatchId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Year { get; set; }
    public string? Season { get; set; }
    public string? Day { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// Client update
// ──────────────────────────────────────────────────────────────────────────────

public class ReleaseArtifactDto
{
    public long Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public int BuildNumber { get; set; }
    public int MinimumSupportedBuild { get; set; } = 1;
    public bool Mandatory { get; set; }
    public int RolloutPercent { get; set; } = 100;
    public bool Active { get; set; } = true;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string? MinimumOsVersion { get; set; }
    public string? SignatureThumbprint { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ClientUpdateCheckRequestDto
{
    public string Application { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public int CurrentBuild { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}

public class ClientUpdateCheckResponseDto
{
    public bool UpdateAvailable { get; set; }
    public bool Mandatory { get; set; }
    public bool CurrentBuildBlocked { get; set; }
    public string? DownloadUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Sha256 { get; set; }
    public string? LatestVersion { get; set; }
    public long? ReleaseId { get; set; }
    public int? LatestBuild { get; set; }
    public int? MinimumSupportedBuild { get; set; }
    public string? ReleaseNotes { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? Platform { get; set; }
    public string? Architecture { get; set; }
    public string? PackageType { get; set; }
    public string? FileName { get; set; }
    public string? MinimumOsVersion { get; set; }
    public string? SignatureThumbprint { get; set; }
    public string ApiVersion { get; set; } = string.Empty;
    public string DatabaseVersion { get; set; } = string.Empty;
}

public class UpdateEventRequestDto
{
    public string Application { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public int CurrentBuild { get; set; }
    public int? TargetBuild { get; set; }
    public long? ReleaseId { get; set; }
    public string? ErrorMessage { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// ProblemDetails (RFC 7807)
// ──────────────────────────────────────────────────────────────────────────────

public class ProblemDetailsDto
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? CorrelationId { get; set; }
}
