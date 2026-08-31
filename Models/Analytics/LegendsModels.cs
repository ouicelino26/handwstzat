using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

public sealed record LegendsSnapshot
{
    public required IReadOnlyList<PlayerGlobalStatsDto> Players { get; init; }

    public static LegendsSnapshot Empty => new() { Players = [] };
}
