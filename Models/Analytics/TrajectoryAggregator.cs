namespace HandWStat.Models.Analytics;

public static class TrajectoryAggregator
{
    // RÈGLE CRITIQUE : les taux s'agrègent en SUM(num)/SUM(den), jamais moyenne des %
    public static double? AggregateRate(IReadOnlyList<PlayerTrajectoryPoint> points)
    {
        var num = 0.0;
        var den = 0.0;
        foreach (var p in points)
        {
            if (p.Availability == "DATA_MISSING" || p.MetricValue == null) continue;
            if (p.Denominator == null || p.Denominator == 0) continue;
            num += p.Numerator ?? 0;
            den += p.Denominator.Value;
        }
        return den > 0 ? num / den * 100.0 : null;
    }

    // Count/PerMatch : SUM/eligibleMatches, exclure DATA_MISSING
    public static double? AggregateCount(IReadOnlyList<PlayerTrajectoryPoint> points, Func<PlayerTrajectoryPoint, double?> selector)
    {
        var sum = 0.0;
        var eligible = 0;
        foreach (var p in points)
        {
            if (p.Availability == "DATA_MISSING") continue;
            var v = selector(p);
            if (v == null) continue;
            sum += v.Value;
            eligible++;
        }
        return eligible > 0 ? sum / eligible : null;
    }

    // Fenêtre Last5/Last10/Season
    public static IReadOnlyList<PlayerTrajectoryPoint> ApplyWindow(
        IReadOnlyList<PlayerTrajectoryPoint> allPoints,
        TrajectoryWindow window)
    {
        var sorted = allPoints.OrderBy(p => p.Date).ThenBy(p => p.MatchId).ToList();
        return window switch
        {
            TrajectoryWindow.Last5 => sorted.TakeLast(5).ToList(),
            TrajectoryWindow.Last10 => sorted.TakeLast(10).ToList(),
            TrajectoryWindow.Season => sorted,
            _ => sorted
        };
    }

    // Moyenne mobile 3 matchs pour ligne de tendance
    public static IReadOnlyList<double?> RollingAverage3(
        IReadOnlyList<PlayerTrajectoryPoint> sortedPoints,
        TrajectoryMetricType type,
        Func<PlayerTrajectoryPoint, double?>? countSelector = null)
    {
        var result = new List<double?>();
        for (int i = 0; i < sortedPoints.Count; i++)
        {
            var window = sortedPoints.Skip(Math.Max(0, i - 2)).Take(Math.Min(3, i + 1)).ToList();
            double? val;
            if (type == TrajectoryMetricType.Rate)
                val = AggregateRate(window);
            else
                val = AggregateCount(window, countSelector ?? (p => p.MetricValue));
            result.Add(val);
        }
        return result;
    }

    // Résumé d'une fenêtre
    public static TrajectoryWindowSummary BuildWindowSummary(
        IReadOnlyList<PlayerTrajectoryPoint> windowPoints,
        IReadOnlyList<PlayerTrajectoryPoint> seasonPoints,
        TrajectoryMetricDefinition metric,
        TrajectoryWindow window)
    {
        double? aggregated;
        if (metric.Type == TrajectoryMetricType.Rate)
            aggregated = AggregateRate(windowPoints);
        else
            aggregated = AggregateCount(windowPoints, p => p.MetricValue);

        double? seasonAgg;
        if (metric.Type == TrajectoryMetricType.Rate)
            seasonAgg = AggregateRate(seasonPoints);
        else
            seasonAgg = AggregateCount(seasonPoints, p => p.MetricValue);

        var delta = (aggregated.HasValue && seasonAgg.HasValue) ? aggregated.Value - seasonAgg.Value : (double?)null;

        var deltaLabel = delta.HasValue
            ? (metric.Type == TrajectoryMetricType.Rate
                ? $"{(delta >= 0 ? "+" : "")}{delta.Value:F1} pts"
                : $"{(delta >= 0 ? "+" : "")}{delta.Value:F1} {metric.Unit}")
            : "";

        var eligible = windowPoints.Count(p => p.Availability != "DATA_MISSING" && p.MetricValue != null);
        var trend = ClassifyTrend(windowPoints, seasonAgg, metric);

        return new TrajectoryWindowSummary(window, aggregated, seasonAgg, delta, deltaLabel, eligible, windowPoints.Count, trend);
    }

    // Classification tendance
    public static TrendState ClassifyTrend(
        IReadOnlyList<PlayerTrajectoryPoint> points,
        double? seasonReference,
        TrajectoryMetricDefinition metric)
    {
        var eligible = points.Where(p => p.Availability != "DATA_MISSING" && p.MetricValue != null).ToList();
        if (eligible.Count < 5) return TrendState.InsufficientData;
        if (!seasonReference.HasValue) return TrendState.InsufficientData;

        double? recent;
        if (metric.Type == TrajectoryMetricType.Rate)
            recent = AggregateRate(eligible.TakeLast(3).ToList());
        else
            recent = AggregateCount(eligible.TakeLast(3).ToList(), p => p.MetricValue);

        if (!recent.HasValue) return TrendState.InsufficientData;

        var diff = recent.Value - seasonReference.Value;
        // Threshold: 5% relative du range pour éviter faux positifs
        var threshold = Math.Abs(seasonReference.Value) * 0.05 + 0.1;

        if (metric.Direction == TrajectoryMetricDirection.LowerIsBetter)
            diff = -diff; // Inverser : baisse valeur = amélioration

        if (diff > threshold) return TrendState.Progressing;
        if (diff < -threshold) return TrendState.Declining;
        return TrendState.Stable;
    }

    // Delta label pour affichage dans le tooltip
    public static string FormatDeltaLabel(double delta, TrajectoryMetricDefinition metric)
    {
        var sign = delta >= 0 ? "+" : "";
        return metric.Type == TrajectoryMetricType.Rate
            ? $"{sign}{delta:F1} pts"
            : $"{sign}{delta:F1} {metric.Unit}";
    }

    // Libellé PlayingTimeAvailability
    public static string GetPlayingTimeLabel(PlayingTimeAvailability status) => status switch
    {
        PlayingTimeAvailability.RecordedDirect => "Temps enregistré",
        PlayingTimeAvailability.RecordedHistoricalId => "Temps enregistré",
        PlayingTimeAvailability.MatchedStrongIdentity => "Temps rapproché",
        PlayingTimeAvailability.MatchedUniqueMatchRoster => "Temps rapproché",
        PlayingTimeAvailability.DerivedFromSubstitutions => "Temps dérivé",
        PlayingTimeAvailability.PartialData => "Temps partiel",
        PlayingTimeAvailability.IdentityConflict => "Identité à vérifier",
        _ => "Temps non disponible"
    };
}
