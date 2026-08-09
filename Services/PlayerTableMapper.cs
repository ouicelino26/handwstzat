using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Services;

/// <summary>
/// Mapper sans dépendances externes : ComparePlayersResponseDto → DashboardPlayerTable.
/// Séparé de StatsDashboardService pour permettre les tests unitaires purs.
/// </summary>
public static class PlayerTableMapper
{
    public static DashboardPlayerTable Build(ComparePlayersResponseDto response)
    {
        var offenseByPlayer = response.Offense.ToDictionary(item => item.PlayerId);
        var defenseByPlayer = response.Defense.ToDictionary(item => item.PlayerId);
        var passingByPlayer = response.Passing.ToDictionary(item => item.PlayerId);
        var sanctionsByPlayer = response.Sanctions.ToDictionary(item => item.PlayerId);
        var goalkeeperByPlayer = response.Goalkeeper.ToDictionary(item => item.PlayerId);

        var fieldPlayers = response.Players
            .Where(player => !player.IsGoalkeeper)
            .Select(player =>
            {
                offenseByPlayer.TryGetValue(player.PlayerId, out var offense);
                defenseByPlayer.TryGetValue(player.PlayerId, out var defense);
                passingByPlayer.TryGetValue(player.PlayerId, out var passing);
                sanctionsByPlayer.TryGetValue(player.PlayerId, out var sanctions);

                var totalGoals = offense?.TotalButs ?? player.TotalGoals;
                var openPlayGoals = offense?.Buts ?? Math.Max(player.TotalGoals - player.PenaltyGoalCount, 0);
                var penaltyGoals = offense?.Buts7m ?? player.PenaltyGoalCount;
                var assists = passing?.PasseDecisive ?? player.AssistCount;
                var totalTurnovers = passing?.TotalPertes ?? player.TurnoverCount;
                var badPasses = passing?.MauvaisePasse ?? 0;

                // Taux tirs ouverts
                var openNumerator = (double)(offense?.Buts ?? openPlayGoals);
                var openDenominator = (double)(offense is null
                    ? player.OpenShotAttempts
                    : offense.Buts + offense.TirsRates);
                var openPlayShotRate = TableRateValue.FromCounts(openNumerator, openDenominator);

                // Taux 7m
                var penNumerator = (double)penaltyGoals;
                var penDenominator = (double)(offense is null
                    ? player.PenaltyAttempts
                    : offense.Buts7m + offense.PenaltyRate);
                var penaltyShotRate = TableRateValue.FromCounts(penNumerator, penDenominator);

                // Taux global
                var totalAttempts = openDenominator + penDenominator;
                var totalShotRate = TableRateValue.FromCounts(totalGoals, totalAttempts);

                var sanctionDetail = new TableSanctionDetail(
                    sanctions?.Avertissements ?? 0,
                    sanctions?.DeuxMinutes ?? 0,
                    sanctions?.Exclusions ?? 0);

                var identity = new TablePlayerIdentity(
                    player.PlayerId,
                    player.FullName,
                    Clean(player.TeamName, "Equipe non renseignee"),
                    player.PositionId,
                    Clean(player.PositionCode ?? player.PositionName, "Poste non renseigne"),
                    false,
                    player.MatchesPlayed);

                var offenseRow = new TableFieldOffense(
                    totalGoals,
                    openPlayGoals,
                    penaltyGoals,
                    assists,
                    null,   // PenaltiesWon — DATA_UNAVAILABLE en V1
                    null,   // SanctionsDrawn — DATA_UNAVAILABLE en V1
                    totalTurnovers,
                    badPasses,
                    false,  // FailedPivotPassesAvailable — toujours false en V1
                    totalShotRate,
                    openPlayShotRate,
                    penaltyShotRate);

                var defenseRow = new TableFieldDefense(
                    defense?.Interceptions ?? player.InterceptionCount,
                    defense?.Contres ?? 0,
                    defense?.PassageForce ?? 0,
                    defense?.Neutralisations ?? 0,
                    sanctions?.PenaltyConcede ?? 0,
                    sanctionDetail);

                return new DashboardFieldPlayerRow(identity, offenseRow, defenseRow);
            })
            .OrderByDescending(row => row.Offense.TotalGoals)
            .ThenBy(row => row.Identity.FullName)
            .ToList();

        var goalkeepers = response.Players
            .Where(player => player.IsGoalkeeper)
            .Select(player =>
            {
                goalkeeperByPlayer.TryGetValue(player.PlayerId, out var goalkeeper);
                passingByPlayer.TryGetValue(player.PlayerId, out var passing);

                var openPlaySaves = goalkeeper?.Arrets ?? 0;
                var penaltySaves = goalkeeper?.ArretsPenalty ?? 0;
                var totalSaves = openPlaySaves + penaltySaves;

                var openPlayConceded = goalkeeper?.ButsPris ?? 0;
                var penaltyConceded = goalkeeper?.ButsPenalty ?? 0;

                var totalShotsFaced = goalkeeper?.TirsSubis ?? (player.ShotsFaced > 0 ? player.ShotsFaced : 0);
                var openPlayShotsFaced = openPlaySaves + openPlayConceded;
                var penaltyShotsFaced = penaltySaves + penaltyConceded;

                var totalSaveRate = TableRateValue.FromCounts(totalSaves, totalShotsFaced);
                var openPlaySaveRate = TableRateValue.FromCounts(openPlaySaves, openPlayShotsFaced);
                var penaltySaveRate = TableRateValue.FromCounts(penaltySaves, penaltyShotsFaced);

                var gkGoals = goalkeeper?.Buts ?? player.TotalGoals;
                var gkAssists = goalkeeper?.PasseDecisives ?? player.AssistCount;
                var gkTurnovers = (goalkeeper?.PerteDeBalle ?? 0) + (goalkeeper?.MauvaisePasse ?? 0);
                var missedShots = goalkeeper?.TirsLoupes ?? 0;

                var identity = new TablePlayerIdentity(
                    player.PlayerId,
                    player.FullName,
                    Clean(player.TeamName, "Equipe non renseignee"),
                    player.PositionId,
                    Clean(player.PositionCode ?? player.PositionName, "GB"),
                    true,
                    player.MatchesPlayed);

                var gkStats = new TableGoalkeeperStats(
                    totalSaves,
                    openPlaySaves,
                    penaltySaves,
                    totalShotsFaced,
                    openPlayShotsFaced,
                    penaltyShotsFaced,
                    totalSaveRate,
                    openPlaySaveRate,
                    penaltySaveRate,
                    gkAssists,
                    gkGoals,
                    gkTurnovers,
                    missedShots);

                return new DashboardGoalkeeperRow(identity, gkStats);
            })
            .OrderByDescending(row => row.Goalkeeper.TotalSaves)
            .ThenBy(row => row.Identity.FullName)
            .ToList();

        return new DashboardPlayerTable(fieldPlayers, goalkeepers);
    }

    private static string Clean(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
