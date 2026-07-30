using HandWStat.Models.Analytics;
using HandWStat.Models.Contracts;

namespace HandWStat.Tests;

public sealed class HandballKpiHelperTests
{
    [Fact]
    public void Ratio_WithZeroDenominator_IsNotCalculable()
    {
        Assert.Null(HandballKpiHelper.Ratio(5, 0));
    }

    [Fact]
    public void Ratio_WithFinitePositiveDenominator_ReturnsQuotient()
    {
        Assert.Equal(2.5d, HandballKpiHelper.Ratio(5, 2));
    }

    [Fact]
    public void FormatRatio_WithNullValue_ReturnsNA()
    {
        Assert.Equal("N/A", HandballKpiHelper.FormatRatio(null));
    }

    [Fact]
    public void ShotAttempts_DoesNotCountBlockedShotTwice()
    {
        var offense = new PlayerOffenseStatsDto
        {
            TotalButs = 2,
            TirsRates = 4,
            PenaltyRate = 1,
            TirContre = 1
        };

        Assert.Equal(7, HandballKpiHelper.ShotAttempts(offense));
        Assert.Equal(5, HandballKpiHelper.ShotWaste(offense));
    }
}
