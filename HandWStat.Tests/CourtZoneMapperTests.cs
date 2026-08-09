using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Tests;

/// <summary>
/// Phase G.3 court zone mapper tests — shot zones, trigger zones,
/// attack type filtering, result filtering, and catalog correctness.
/// </summary>
public sealed class CourtZoneMapperTests
{
    // ── MapShotZone ───────────────────────────────────────────────────────

    [Fact]
    public void Court_MapShotZone_ReturnsLabelFromCatalog()
    {
        var dto = new ZoneStatDto { ZoneCode = "BG7", Attempts = 10, SuccessCount = 5, SuccessRate = 50.0 };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.Equal("Centre-bas gauche", result.Label);
    }

    [Fact]
    public void Court_MapShotZone_SampleReliable_WhenFiveOrMoreAttempts()
    {
        var dto = new ZoneStatDto { ZoneCode = "BD1", Attempts = 5, SuccessCount = 2, SuccessRate = 40.0 };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.True(result.SampleReliable);
    }

    [Fact]
    public void Court_MapShotZone_SampleNotReliable_WhenFewerThanFive()
    {
        var dto = new ZoneStatDto { ZoneCode = "BD1", Attempts = 4, SuccessCount = 2, SuccessRate = 50.0 };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.False(result.SampleReliable);
    }

    [Fact]
    public void Court_MapShotZone_IsAvailable_WhenAttemptsGreaterThanZero()
    {
        var dto = new ZoneStatDto { ZoneCode = "BG1", Attempts = 3, SuccessCount = 1, SuccessRate = 33.3 };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.True(result.IsAvailable);
    }

    [Fact]
    public void Court_MapShotZone_NotAvailable_WhenZeroAttempts()
    {
        var dto = new ZoneStatDto { ZoneCode = "BG1", Attempts = 0, SuccessCount = 0, SuccessRate = 0.0 };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.False(result.IsAvailable);
    }

    // ── MapTriggerZone ────────────────────────────────────────────────────

    [Fact]
    public void Court_MapTriggerZone_AppliesVisualKeyInversion()
    {
        // TG3 from backend → TD3 as visual key (TG<->TD swap)
        var dto = new TriggerZoneStatDto { TriggerCode = "TG3", Attempts = 8, SuccessCount = 4, SuccessRate = 50.0 };
        var result = CourtZoneMapper.MapTriggerZone(dto);
        Assert.Equal("TD3", result.Key);
    }

    [Fact]
    public void Court_MapTriggerZone_ReturnsLabelFromCatalog()
    {
        // TD3 visual key → "Bord gauche avance"
        var dto = new TriggerZoneStatDto { TriggerCode = "TG3", Attempts = 8, SuccessCount = 4, SuccessRate = 50.0 };
        var result = CourtZoneMapper.MapTriggerZone(dto);
        Assert.Equal("Bord gauche avance", result.Label);
    }

    [Fact]
    public void Court_MapTriggerZone_SampleReliable_WhenFiveOrMoreAttempts()
    {
        var dto = new TriggerZoneStatDto { TriggerCode = "TD9", Attempts = 5, SuccessCount = 2, SuccessRate = 40.0 };
        var result = CourtZoneMapper.MapTriggerZone(dto);
        Assert.True(result.SampleReliable);
    }

    // ── Zone key separation ───────────────────────────────────────────────

    [Fact]
    public void Court_SeparatesShotAndTriggerZones_KeysDoNotOverlap()
    {
        var shotKeys = ZoneNameCatalog.AllShotZoneLabels.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var triggerKeys = ZoneNameCatalog.AllTriggerZoneLabels.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var overlap = shotKeys.Intersect(triggerKeys).ToList();
        Assert.Empty(overlap);
    }

    [Fact]
    public void Court_DoesNotInferTriggerFromShotZone()
    {
        // A shot zone key (BG1) must not be identified as a trigger zone key
        Assert.False(SpatialZoneVisuals.IsTriggerZoneKey("BG1"));
        Assert.True(SpatialZoneVisuals.IsTriggerZoneKey("TG1"));
        Assert.True(SpatialZoneVisuals.IsTriggerZoneKey("TD1"));
    }

    // ── AttackType filter ─────────────────────────────────────────────────

    [Fact]
    public void Court_AllFilterIncludesAllSupportedShots()
    {
        var zone = MakeZoneWithOutcomes([
            ("But", 3),
            ("But sur penalty", 2),
            ("Tir arrete", 1),
            ("Penalty arrete", 1),
        ]);

        var result = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.All, false);
        // All filter returns the zone unchanged
        Assert.Equal(7, result.Attempts);
    }

    [Fact]
    public void Court_OpenPlayFilterExcludesSevenMeterOutcomes()
    {
        var zone = MakeZoneWithOutcomes([
            ("But", 3),
            ("But sur penalty", 2),
            ("Tir arrete", 1),
        ]);

        var result = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.OpenPlay, false);
        // Only open play events: "But" (3) + "Tir arrete" (1) = 4
        Assert.Equal(4, result.Attempts);
        Assert.DoesNotContain(result.Outcomes, o => o.Label == "But sur penalty");
    }

    [Fact]
    public void Court_SevenMeterFilterExcludesOpenPlayOutcomes()
    {
        var zone = MakeZoneWithOutcomes([
            ("But", 3),
            ("But sur penalty", 2),
            ("Penalty arrete", 1),
        ]);

        var result = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.SevenMeter, false);
        // Only 7m events: "But sur penalty" (2) + "Penalty arrete" (1) = 3
        Assert.Equal(3, result.Attempts);
        Assert.DoesNotContain(result.Outcomes, o => o.Label == "But");
    }

    [Fact]
    public void Court_FilterByAttackType_ZeroAttemptsWhenNoMatchingOutcomes()
    {
        var zone = MakeZoneWithOutcomes([
            ("But", 3),
            ("Tir arrete", 2),
        ]);

        // Request 7m filter but no 7m events exist
        var result = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.SevenMeter, false);
        Assert.Equal(0, result.Attempts);
        Assert.False(result.IsAvailable);
    }

    // ── Rate and reliability ──────────────────────────────────────────────

    [Fact]
    public void Court_ZeroAttemptsReturnsUnavailableRate()
    {
        var dto = new ZoneStatDto { ZoneCode = "BG12", Attempts = 0, SuccessCount = 0, SuccessRate = 0.0 };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.Equal(0, result.Attempts);
        Assert.False(result.IsAvailable);
        Assert.Equal(0.0, result.Rate);
    }

    [Fact]
    public void Court_PreservesSampleReliabilityAfterFilter()
    {
        var zone = MakeZoneWithOutcomes([
            ("But", 3),
            ("Tir arrete", 2),
            ("Tir a cote", 2),
        ]);
        // 7 total open play shots -> reliable after open play filter
        var result = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.OpenPlay, false);
        Assert.True(result.SampleReliable);
    }

    // ── Seven meter semantics ─────────────────────────────────────────────

    [Fact]
    public void SevenMeterExecution_UsesExecutionZoneOnly()
    {
        // 7m shots map to shot zones (execution point), not trigger zones
        var dto = new ZoneStatDto
        {
            ZoneCode = "BG1",
            Attempts = 5,
            SuccessCount = 3,
            SuccessRate = 60.0,
            Outcomes =
            [
                new ZoneOutcomeDto { EventName = "But sur penalty", Count = 3 },
                new ZoneOutcomeDto { EventName = "Penalty arrete", Count = 2 },
            ]
        };
        var result = CourtZoneMapper.MapShotZone(dto);
        Assert.False(SpatialZoneVisuals.IsTriggerZoneKey(result.Key));
        Assert.Equal(5, result.Attempts);
    }

    [Fact]
    public void SevenMeterTrigger_NeverFallsBackToExecutionZone()
    {
        // A trigger zone key must always remain a trigger zone key
        var dto = new TriggerZoneStatDto { TriggerCode = "TD9", Attempts = 4, SuccessCount = 2, SuccessRate = 50.0 };
        var result = CourtZoneMapper.MapTriggerZone(dto);
        Assert.True(SpatialZoneVisuals.IsTriggerZoneKey(result.Key));
    }

    [Fact]
    public void SevenMeterResults_SeparateGoalsSavesFromOpenPlay()
    {
        var zone = MakeZoneWithOutcomes([
            ("But", 3),
            ("But sur penalty", 2),
            ("Tir arrete", 1),
            ("Penalty arrete", 1),
        ]);

        var openPlay = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.OpenPlay, false);
        var sevenMeter = CourtZoneMapper.FilterByAttackType(zone, PlayerCourtAttackType.SevenMeter, false);

        // Open play goals (But) and 7m goals (But sur penalty) must be separate
        Assert.Equal(3, openPlay.Successes);
        Assert.Equal(2, sevenMeter.Successes);
    }

    // ── Label correctness ─────────────────────────────────────────────────

    [Fact]
    public void Court_ShotZoneLabel_NotEqualToKey_ForKnownZones()
    {
        foreach (var kvp in ZoneNameCatalog.AllShotZoneLabels)
        {
            Assert.NotEqual(kvp.Key, kvp.Value, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Court_TriggerZoneLabel_NotEqualToKey_ForKnownZones()
    {
        foreach (var kvp in ZoneNameCatalog.AllTriggerZoneLabels)
        {
            Assert.NotEqual(kvp.Key, kvp.Value, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Court_UnknownZoneKey_FallsBackToKey()
    {
        var unknownKey = "UNKNOWN_XYZ";
        Assert.Equal(unknownKey, ZoneNameCatalog.GetShotZoneLabel(unknownKey));
        Assert.Equal(unknownKey, ZoneNameCatalog.GetTriggerZoneLabel(unknownKey));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static CourtZoneStat MakeZoneWithOutcomes(IEnumerable<(string Name, int Count)> outcomes)
    {
        var outcomeList = outcomes
            .Select(o => new OutcomeCount(o.Name, o.Count))
            .ToList()
            .AsReadOnly();

        int total = outcomeList.Sum(o => o.Count);
        return new CourtZoneStat(
            Key: "BG1",
            Label: "Zone test",
            Rate: 50.0,
            Attempts: total,
            Successes: 0,
            SampleReliable: total >= 5,
            IsAvailable: total > 0,
            Outcomes: outcomeList);
    }
}
