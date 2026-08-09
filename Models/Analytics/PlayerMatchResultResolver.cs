namespace HandWStat.Models.Analytics;

public static class PlayerMatchResultResolver
{
    public static string? ResolveOpponentName(string? playerTeamId, string? team1Id, string? team1Name, string? team2Id, string? team2Name)
    {
        if (string.IsNullOrEmpty(playerTeamId)) return null;
        if (playerTeamId == team1Id) return team2Name;
        if (playerTeamId == team2Id) return team1Name;
        return null;
    }

    public static string? ResolveOpponentId(string? playerTeamId, string? team1Id, string? team2Id)
    {
        if (string.IsNullOrEmpty(playerTeamId)) return null;
        if (playerTeamId == team1Id) return team2Id;
        if (playerTeamId == team2Id) return team1Id;
        return null;
    }

    public static bool? ResolveIsHome(string? playerTeamId, string? team1Id) =>
        string.IsNullOrEmpty(playerTeamId) || string.IsNullOrEmpty(team1Id) ? null : playerTeamId == team1Id;

    public static PlayerMatchResult ResolveResult(string? playerTeamId, string? team1Id, int? team1Score, int? team2Score)
    {
        if (string.IsNullOrEmpty(playerTeamId) || team1Score == null || team2Score == null)
            return PlayerMatchResult.Unknown;

        bool playerIsTeam1 = playerTeamId == team1Id;
        var playerScore = playerIsTeam1 ? team1Score.Value : team2Score.Value;
        var opponentScore = playerIsTeam1 ? team2Score.Value : team1Score.Value;

        if (playerScore > opponentScore) return PlayerMatchResult.Win;
        if (playerScore < opponentScore) return PlayerMatchResult.Loss;
        return PlayerMatchResult.Draw;
    }

    public static string FormatScore(string? playerTeamId, string? team1Id, int? team1Score, int? team2Score)
    {
        if (team1Score == null || team2Score == null) return "—";
        bool playerIsTeam1 = playerTeamId == team1Id;
        var ps = playerIsTeam1 ? team1Score.Value : team2Score.Value;
        var os = playerIsTeam1 ? team2Score.Value : team1Score.Value;
        return $"{ps}–{os}";
    }

    public static string GetResultLabel(PlayerMatchResult result) => result switch
    {
        PlayerMatchResult.Win => "V",
        PlayerMatchResult.Draw => "N",
        PlayerMatchResult.Loss => "D",
        _ => "—"
    };

    public static string GetResultAccessibleLabel(PlayerMatchResult result) => result switch
    {
        PlayerMatchResult.Win => "Victoire",
        PlayerMatchResult.Draw => "Nul",
        PlayerMatchResult.Loss => "Défaite",
        _ => "Résultat inconnu"
    };
}
