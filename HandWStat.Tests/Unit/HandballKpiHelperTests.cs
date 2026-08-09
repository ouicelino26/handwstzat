using HandWStat.Models.Analytics;

namespace HandWStat.Tests.Unit;

public class HandballKpiHelperTests
{
    // --- PerMatch ---

    [Fact]
    public void PerMatch_NormalValues_ReturnsDivision()
    {
        Assert.Equal(2.0, HandballKpiHelper.PerMatch(10, 5));
    }

    [Fact]
    public void PerMatch_ZeroNumerator_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.PerMatch(0, 5));
    }

    [Fact]
    public void PerMatch_ZeroMatches_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.PerMatch(10, 0));
    }

    // --- Ratio ---

    [Fact]
    public void Ratio_NormalValues_ReturnsDivision()
    {
        Assert.Equal(0.3, HandballKpiHelper.Ratio(3, 10), precision: 10);
    }

    [Fact]
    public void Ratio_BothZero_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.Ratio(0, 0));
    }

    [Fact]
    public void Ratio_ZeroDenominatorPositiveNumerator_ReturnsNumerator()
    {
        // Edge case KH-02: documented intentional behavior
        Assert.Equal(5.0, HandballKpiHelper.Ratio(5, 0));
    }

    [Fact]
    public void Ratio_ZeroDenominatorZeroNumerator_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.Ratio(0, 0));
    }

    // --- Share ---

    [Fact]
    public void Share_NormalValues_ReturnsPercentage()
    {
        Assert.Equal(25.0, HandballKpiHelper.Share(25, 100));
    }

    [Fact]
    public void Share_BothZero_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.Share(0, 0));
    }

    // --- SuccessVsWasteShare ---

    [Fact]
    public void SuccessVsWasteShare_NormalValues_ReturnsSuccessPercent()
    {
        Assert.Equal(80.0, HandballKpiHelper.SuccessVsWasteShare(8, 2));
    }

    [Fact]
    public void SuccessVsWasteShare_BothZero_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.SuccessVsWasteShare(0, 0));
    }

    [Fact]
    public void SuccessVsWasteShare_AllSuccesses_Returns100()
    {
        Assert.Equal(100.0, HandballKpiHelper.SuccessVsWasteShare(5, 0));
    }

    [Fact]
    public void SuccessVsWasteShare_AllFailures_ReturnsZero()
    {
        Assert.Equal(0.0, HandballKpiHelper.SuccessVsWasteShare(0, 5));
    }
}
