namespace HandWStat.Models.Analytics;

public static class SpatialZoneVisuals
{
    public const double GoalkeeperHeatMinRate = 10;
    public const double GoalkeeperHeatMaxRate = 55;

    public static bool IsTriggerZoneKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalized = key.Trim().ToUpperInvariant();
        return normalized.StartsWith("TG", StringComparison.Ordinal)
            || normalized.StartsWith("TD", StringComparison.Ordinal);
    }

    public static string ToVisualTriggerKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var normalized = key.Trim().ToUpperInvariant();

        if (normalized.StartsWith("TG", StringComparison.Ordinal))
        {
            return $"TD{normalized[2..]}";
        }

        if (normalized.StartsWith("TD", StringComparison.Ordinal))
        {
            return $"TG{normalized[2..]}";
        }

        return normalized;
    }

    public static string ToBackendTriggerKey(string? key)
    {
        return ToVisualTriggerKey(key);
    }

    public static double ToPaletteRate(double rawRate, bool isGoalkeeper)
    {
        var boundedRate = Math.Clamp(rawRate, 0, 100);

        if (!isGoalkeeper)
        {
            return boundedRate;
        }

        var span = GoalkeeperHeatMaxRate - GoalkeeperHeatMinRate;

        if (span <= 0)
        {
            return boundedRate;
        }

        var normalizedRate = (boundedRate - GoalkeeperHeatMinRate) / span;
        return Math.Clamp(normalizedRate * 100d, 0, 100);
    }
}
