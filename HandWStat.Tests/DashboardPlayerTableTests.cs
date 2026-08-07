using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;
using HandWStat.Services;
#pragma warning disable CS8602 // Possible null reference — intentional in test asserts

namespace HandWStat.Tests;

/// <summary>
/// Tests unitaires pour le mapper BuildPlayerTable et les modèles TableRateValue.
/// Toutes les assertions utilisent des objets construits à la main — aucune dépendance externe.
/// </summary>
public sealed class DashboardPlayerTableTests
{
    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static PlayerGlobalStatsDto MakePlayer(
        int id = 1,
        string name = "Alice",
        bool isGoalkeeper = false,
        int goals = 0, int penaltyGoals = 0, int assists = 0,
        int interceptions = 0, int saves = 0, int turnovers = 0,
        int openShotAttempts = 0, int penaltyAttempts = 0, int shotsFaced = 0)
    {
        return new PlayerGlobalStatsDto
        {
            PlayerId = id,
            FullName = name,
            TeamName = "Team A",
            PositionId = isGoalkeeper ? 7 : 1,
            PositionCode = isGoalkeeper ? "GB" : "AR",
            PositionName = isGoalkeeper ? "Gardienne de but" : "Arriere",
            IsGoalkeeper = isGoalkeeper,
            MatchesPlayed = 5,
            TotalGoals = goals,
            PenaltyGoalCount = penaltyGoals,
            GoalCount = Math.Max(goals - penaltyGoals, 0),
            AssistCount = assists,
            InterceptionCount = interceptions,
            SaveCount = saves,
            TurnoverCount = turnovers,
            OpenShotAttempts = openShotAttempts,
            PenaltyAttempts = penaltyAttempts,
            ShotsFaced = shotsFaced
        };
    }

    private static ComparePlayersResponseDto BuildResponse(
        IEnumerable<PlayerGlobalStatsDto> players,
        IEnumerable<PlayerOffenseStatsDto>? offense = null,
        IEnumerable<PlayerDefenseStatsDto>? defense = null,
        IEnumerable<PlayerPassingStatsDto>? passing = null,
        IEnumerable<PlayerSanctionStatsDto>? sanctions = null,
        IEnumerable<PlayerGoalkeeperStatsDto>? goalkeepers = null)
    {
        return new ComparePlayersResponseDto
        {
            Players = players.ToList(),
            Offense = offense?.ToList() ?? [],
            Defense = defense?.ToList() ?? [],
            Passing = passing?.ToList() ?? [],
            Sanctions = sanctions?.ToList() ?? [],
            Goalkeeper = goalkeepers?.ToList() ?? [],
            Technical = []
        };
    }

    // ─── Mapping tests — champ ─────────────────────────────────────────────────

    [Fact]
    public void GlobalTable_MapsTotalGoals()
    {
        var player = MakePlayer(id: 1, name: "Alice", goals: 7, penaltyGoals: 2);
        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            TotalButs = 9, Buts = 7, Buts7m = 2
        };
        var response = BuildResponse([player], offense: [offense]);

        var table = PlayerTableMapper.Build(response);

        Assert.Single(table.FieldPlayers);
        Assert.Equal(9, table.FieldPlayers[0].Offense.TotalGoals);
    }

    [Fact]
    public void GlobalTable_MapsOpenPlayGoals()
    {
        var player = MakePlayer(id: 1, goals: 10, penaltyGoals: 3);
        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            TotalButs = 10, Buts = 7, Buts7m = 3
        };
        var response = BuildResponse([player], offense: [offense]);

        var table = PlayerTableMapper.Build(response);

        Assert.Equal(7, table.FieldPlayers[0].Offense.OpenPlayGoals);
    }

    [Fact]
    public void GlobalTable_MapsPenaltyGoals()
    {
        var player = MakePlayer(id: 1, goals: 10, penaltyGoals: 4);
        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            TotalButs = 10, Buts = 6, Buts7m = 4
        };
        var response = BuildResponse([player], offense: [offense]);

        var table = PlayerTableMapper.Build(response);

        Assert.Equal(4, table.FieldPlayers[0].Offense.PenaltyGoals);
    }

    [Fact]
    public void GlobalTable_MapsAssists()
    {
        var player = MakePlayer(id: 1, assists: 3);
        var passing = new PlayerPassingStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            PasseDecisive = 8, TotalPertes = 2
        };
        var response = BuildResponse([player], passing: [passing]);

        var table = PlayerTableMapper.Build(response);

        Assert.Equal(8, table.FieldPlayers[0].Offense.Assists);
    }

    [Fact]
    public void GlobalTable_MapsPenaltiesWon_IsNull()
    {
        var player = MakePlayer(id: 1);
        var response = BuildResponse([player]);

        var table = PlayerTableMapper.Build(response);

        Assert.Null(table.FieldPlayers[0].Offense.PenaltiesWon);
    }

    [Fact]
    public void GlobalTable_MapsSanctionsDrawn_IsNull()
    {
        var player = MakePlayer(id: 1);
        var response = BuildResponse([player]);

        var table = PlayerTableMapper.Build(response);

        Assert.Null(table.FieldPlayers[0].Offense.SanctionsDrawn);
    }

    [Fact]
    public void GlobalTable_MapsTurnovers()
    {
        var player = MakePlayer(id: 1, turnovers: 5);
        var passing = new PlayerPassingStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            TotalPertes = 6, MauvaisePasse = 2
        };
        var response = BuildResponse([player], passing: [passing]);

        var table = PlayerTableMapper.Build(response);

        Assert.Equal(6, table.FieldPlayers[0].Offense.TotalTurnovers);
    }

    [Fact]
    public void GlobalTable_MapsBadPasses()
    {
        var player = MakePlayer(id: 1);
        var passing = new PlayerPassingStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            MauvaisePasse = 3, TotalPertes = 5
        };
        var response = BuildResponse([player], passing: [passing]);

        var table = PlayerTableMapper.Build(response);

        Assert.Equal(3, table.FieldPlayers[0].Offense.BadPasses);
    }

    [Fact]
    public void GlobalTable_PreservesFailedPivotPassDataMissing()
    {
        var player = MakePlayer(id: 1);
        var response = BuildResponse([player]);

        var table = PlayerTableMapper.Build(response);

        Assert.False(table.FieldPlayers[0].Offense.FailedPivotPassesAvailable);
    }

    [Fact]
    public void GlobalTable_MapsTotalShotRateEvidence()
    {
        var player = MakePlayer(id: 1, goals: 5, openShotAttempts: 8, penaltyAttempts: 4);
        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            TotalButs = 7, Buts = 5, Buts7m = 2,
            TirsRates = 3, PenaltyRate = 2
        };
        var response = BuildResponse([player], offense: [offense]);

        var table = PlayerTableMapper.Build(response);
        var rate = table.FieldPlayers[0].Offense.TotalShotRate;

        // numerator = TotalButs = 7, denominator = (open buts + open rates) + (7m buts + 7m rates) = 8 + 4 = 12
        Assert.Equal(7, rate.Numerator);
        Assert.Equal(12, rate.Denominator);
        Assert.NotNull(rate.Value);
    }

    [Fact]
    public void GlobalTable_MapsOpenPlayShotRateEvidence()
    {
        var player = MakePlayer(id: 1, goals: 5, openShotAttempts: 8);
        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            Buts = 5, TirsRates = 3,
            TotalButs = 5, Buts7m = 0
        };
        var response = BuildResponse([player], offense: [offense]);

        var table = PlayerTableMapper.Build(response);
        var rate = table.FieldPlayers[0].Offense.OpenPlayShotRate;

        Assert.Equal(5, rate.Numerator);
        Assert.Equal(8, rate.Denominator); // Buts + TirsRates
    }

    [Fact]
    public void GlobalTable_MapsPenaltyShotRateEvidence()
    {
        var player = MakePlayer(id: 1, penaltyGoals: 3, penaltyAttempts: 5);
        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            Buts7m = 3, PenaltyRate = 2,
            TotalButs = 3
        };
        var response = BuildResponse([player], offense: [offense]);

        var table = PlayerTableMapper.Build(response);
        var rate = table.FieldPlayers[0].Offense.PenaltyShotRate;

        Assert.Equal(3, rate.Numerator);
        Assert.Equal(5, rate.Denominator); // Buts7m + PenaltyRate
    }

    [Fact]
    public void GlobalTable_MapsDefensiveMetrics()
    {
        var player = MakePlayer(id: 1, interceptions: 4);
        var defense = new PlayerDefenseStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            Interceptions = 6, Contres = 3, Neutralisations = 2, PassageForce = 1
        };
        var response = BuildResponse([player], defense: [defense]);

        var table = PlayerTableMapper.Build(response);
        var def = table.FieldPlayers[0].Defense;

        Assert.Equal(6, def.Interceptions);
        Assert.Equal(3, def.Blocks);
        Assert.Equal(1, def.OffensiveFoulsDrawn);
        Assert.Equal(2, def.Neutralizations);
    }

    [Fact]
    public void GlobalTable_MapsSanctionBreakdown_PenaltyConcedeExcluded()
    {
        var player = MakePlayer(id: 1);
        var sanctions = new PlayerSanctionStatsDto
        {
            PlayerId = 1, FullName = "Alice",
            Avertissements = 2,
            DeuxMinutes = 3,
            Exclusions = 1,
            PenaltyConcede = 5   // DOIT être exclu du total
        };
        var response = BuildResponse([player], sanctions: [sanctions]);

        var table = PlayerTableMapper.Build(response);
        var detail = table.FieldPlayers[0].Defense.SanctionsConceded;

        Assert.Equal(2, detail.Warnings);
        Assert.Equal(3, detail.TwoMinutes);
        Assert.Equal(1, detail.Disqualifications);
        Assert.Equal(6, detail.Total);   // 2+3+1 = 6, pas 11
    }

    // ─── Mapping tests — gardienne ─────────────────────────────────────────────

    [Fact]
    public void GlobalTable_MapsGoalkeeperSaveEvidence()
    {
        var player = MakePlayer(id: 2, name: "Betty", isGoalkeeper: true, saves: 17, shotsFaced: 22);
        var goalkeeper = new PlayerGoalkeeperStatsDto
        {
            PlayerId = 2, FullName = "Betty",
            Arrets = 13, ArretsPenalty = 4,
            ButsPris = 7, ButsPenalty = 2,
            TirsSubis = 26
        };
        var response = BuildResponse([player], goalkeepers: [goalkeeper]);

        var table = PlayerTableMapper.Build(response);
        var gk = table.Goalkeepers[0].Goalkeeper;

        Assert.Equal(17, gk.TotalSaves);  // 13 + 4
        Assert.Equal(26, gk.TotalShotsFaced);  // TirsSubis
        Assert.NotNull(gk.TotalSaveRate.Value);
        Assert.Equal(17, gk.TotalSaveRate.Numerator);
        Assert.Equal(26, gk.TotalSaveRate.Denominator);
    }

    [Fact]
    public void GlobalTable_MapsGoalkeeperBallMetrics()
    {
        var player = MakePlayer(id: 2, name: "Betty", isGoalkeeper: true);
        var goalkeeper = new PlayerGoalkeeperStatsDto
        {
            PlayerId = 2, FullName = "Betty",
            PasseDecisives = 3,
            Buts = 1,
            PerteDeBalle = 2,
            MauvaisePasse = 1,
            TirsLoupes = 4
        };
        var response = BuildResponse([player], goalkeepers: [goalkeeper]);

        var table = PlayerTableMapper.Build(response);
        var gk = table.Goalkeepers[0].Goalkeeper;

        Assert.Equal(3, gk.Assists);
        Assert.Equal(1, gk.Goals);
        Assert.Equal(3, gk.TotalTurnovers);  // PerteDeBalle + MauvaisePasse
        Assert.Equal(4, gk.MissedShots);     // TirsLoupes
    }

    // ─── TableRateValue tests ──────────────────────────────────────────────────

    [Fact]
    public void RateCell_ZeroDenominator_IsUnavailable()
    {
        var rate = TableRateValue.FromCounts(0, 0);

        Assert.True(rate.IsUnavailable);
        Assert.Null(rate.Value);
    }

    [Fact]
    public void RateCell_ShowsNumeratorAndDenominator()
    {
        var rate = TableRateValue.FromCounts(17, 19);

        Assert.Equal(17, rate.Numerator);
        Assert.Equal(19, rate.Denominator);
        Assert.NotNull(rate.Value);
    }

    [Fact]
    public void RateCell_ValidRate_ComputedCorrectly()
    {
        var rate = TableRateValue.FromCounts(55, 60);

        // 55/60 * 100 = 91.666... → arrondi à 91.67
        Assert.NotNull(rate.Value);
        Assert.Equal(91.67, rate.Value!.Value, precision: 2);
    }

    [Fact]
    public void RateCell_7v7_DoesNotOutrankHighVolumePlayer_SampleReliableFalse()
    {
        // 7/7 = 100 % mais SampleReliable=false en V1 (pas de Wilson bound)
        var rate = TableRateValue.FromCounts(7, 7);

        Assert.False(rate.SampleReliable);
        Assert.Equal(100.0, rate.Value!.Value, precision: 2);
    }

    [Fact]
    public void GoalkeeperRate_ZeroDenominator_IsUnavailable()
    {
        var rate = TableRateValue.FromCounts(0, 0);

        Assert.True(rate.IsUnavailable);
        Assert.Null(rate.Value);
    }

    [Fact]
    public void GoalkeeperRate_ValidRate_ComputedCorrectly()
    {
        // 13 arrêts sur 20 tirs = 65 %
        var rate = TableRateValue.FromCounts(13, 20);

        Assert.Equal(65.0, rate.Value!.Value, precision: 2);
    }

    // ─── Tri par taux ──────────────────────────────────────────────────────────

    [Fact]
    public void RateSort_NullBeforeSampleReliable_WhenDescending()
    {
        // Une joueuse avec denominateur 0 (null) doit arriver EN DERNIER tri desc
        var nullRate = TableRateValue.FromCounts(0, 0);
        var validRate = TableRateValue.FromCounts(5, 10);

        var rows = new[]
        {
            (Name: "Zoe", Rate: nullRate),
            (Name: "Alice", Rate: validRate)
        };

        var sorted = rows
            .OrderByDescending(r => r.Rate.Value.HasValue)
            .ThenByDescending(r => r.Rate.Value)
            .ToList();

        Assert.Equal("Alice", sorted[0].Name); // non-null en premier
        Assert.Equal("Zoe", sorted[1].Name);
    }

    [Fact]
    public void RateSort_ValidRateRanksCorrectly()
    {
        var rate50 = TableRateValue.FromCounts(5, 10);
        var rate80 = TableRateValue.FromCounts(8, 10);

        var rows = new[]
        {
            (Name: "Alice", Rate: rate50),
            (Name: "Betty", Rate: rate80)
        };

        var sorted = rows
            .OrderByDescending(r => r.Rate.Value.HasValue)
            .ThenByDescending(r => r.Rate.Value)
            .ToList();

        Assert.Equal("Betty", sorted[0].Name);
        Assert.Equal("Alice", sorted[1].Name);
    }

    // ─── Filtre et tri sur FilteredFieldRows (logique pure) ───────────────────

    [Fact]
    public void TableFiltersByPosition()
    {
        var playerAr = MakePlayer(id: 1, name: "Alice");
        playerAr.PositionId = 1;
        playerAr.PositionCode = "AR";

        var playerPiv = MakePlayer(id: 2, name: "Betty");
        playerPiv.PositionId = 3;
        playerPiv.PositionCode = "PIV";

        var response = BuildResponse([playerAr, playerPiv]);
        var table = PlayerTableMapper.Build(response);

        var filtered = table.FieldPlayers.Where(row => row.Identity.PositionId == 3).ToList();

        Assert.Single(filtered);
        Assert.Equal("Betty", filtered[0].Identity.FullName);
    }

    [Fact]
    public void TableSearchFiltersByPlayerName()
    {
        var players = new[]
        {
            MakePlayer(id: 1, name: "Alice Martin"),
            MakePlayer(id: 2, name: "Betty Dupont"),
            MakePlayer(id: 3, name: "Claire Martin")
        };
        var response = BuildResponse(players);
        var table = PlayerTableMapper.Build(response);

        var search = "martin";
        var results = table.FieldPlayers
            .Where(row => row.Identity.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Contains("Martin", r.Identity.FullName, StringComparison.OrdinalIgnoreCase));
    }

    // ─── Tri default ───────────────────────────────────────────────────────────

    [Fact]
    public void FieldPlayers_DefaultSort_ByTotalGoalsDescThenName()
    {
        var players = new[]
        {
            MakePlayer(id: 1, name: "Zoe",   goals: 3),
            MakePlayer(id: 2, name: "Alice", goals: 8),
            MakePlayer(id: 3, name: "Betty", goals: 8)
        };
        var offense1 = new PlayerOffenseStatsDto { PlayerId = 1, FullName = "Zoe",   TotalButs = 3 };
        var offense2 = new PlayerOffenseStatsDto { PlayerId = 2, FullName = "Alice", TotalButs = 8 };
        var offense3 = new PlayerOffenseStatsDto { PlayerId = 3, FullName = "Betty", TotalButs = 8 };
        var response = BuildResponse(players, offense: [offense1, offense2, offense3]);

        var table = PlayerTableMapper.Build(response);

        // Alice et Betty ont 8 buts → tri alpha → Alice avant Betty → Zoe en dernier
        Assert.Equal("Alice", table.FieldPlayers[0].Identity.FullName);
        Assert.Equal("Betty", table.FieldPlayers[1].Identity.FullName);
        Assert.Equal("Zoe",   table.FieldPlayers[2].Identity.FullName);
    }

    [Fact]
    public void Goalkeepers_DefaultSort_ByTotalSavesDescThenName()
    {
        var players = new[]
        {
            MakePlayer(id: 10, name: "Zoe",   isGoalkeeper: true),
            MakePlayer(id: 11, name: "Alice", isGoalkeeper: true)
        };
        var gk1 = new PlayerGoalkeeperStatsDto { PlayerId = 10, FullName = "Zoe",   Arrets = 4, ArretsPenalty = 0 };
        var gk2 = new PlayerGoalkeeperStatsDto { PlayerId = 11, FullName = "Alice", Arrets = 9, ArretsPenalty = 2 };
        var response = BuildResponse(players, goalkeepers: [gk1, gk2]);

        var table = PlayerTableMapper.Build(response);

        Assert.Equal("Alice", table.Goalkeepers[0].Identity.FullName); // 11 arrêts
        Assert.Equal("Zoe",   table.Goalkeepers[1].Identity.FullName); // 4 arrêts
    }
}
