using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public static class MatchKpiCatalog
{
    public static IReadOnlyList<KpiTile> BuildSummaryKpis(
        MatchSummaryDto? summary,
        MatchListItemDto? match,
        IReadOnlyList<PlayerGlobalStatsDto> players)
    {
        if (summary is null)
        {
            return [];
        }

        var totalScore = (match?.Team1Score ?? summary.Team1Score ?? 0) + (match?.Team2Score ?? summary.Team2Score ?? 0);
        var finalDiff = Math.Abs((match?.Team1Score ?? summary.Team1Score ?? 0) - (match?.Team2Score ?? summary.Team2Score ?? 0));
        var threeGoalScorers = summary.TopScorers?.Count(scorer => scorer.Value >= 3) ?? 0;
        var defensiveActions = summary.InterceptionCount + summary.SaveCount;
        var technical = summary.Technical;

        return
        [
            new KpiTile(
                "Buts cumules",
                totalScore.ToString(),
                "Intensite offensive brute du match.",
                "positive",
                $"{summary.GoalCount + summary.PenaltyGoalCount} buts valides dans le resume"),
            new KpiTile(
                "Ecart final",
                finalDiff.ToString(),
                "Mesure de maitrise ou de suspense au score.",
                "neutral",
                $"{match?.Team1Score ?? summary.Team1Score ?? 0} - {match?.Team2Score ?? summary.Team2Score ?? 0} au final"),
            new KpiTile(
                "Jeu prepare",
                HandballKpiHelper.FormatPercent(HandballKpiHelper.Share(summary.AssistCount, Math.Max(totalScore, 1))),
                "Part des buts amenes par une passe decisive.",
                HandballKpiHelper.HigherIsBetterTone(HandballKpiHelper.Share(summary.AssistCount, Math.Max(totalScore, 1))),
                HandballKpiHelper.FormatBase(summary.AssistCount, Math.Max(totalScore, 1), "buts")),
            new KpiTile(
                "Ballons valorises",
                HandballKpiHelper.FormatPercent(HandballKpiHelper.SuccessVsWasteShare(summary.AssistCount, summary.TurnoverCount)),
                "Lecture de la qualite de possession sur le match.",
                HandballKpiHelper.HigherIsBetterTone(HandballKpiHelper.SuccessVsWasteShare(summary.AssistCount, summary.TurnoverCount)),
                $"{summary.AssistCount} passes pour {summary.TurnoverCount} pertes"),
            new KpiTile(
                "Actions def.",
                defensiveActions.ToString(),
                "Interceptions et arrets cumules sur la rencontre.",
                "neutral",
                $"{summary.InterceptionCount} inter. + {summary.SaveCount} arrets"),
            new KpiTile(
                "Tirs engages",
                technical.ShotAttempts.ToString(),
                "Volume total de tirs, penalties et contres engages.",
                "neutral",
                HandballKpiHelper.FormatBase(technical.ShotAttempts, totalScore, "actions")),
            new KpiTile(
                "Dechet tir",
                technical.ShotWaste.ToString(),
                "Tirs manques, penalties manques et tirs contrés.",
                "warning",
                HandballKpiHelper.FormatBase(technical.ShotWaste, Math.Max(technical.ShotAttempts, 1), "dechets")),
            new KpiTile(
                "Pertes techniques",
                technical.TechnicalLosses.ToString(),
                "Mauvaises passes, pertes de balle, fautes techniques et passages en force.",
                "warning",
                HandballKpiHelper.FormatBase(technical.TechnicalLosses, summary.TurnoverCount + summary.AssistCount, "pertes")),
            new KpiTile(
                "Stop 7m",
                HandballKpiHelper.FormatPercent(technical.GoalkeeperPenaltyStopRate),
                "Efficacite gardienne sur les penalties de la rencontre.",
                HandballKpiHelper.GoalkeeperPenaltyStopRateTone(technical.GoalkeeperPenaltyStopRate),
                HandballKpiHelper.FormatBase(technical.GoalkeeperPenaltyStops, Math.Max(technical.GoalkeeperPenaltyStops + technical.GoalkeeperPenaltyConcededGoals, 1), "penalties arretes")),
            new KpiTile(
                "Scoreuses a 3+ buts",
                threeGoalScorers.ToString(),
                "Nombre de menaces offensives vraiment actives.",
                "neutral",
                $"{threeGoalScorers} joueuses au-dessus du seuil")
        ];
    }
}
