namespace HandWStat.Models.Analytics;

public static class AudienceLens
{
    public const string Club = "club";
    public const string Analyst = "analyste";
    public const string Player = "joueuse";

    public static IReadOnlyList<(string Value, string Label, string Description)> Options { get; } =
    [
        (Club, "Club", "Vue de pilotage rapide pour staff et encadrement."),
        (Analyst, "Analyste", "Lecture dense pour comparer, zoomer et decortiquer."),
        (Player, "Joueuse", "Vue claire pour comprendre le role et les reperes.")
    ];
}
