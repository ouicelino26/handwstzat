namespace HandWStat.Models.Analytics;

public enum TeamOfTheDayPieMode
{
    Global,
    Offense,
    Defense
}

public enum TeamOfTheDayRankingMetric
{
    PieGlobal,
    PieOffense,
    PieDefense,
    TotalActions,
    Goals,
    Assists,
    DefensiveImpact,
    GoalkeeperStops
}

public sealed record TeamOfTheDaySnapshotDto
{
    public string? EffectiveSeason { get; init; }

    public string? EffectiveDay { get; init; }

    public int MatchCount { get; init; }

    public int CandidateCount { get; init; }

    public string? WarningMessage { get; init; }

    public IReadOnlyList<TeamOfTheDayPositionGroupDto> Groups { get; init; } = [];

    public int FilledSlotCount => Groups.Count(group => group.Candidates.Count > 0);

    public string ScopeLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(EffectiveSeason) && !string.IsNullOrWhiteSpace(EffectiveDay))
            {
                return $"{EffectiveSeason} - {EffectiveDay}";
            }

            if (!string.IsNullOrWhiteSpace(EffectiveDay))
            {
                return EffectiveDay!;
            }

            if (!string.IsNullOrWhiteSpace(EffectiveSeason))
            {
                return EffectiveSeason!;
            }

            return "Journee non selectionnee";
        }
    }

    public IReadOnlyList<TeamOfTheDayCandidateDto> GetLineup(TeamOfTheDayPieMode mode)
    {
        return Groups
            .OrderBy(group => group.SortOrder)
            .Select(group => group.GetBestCandidate(mode))
            .Where(candidate => candidate is not null)
            .Cast<TeamOfTheDayCandidateDto>()
            .ToList();
    }

    public IReadOnlyList<TeamOfTheDayCandidateDto> GetLineup(
        TeamOfTheDayRankingMetric metric,
        double? minimumPie = null,
        int? minimumActions = null)
    {
        return Groups
            .OrderBy(group => group.SortOrder)
            .Select(group => group.GetBestCandidate(metric, minimumPie, minimumActions))
            .Where(candidate => candidate is not null)
            .Cast<TeamOfTheDayCandidateDto>()
            .ToList();
    }

    public static TeamOfTheDaySnapshotDto Empty(string? warningMessage = null)
    {
        return new TeamOfTheDaySnapshotDto
        {
            WarningMessage = warningMessage ?? "Aucune donnee exploitable pour construire l'equipe type."
        };
    }
}

public sealed record TeamOfTheDayPositionGroupDto
{
    public required string SlotKey { get; init; }

    public required string PositionLabel { get; init; }

    public required string FormationArea { get; init; }

    public int SortOrder { get; init; }

    public IReadOnlyList<TeamOfTheDayCandidateDto> Candidates { get; init; } = [];

    public TeamOfTheDayCandidateDto? GetBestCandidate(TeamOfTheDayPieMode mode)
    {
        return Candidates
            .OrderByDescending(candidate => candidate.ScoreFor(mode))
            .ThenByDescending(candidate => candidate.PlayingTimeMinutes)
            .ThenByDescending(candidate => candidate.StatLine.DirectContributions + candidate.StatLine.DefensiveImpact + candidate.StatLine.GoalkeeperStops)
            .ThenBy(candidate => candidate.StatLine.Turnovers)
            .ThenBy(candidate => candidate.FullName)
            .FirstOrDefault();
    }

    public TeamOfTheDayCandidateDto? GetBestCandidate(
        TeamOfTheDayRankingMetric metric,
        double? minimumPie = null,
        int? minimumActions = null)
    {
        return Candidates
            .Where(candidate => candidate.MatchesFilters(metric, minimumPie, minimumActions))
            .OrderByDescending(candidate => candidate.ScoreFor(metric))
            .ThenByDescending(candidate => candidate.PieForFilter(metric))
            .ThenByDescending(candidate => candidate.PlayingTimeMinutes)
            .ThenByDescending(candidate => candidate.StatLine.TotalActions)
            .ThenBy(candidate => candidate.StatLine.Turnovers)
            .ThenBy(candidate => candidate.FullName)
            .FirstOrDefault();
    }

    public int TieCount(TeamOfTheDayPieMode mode)
    {
        var best = GetBestCandidate(mode);
        if (best is null)
        {
            return 0;
        }

        var bestScore = best.ScoreFor(mode);
        return Candidates.Count(candidate => Math.Abs(candidate.ScoreFor(mode) - bestScore) < 0.05d);
    }

    public int TieCount(
        TeamOfTheDayRankingMetric metric,
        double? minimumPie = null,
        int? minimumActions = null)
    {
        var best = GetBestCandidate(metric, minimumPie, minimumActions);
        if (best is null)
        {
            return 0;
        }

        var bestScore = best.ScoreFor(metric);
        return Candidates.Count(candidate =>
            candidate.MatchesFilters(metric, minimumPie, minimumActions)
            && Math.Abs(candidate.ScoreFor(metric) - bestScore) < 0.05d);
    }
}

public sealed record TeamOfTheDayCandidateDto
{
    public required int PlayerId { get; init; }

    public required string FullName { get; init; }

    public required string TeamName { get; init; }

    public int? TeamId { get; init; }

    public required string PositionLabel { get; init; }

    public required string SlotKey { get; init; }

    public required string FormationArea { get; init; }

    public bool IsGoalkeeper { get; init; }

    public int MatchesPlayed { get; init; }

    public int? MatchId { get; init; }

    public string? MatchLabel { get; init; }

    public string? MatchScore { get; init; }

    public string? OpponentName { get; init; }

    public double PlayingTimeMinutes { get; init; }

    public double PieGlobal { get; init; }

    public double PieOffense { get; init; }

    public double PieDefense { get; init; }

    public required TeamOfTheDayStatLineDto StatLine { get; init; }

    public string Initials
    {
        get
        {
            var initials = FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(part => char.ToUpperInvariant(part[0]))
                .ToArray();

            return initials.Length == 0 ? "?" : new string(initials);
        }
    }

    public double ScoreFor(TeamOfTheDayPieMode mode)
    {
        return mode switch
        {
            TeamOfTheDayPieMode.Offense => PieOffense,
            TeamOfTheDayPieMode.Defense => PieDefense,
            _ => PieGlobal
        };
    }

    public double ScoreFor(TeamOfTheDayRankingMetric metric)
    {
        return metric switch
        {
            TeamOfTheDayRankingMetric.PieOffense => PieOffense,
            TeamOfTheDayRankingMetric.PieDefense => PieDefense,
            TeamOfTheDayRankingMetric.TotalActions => StatLine.TotalActions,
            TeamOfTheDayRankingMetric.Goals => StatLine.Goals,
            TeamOfTheDayRankingMetric.Assists => StatLine.Assists,
            TeamOfTheDayRankingMetric.DefensiveImpact => StatLine.DefensiveImpact,
            TeamOfTheDayRankingMetric.GoalkeeperStops => StatLine.GoalkeeperStops,
            _ => PieGlobal
        };
    }

    public double PieForFilter(TeamOfTheDayRankingMetric metric)
    {
        return metric switch
        {
            TeamOfTheDayRankingMetric.PieOffense => PieOffense,
            TeamOfTheDayRankingMetric.PieDefense => PieDefense,
            _ => PieGlobal
        };
    }

    public bool MatchesFilters(TeamOfTheDayRankingMetric metric, double? minimumPie, int? minimumActions)
    {
        if (minimumPie.HasValue && PieForFilter(metric) < minimumPie.Value)
        {
            return false;
        }

        return !minimumActions.HasValue || StatLine.TotalActions >= minimumActions.Value;
    }
}

public sealed record TeamOfTheDayStatLineDto
{
    public int Goals { get; init; }

    public int PenaltyGoals { get; init; }

    public int Assists { get; init; }

    public int Interceptions { get; init; }

    public int Blocks { get; init; }

    public int Neutralisations { get; init; }

    public int ForcedOffensiveFouls { get; init; }

    public int Saves { get; init; }

    public int PenaltySaves { get; init; }

    public int GoalsConceded { get; init; }

    public int ShotsFaced { get; init; }

    public int ShotAttempts { get; init; }

    public int ShotWaste { get; init; }

    public int Turnovers { get; init; }

    public int TechnicalLosses { get; init; }

    public int Sanctions { get; init; }

    public int PenaltiesConceded { get; init; }

    public double ShotSuccessRate { get; init; }

    public double OverallShotSuccessRate { get; init; }

    public double GoalkeeperSaveRate { get; init; }

    public int DirectContributions => Goals + Assists;

    public int DefensiveImpact => Interceptions + Blocks + Neutralisations + ForcedOffensiveFouls;

    public int GoalkeeperStops => Saves + PenaltySaves;

    public int TotalActions => DirectContributions + DefensiveImpact + GoalkeeperStops;

    public int TotalVolume => Goals
        + Assists
        + DefensiveImpact
        + GoalkeeperStops
        + Math.Max(ShotAttempts, 0)
        + Math.Max(ShotsFaced, 0)
        + Turnovers
        + TechnicalLosses
        + Sanctions
        + Math.Max(GoalsConceded, 0);
}

public static class TeamOfTheDayPieScoring
{
    public static (double Offense, double Defense, double Global) Calculate(TeamOfTheDayStatLineDto stats, bool isGoalkeeper)
    {
        var offense = isGoalkeeper
            ? (stats.Goals * 5d)
                + (stats.Assists * 3d)
                + (stats.ShotSuccessRate * 0.05d)
                - (stats.Turnovers * 1.2d)
            : (stats.Goals * 6d)
                + (stats.PenaltyGoals * 2d)
                + (stats.Assists * 4d)
                + (stats.OverallShotSuccessRate * 0.08d)
                + (stats.ShotSuccessRate * 0.05d)
                - (stats.ShotWaste * 2d)
                - (stats.Turnovers * 1.4d)
                - (stats.TechnicalLosses * 0.6d);

        var defense = isGoalkeeper
            ? (stats.GoalkeeperStops * 4.2d)
                + (stats.PenaltySaves * 1.3d)
                + (stats.GoalkeeperSaveRate * 0.18d)
                - (stats.GoalsConceded * 0.55d)
                - (stats.ShotsFaced * 0.03d)
                - (stats.Sanctions * 0.8d)
            : (stats.Interceptions * 5d)
                + (stats.Blocks * 4d)
                + (stats.Neutralisations * 2.8d)
                + (stats.ForcedOffensiveFouls * 3d)
                - (stats.Sanctions * 1.2d)
                - (stats.PenaltiesConceded * 1.5d)
                - (stats.Turnovers * 0.4d);

        var positiveActions = stats.DirectContributions + stats.DefensiveImpact + stats.GoalkeeperStops;
        var negativeActions = stats.ShotWaste + stats.TechnicalLosses + stats.Turnovers + stats.Sanctions + stats.GoalsConceded;
        var balanceBonus = HandballKpiHelper.SuccessVsWasteShare(positiveActions, negativeActions) * 0.03d;

        var global = isGoalkeeper
            ? (defense * 0.72d) + (offense * 0.28d) + balanceBonus
            : (offense * 0.58d) + (defense * 0.42d) + balanceBonus;

        return (Normalize(offense), Normalize(defense), Normalize(global));
    }

    private static double Normalize(double score)
    {
        return Math.Round(Math.Max(score, 0d), 1, MidpointRounding.AwayFromZero);
    }
}
