namespace HandWStat.Models.Matches;

public sealed record TeamIdentityDisplay(
    string? TeamId,
    string? TeamName,
    string? LogoPath,
    bool IsHome  // true si Team1 = domicile ET convention garantie
);

public sealed record MatchIdentityDisplay(
    string MatchId,
    TeamIdentityDisplay Team1,
    TeamIdentityDisplay Team2,
    int? Score1,
    int? Score2,
    string? CompetitionName,
    string? Season,
    int? Day,
    DateTime? Date,
    string? Status,   // null si aucune source contractuelle
    bool HomeAwayAvailable  // false si convention non garantie
);
