using HandWStat.Models.Analytics;

namespace HandWStat.Tests;

/// <summary>
/// Phase G.3 court zone name catalog tests — label counts, fallback behavior, and code exclusion.
/// </summary>
public sealed class CourtZoneNameCatalogTests
{
    [Fact]
    public void ZoneNameCatalog_Returns24ShotZoneLabels()
    {
        Assert.Equal(24, ZoneNameCatalog.AllShotZoneLabels.Count);
    }

    [Fact]
    public void ZoneNameCatalog_Returns18TriggerZoneLabels()
    {
        Assert.Equal(18, ZoneNameCatalog.AllTriggerZoneLabels.Count);
    }

    [Fact]
    public void ZoneNameCatalog_UnknownKey_FallsBackToKey()
    {
        var key = "ZZZUNKNOWN";
        Assert.Equal(key, ZoneNameCatalog.GetShotZoneLabel(key));
        Assert.Equal(key, ZoneNameCatalog.GetTriggerZoneLabel(key));
    }

    [Fact]
    public void ZoneNameCatalog_ShotZoneLabelsAreNotCodes()
    {
        // Labels must not contain "BG" or "BD" as the entire label (they are human-readable)
        foreach (var label in ZoneNameCatalog.AllShotZoneLabels.Values)
        {
            Assert.False(
                label.StartsWith("BG", StringComparison.OrdinalIgnoreCase) && label.Length <= 4,
                $"Label '{label}' looks like a raw zone code");
            Assert.False(
                label.StartsWith("BD", StringComparison.OrdinalIgnoreCase) && label.Length <= 4,
                $"Label '{label}' looks like a raw zone code");
        }
    }

    [Fact]
    public void ZoneNameCatalog_TriggerZoneLabelsAreNotCodes()
    {
        // Labels must not look like raw TG/TD codes (length constraint separates codes from labels)
        foreach (var label in ZoneNameCatalog.AllTriggerZoneLabels.Values)
        {
            Assert.False(
                label.StartsWith("TG", StringComparison.OrdinalIgnoreCase) && label.Length <= 4,
                $"Label '{label}' looks like a raw trigger code");
            Assert.False(
                label.StartsWith("TD", StringComparison.OrdinalIgnoreCase) && label.Length <= 4,
                $"Label '{label}' looks like a raw trigger code");
        }
    }
}
