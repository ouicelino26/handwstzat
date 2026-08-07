// Models/Analytics/CourtZoneMapper.cs
using HandWStat.Models.Contracts;

namespace HandWStat.Models.Analytics;

/// <summary>
/// Maps API DTOs to CourtZoneStat and applies client-side filters.
/// NOTE: AttackType filtering is approximate — it operates on OutcomeCount aggregates
/// (grouped by EventName), not on individual event records. The denominator may not
/// exactly match what a server-side filter would produce.
/// </summary>
public static class CourtZoneMapper
{
    // ── Event name sets ──────────────────────────────────────────────────
    private static readonly HashSet<string> OpenPlayGoalEvents =
        new(StringComparer.OrdinalIgnoreCase) { "But" };

    private static readonly HashSet<string> OpenPlayShotEvents =
        new(StringComparer.OrdinalIgnoreCase) { "But", "Tir a cote", "Tir sur poteau", "Tir arrete", "Tir rate", "Tir contre" };

    private static readonly HashSet<string> SevenMeterGoalEvents =
        new(StringComparer.OrdinalIgnoreCase) { "But sur penalty" };

    private static readonly HashSet<string> SevenMeterShotEvents =
        new(StringComparer.OrdinalIgnoreCase) { "But sur penalty", "Penalty sur poteau", "Penalty rate", "Penalty arrete" };

    private static readonly HashSet<string> SaveEvents =
        new(StringComparer.OrdinalIgnoreCase) { "Tir arrete", "Penalty arrete" };

    private static readonly HashSet<string> OffTargetEvents =
        new(StringComparer.OrdinalIgnoreCase) { "Tir a cote", "Tir sur poteau", "Penalty sur poteau" };

    private static readonly HashSet<string> BlockedEvents =
        new(StringComparer.OrdinalIgnoreCase) { "Tir contre" };

    private static readonly HashSet<string> GoalkeeperSaveEvents =
        new(StringComparer.OrdinalIgnoreCase) { "Gardien arrete le tir", "Gardien arrete le penalty" };

    // ── Mapping ──────────────────────────────────────────────────────────

    public static CourtZoneStat MapShotZone(ZoneStatDto dto)
    {
        var key = dto.ZoneCode ?? string.Empty;
        var label = ZoneNameCatalog.GetShotZoneLabel(key);
        var outcomes = MapOutcomes(dto.Outcomes);
        return new CourtZoneStat(
            Key: key,
            Label: label,
            Rate: dto.SuccessRate,
            Attempts: dto.Attempts,
            Successes: dto.SuccessCount,
            SampleReliable: dto.Attempts >= 5,
            IsAvailable: dto.Attempts > 0,
            Outcomes: outcomes);
    }

    public static CourtZoneStat MapTriggerZone(TriggerZoneStatDto dto)
    {
        var rawKey = dto.TriggerCode ?? string.Empty;
        // Apply visual inversion TG<->TD so the map renders correctly (180 degree rotation context)
        var visualKey = SpatialZoneVisuals.ToVisualTriggerKey(rawKey);
        var label = ZoneNameCatalog.GetTriggerZoneLabel(visualKey);
        var outcomes = MapOutcomes(dto.Outcomes);
        return new CourtZoneStat(
            Key: visualKey,
            Label: label,
            Rate: dto.SuccessRate,
            Attempts: dto.Attempts,
            Successes: dto.SuccessCount,
            SampleReliable: dto.Attempts >= 5,
            IsAvailable: dto.Attempts > 0,
            Outcomes: outcomes);
    }

    // ── AttackType filter ────────────────────────────────────────────────

    /// <summary>
    /// Filters a zone's statistics by attack type using client-side outcome aggregates.
    /// APPROXIMATE: uses EventName-based OutcomeCount groups, not individual events.
    /// </summary>
    public static CourtZoneStat FilterByAttackType(CourtZoneStat zone, PlayerCourtAttackType attackType, bool isGoalkeeper)
    {
        if (attackType == PlayerCourtAttackType.All)
            return zone;

        var shotSet = attackType == PlayerCourtAttackType.OpenPlay ? OpenPlayShotEvents : SevenMeterShotEvents;
        var goalSet = attackType == PlayerCourtAttackType.OpenPlay ? OpenPlayGoalEvents : SevenMeterGoalEvents;

        var filteredOutcomes = zone.Outcomes
            .Where(o => shotSet.Contains(o.Label))
            .ToList();

        int attempts = filteredOutcomes.Sum(o => o.Count);
        int successes = filteredOutcomes
            .Where(o => goalSet.Contains(o.Label))
            .Sum(o => o.Count);

        double rate = attempts > 0 ? (double)successes / attempts * 100.0 : 0.0;

        return zone with
        {
            Attempts = attempts,
            Successes = successes,
            Rate = rate,
            SampleReliable = attempts >= 5,
            IsAvailable = attempts > 0,
            Outcomes = filteredOutcomes
        };
    }

    /// <summary>
    /// Filters a zone's statistics by shot result.
    /// APPROXIMATE: uses EventName-based OutcomeCount groups, not individual events.
    /// </summary>
    public static CourtZoneStat ApplyResultFilter(CourtZoneStat zone, PlayerCourtShotResult result, bool isGoalkeeper)
    {
        if (result == PlayerCourtShotResult.All)
            return zone;

        HashSet<string> keepSet = result switch
        {
            PlayerCourtShotResult.Goal     => isGoalkeeper ? GoalkeeperSaveEvents : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "But", "But sur penalty" },
            PlayerCourtShotResult.Save     => SaveEvents,
            PlayerCourtShotResult.OffTarget => OffTargetEvents,
            PlayerCourtShotResult.Blocked  => BlockedEvents,
            _                              => throw new ArgumentOutOfRangeException(nameof(result))
        };

        var filteredOutcomes = zone.Outcomes
            .Where(o => keepSet.Contains(o.Label))
            .ToList();

        int attempts = filteredOutcomes.Sum(o => o.Count);
        // For result filters, "successes" keeps the same meaning as the overall zone
        int successes = filteredOutcomes
            .Where(o => OpenPlayGoalEvents.Contains(o.Label) || SevenMeterGoalEvents.Contains(o.Label))
            .Sum(o => o.Count);

        double rate = attempts > 0 ? (double)successes / attempts * 100.0 : 0.0;

        return zone with
        {
            Attempts = attempts,
            Successes = successes,
            Rate = rate,
            SampleReliable = attempts >= 5,
            IsAvailable = attempts > 0,
            Outcomes = filteredOutcomes
        };
    }

    // ── Adapters from ZoneStat (legacy compatibility) ────────────────────

    /// <summary>
    /// Converts a legacy ZoneStat to CourtZoneStat for components that have not yet been migrated.
    /// </summary>
    public static CourtZoneStat FromZoneStat(ZoneStat zone)
    {
        return new CourtZoneStat(
            Key: zone.Key,
            Label: zone.Label,
            Rate: zone.Rate,
            Attempts: zone.Attempts,
            Successes: zone.Successes,
            SampleReliable: zone.Attempts >= 5,
            IsAvailable: zone.Attempts > 0,
            Outcomes: zone.Outcomes);
    }

    public static IReadOnlyList<CourtZoneStat> FromZoneStats(IReadOnlyList<ZoneStat> zones)
        => zones.Select(FromZoneStat).ToList();

    // ── Helpers ──────────────────────────────────────────────────────────

    private static IReadOnlyList<OutcomeCount> MapOutcomes(List<ZoneOutcomeDto>? dtoOutcomes)
    {
        if (dtoOutcomes is null || dtoOutcomes.Count == 0)
            return [];
        return dtoOutcomes
            .Select(o => new OutcomeCount(o.EventName ?? string.Empty, o.Count))
            .ToList();
    }
}
