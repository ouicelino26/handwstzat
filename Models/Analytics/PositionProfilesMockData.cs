using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public static class PositionProfilesMockData
{
    private sealed record AxisBlueprint(string Key, string Label, string Category, bool HigherIsBetter = true, string Format = "number");

    private static readonly AxisBlueprint[] Blueprints =
    [
        new("open_goals_per60", "Finition", "Offense"),
        new("assists_per60", "Creation", "Offense"),
        new("interceptions_per60", "Interceptions", "Defense"),
        new("blocks_per60", "Contres", "Defense"),
        new("turnovers_per60", "Pertes de balle", "Discipline", HigherIsBetter: false),
        new("penalties_won_per60", "7m obtenus", "Offense"),
        new("two_minutes_per60", "2 minutes", "Discipline", HigherIsBetter: false),
        new("shot_misses_per60", "Tirs manques", "Offense", HigherIsBetter: false)
    ];

    public static PositionProfileResponseDto CreateDemoProfile()
    {
        var selectedAxes = CreateAxes(
            [78, 66, 54, 61, 31, 69, 28, 24],
            [84, 72, 57, 63, 29, 74, 26, 22]);

        var medianAxes = CreateAxes([50, 50, 50, 50, 50, 50, 50, 50], [50, 50, 50, 50, 50, 50, 50, 50]);

        var supportPlayer = CreatePlayer(
            playerId: 204,
            fullName: "Lina Morel",
            teamName: "Nantes HB",
            positionCode: "ALG",
            positionName: "Arriere gauche",
            matchesPlayed: 13,
            playingTimeMinutes: 492,
            axes: CreateAxes([71, 63, 49, 56, 39, 58, 34, 27], [76, 68, 52, 58, 37, 61, 31, 26]));

        var anchorPlayer = CreatePlayer(
            playerId: 221,
            fullName: "Sara Lopez",
            teamName: "Brest Bretagne",
            positionCode: "ALG",
            positionName: "Arriere gauche",
            matchesPlayed: 12,
            playingTimeMinutes: 468,
            axes: CreateAxes([63, 57, 62, 68, 35, 54, 33, 29], [68, 61, 65, 70, 36, 59, 32, 30]));

        var selectedPlayer = CreatePlayer(
            playerId: 187,
            fullName: "Camille Martin",
            teamName: "Metz HB",
            positionCode: "ALG",
            positionName: "Arriere gauche",
            matchesPlayed: 14,
            playingTimeMinutes: 541,
            axes: selectedAxes);

        var profile = new PositionProfileResponseDto
        {
            PositionId = 3,
            PositionCode = "ALG",
            PositionName = "Arriere gauche",
            IsGoalkeeperProfile = false,
            CohortPlayerCount = 14,
            SelectedPlayer = selectedPlayer,
            MedianProfile = CreatePlayer(
                playerId: 0,
                fullName: "Mediane poste",
                teamName: "Cohorte",
                positionCode: "ALG",
                positionName: "Arriere gauche",
                matchesPlayed: 0,
                playingTimeMinutes: 0,
                axes: medianAxes),
            Players = [selectedPlayer, supportPlayer, anchorPlayer]
        };

        ApplyAxisBounds(profile.SelectedPlayer!, profile.MedianProfile!, supportPlayer, anchorPlayer);
        return profile;
    }

    public static IReadOnlyList<PlayerListItemDto> CreateDemoDirectory()
    {
        var profile = CreateDemoProfile();

        return
        [
            profile.SelectedPlayer!,
            new PlayerListItemDto
            {
                PlayerId = 204,
                FullName = "Lina Morel",
                TeamId = 12,
                TeamCode = "NHB",
                TeamName = "Nantes HB",
                PositionId = 3,
                PositionCode = "ALG",
                PositionName = "Arriere gauche",
                Nationality = "France",
                IsActive = true
            },
            new PlayerListItemDto
            {
                PlayerId = 221,
                FullName = "Sara Lopez",
                TeamId = 18,
                TeamCode = "BBH",
                TeamName = "Brest Bretagne",
                PositionId = 3,
                PositionCode = "ALG",
                PositionName = "Arriere gauche",
                Nationality = "Espagne",
                IsActive = true
            },
            new PlayerListItemDto
            {
                PlayerId = 233,
                FullName = "Maya Bernard",
                TeamId = 8,
                TeamCode = "LHB",
                TeamName = "Lyon HB",
                PositionId = 2,
                PositionCode = "DCD",
                PositionName = "Demi centre",
                Nationality = "France",
                IsActive = true
            },
            new PlayerListItemDto
            {
                PlayerId = 248,
                FullName = "Eva Caron",
                TeamId = 5,
                TeamCode = "PRD",
                TeamName = "Paris 92",
                PositionId = 3,
                PositionCode = "ALG",
                PositionName = "Arriere gauche",
                Nationality = "France",
                IsActive = true
            }
        ];
    }

    private static PositionProfilePlayerDto CreatePlayer(
        int playerId,
        string fullName,
        string teamName,
        string positionCode,
        string positionName,
        int matchesPlayed,
        double playingTimeMinutes,
        IReadOnlyList<PositionProfileAxisDto> axes)
    {
        return new PositionProfilePlayerDto
        {
            PlayerId = playerId,
            FullName = fullName,
            TeamId = 1,
            TeamCode = teamName[..Math.Min(3, teamName.Length)].ToUpperInvariant(),
            TeamName = teamName,
            PositionId = 3,
            PositionCode = positionCode,
            PositionName = positionName,
            Nationality = "France",
            IsActive = true,
            MatchesPlayed = matchesPlayed,
            MatchesWithPlayingTime = matchesPlayed,
            PlayingTimeMinutes = playingTimeMinutes,
            AveragePlayingTimePerMatchMinutes = matchesPlayed == 0 ? 0 : playingTimeMinutes / matchesPlayed,
            Axes = axes.ToList()
        };
    }

    private static List<PositionProfileAxisDto> CreateAxes(IReadOnlyList<double> values, IReadOnlyList<double> percentiles)
    {
        var result = new List<PositionProfileAxisDto>(Blueprints.Length);

        for (var index = 0; index < Blueprints.Length; index++)
        {
            var blueprint = Blueprints[index];
            var percentile = percentiles[index];

            result.Add(new PositionProfileAxisDto
            {
                Key = blueprint.Key,
                Label = blueprint.Label,
                Category = blueprint.Category,
                HigherIsBetter = blueprint.HigherIsBetter,
                Format = blueprint.Format,
                Value = values[index],
                MedianValue = 50,
                Percentile = percentile,
                Tone = percentile > 65 ? "positive" : percentile >= 35 ? "warning" : "danger"
            });
        }

        return result;
    }

    private static void ApplyAxisBounds(params PositionProfilePlayerDto[] players)
    {
        if (players.Length == 0)
        {
            return;
        }

        var axisCount = players[0].Axes.Count;

        for (var index = 0; index < axisCount; index++)
        {
            var values = players
                .Select(player => player.Axes[index].Value)
                .ToList();

            var min = values.Min();
            var max = values.Max();

            foreach (var player in players)
            {
                player.Axes[index].MinValue = min;
                player.Axes[index].MaxValue = max;
            }
        }
    }
}
