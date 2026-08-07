namespace HandWStat.Models.Matches;

public static class MatchScoreFormatter
{
    // Règle absolue : null ≠ 0
    public static string Format(int? score1, int? score2)
    {
        if (score1 == null && score2 == null) return "—";
        if (score1 == null || score2 == null) return "Score incomplet";
        return $"{score1} – {score2}";
    }

    // Pour le score depuis la perspective d'une équipe
    public static string FormatFromTeamPerspective(int? teamScore, int? opponentScore)
    {
        if (teamScore == null || opponentScore == null) return "—";
        return $"{teamScore} – {opponentScore}";
    }

    // Accessible pour aria-label : "Brest 31, Metz 27"
    public static string FormatAccessible(string? team1Name, int? score1, string? team2Name, int? score2)
    {
        if (score1 == null || score2 == null) return $"{team1Name ?? "Équipe 1"} contre {team2Name ?? "Équipe 2"}, score non disponible";
        return $"{team1Name ?? "Équipe 1"} {score1}, {team2Name ?? "Équipe 2"} {score2}";
    }

    // Vrai 0-0 doit rester 0-0
    public static bool IsRealZeroZero(int? score1, int? score2) => score1 == 0 && score2 == 0;
}
