using HandWStat.Models.Analytics;

namespace HandWStat.Tests;

public sealed class TeamOfTheDayModelTests
{
    // ── TeamOfTheDaySnapshotDto.Empty ─────────────────────────────────────────

    [Fact]
    public void Empty_ReturnsSnapshotWithWarningAndNoGroups()
    {
        var snapshot = TeamOfTheDaySnapshotDto.Empty("Aucun match disponible.");

        Assert.NotNull(snapshot);
        Assert.Equal("Aucun match disponible.", snapshot.WarningMessage);
        Assert.Empty(snapshot.Groups);
        Assert.Equal(0, snapshot.CandidateCount);
        Assert.Equal(0, snapshot.FilledSlotCount);
    }

    [Fact]
    public void Empty_WithoutMessage_HasDefaultWarning()
    {
        var snapshot = TeamOfTheDaySnapshotDto.Empty();

        Assert.NotNull(snapshot.WarningMessage);
        Assert.NotEmpty(snapshot.WarningMessage);
    }

    // ── ScopeLabel ────────────────────────────────────────────────────────────

    [Fact]
    public void ScopeLabel_WithSeasonAndDay_CombinesBoth()
    {
        var snapshot = new TeamOfTheDaySnapshotDto
        {
            EffectiveSeason = "2025-2026",
            EffectiveDay = "J12",
            Groups = []
        };

        Assert.Contains("2025-2026", snapshot.ScopeLabel, StringComparison.Ordinal);
        Assert.Contains("J12", snapshot.ScopeLabel, StringComparison.Ordinal);
    }

    [Fact]
    public void ScopeLabel_DayOnlyNoSeason_ReturnsDay()
    {
        var snapshot = new TeamOfTheDaySnapshotDto { EffectiveDay = "J8", Groups = [] };

        Assert.Equal("J8", snapshot.ScopeLabel);
    }

    [Fact]
    public void ScopeLabel_NoDayNoSeason_ReturnsFallback()
    {
        var snapshot = new TeamOfTheDaySnapshotDto { Groups = [] };

        Assert.Equal("Journee non selectionnee", snapshot.ScopeLabel);
    }

    // ── TeamOfTheDayStatLineDto computed properties ───────────────────────────

    [Fact]
    public void StatLine_DirectContributions_IsGoalsPlusAssists()
    {
        var stat = new TeamOfTheDayStatLineDto { Goals = 3, Assists = 2 };

        Assert.Equal(5, stat.DirectContributions);
    }

    [Fact]
    public void StatLine_DefensiveImpact_IsInterceptionsPlusBlocksPlusNeutralisationsPlusFouls()
    {
        var stat = new TeamOfTheDayStatLineDto
        {
            Interceptions = 2,
            Blocks = 1,
            Neutralisations = 1,
            ForcedOffensiveFouls = 1
        };

        Assert.Equal(5, stat.DefensiveImpact);
    }

    [Fact]
    public void StatLine_GoalkeeperStops_IsSavesPlusPenaltySaves()
    {
        var stat = new TeamOfTheDayStatLineDto { Saves = 8, PenaltySaves = 2 };

        Assert.Equal(10, stat.GoalkeeperStops);
    }

    [Fact]
    public void StatLine_TotalActions_IsSumOfContributions()
    {
        var stat = new TeamOfTheDayStatLineDto
        {
            Goals = 2, Assists = 1,
            Interceptions = 3,
            Saves = 5
        };

        Assert.Equal(11, stat.TotalActions);
    }

    [Fact]
    public void StatLine_TotalVolume_IsZeroForAllZero()
    {
        var stat = new TeamOfTheDayStatLineDto();

        Assert.Equal(0, stat.TotalVolume);
    }

    // ── TeamOfTheDayPieScoring.Calculate ─────────────────────────────────────

    [Fact]
    public void PieScoring_FieldPlayer_HighGoals_OffenseDominates()
    {
        var stat = new TeamOfTheDayStatLineDto
        {
            Goals = 8,
            Assists = 2,
            ShotAttempts = 12
        };

        var (offense, defense, global) = TeamOfTheDayPieScoring.Calculate(stat, isGoalkeeper: false);

        Assert.True(offense > defense, $"Expected offense ({offense}) > defense ({defense}) for high-scorer");
        Assert.True(global > 0);
    }

    [Fact]
    public void PieScoring_Goalkeeper_HighStops_DefenseDominates()
    {
        var stat = new TeamOfTheDayStatLineDto
        {
            Saves = 12,
            PenaltySaves = 2,
            GoalkeeperSaveRate = 0.75,
            GoalsConceded = 5,
            ShotsFaced = 17
        };

        var (offense, defense, global) = TeamOfTheDayPieScoring.Calculate(stat, isGoalkeeper: true);

        Assert.True(defense > offense, $"Expected defense ({defense}) > offense ({offense}) for goalkeeper");
        Assert.True(global > 0);
    }

    [Fact]
    public void PieScoring_NegativeScores_ClampedToZero()
    {
        var stat = new TeamOfTheDayStatLineDto
        {
            Turnovers = 15,
            ShotWaste = 10,
            TechnicalLosses = 5,
            Sanctions = 3
        };

        var (offense, defense, global) = TeamOfTheDayPieScoring.Calculate(stat, isGoalkeeper: false);

        Assert.True(offense >= 0, "Offense score must not be negative");
        Assert.True(defense >= 0, "Defense score must not be negative");
        Assert.True(global >= 0, "Global score must not be negative");
    }

    [Fact]
    public void PieScoring_ZeroStats_ReturnsZero()
    {
        var stat = new TeamOfTheDayStatLineDto();

        var (offense, defense, global) = TeamOfTheDayPieScoring.Calculate(stat, isGoalkeeper: false);

        Assert.Equal(0.0, offense);
        Assert.Equal(0.0, defense);
        Assert.Equal(0.0, global);
    }

    // ── GetBestCandidate ──────────────────────────────────────────────────────

    [Fact]
    public void GetBestCandidate_SelectsHighestPieGlobal()
    {
        var group = new TeamOfTheDayPositionGroupDto
        {
            SlotKey = "left-back",
            PositionLabel = "Arriere gauche",
            FormationArea = "left-back",
            Candidates =
            [
                MakeCandidate(1, "Alice", pieGlobal: 8.5),
                MakeCandidate(2, "Bob", pieGlobal: 12.1),
                MakeCandidate(3, "Carol", pieGlobal: 7.0)
            ]
        };

        var best = group.GetBestCandidate(TeamOfTheDayPieMode.Global);

        Assert.NotNull(best);
        Assert.Equal(2, best.PlayerId);
        Assert.Equal("Bob", best.FullName);
    }

    [Fact]
    public void GetBestCandidate_EmptyCandidates_ReturnsNull()
    {
        var group = new TeamOfTheDayPositionGroupDto
        {
            SlotKey = "pivot",
            PositionLabel = "Pivot",
            FormationArea = "pivot",
            Candidates = []
        };

        var best = group.GetBestCandidate(TeamOfTheDayPieMode.Global);

        Assert.Null(best);
    }

    [Fact]
    public void GetBestCandidate_Tie_UsesPlayingTimeTieBreaker()
    {
        var group = new TeamOfTheDayPositionGroupDto
        {
            SlotKey = "center-back",
            PositionLabel = "Demi-centre",
            FormationArea = "center-back",
            Candidates =
            [
                MakeCandidate(1, "Alice", pieGlobal: 10.0, playingTime: 60),
                MakeCandidate(2, "Bob", pieGlobal: 10.0, playingTime: 50)
            ]
        };

        var best = group.GetBestCandidate(TeamOfTheDayPieMode.Global);

        Assert.NotNull(best);
        Assert.Equal(1, best.PlayerId);
    }

    [Fact]
    public void TieCount_AllSameScore_ReturnsAllCandidates()
    {
        var group = new TeamOfTheDayPositionGroupDto
        {
            SlotKey = "right-wing",
            PositionLabel = "Ailiere droite",
            FormationArea = "right-wing",
            Candidates =
            [
                MakeCandidate(1, "Alice", pieGlobal: 5.0),
                MakeCandidate(2, "Bob", pieGlobal: 5.0),
                MakeCandidate(3, "Carol", pieGlobal: 5.0)
            ]
        };

        var tieCount = group.TieCount(TeamOfTheDayPieMode.Global);

        Assert.Equal(3, tieCount);
    }

    [Fact]
    public void TieCount_NoCandidates_ReturnsZero()
    {
        var group = new TeamOfTheDayPositionGroupDto
        {
            SlotKey = "goalkeeper",
            PositionLabel = "Gardienne",
            FormationArea = "goalkeeper",
            Candidates = []
        };

        Assert.Equal(0, group.TieCount(TeamOfTheDayPieMode.Global));
    }

    // ── GetLineup ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetLineup_PopulatedSnapshot_ReturnsOnePerGroup()
    {
        var snapshot = MakeSnapshotWithThreeGroups();

        var lineup = snapshot.GetLineup(TeamOfTheDayPieMode.Global);

        Assert.Equal(3, lineup.Count);
    }

    [Fact]
    public void GetLineup_EmptySnapshot_ReturnsEmpty()
    {
        var snapshot = TeamOfTheDaySnapshotDto.Empty();

        Assert.Empty(snapshot.GetLineup(TeamOfTheDayPieMode.Global));
    }

    // ── TeamOfTheDayCandidateDto.Initials ─────────────────────────────────────

    [Fact]
    public void Candidate_Initials_TwoWordName_ReturnsFirstLetters()
    {
        var candidate = MakeCandidate(1, "Alice Martin", pieGlobal: 5.0);

        Assert.Equal("AM", candidate.Initials);
    }

    [Fact]
    public void Candidate_Initials_SingleName_ReturnsSingleLetter()
    {
        var candidate = MakeCandidate(1, "Alice", pieGlobal: 5.0);

        Assert.Equal("A", candidate.Initials);
    }

    // ── Exploratory label ─────────────────────────────────────────────────────

    [Fact]
    public void PieScoring_Calculate_IsLocalComputation_NotFromServer()
    {
        // Verification that PIE scores are computed locally from stat inputs — not from server.
        // If Calculate() returns results that differ from zero for non-zero inputs, local calculation is live.
        var stat = new TeamOfTheDayStatLineDto { Goals = 5 };
        var (_, _, global) = TeamOfTheDayPieScoring.Calculate(stat, isGoalkeeper: false);

        Assert.True(global > 0, "PIE global must be > 0 for a player with 5 goals — computed locally.");
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static TeamOfTheDayCandidateDto MakeCandidate(int id, string name, double pieGlobal, double playingTime = 30)
    {
        return new TeamOfTheDayCandidateDto
        {
            PlayerId = id,
            FullName = name,
            TeamName = "Team",
            PositionLabel = "Position",
            SlotKey = "left-back",
            FormationArea = "left-back",
            PieGlobal = pieGlobal,
            PieOffense = pieGlobal * 0.6,
            PieDefense = pieGlobal * 0.4,
            PlayingTimeMinutes = playingTime,
            StatLine = new TeamOfTheDayStatLineDto()
        };
    }

    private static TeamOfTheDaySnapshotDto MakeSnapshotWithThreeGroups()
    {
        return new TeamOfTheDaySnapshotDto
        {
            EffectiveSeason = "2025-2026",
            EffectiveDay = "J12",
            MatchCount = 3,
            CandidateCount = 6,
            Groups =
            [
                new TeamOfTheDayPositionGroupDto
                {
                    SlotKey = "goalkeeper",
                    PositionLabel = "Gardienne",
                    FormationArea = "goalkeeper",
                    SortOrder = 1,
                    Candidates = [MakeCandidate(1, "Alice GB", pieGlobal: 9.0)]
                },
                new TeamOfTheDayPositionGroupDto
                {
                    SlotKey = "left-back",
                    PositionLabel = "Arriere gauche",
                    FormationArea = "left-back",
                    SortOrder = 3,
                    Candidates = [MakeCandidate(2, "Bob ARG", pieGlobal: 7.0), MakeCandidate(3, "Carol ARG", pieGlobal: 6.0)]
                },
                new TeamOfTheDayPositionGroupDto
                {
                    SlotKey = "pivot",
                    PositionLabel = "Pivot",
                    FormationArea = "pivot",
                    SortOrder = 5,
                    Candidates = [MakeCandidate(4, "Dana Pivot", pieGlobal: 8.5)]
                }
            ]
        };
    }
}
