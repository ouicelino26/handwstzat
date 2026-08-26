using HandWStat.Services.Analytics;

namespace HandWStat.Tests;

// R5 — CompareAnalyticsBuilder.BuildChartKey regression tests.
// Key must reflect player identity, not just count, to force ApexChart reinitialization on player swap.
public sealed class ComparePage_ChartKeyTests
{
    [Fact]
    public void BuildChartKey_EmptyList_ReturnsEmptyString()
    {
        var key = CompareAnalyticsBuilder.BuildChartKey([]);
        Assert.Equal(string.Empty, key);
    }

    [Fact]
    public void BuildChartKey_SinglePlayer_ReturnsIdString()
    {
        var key = CompareAnalyticsBuilder.BuildChartKey([42]);
        Assert.Equal("42", key);
    }

    [Fact]
    public void BuildChartKey_TwoPlayers_ReturnsBothIds()
    {
        var key = CompareAnalyticsBuilder.BuildChartKey([1, 2]);
        Assert.Equal("1_2", key);
    }

    [Fact]
    public void BuildChartKey_DifferentPlayersSameCount_ProducesDifferentKeys()
    {
        // The old Count-based key would give "2" for both → same key (bug).
        var keyAB = CompareAnalyticsBuilder.BuildChartKey([1, 2]);
        var keyCD = CompareAnalyticsBuilder.BuildChartKey([3, 4]);
        Assert.NotEqual(keyAB, keyCD);
    }

    [Fact]
    public void BuildChartKey_SamePlayersDifferentOrder_ProduceDifferentKeys()
    {
        // Swap order in the comparison list → different key → chart reinitializes.
        var keyAB = CompareAnalyticsBuilder.BuildChartKey([1, 2]);
        var keyBA = CompareAnalyticsBuilder.BuildChartKey([2, 1]);
        Assert.NotEqual(keyAB, keyBA);
    }

    [Fact]
    public void BuildChartKey_SamePlayersAndOrder_ProduceSameKey()
    {
        var key1 = CompareAnalyticsBuilder.BuildChartKey([5, 10, 15]);
        var key2 = CompareAnalyticsBuilder.BuildChartKey([5, 10, 15]);
        Assert.Equal(key1, key2);
    }
}
