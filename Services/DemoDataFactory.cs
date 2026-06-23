using HandWStat.Models.Analytics;
using HandballManagerCore.DTO;

namespace HandWStat.Services;

public static class DemoDataFactory
{
    private static readonly string[] GoalZoneKeys =
    [
        "BG12", "BG11", "BD11", "BD12",
        "BG10", "BG9", "BG8", "BG7", "BD7", "BD8", "BD9", "BD10",
        "BG6", "BG5", "BD5", "BD6",
        "BG4", "BG3", "BD3", "BD4",
        "BG2", "BG1", "BD1", "BD2"
    ];

    private static readonly string[] TriggerZoneKeys =
    [
        "TG6", "TG9", "TD9", "TD6",
        "TG5", "TG8", "TD8", "TD5",
        "TG3", "TG4", "TG7", "TD7",
        "TD4", "TD3", "TG1", "TG2",
        "TD2", "TD1"
    ];

    public static DashboardSnapshot Create(int? selectedPlayerId, string? warningMessage = null)
    {
        var players = new List<PlayerDirectoryItem>
        {
            new(12, "Nadege Legrand", "Metz HB", "ARD", "France", 27, false),
            new(7, "Lea Martin", "Brest Bretagne", "ALG", "France", 24, false),
            new(1, "Zoe Bernard", "Nantes", "GB", "France", 29, true)
        };

        var spotlights = new Dictionary<int, PlayerSpotlight>
        {
            [12] = CreateBackCourtSpotlight(players[0]),
            [7] = CreateWingSpotlight(players[1]),
            [1] = CreateKeeperSpotlight(players[2])
        };

        var activePlayer = selectedPlayerId.HasValue && spotlights.ContainsKey(selectedPlayerId.Value)
            ? selectedPlayerId.Value
            : 12;

        return new DashboardSnapshot
        {
            Overview = new OverviewMetrics(
                PlayerCount: 26,
                TeamCount: 11,
                MatchCount: 42,
                EventCount: 3684,
                GoalCount: 1194,
                AssistCount: 384,
                InterceptionCount: 255,
                SaveCount: 318,
                TurnoverCount: 277,
                SanctionCount: 123),
            Players = players,
            TeamOfTheDay = CreateTeamOfTheDay(),
            TopScorers =
            [
                new("Nadege Legrand", "Metz HB", 94, "94 buts", 12, "goals"),
                new("Lea Martin", "Brest Bretagne", 81, "81 buts", 7, "goals"),
                new("Camille Durand", "Paris 92", 75, "75 buts", 3, "goals"),
                new("Sarah Petit", "Dijon", 70, "70 buts", 5, "goals"),
                new("Amina Kebe", "Nantes", 68, "68 buts", 6, "goals")
            ],
            EfficiencyRanking =
            [
                new("Nadege Legrand", "Metz HB", 67.4, "67,4 %", 12, "shotsuccess"),
                new("Lea Martin", "Brest Bretagne", 64.1, "64,1 %", 7, "shotsuccess"),
                new("Sarah Petit", "Dijon", 62.8, "62,8 %", 5, "shotsuccess"),
                new("Camille Durand", "Paris 92", 60.4, "60,4 %", 3, "shotsuccess"),
                new("Amina Kebe", "Nantes", 58.9, "58,9 %", 6, "shotsuccess")
            ],
            RequestedRanking =
            [
                new("Nadege Legrand", "Metz HB", 94, "94 buts", 12, "goals"),
                new("Lea Martin", "Brest Bretagne", 81, "81 buts", 7, "goals"),
                new("Camille Durand", "Paris 92", 75, "75 buts", 3, "goals"),
                new("Sarah Petit", "Dijon", 70, "70 buts", 5, "goals"),
                new("Amina Kebe", "Nantes", 68, "68 buts", 6, "goals")
            ],
            RequestedRankingLabel = "Buts",
            InterceptionRanking =
            [
                new("Claire Mendy", "Chambray", 18, "18 interceptions", 18, "interceptions"),
                new("Nadege Legrand", "Metz HB", 16, "16 interceptions", 12, "interceptions"),
                new("Lea Martin", "Brest Bretagne", 14, "14 interceptions", 7, "interceptions")
            ],
            RecentMatches =
            [
                new(301, "Metz HB vs Nantes", "31 - 24", "J18", "09 avril 2026"),
                new(302, "Brest Bretagne vs Paris 92", "28 - 28", "J18", "08 avril 2026"),
                new(303, "Dijon vs Nice", "26 - 22", "J17", "02 avril 2026"),
                new(304, "Nantes vs Toulon", "34 - 27", "J17", "01 avril 2026")
            ],
            Spotlight = spotlights[activePlayer],
            DataOrigin = "Apercu",
            WarningMessage = warningMessage ?? "Mode apercu actif. Connectez-vous pour afficher les dernieres donnees.",
            IsDemo = true
        };
    }

    private static TeamOfTheDaySnapshotDto CreateTeamOfTheDay()
    {
        var candidates = new[]
        {
            CreateTeamOfTheDayCandidate(1, "Zoe Bernard", "Nantes", "Gardienne", "goalkeeper", "goalkeeper", true, 301, "Metz HB vs Nantes", "31 - 24", "Metz HB", new TeamOfTheDayStatLineDto
            {
                Saves = 14,
                PenaltySaves = 2,
                GoalsConceded = 24,
                ShotsFaced = 40,
                Assists = 1,
                GoalkeeperSaveRate = 40.0
            }),
            CreateTeamOfTheDayCandidate(7, "Lea Martin", "Brest Bretagne", "Ailiere gauche", "left-wing", "left-wing", false, 302, "Brest Bretagne vs Paris 92", "28 - 28", "Paris 92", new TeamOfTheDayStatLineDto
            {
                Goals = 7,
                Assists = 2,
                ShotAttempts = 9,
                ShotWaste = 2,
                ShotSuccessRate = 77.8,
                OverallShotSuccessRate = 77.8,
                Interceptions = 1,
                Turnovers = 1
            }),
            CreateTeamOfTheDayCandidate(12, "Nadege Legrand", "Metz HB", "Arriere droite", "right-back", "right-back", false, 301, "Metz HB vs Nantes", "31 - 24", "Nantes", new TeamOfTheDayStatLineDto
            {
                Goals = 9,
                PenaltyGoals = 2,
                Assists = 4,
                ShotAttempts = 13,
                ShotWaste = 4,
                ShotSuccessRate = 72.7,
                OverallShotSuccessRate = 69.2,
                Interceptions = 2,
                Blocks = 1,
                Turnovers = 2
            }),
            CreateTeamOfTheDayCandidate(23, "Camille Durand", "Paris 92", "Arriere gauche", "left-back", "left-back", false, 302, "Brest Bretagne vs Paris 92", "28 - 28", "Brest Bretagne", new TeamOfTheDayStatLineDto
            {
                Goals = 8,
                Assists = 3,
                ShotAttempts = 12,
                ShotWaste = 3,
                ShotSuccessRate = 75.0,
                OverallShotSuccessRate = 66.7,
                Blocks = 2,
                Turnovers = 1
            }),
            CreateTeamOfTheDayCandidate(31, "Sarah Petit", "Dijon", "Demi-centre", "center-back", "center-back", false, 303, "Dijon vs Nice", "26 - 22", "Nice", new TeamOfTheDayStatLineDto
            {
                Goals = 5,
                Assists = 8,
                ShotAttempts = 8,
                ShotWaste = 2,
                ShotSuccessRate = 62.5,
                OverallShotSuccessRate = 62.5,
                Interceptions = 2,
                Neutralisations = 1,
                Turnovers = 2
            }),
            CreateTeamOfTheDayCandidate(44, "Amina Kebe", "Nantes", "Pivot", "pivot", "pivot", false, 301, "Metz HB vs Nantes", "31 - 24", "Metz HB", new TeamOfTheDayStatLineDto
            {
                Goals = 6,
                Assists = 1,
                ShotAttempts = 7,
                ShotWaste = 1,
                ShotSuccessRate = 85.7,
                OverallShotSuccessRate = 85.7,
                Neutralisations = 3,
                ForcedOffensiveFouls = 1,
                Sanctions = 1
            }),
            CreateTeamOfTheDayCandidate(52, "Claire Mendy", "Chambray", "Ailiere droite", "right-wing", "right-wing", false, 304, "Nantes vs Toulon", "34 - 27", "Toulon", new TeamOfTheDayStatLineDto
            {
                Goals = 6,
                Assists = 3,
                ShotAttempts = 8,
                ShotWaste = 1,
                ShotSuccessRate = 75.0,
                OverallShotSuccessRate = 75.0,
                Interceptions = 3,
                Turnovers = 1
            }),
            CreateTeamOfTheDayCandidate(61, "Ines Moreau", "Nice", "Demi-centre", "center-back", "center-back", false, 303, "Dijon vs Nice", "26 - 22", "Dijon", new TeamOfTheDayStatLineDto
            {
                Goals = 4,
                Assists = 7,
                ShotAttempts = 7,
                ShotWaste = 2,
                ShotSuccessRate = 57.1,
                OverallShotSuccessRate = 57.1,
                Interceptions = 1,
                Turnovers = 1
            })
        };

        var groups = candidates
            .GroupBy(candidate => candidate.SlotKey)
            .Select(group => new TeamOfTheDayPositionGroupDto
            {
                SlotKey = group.Key,
                PositionLabel = group.First().PositionLabel,
                FormationArea = group.First().FormationArea,
                SortOrder = group.Key switch
                {
                    "goalkeeper" => 1,
                    "left-wing" => 2,
                    "left-back" => 3,
                    "center-back" => 4,
                    "pivot" => 5,
                    "right-back" => 6,
                    "right-wing" => 7,
                    _ => 99
                },
                Candidates = group
                    .OrderByDescending(candidate => candidate.PieGlobal)
                    .ThenBy(candidate => candidate.FullName)
                    .ToList()
            })
            .OrderBy(group => group.SortOrder)
            .ToList();

        return new TeamOfTheDaySnapshotDto
        {
            EffectiveSeason = "2025-2026",
            EffectiveDay = "J18",
            MatchCount = 4,
            CandidateCount = candidates.Length,
            Groups = groups
        };
    }

    private static TeamOfTheDayCandidateDto CreateTeamOfTheDayCandidate(
        int playerId,
        string fullName,
        string teamName,
        string positionLabel,
        string slotKey,
        string formationArea,
        bool isGoalkeeper,
        int matchId,
        string matchLabel,
        string matchScore,
        string opponentName,
        TeamOfTheDayStatLineDto statLine)
    {
        var scores = TeamOfTheDayPieScoring.Calculate(statLine, isGoalkeeper);

        return new TeamOfTheDayCandidateDto
        {
            PlayerId = playerId,
            FullName = fullName,
            TeamName = teamName,
            PositionLabel = positionLabel,
            SlotKey = slotKey,
            FormationArea = formationArea,
            IsGoalkeeper = isGoalkeeper,
            MatchesPlayed = 1,
            MatchId = matchId,
            MatchLabel = matchLabel,
            MatchScore = matchScore,
            OpponentName = opponentName,
            PlayingTimeMinutes = 45,
            PieGlobal = scores.Global,
            PieOffense = scores.Offense,
            PieDefense = scores.Defense,
            StatLine = statLine
        };
    }

    private static PlayerSpotlight CreateBackCourtSpotlight(PlayerDirectoryItem player)
    {
        return CreateSpotlight(
            player,
            matchesPlayed: 19,
            totalGoals: 94,
            penaltyGoals: 11,
            missedShots: 45,
            shotRate: 67.4,
            penaltyRate: 73.3,
            assists: 32,
            badPasses: 7,
            turnovers: 19,
            technicalFaults: 5,
            interceptions: 8,
            blocks: 3,
            neutralisations: 2,
            forcedOffensiveFouls: 1,
            exclusions: 1,
            warnings: 2,
            twoMinutes: 3,
            penaltiesConceded: 1,
            saves: 0,
            penaltySaves: 0,
            goalsConceded: 0,
            penaltyGoalsConceded: 0,
            saveRate: 0,
            distribution:
            [
                new("Buts", 94),
                new("Tirs rates", 45),
                new("Passes decisives", 32),
                new("Pertes", 19)
            ],
            matches:
            [
                CreatePlayerMatch(301, "J18", "09 avril 2026", "Metz HB", "Nantes", "31 - 24", 7, 2, 2, 1, 0, 1, 0),
                CreatePlayerMatch(298, "J17", "02 avril 2026", "Dijon", "Metz HB", "26 - 30", 6, 1, 1, 0, 0, 2, 1),
                CreatePlayerMatch(294, "J16", "27 mars 2026", "Metz HB", "Brest Bretagne", "29 - 27", 5, 0, 3, 1, 0, 1, 0)
            ],
            goalValues: new Dictionary<string, (int Attempts, int Successes)>
            {
                ["BG12"] = (3, 1), ["BG11"] = (8, 4), ["BD11"] = (7, 5), ["BD12"] = (3, 2),
                ["BG10"] = (6, 2), ["BG9"] = (10, 4), ["BG8"] = (15, 9), ["BG7"] = (21, 15),
                ["BD7"] = (25, 17), ["BD8"] = (14, 8), ["BD9"] = (8, 4), ["BD10"] = (5, 2),
                ["BG6"] = (13, 9), ["BG5"] = (17, 12), ["BD5"] = (16, 11), ["BD6"] = (12, 8),
                ["BG4"] = (6, 2), ["BG3"] = (4, 1), ["BD3"] = (5, 2), ["BD4"] = (7, 3),
                ["BG2"] = (9, 5), ["BG1"] = (11, 7), ["BD1"] = (12, 8), ["BD2"] = (10, 6)
            },
            triggerValues: new Dictionary<string, (int Attempts, int Successes)>
            {
                ["TG6"] = (17, 11), ["TG9"] = (13, 9), ["TD9"] = (12, 8), ["TD6"] = (15, 10),
                ["TG5"] = (15, 9), ["TG8"] = (10, 7), ["TD8"] = (9, 6), ["TD5"] = (14, 8),
                ["TG3"] = (11, 6), ["TG4"] = (12, 6), ["TG7"] = (14, 9), ["TD7"] = (16, 11),
                ["TD4"] = (13, 7), ["TD3"] = (10, 5), ["TG1"] = (19, 12), ["TG2"] = (28, 18),
                ["TD2"] = (24, 15), ["TD1"] = (11, 7)
            });
    }

    private static PlayerSpotlight CreateWingSpotlight(PlayerDirectoryItem player)
    {
        return CreateSpotlight(
            player,
            matchesPlayed: 18,
            totalGoals: 81,
            penaltyGoals: 6,
            missedShots: 31,
            shotRate: 72.3,
            penaltyRate: 66.7,
            assists: 18,
            badPasses: 4,
            turnovers: 11,
            technicalFaults: 3,
            interceptions: 6,
            blocks: 1,
            neutralisations: 1,
            forcedOffensiveFouls: 1,
            exclusions: 0,
            warnings: 1,
            twoMinutes: 1,
            penaltiesConceded: 0,
            saves: 0,
            penaltySaves: 0,
            goalsConceded: 0,
            penaltyGoalsConceded: 0,
            saveRate: 0,
            distribution:
            [
                new("Buts", 81),
                new("Tirs rates", 31),
                new("Passes decisives", 18),
                new("Pertes", 11)
            ],
            matches:
            [
                CreatePlayerMatch(302, "J18", "08 avril 2026", "Brest Bretagne", "Paris 92", "28 - 28", 4, 1, 1, 1, 0, 1, 0),
                CreatePlayerMatch(293, "J16", "25 mars 2026", "Nice", "Brest Bretagne", "21 - 29", 6, 0, 2, 0, 0, 0, 0),
                CreatePlayerMatch(288, "J15", "18 mars 2026", "Brest Bretagne", "Dijon", "30 - 25", 5, 1, 1, 1, 0, 1, 0)
            ],
            goalValues: new Dictionary<string, (int Attempts, int Successes)>
            {
                ["BG12"] = (7, 5), ["BG11"] = (12, 9), ["BD11"] = (5, 2), ["BD12"] = (2, 1),
                ["BG10"] = (14, 10), ["BG9"] = (19, 13), ["BG8"] = (11, 8), ["BG7"] = (9, 7),
                ["BD7"] = (4, 2), ["BD8"] = (3, 1), ["BG6"] = (8, 6), ["BG5"] = (6, 4),
                ["BD5"] = (3, 2), ["BD6"] = (4, 2), ["BG4"] = (12, 8), ["BG3"] = (9, 6),
                ["BD3"] = (1, 0), ["BD4"] = (2, 1), ["BG2"] = (10, 8), ["BG1"] = (7, 5),
                ["BD1"] = (2, 1), ["BD2"] = (1, 0)
            },
            triggerValues: new Dictionary<string, (int Attempts, int Successes)>
            {
                ["TG6"] = (6, 3), ["TG9"] = (4, 2), ["TD9"] = (5, 3), ["TD6"] = (3, 1),
                ["TG5"] = (20, 15), ["TG8"] = (7, 5), ["TD8"] = (6, 4), ["TD5"] = (9, 7),
                ["TG3"] = (23, 17), ["TG4"] = (17, 12), ["TG7"] = (14, 10), ["TD7"] = (8, 6),
                ["TD4"] = (10, 7), ["TD3"] = (5, 3), ["TG1"] = (8, 5), ["TG2"] = (12, 8),
                ["TD2"] = (9, 6), ["TD1"] = (4, 2)
            });
    }

    private static PlayerSpotlight CreateKeeperSpotlight(PlayerDirectoryItem player)
    {
        return CreateSpotlight(
            player,
            matchesPlayed: 17,
            totalGoals: 3,
            penaltyGoals: 0,
            missedShots: 5,
            shotRate: 37.5,
            penaltyRate: 0,
            assists: 9,
            badPasses: 3,
            turnovers: 4,
            technicalFaults: 1,
            interceptions: 0,
            blocks: 0,
            neutralisations: 0,
            forcedOffensiveFouls: 0,
            exclusions: 0,
            warnings: 0,
            twoMinutes: 0,
            penaltiesConceded: 0,
            saves: 68,
            penaltySaves: 8,
            goalsConceded: 64,
            penaltyGoalsConceded: 7,
            saveRate: 54.3,
            distribution:
            [
                new("Arrets", 76),
                new("Buts encaisses", 64),
                new("Passes decisives", 9),
                new("Pertes", 4)
            ],
            matches:
            [
                CreatePlayerMatch(304, "J17", "01 avril 2026", "Nantes", "Toulon", "34 - 27", 0, 0, 1, 0, 5, 0, 0),
                CreatePlayerMatch(301, "J18", "09 avril 2026", "Metz HB", "Nantes", "31 - 24", 0, 0, 0, 0, 11, 1, 0),
                CreatePlayerMatch(289, "J15", "17 mars 2026", "Nantes", "Chambray", "28 - 23", 1, 0, 0, 0, 7, 0, 0)
            ],
            goalValues: new Dictionary<string, (int Attempts, int Successes)>
            {
                ["BG11"] = (1, 0), ["BD11"] = (2, 1), ["BG7"] = (1, 0),
                ["BD7"] = (1, 1), ["BG1"] = (2, 1), ["BD1"] = (1, 0)
            },
            triggerValues: new Dictionary<string, (int Attempts, int Successes)>
            {
                ["TG1"] = (2, 1), ["TG2"] = (1, 0), ["TG5"] = (3, 1), ["TG8"] = (2, 1),
                ["TD7"] = (1, 0), ["TD5"] = (1, 0)
            });
    }

    private static PlayerSpotlight CreateSpotlight(
        PlayerDirectoryItem player,
        int matchesPlayed,
        int totalGoals,
        int penaltyGoals,
        int missedShots,
        double shotRate,
        double penaltyRate,
        int assists,
        int badPasses,
        int turnovers,
        int technicalFaults,
        int interceptions,
        int blocks,
        int neutralisations,
        int forcedOffensiveFouls,
        int exclusions,
        int warnings,
        int twoMinutes,
        int penaltiesConceded,
        int saves,
        int penaltySaves,
        int goalsConceded,
        int penaltyGoalsConceded,
        double saveRate,
        IReadOnlyList<SliceValue> distribution,
        IReadOnlyList<PlayerMatchItemDto> matches,
        IReadOnlyDictionary<string, (int Attempts, int Successes)> goalValues,
        IReadOnlyDictionary<string, (int Attempts, int Successes)> triggerValues)
    {
        var teamHistory = CreateTeamHistory(player, matchesPlayed, totalGoals, assists, interceptions, saves, penaltySaves, turnovers, badPasses, technicalFaults, forcedOffensiveFouls);

        var profile = new PlayerProfileDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            TotalGoals = totalGoals,
            TotalAssists = assists,
            TotalInterceptions = interceptions,
            TotalSaves = saves + penaltySaves,
            TotalTurnovers = turnovers,
            ShotSuccessRate = shotRate,
            PenaltySuccessRate = penaltyRate
        };

        var global = new PlayerGlobalStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            GoalCount = totalGoals - penaltyGoals,
            PenaltyGoalCount = penaltyGoals,
            TotalGoals = totalGoals,
            AssistCount = assists,
            InterceptionCount = interceptions,
            SaveCount = saves + penaltySaves,
            TurnoverCount = turnovers,
            SanctionCount = exclusions + warnings + twoMinutes + penaltiesConceded,
            ShotSuccessRate = shotRate,
            PenaltySuccessRate = penaltyRate,
            GoalkeeperSaveRate = saveRate
        };

        var offense = new PlayerOffenseStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            Buts = totalGoals - penaltyGoals,
            Buts7m = penaltyGoals,
            TotalButs = totalGoals,
            TirsRates = missedShots,
            PenaltyRate = Math.Max(0, (int)Math.Round((penaltyGoals / Math.Max(penaltyRate, 1d)) - penaltyGoals)),
            TauxReussiteTir = shotRate,
            TauxReussitePenalty = penaltyRate
        };

        var defense = new PlayerDefenseStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            Interceptions = interceptions,
            Contres = blocks,
            Neutralisations = neutralisations,
            PassageForce = forcedOffensiveFouls
        };

        var passing = new PlayerPassingStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            PasseDecisive = assists,
            MauvaisePasse = badPasses,
            PerteDeBalle = turnovers,
            FauteTechnique = technicalFaults,
            PassageEnForce = forcedOffensiveFouls,
            TotalPertes = turnovers + badPasses + technicalFaults
        };

        var sanctions = new PlayerSanctionStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            Exclusions = exclusions,
            Avertissements = warnings,
            DeuxMinutes = twoMinutes,
            PenaltyConcede = penaltiesConceded
        };

        var goalkeeper = new PlayerGoalkeeperStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            Arrets = saves,
            ArretsPenalty = penaltySaves,
            ButsPris = goalsConceded,
            ButsPenalty = penaltyGoalsConceded,
            Buts = totalGoals,
            TirsRates = missedShots,
            TirsSubis = goalsConceded + penaltyGoalsConceded + saves + penaltySaves,
            PasseDecisives = assists,
            MauvaisePasse = badPasses,
            PerteDeBalle = turnovers,
            TauxReussiteTir = shotRate,
            TauxArret = saveRate
        };

        var penaltyAttempts = penaltyRate > 0
            ? Math.Max(penaltyGoals, (int)Math.Round(penaltyGoals * 100d / penaltyRate, MidpointRounding.AwayFromZero))
            : penaltyGoals;
        var shotAttempts = (totalGoals - penaltyGoals) + missedShots + (penaltyAttempts - penaltyGoals) + Math.Max(blocks, 0);
        var technical = new PlayerTechnicalStatsDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            TeamName = player.TeamName,
            PositionName = player.PositionName,
            Nationality = player.CountryName,
            Age = player.Age,
            IsGoalkeeper = player.IsGoalkeeper,
            MatchesPlayed = matchesPlayed,
            TeamHistory = teamHistory,
            Technical = new TechnicalStatsDto
            {
                ShotAttempts = shotAttempts,
                ShotWaste = missedShots + (penaltyAttempts - penaltyGoals) + Math.Max(blocks, 0),
                PenaltyAttempts = penaltyAttempts,
                TechnicalLosses = turnovers + badPasses + technicalFaults + forcedOffensiveFouls,
                DefensiveImpact = interceptions + blocks + neutralisations + forcedOffensiveFouls,
                GoalkeeperStops = saves + penaltySaves,
                GoalkeeperPenaltyStops = penaltySaves,
                GoalkeeperConcededGoals = goalsConceded,
                GoalkeeperPenaltyConcededGoals = penaltyGoalsConceded,
                TirsSubis = saves + penaltySaves + goalsConceded + penaltyGoalsConceded,
                Sanctions = exclusions + warnings + twoMinutes + penaltiesConceded,
                OpenShotSuccessRate = shotRate,
                OverallShotSuccessRate = HandballKpiHelper.Share(totalGoals, Math.Max(shotAttempts, 1)),
                PenaltySuccessRate = penaltyRate,
                GoalkeeperSaveRate = saveRate,
                GoalkeeperPenaltyStopRate = HandballKpiHelper.Share(penaltySaves, Math.Max(penaltySaves + penaltyGoalsConceded, 1))
            }
        };

        return new PlayerSpotlight
        {
            Profile = profile,
            Global = global,
            Offense = offense,
            Defense = defense,
            Passing = passing,
            Sanctions = sanctions,
            Goalkeeper = goalkeeper,
            Technical = technical,
            Matches = matches,
            GoalZones = BuildGoalZones(goalValues),
            TriggerZones = BuildTriggerZones(triggerValues),
            Distribution = distribution
        };
    }

    private static List<PlayerTeamHistoryDto> CreateTeamHistory(
        PlayerDirectoryItem player,
        int matchesPlayed,
        int totalGoals,
        int assists,
        int interceptions,
        int saves,
        int penaltySaves,
        int turnovers,
        int badPasses,
        int technicalFaults,
        int forcedOffensiveFouls)
    {
        var eventCount = totalGoals + assists + interceptions + saves + penaltySaves + turnovers + badPasses + technicalFaults + forcedOffensiveFouls;

        return
        [
            new PlayerTeamHistoryDto
            {
                TeamName = player.TeamName,
                MatchesPlayed = matchesPlayed,
                EventCount = eventCount,
                PlayingTimeMinutes = Math.Round(Math.Max(matchesPlayed, 1) * 42.5d, 1, MidpointRounding.AwayFromZero)
            }
        ];
    }

    private static PlayerMatchItemDto CreatePlayerMatch(
        int matchId,
        string day,
        string dateLabel,
        string team1,
        string team2,
        string score,
        int goals,
        int penaltyGoals,
        int assists,
        int interceptions,
        int saves,
        int turnovers,
        int sanctions)
    {
        var parts = score.Split('-', StringSplitOptions.TrimEntries);

        return new PlayerMatchItemDto
        {
            MatchId = matchId,
            Day = day,
            Date = DateTime.TryParse(dateLabel, out var parsedDate) ? parsedDate : null,
            Team1Name = team1,
            Team2Name = team2,
            Team1Score = parts.Length > 0 && int.TryParse(parts[0], out var team1Score) ? team1Score : 0,
            Team2Score = parts.Length > 1 && int.TryParse(parts[1], out var team2Score) ? team2Score : 0,
            Goals = goals,
            PenaltyGoals = penaltyGoals,
            Assists = assists,
            Interceptions = interceptions,
            Saves = saves,
            Turnovers = turnovers,
            Sanctions = sanctions
        };
    }

    private static IReadOnlyList<ZoneStat> BuildGoalZones(IReadOnlyDictionary<string, (int Attempts, int Successes)> values)
    {
        return GoalZoneKeys.Select(key => BuildZone(key, values)).ToList();
    }

    private static IReadOnlyList<ZoneStat> BuildTriggerZones(IReadOnlyDictionary<string, (int Attempts, int Successes)> values)
    {
        return TriggerZoneKeys.Select(key => BuildZone(key, values)).ToList();
    }

    private static ZoneStat BuildZone(string key, IReadOnlyDictionary<string, (int Attempts, int Successes)> values)
    {
        var stat = values.TryGetValue(key, out var value)
            ? value
            : (Attempts: 0, Successes: 0);

        var rate = stat.Attempts == 0 ? 0 : Math.Round(stat.Successes * 100d / stat.Attempts, 1);

        var outcomes = stat.Attempts == 0
            ? Array.Empty<OutcomeCount>()
            : new[]
            {
                new OutcomeCount("But / gain", stat.Successes),
                new OutcomeCount("Rate / perdu", Math.Max(stat.Attempts - stat.Successes, 0))
            };

        return new ZoneStat(key, key, rate, stat.Attempts, stat.Successes, outcomes);
    }
}
