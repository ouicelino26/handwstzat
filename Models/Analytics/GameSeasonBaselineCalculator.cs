namespace HandWStat.Models.Analytics;

public static class GameSeasonBaselineCalculator
{
    public static double? CountBaseline(IReadOnlyList<PlayerGameAnalysisRow> rows, Func<PlayerGameAnalysisRow, int?> selector)
    {
        var sum = 0;
        var eligible = 0;
        foreach (var r in rows)
        {
            var v = selector(r);
            if (v == null) continue;
            sum += v.Value;
            eligible++;
        }
        return eligible > 0 ? (double)sum / eligible : null;
    }

    public static double? RateBaseline(IReadOnlyList<PlayerGameAnalysisRow> rows,
        Func<PlayerGameAnalysisRow, int?> numeratorSelector,
        Func<PlayerGameAnalysisRow, int?> denominatorSelector)
    {
        var totalNum = 0;
        var totalDen = 0;
        foreach (var r in rows)
        {
            var den = denominatorSelector(r);
            if (den == null || den == 0) continue;
            var num = numeratorSelector(r);
            if (num == null) continue;
            totalNum += num.Value;
            totalDen += den.Value;
        }
        return totalDen > 0 ? (double)totalNum / totalDen * 100.0 : null;
    }

    public static GameSeasonBaseline Build(IReadOnlyList<PlayerGameAnalysisRow> rows, bool isGoalkeeper)
    {
        if (isGoalkeeper)
        {
            return new GameSeasonBaseline(
                GoalsPerMatch: null,
                AssistsPerMatch: CountBaseline(rows, r => r.GkMetrics?.Assists),
                ShotRate: null,
                OpenPlayShotRate: null,
                InterceptionsPerMatch: null,
                TurnoversPerMatch: CountBaseline(rows, r => r.GkMetrics?.Turnovers),
                SavesPerMatch: CountBaseline(rows, r => r.GkMetrics?.Saves),
                SaveRate: RateBaseline(rows, r => r.GkMetrics?.Saves, r => r.GkMetrics?.ShotsFaced),
                OpenPlaySaveRate: RateBaseline(rows, r => r.GkMetrics?.OpenPlaySaves, r => r.GkMetrics?.OpenPlayShotsFaced),
                PenaltySaveRate: RateBaseline(rows, r => r.GkMetrics?.PenaltySaves, r => r.GkMetrics?.PenaltyShotsFaced),
                ShotsFacedPerMatch: CountBaseline(rows, r => r.GkMetrics?.ShotsFaced),
                GoalsConcededPerMatch: CountBaseline(rows, r => r.GkMetrics?.GoalsConceded)
            );
        }
        else
        {
            return new GameSeasonBaseline(
                GoalsPerMatch: CountBaseline(rows, r => r.FieldMetrics?.Goals),
                AssistsPerMatch: CountBaseline(rows, r => r.FieldMetrics?.Assists),
                ShotRate: RateBaseline(rows, r => r.FieldMetrics?.ShotGoals, r => r.FieldMetrics?.ShotAttempts),
                OpenPlayShotRate: RateBaseline(rows, r => r.FieldMetrics?.OpenPlayShotGoals, r => r.FieldMetrics?.OpenPlayShotAttempts),
                InterceptionsPerMatch: CountBaseline(rows, r => r.FieldMetrics?.Interceptions),
                TurnoversPerMatch: CountBaseline(rows, r => r.FieldMetrics?.Turnovers),
                SavesPerMatch: null,
                SaveRate: null,
                OpenPlaySaveRate: null,
                PenaltySaveRate: null,
                ShotsFacedPerMatch: null,
                GoalsConcededPerMatch: null
            );
        }
    }
}
