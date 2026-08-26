using HandWStat.Models.Analytics;

namespace HandWStat.Services.Analytics;

public sealed record SpatialZoneMetric(
    CourtZoneStat Zone,
    double? AttemptShare,
    double? GoalShare,
    QualityTierResult Quality);

public static class SpatialAnalyticsBuilder
{
    public const int MinShotZoneSample = 5;

    public static double? ComputeAttemptShare(int zoneAttempts, int totalAttempts)
    {
        if (totalAttempts <= 0) return null;
        return (double)zoneAttempts / totalAttempts * 100.0;
    }

    public static double? ComputeGoalShare(int zoneGoals, int totalGoals)
    {
        if (totalGoals <= 0) return null;
        return (double)zoneGoals / totalGoals * 100.0;
    }

    public static double? ComputeShotSuccessRate(int goals, int attempts)
    {
        if (attempts <= 0) return null;
        return (double)goals / attempts * 100.0;
    }

    public static double? ComputeSpatialCoverage(int mappedAttempts, int totalAttempts)
    {
        if (totalAttempts <= 0) return null;
        return (double)mappedAttempts / totalAttempts * 100.0;
    }

    public static (int TotalAttempts, int TotalGoals) AggregateZoneTotals(IReadOnlyList<CourtZoneStat> zones)
    {
        return (zones.Sum(z => z.Attempts), zones.Sum(z => z.Successes));
    }

    public static SpatialZoneMetric BuildZoneMetric(
        CourtZoneStat zone, int totalAttempts, int totalGoals, int minSample = MinShotZoneSample)
    {
        var quality = AnalyticsQualityPolicy.EvaluateTier(null, zone.Attempts, minSample);
        var attemptShare = ComputeAttemptShare(zone.Attempts, totalAttempts);
        var goalShare = ComputeGoalShare(zone.Successes, totalGoals);
        return new SpatialZoneMetric(zone, attemptShare, goalShare, quality);
    }

    public static IReadOnlyList<SpatialZoneMetric> BuildZoneMetrics(
        IReadOnlyList<CourtZoneStat> zones, int minSample = MinShotZoneSample)
    {
        var (totalAttempts, totalGoals) = AggregateZoneTotals(zones);
        return zones.Select(z => BuildZoneMetric(z, totalAttempts, totalGoals, minSample))
                    .ToList()
                    .AsReadOnly();
    }
}
