using HandWStat.Models.Analytics;

namespace HandWStat.Tests.Unit;

public class SpatialZoneVisualsTests
{
    // --- ToVisualTriggerKey ---

    [Fact]
    public void ToVisualTriggerKey_TG_ReturnsTD()
    {
        Assert.Equal("TD", SpatialZoneVisuals.ToVisualTriggerKey("TG"));
    }

    [Fact]
    public void ToVisualTriggerKey_TD_ReturnsTG()
    {
        Assert.Equal("TG", SpatialZoneVisuals.ToVisualTriggerKey("TD"));
    }

    [Fact]
    public void ToVisualTriggerKey_TGWithSuffix_MirrorsSuffix()
    {
        Assert.Equal("TD1", SpatialZoneVisuals.ToVisualTriggerKey("TG1"));
    }

    [Fact]
    public void ToVisualTriggerKey_Null_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, SpatialZoneVisuals.ToVisualTriggerKey(null));
    }

    [Fact]
    public void ToVisualTriggerKey_Whitespace_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, SpatialZoneVisuals.ToVisualTriggerKey("   "));
    }

    [Fact]
    public void ToVisualTriggerKey_OtherKey_ReturnsNormalized()
    {
        Assert.Equal("BG", SpatialZoneVisuals.ToVisualTriggerKey("bg"));
    }

    // --- ToPaletteRate (non-goalkeeper: passthrough clamp) ---

    [Fact]
    public void ToPaletteRate_FieldPlayer_ReturnsRateDirectly()
    {
        Assert.Equal(32.0, SpatialZoneVisuals.ToPaletteRate(32, isGoalkeeper: false));
    }

    [Fact]
    public void ToPaletteRate_FieldPlayer_ClampsAtZero()
    {
        Assert.Equal(0.0, SpatialZoneVisuals.ToPaletteRate(-10, isGoalkeeper: false));
    }

    [Fact]
    public void ToPaletteRate_FieldPlayer_ClampsAt100()
    {
        Assert.Equal(100.0, SpatialZoneVisuals.ToPaletteRate(150, isGoalkeeper: false));
    }

    // --- ToPaletteRate (goalkeeper: normalized 10%–55%) ---

    [Fact]
    public void ToPaletteRate_Goalkeeper_AtMinBoundary_ReturnsZero()
    {
        var result = SpatialZoneVisuals.ToPaletteRate(10, isGoalkeeper: true);
        Assert.Equal(0.0, result, precision: 6);
    }

    [Fact]
    public void ToPaletteRate_Goalkeeper_AtMaxBoundary_Returns100()
    {
        var result = SpatialZoneVisuals.ToPaletteRate(55, isGoalkeeper: true);
        Assert.Equal(100.0, result, precision: 6);
    }

    [Fact]
    public void ToPaletteRate_Goalkeeper_AtMidpoint_Returns50()
    {
        var result = SpatialZoneVisuals.ToPaletteRate(32.5, isGoalkeeper: true);
        Assert.Equal(50.0, result, precision: 4);
    }

    [Fact]
    public void ToPaletteRate_Goalkeeper_BelowMin_ClampsAtZero()
    {
        var result = SpatialZoneVisuals.ToPaletteRate(0, isGoalkeeper: true);
        Assert.Equal(0.0, result, precision: 6);
    }

    [Fact]
    public void ToPaletteRate_Goalkeeper_AboveMax_ClampsAt100()
    {
        var result = SpatialZoneVisuals.ToPaletteRate(100, isGoalkeeper: true);
        Assert.Equal(100.0, result, precision: 6);
    }
}
